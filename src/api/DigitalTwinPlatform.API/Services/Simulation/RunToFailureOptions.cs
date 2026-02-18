namespace DigitalTwinPlatform.API.Services.Simulation;

public class RunToFailureOptions
{
    public TimeSpan? MaxSimulationTime { get; set; }
    public int? MaxSteps { get; set; }
    public TimeSpan StepInterval { get; set; } = TimeSpan.FromMinutes(10);
    public bool GenerateTelemetry { get; set; } = true;
    public bool StoreTrajectory { get; set; } = true;
    public int? RandomSeed { get; set; } // For reproducibility
}

public class RunToFailureResult
{
    public Guid MachineId { get; set; }
    public TimeSpan TimeToFailure { get; set; }
    public int StepsToFailure { get; set; }
    public double FinalDegradationState { get; set; }
    public List<DegradationSnapshot> Trajectory { get; set; } = new();
    public List<TelemetryData> GeneratedTelemetry { get; set; } = new();
    public bool ReachedFailureThreshold { get; set; }
    public string? TerminationReason { get; set; }
}

public class DegradationSnapshot
{
    public int Step { get; set; }
    public DateTime Timestamp { get; set; }
    public double DegradationState { get; set; }
    public Dictionary<string, double> SensorReadings { get; set; } = new();
}

public class DegradationTrajectory
{
    public string MachineType { get; set; } = string.Empty;
    public Guid TrajectoryId { get; set; }
    public List<DegradationSnapshot> Snapshot { get; set; } = new();
    public TimeSpan TimeToFailure { get; set; }
}
