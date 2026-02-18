using DigitalTwinPlatform.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.Tenancy;
using DigitalTwinPlatform.Domain.Entities.Enums;

namespace DigitalTwinPlatform.API.Services.Simulation;

public class SimulationSchedulerService : ISimulationSchedulerService
{
    private readonly IMachineRepository _machineRepository;
    private readonly ISimulationService _simulationService;
    private readonly ITenantService _tenantService;
    private readonly IOptions<SimulationOptions> _options;
    private readonly ILogger<SimulationSchedulerService> _logger;
    private readonly ConcurrentDictionary<Guid, DateTime> _machineSchedule;
    private readonly Timer _schedulerTimer;

    public SimulationSchedulerService(
        IMachineRepository machineRepository,
        ISimulationService simulationService,
        ITenantService tenantService,
        IOptions<SimulationOptions> options,
        ILogger<SimulationSchedulerService> logger)
    {
        _machineRepository = machineRepository;
        _simulationService = simulationService;
        _tenantService = tenantService;
        _options = options;
        _logger = logger;
        _machineSchedule = new ConcurrentDictionary<Guid, DateTime>();

        // Start the background scheduler timer
        _schedulerTimer = new Timer(ExecuteScheduledSimulations, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public async Task ScheduleAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting simulation scheduling for all machines");

            // Get all active machines
            var machines = await _machineRepository.GetAllAsync(m => m.IsActive, ct);
            
            // Group by tenant for multi-tenant scheduling
            var tenantGroups = machines.GroupBy(m => m.TenantId ?? Guid.Empty);

            foreach (var tenantGroup in tenantGroups)
            {
                var tenantId = tenantGroup.Key;
                _logger.LogInformation("Scheduling simulations for tenant {TenantId} with {MachineCount} machines", 
                    tenantId, tenantGroup.Count());

                // Set tenant context for this batch
                if (tenantId != Guid.Empty)
                {
                    _tenantService.SetTenantContext(tenantId.ToString());
                }

                // Schedule each machine based on its configuration and priority
                foreach (var machine in tenantGroup)
                {
                    await ScheduleMachineAsync(machine);
                }
            }

            _logger.LogInformation("Simulation scheduling completed. Total scheduled machines: {Count}", 
                _machineSchedule.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during simulation scheduling");
            throw;
        }
    }

    private Task ScheduleMachineAsync(Machine machine)
    {
        // Determine simulation frequency based on machine health and configuration
        var frequency = GetSimulationFrequency(machine);
        var nextRunTime = DateTime.UtcNow.Add(frequency);

        // Store or update the schedule
        _machineSchedule.AddOrUpdate(machine.Id, nextRunTime, (_, _) => nextRunTime);

        _logger.LogDebug("Scheduled machine {MachineId} for simulation at {NextRun}", 
            machine.Id, nextRunTime);
        return Task.CompletedTask;
    }

    private TimeSpan GetSimulationFrequency(Machine machine)
    {
        // Base frequency from options
        var baseFrequency = TimeSpan.FromSeconds(_options.Value.IntervalSeconds);

        // Adjust based on machine health status
        if (machine.HealthStatus.HasValue)
        {
            return machine.HealthStatus.Value switch
            {
                HealthClassification.FailureImminent => baseFrequency / 4,  // 4x more frequent
                HealthClassification.SignificantDegradation => baseFrequency / 2, // 2x more frequent
                HealthClassification.MinorDegradation => baseFrequency * 0.75, // 1.33x more frequent
                HealthClassification.Normal => baseFrequency,
                HealthClassification.Healthy => baseFrequency * 2, // 2x less frequent
                _ => baseFrequency
            };
        }

        // Adjust based on RUL if available
        if (machine.RemainingUsefulLifeDays.HasValue)
        {
            var rul = machine.RemainingUsefulLifeDays.Value;
            return rul switch
            {
                < 7 => TimeSpan.FromMinutes(15),  // Critical: every 15 minutes
                < 30 => TimeSpan.FromMinutes(30), // Warning: every 30 minutes
                < 90 => TimeSpan.FromHours(1),    // Caution: every hour
                _ => baseFrequency
            };
        }

        return baseFrequency;
    }

    private async void ExecuteScheduledSimulations(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var machinesToRun = _machineSchedule
                .Where(kvp => kvp.Value <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            if (!machinesToRun.Any())
                return;

            _logger.LogInformation("Executing {Count} scheduled simulations", machinesToRun.Count);

            // Group by tenant for execution
            var machinesByTenant = new Dictionary<Guid, List<Guid>>();
            
            foreach (var machineId in machinesToRun)
            {
                // Get machine to determine tenant
                var machine = await _machineRepository.GetAsync(machineId);
                if (machine == null) continue;

                var tenantId = machine.TenantId ?? Guid.Empty;
                
                if (!machinesByTenant.ContainsKey(tenantId))
                    machinesByTenant[tenantId] = new List<Guid>();
                
                machinesByTenant[tenantId].Add(machineId);
            }

            // Execute simulations per tenant
            foreach (var tenantGroup in machinesByTenant)
            {
                var tenantId = tenantGroup.Key;
                var machineIds = tenantGroup.Value;

                // Set tenant context
                if (tenantId != Guid.Empty)
                {
                    _tenantService.SetTenantContext(tenantId.ToString());
                }

                // Execute simulations in batches
                var batchSize = Math.Min(_options.Value.MachinesPerBatch, machineIds.Count);
                
                for (int i = 0; i < machineIds.Count; i += batchSize)
                {
                    var batch = machineIds.Skip(i).Take(batchSize).ToList();
                    var tasks = batch.Select(async machineId =>
                    {
                        try
                        {
                            // Schedule next run
                            var machine = await _machineRepository.GetAsync(machineId);
                            if (machine != null)
                            {
                                await ScheduleMachineAsync(machine);
                            }

                            return true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to execute scheduled simulation for machine {MachineId}", machineId);
                            return false;
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }

            _logger.LogInformation("Completed execution of {Count} scheduled simulations", machinesToRun.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled simulation execution");
        }
    }

    public async Task UnscheduleMachineAsync(Guid machineId, CancellationToken ct = default)
    {
        _machineSchedule.TryRemove(machineId, out _);
        _logger.LogDebug("Unscheduled machine {MachineId} from simulation scheduling", machineId);
        await Task.CompletedTask;
    }

    public async Task RescheduleMachineAsync(Guid machineId, CancellationToken ct = default)
    {
        var machine = await _machineRepository.GetAsync(machineId, ct);
        if (machine != null)
        {
            await ScheduleMachineAsync(machine);
        }
    }

    public IReadOnlyDictionary<Guid, DateTime> GetScheduledMachines()
    {
        return _machineSchedule.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public void Dispose()
    {
        _schedulerTimer.Dispose();
    }
}

