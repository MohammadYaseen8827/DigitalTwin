namespace DigitalTwinPlatform.Domain.Entities;

public class PerformanceMetricsEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OperationName { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MachineId { get; set; } = string.Empty;
    public bool ExceededThreshold { get; set; }
    public string MetadataJson { get; set; } = "{}";
}
