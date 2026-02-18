namespace DigitalTwinPlatform.API.Services.Infrastructure;

public interface IPerformanceMetricsCollector
{
    Task RecordMetricAsync(PerformanceMetrics metric);
    Task<List<PerformanceMetrics>> GetMetricsAsync(
        DateTime startTime,
        DateTime endTime,
        string? operationName = null);
    Task<PerformanceStatistics> GetStatisticsAsync(
        DateTime startTime,
        DateTime endTime,
        string? operationName = null);
}

public class PerformanceStatistics
{
    public double AverageLatencyMs { get; set; }
    public double P50LatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public double P99LatencyMs { get; set; }
    public int TotalOperations { get; set; }
    public int ExceededThresholdCount { get; set; }
    public double ExceededThresholdPercentage { get; set; }
}
