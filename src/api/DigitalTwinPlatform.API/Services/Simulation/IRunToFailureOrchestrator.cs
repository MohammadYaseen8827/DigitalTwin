namespace DigitalTwinPlatform.API.Services.Simulation;

public interface IRunToFailureOrchestrator
{
    Task<RunToFailureResult> RunToFailureAsync(
        Guid machineId,
        RunToFailureOptions options,
        CancellationToken ct = default);
    
    Task<List<DegradationTrajectory>> GenerateTrajectoriesAsync(
        string machineType,
        int count,
        RunToFailureOptions options,
        CancellationToken ct = default);
    
    Task<List<RunToFailureResult>> GetStoredResultsAsync(
        Guid machineId,
        CancellationToken ct = default);
}
