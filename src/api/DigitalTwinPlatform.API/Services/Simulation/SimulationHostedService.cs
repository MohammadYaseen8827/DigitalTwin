using DigitalTwinPlatform.API.Services.Core;
using DigitalTwinPlatform.Domain.Enums;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.Tenancy;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using DigitalTwinPlatform.Domain.Entities.Simulation;

namespace DigitalTwinPlatform.API.Services.Simulation;

public class SimulationHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SimulationOptions _options;
    private readonly ILogger<SimulationHostedService> _logger;
    private readonly ConcurrentDictionary<Guid, SimulationState> _activeSimulations = new();

    public SimulationHostedService(
        IServiceScopeFactory scopeFactory, 
        IOptions<SimulationOptions> options, 
        ILogger<SimulationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("SimulationHostedService disabled via config");
            return;
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var machineRepo = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
                    var simulationService = scope.ServiceProvider.GetRequiredService<ISimulationService>();

                    var tenantId = scope.ServiceProvider.GetRequiredService<ITenantService>().GetCurrentTenantId();
                    var machines = (await machineRepo.GetAllAsync(m =>
                            (!_options.OnlyActiveMachines || m.Status == EquipmentStatus.Operational || m.Status == EquipmentStatus.Warning) &&
                            (_options.MachineIds.Count == 0 || _options.MachineIds.Contains(m.Id))))
                        .Take(_options.MachinesPerBatch)
                        .ToList();

                    _logger.LogInformation("SimulationHostedService running for tenant {TenantId}, {Count} machines", tenantId, machines.Count);

                    foreach (var machine in machines)
                    {
                        try
                        {
                            // Check if we already have a simulation for this machine
                            if (!_activeSimulations.TryGetValue(machine.Id, out var simulationState))
                            {
                                // Create a new simulation for this machine
                                var parameters = new Dictionary<string, object>
                                {
                                    ["machineId"] = machine.Id,
                                    ["machineName"] = machine.Name.Value,
                                    ["simulationType"] = "machine_health",
                                    ["totalSteps"] = int.MaxValue, // Run indefinitely until cancelled
                                    ["intervalSeconds"] = _options.IntervalSeconds
                                };

                                simulationState = await simulationService.CreateSimulationAsync(parameters, stoppingToken);
                                _activeSimulations[machine.Id] = simulationState;
                                _logger.LogInformation("Created new simulation {SimulationId} for machine {MachineId}", 
                                    simulationState.Id, machine.Id);
                            }

                            // Run the next step of the simulation
                            var result = await simulationService.RunSimulationAsync(simulationState.Id, stoppingToken);
                            
                            // Update machine status based on simulation result if needed
                            // You can add your custom logic here based on the simulation results

                            _logger.LogDebug("Simulation step completed for machine {MachineId}. Step: {Step}", 
                                machine.Id, simulationState.CurrentStep);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error running simulation for machine {MachineId}", machine.Id);
                            // Remove the simulation if it's in a terminal state
                            if (_activeSimulations.TryGetValue(machine.Id, out var state) && 
                                (state.Status == SimulationStatus.Completed || 
                                 state.Status == SimulationStatus.Failed))
                            {
                                _activeSimulations.TryRemove(machine.Id, out _);
                            }
                        }
                    }

                    // Clean up completed or failed simulations
                    var toRemove = _activeSimulations
                        .Where(kvp => kvp.Value.Status == SimulationStatus.Completed || 
                                     kvp.Value.Status == SimulationStatus.Failed)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var machineId in toRemove)
                    {
                        _activeSimulations.TryRemove(machineId, out _);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SimulationHostedService iteration failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when service is stopping - exit gracefully
            _logger.LogInformation("SimulationHostedService is stopping due to cancellation request");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Clean up any running simulations when the service is stopping
        using var scope = _scopeFactory.CreateScope();
        var simulationService = scope.ServiceProvider.GetRequiredService<ISimulationService>();

        foreach (var (machineId, simulation) in _activeSimulations)
        {
            try
            {
                await simulationService.CancelSimulationAsync(simulation.Id, cancellationToken);
                _logger.LogInformation("Cancelled simulation {SimulationId} for machine {MachineId} during shutdown", 
                    simulation.Id, machineId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling simulation {SimulationId} for machine {MachineId} during shutdown", 
                    simulation.Id, machineId);
            }
        }

        _activeSimulations.Clear();
        await base.StopAsync(cancellationToken);
    }
}
