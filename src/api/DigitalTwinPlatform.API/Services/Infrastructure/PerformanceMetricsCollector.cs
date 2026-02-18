using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace DigitalTwinPlatform.API.Services.Infrastructure;

public class PerformanceMetricsCollector : IPerformanceMetricsCollector
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PerformanceMetricsCollector> _logger;
    private readonly ConcurrentQueue<PerformanceMetrics> _metricsQueue = new();
    private const int MaxCachedMetrics = 10000;
    private const string CacheKey = "performance_metrics";

    public PerformanceMetricsCollector(
        IMemoryCache cache,
        ILogger<PerformanceMetricsCollector> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task RecordMetricAsync(PerformanceMetrics metric)
    {
        try
        {
            // Add to in-memory queue for real-time access
            _metricsQueue.Enqueue(metric);
            
            // Maintain queue size
            while (_metricsQueue.Count > MaxCachedMetrics)
            {
                _metricsQueue.TryDequeue(out _);
            }
            
            // Store in cache for quick retrieval
            var cacheKey = $"{CacheKey}_{metric.OperationName}_{metric.MachineId}";
            var metricsList = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                return new List<PerformanceMetrics>();
            });
            
            if (metricsList != null)
            {
                lock (metricsList)
                {
                    metricsList.Add(metric);
                    // Keep only recent metrics (last 1000)
                    if (metricsList.Count > 1000)
                    {
                        metricsList.RemoveRange(0, metricsList.Count - 1000);
                    }
                }
            }
            
            if (metric.ExceededThreshold)
            {
                _logger.LogWarning(
                    "Performance threshold exceeded. Operation: {Operation}, Duration: {Duration}ms, Machine: {MachineId}",
                    metric.OperationName,
                    metric.Duration.TotalMilliseconds,
                    metric.MachineId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record performance metric");
        }
        
        return Task.CompletedTask;
    }

    public Task<List<PerformanceMetrics>> GetMetricsAsync(
        DateTime startTime,
        DateTime endTime,
        string? operationName = null)
    {
        var metrics = _metricsQueue
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .Where(m => operationName == null || m.OperationName.Equals(operationName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.Timestamp)
            .ToList();
        
        return Task.FromResult(metrics);
    }

    public Task<PerformanceStatistics> GetStatisticsAsync(
        DateTime startTime,
        DateTime endTime,
        string? operationName = null)
    {
        var metrics = _metricsQueue
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .Where(m => operationName == null || m.OperationName.Equals(operationName, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Duration.TotalMilliseconds)
            .OrderBy(d => d)
            .ToList();
        
        if (metrics.Count == 0)
        {
            return Task.FromResult(new PerformanceStatistics());
        }
        
        var exceededCount = _metricsQueue
            .Count(m => m.Timestamp >= startTime && 
                       m.Timestamp <= endTime &&
                       (operationName == null || m.OperationName.Equals(operationName, StringComparison.OrdinalIgnoreCase)) &&
                       m.ExceededThreshold);
        
        return Task.FromResult(new PerformanceStatistics
        {
            AverageLatencyMs = metrics.Average(),
            P50LatencyMs = GetPercentile(metrics, 50),
            P95LatencyMs = GetPercentile(metrics, 95),
            P99LatencyMs = GetPercentile(metrics, 99),
            TotalOperations = metrics.Count,
            ExceededThresholdCount = exceededCount,
            ExceededThresholdPercentage = metrics.Count > 0 ? (double)exceededCount / metrics.Count * 100 : 0
        });
    }

    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0) return 0;
        
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        index = Math.Max(0, Math.Min(index, sortedValues.Count - 1));
        return sortedValues[index];
    }
}
