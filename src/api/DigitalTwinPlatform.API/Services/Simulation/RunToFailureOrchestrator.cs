using DigitalTwinPlatform.API.Services.Simulation.DegradationModels;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Analytics.Degradation.Models;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Models;
using DigitalTwinPlatform.Domain.Common;
using DigitalTwinPlatform.Domain.ValueObjects;

namespace DigitalTwinPlatform.API.Services.Simulation;

/// <summary>
/// Orchestrates run-to-failure simulations by managing degradation models, sensor data generation,
/// and telemetry storage. Executes simulations until failure threshold is reached or termination conditions are met.
/// </summary>
public class RunToFailureOrchestrator : IRunToFailureOrchestrator
{
    private readonly IMachineRepository _machineRepository;
    private readonly IMachineConfigurationService _configService;
    private readonly IDegradationModelFactory _modelFactory;
    private readonly ISensorDataGenerator _sensorGenerator;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RunToFailureOrchestrator> _logger;

    public RunToFailureOrchestrator(
        IMachineRepository machineRepository,
        IMachineConfigurationService configService,
        IDegradationModelFactory modelFactory,
        ISensorDataGenerator sensorGenerator,
        ITelemetryRepository telemetryRepository,
        IUnitOfWork unitOfWork,
        ILogger<RunToFailureOrchestrator> logger)
    {
        _machineRepository = machineRepository;
        _configService = configService;
        _modelFactory = modelFactory;
        _sensorGenerator = sensorGenerator;
        _telemetryRepository = telemetryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RunToFailureResult> RunToFailureAsync(
        Guid machineId,
        RunToFailureOptions options,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting run-to-failure simulation for machine {MachineId}", machineId);

        var simulationContext = await InitializeSimulationContextAsync(machineId, options, ct);
        var simulationResult = await ExecuteDegradationSimulationAsync(simulationContext, options, ct);
        await StoreSimulationResultsAsync(simulationResult, options, ct);

        LogSimulationCompletion(simulationResult);
        return CreateRunToFailureResult(machineId, simulationResult, options);
    }

    /// <summary>
    /// Initializes the simulation context by loading machine and configuration.
    /// </summary>
    private async Task<SimulationContext> InitializeSimulationContextAsync(
        Guid machineId,
        RunToFailureOptions options,
        CancellationToken ct)
    {
        var machine = await _machineRepository.GetAsync(machineId, ct);
        if (machine == null)
        {
            throw new KeyNotFoundException($"Machine with ID {machineId} not found");
        }

        var machineType = machine.Type.Value;
        var config = await _configService.LoadConfigurationAsync(machineType, ct);
        _logger.LogInformation("Loaded configuration for machine type: {MachineType}", machineType);

        var degradationModel = _modelFactory.Create(config.DegradationModel);
        var random = options.RandomSeed.HasValue 
            ? new Random(options.RandomSeed.Value) 
            : new Random();

        _logger.LogInformation("Starting degradation simulation. Failure threshold: {Threshold}", 
            config.FailureThresholds.DegradationThreshold);

        return new SimulationContext
        {
            MachineId = machineId,
            MachineType = machineType,
            Config = config,
            DegradationModel = degradationModel,
            Random = random,
            StartTime = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Executes the degradation simulation loop until failure or termination condition.
    /// </summary>
    private async Task<RunToFailureSimulationResult> ExecuteDegradationSimulationAsync(
        SimulationContext context,
        RunToFailureOptions options,
        CancellationToken ct)
    {
        var result = new RunToFailureSimulationResult
        {
            Trajectory = new List<DegradationSnapshot>(),
            GeneratedTelemetry = new List<TelemetryData>()
        };

        while (result.CurrentDegradation < context.Config.FailureThresholds.DegradationThreshold)
        {
            if (ShouldTerminateSimulation(result, options, context.StartTime, out var terminationReason))
            {
                result.TerminationReason = terminationReason;
                break;
            }

            result.CurrentDegradation = AdvanceDegradation(context, result.CurrentDegradation, options.StepInterval);

            if (HasReachedFailureThreshold(result.CurrentDegradation, context.Config, result.Step))
            {
                result.ReachedFailureThreshold = true;
                result.TerminationReason = "Failure threshold reached";
                break;
            }

            if (options.GenerateTelemetry)
            {
                GenerateTelemetryForStep(context, result, options, result.Step);
            }

            if (options.StoreTrajectory)
            {
                RecordDegradationSnapshot(context, result, options, result.Step);
            }

            result.Step++;

            if (result.Step % 100 == 0)
            {
                await Task.Delay(1, ct);
                _logger.LogDebug("Run-to-failure step {Step}/{MaxSteps}. Degradation: {Degradation:F4}", 
                    result.Step, options.MaxSteps ?? int.MaxValue, result.CurrentDegradation);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if simulation should terminate based on step or time limits.
    /// </summary>
    private static bool ShouldTerminateSimulation(
        RunToFailureSimulationResult result,
        RunToFailureOptions options,
        DateTime startTime,
        out string? terminationReason)
    {
        terminationReason = null;

        if (options.MaxSteps.HasValue && result.Step >= options.MaxSteps.Value)
        {
            terminationReason = $"Maximum steps ({options.MaxSteps.Value}) reached";
            return true;
        }

        if (options.MaxSimulationTime.HasValue && 
            DateTime.UtcNow - startTime > options.MaxSimulationTime.Value)
        {
            terminationReason = $"Maximum simulation time ({options.MaxSimulationTime.Value}) exceeded";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances the degradation model by one step.
    /// </summary>
    private double AdvanceDegradation(
        SimulationContext context,
        double currentDegradation,
        TimeSpan stepInterval)
    {
        return context.DegradationModel.Step(
            currentDegradation,
            stepInterval,
            context.Random
        );
    }

    /// <summary>
    /// Checks if the failure threshold has been reached.
    /// </summary>
    private bool HasReachedFailureThreshold(
        double currentDegradation,
        MachineConfiguration config,
        int step)
    {
        if (currentDegradation >= config.FailureThresholds.DegradationThreshold)
        {
            _logger.LogInformation("Failure threshold reached at step {Step}. Degradation: {Degradation}", 
                step, currentDegradation);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Generates telemetry data for the current simulation step.
    /// </summary>
    private void GenerateTelemetryForStep(
        SimulationContext context,
        RunToFailureSimulationResult result,
        RunToFailureOptions options,
        int step)
    {
        var sensorReadings = new Dictionary<string, double>();
        
        foreach (var sensorMapping in context.Config.SensorMappings)
        {
            var reading = _sensorGenerator.GenerateReading(
                sensorMapping,
                result.CurrentDegradation,
                context.Random
            );
            sensorReadings[sensorMapping.SensorType] = reading;

            if (options.StoreTrajectory)
            {
                var timestamp = context.StartTime.Add(TimeSpan.FromSeconds(step * options.StepInterval.TotalSeconds));
                var telemetry = _sensorGenerator.CreateTelemetryData(
                    context.MachineId,
                    sensorMapping,
                    reading,
                    timestamp
                );
                result.GeneratedTelemetry.Add(telemetry);
            }
        }
    }

    /// <summary>
    /// Records a degradation snapshot for trajectory tracking.
    /// </summary>
    private void RecordDegradationSnapshot(
        SimulationContext context,
        RunToFailureSimulationResult result,
        RunToFailureOptions options,
        int step)
    {
        var timestamp = context.StartTime.Add(TimeSpan.FromSeconds(step * options.StepInterval.TotalSeconds));
        var sensorReadings = context.Config.SensorMappings
            .ToDictionary(
                sm => sm.SensorType,
                sm => _sensorGenerator.GenerateReading(sm, result.CurrentDegradation, context.Random)
            );

        result.Trajectory.Add(new DegradationSnapshot
        {
            Step = step,
            Timestamp = timestamp,
            DegradationState = result.CurrentDegradation,
            SensorReadings = sensorReadings
        });
    }

    /// <summary>
    /// Stores generated telemetry data to the repository.
    /// </summary>
    private async Task StoreSimulationResultsAsync(
        RunToFailureSimulationResult result,
        RunToFailureOptions options,
        CancellationToken ct)
    {
        if (options.StoreTrajectory && result.GeneratedTelemetry.Any())
        {
            _logger.LogInformation("Storing {Count} telemetry data points", result.GeneratedTelemetry.Count);
            await _telemetryRepository.AddRangeAsync(result.GeneratedTelemetry, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Creates the final run-to-failure result from simulation data.
    /// </summary>
    private RunToFailureResult CreateRunToFailureResult(
        Guid machineId,
        RunToFailureSimulationResult result,
        RunToFailureOptions options)
    {
        var timeToFailure = TimeSpan.FromSeconds(result.Step * options.StepInterval.TotalSeconds);

        return new RunToFailureResult
        {
            MachineId = machineId,
            TimeToFailure = timeToFailure,
            StepsToFailure = result.Step,
            FinalDegradationState = result.CurrentDegradation,
            Trajectory = result.Trajectory,
            GeneratedTelemetry = result.GeneratedTelemetry,
            ReachedFailureThreshold = result.ReachedFailureThreshold,
            TerminationReason = result.TerminationReason
        };
    }

    /// <summary>
    /// Logs the completion of the simulation.
    /// </summary>
    private void LogSimulationCompletion(RunToFailureSimulationResult result)
    {
        _logger.LogInformation(
            "Run-to-failure simulation completed. Steps: {Steps}, Degradation: {Degradation}, ReachedThreshold: {Reached}",
            result.Step, result.CurrentDegradation, result.ReachedFailureThreshold);
    }

    public async Task<List<DegradationTrajectory>> GenerateTrajectoriesAsync(
        string machineType,
        int count,
        RunToFailureOptions options,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating {Count} degradation trajectories for machine type: {MachineType}", 
            count, machineType);

        var trajectories = new List<DegradationTrajectory>();
        
        // Load configuration once
        var config = await _configService.LoadConfigurationAsync(machineType, ct);
        
        // Generate multiple trajectories with different random seeds
        for (int i = 0; i < count; i++)
        {
            _logger.LogDebug("Generating trajectory {Index}/{Count}", i + 1, count);
            
            var trajectoryOptions = CreateTrajectoryOptions(options, i);
            var tempMachine = await CreateTemporaryMachineAsync(machineType, i, ct);
            
            if (tempMachine == null)
            {
                continue;
            }
            
            try
            {
                var result = await RunToFailureAsync(tempMachine.Id, trajectoryOptions, ct);
                
                trajectories.Add(new DegradationTrajectory
                {
                    MachineType = machineType,
                    TrajectoryId = Guid.NewGuid(),
                    Snapshot = result.Trajectory,
                    TimeToFailure = result.TimeToFailure
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate trajectory {Index} for machine type {MachineType}", 
                    i, machineType);
                // Continue with next trajectory
            }
            finally
            {
                await CleanupTemporaryMachineAsync(tempMachine, ct);
            }
        }
        
        _logger.LogInformation("Generated {Count} trajectories successfully", trajectories.Count);
        return trajectories;
    }

    /// <summary>
    /// Creates trajectory options for a specific trajectory index.
    /// </summary>
    private static RunToFailureOptions CreateTrajectoryOptions(RunToFailureOptions baseOptions, int index)
    {
        return new RunToFailureOptions
        {
            MaxSimulationTime = baseOptions.MaxSimulationTime,
            MaxSteps = baseOptions.MaxSteps,
            StepInterval = baseOptions.StepInterval,
            GenerateTelemetry = baseOptions.GenerateTelemetry,
            StoreTrajectory = true, // Store trajectory for analysis
            RandomSeed = baseOptions.RandomSeed.HasValue 
                ? baseOptions.RandomSeed.Value + index 
                : index * 1000 // Different seed for each trajectory
        };
    }

    /// <summary>
    /// Creates a temporary machine for trajectory generation.
    /// </summary>
    private async Task<Machine?> CreateTemporaryMachineAsync(
        string machineType,
        int index,
        CancellationToken ct)
    {
        var machineNameResult = MachineName.Create($"Temp-{machineType}-{index}");
        var machineTypeResult = MachineType.Create(machineType);

        if (machineNameResult is not Result<MachineName>.Success nameSuccess ||
            machineTypeResult is not Result<MachineType>.Success typeSuccess)
        {
            var error = machineNameResult is Result<MachineName>.Failure nameFailure 
                ? nameFailure.Error 
                : machineTypeResult is Result<MachineType>.Failure typeFailure 
                    ? typeFailure.Error 
                    : "Unknown error";
            _logger.LogWarning("Failed to create temporary machine for trajectory {Index}: {Error}", 
                index, error);
            return null;
        }

        var tempMachineResult = Machine.Create(nameSuccess.Value, typeSuccess.Value);
        if (tempMachineResult is not Result<Machine>.Success machineSuccess)
        {
            var error = tempMachineResult is Result<Machine>.Failure failure ? failure.Error : "Unknown error";
            _logger.LogWarning("Failed to create temporary machine for trajectory {Index}: {Error}", 
                index, error);
            return null;
        }

        var tempMachine = machineSuccess.Value;
        await _machineRepository.AddAsync(tempMachine, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return tempMachine;
    }

    /// <summary>
    /// Cleans up a temporary machine after trajectory generation.
    /// </summary>
    private async Task CleanupTemporaryMachineAsync(Machine machine, CancellationToken ct)
    {
        try
        {
            await _machineRepository.DeleteAsync(machine, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temporary machine {MachineId}", machine.Id);
        }
    }

    public async Task<List<RunToFailureResult>> GetStoredResultsAsync(
        Guid machineId,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving stored run-to-failure results for machine {MachineId}", machineId);

        try
        {
            // Get machine to verify it exists
            var machine = await _machineRepository.GetAsync(machineId, ct);
            if (machine == null)
            {
                throw new KeyNotFoundException($"Machine with ID {machineId} not found");
            }

            // For now, return empty list as we don't have persistent storage
            // In production, this would query a database or cache
            var results = new List<RunToFailureResult>();
            
            _logger.LogInformation("Retrieved {Count} stored results for machine {MachineId}", 
                results.Count, machineId);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve stored results for machine {MachineId}", machineId);
            throw;
        }
    }
}

internal class SimulationContext
{
    public Guid MachineId { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public MachineConfiguration Config { get; set; } = null!;
    public IDegradationModel DegradationModel { get; set; } = null!;
    public Random Random { get; set; } = null!;
    public DateTime StartTime { get; set; }
}

public class RunToFailureSimulationResult
{
    public int Step { get; set; }
    public double CurrentDegradation { get; set; }
    public List<DegradationSnapshot> Trajectory { get; set; } = new();
    public List<TelemetryData> GeneratedTelemetry { get; set; } = new();
    public bool ReachedFailureThreshold { get; set; }
    public string? TerminationReason { get; set; }
}
