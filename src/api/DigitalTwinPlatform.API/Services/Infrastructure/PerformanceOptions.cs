namespace DigitalTwinPlatform.API.Services.Infrastructure;

public class PerformanceOptions
{
    public LatencyThresholds LatencyThresholds { get; set; } = new();
}

public class LatencyThresholds
{
    public TimeSpan DigitalTwinUpdate { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan PredictionGeneration { get; set; } = TimeSpan.FromMilliseconds(1000);
    public TimeSpan SimulationStep { get; set; } = TimeSpan.FromMilliseconds(500);
}

public class PerformanceMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; }
    public string MachineId { get; set; } = string.Empty;
    public bool ExceededThreshold { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
