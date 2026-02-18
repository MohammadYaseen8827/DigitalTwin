using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.API.Services.Analytics;

/// <summary>
/// Represents drift detection metrics and thresholds
/// </summary>
public class DriftMetrics
{
    public double FeatureDriftScore { get; set; }
    public double PredictionDriftScore { get; set; }
    public double LabelDriftScore { get; set; }
    public Dictionary<string, double> FeatureWiseDrift { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public DriftLevel DriftLevel { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Drift detection thresholds configuration
/// </summary>
public class DriftThresholds
{
    [Range(0, 1)]
    public double FeatureDriftThreshold { get; set; } = 0.1;
    
    [Range(0, 1)]
    public double PredictionDriftThreshold { get; set; } = 0.15;
    
    [Range(0, 1)]
    public double LabelDriftThreshold { get; set; } = 0.2;
    
    public Dictionary<string, double> FeatureSpecificThresholds { get; set; } = new();
}

/// <summary>
/// Drift detection result
/// </summary>
public class DriftDetectionResult
{
    public bool IsDriftDetected { get; set; }
    public DriftMetrics Metrics { get; set; } = new();
    public List<string> DriftedFeatures { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Drift alert configuration
/// </summary>
public class DriftAlertConfig
{
    public bool EnableAlerts { get; set; } = true;
    public List<string> AlertRecipients { get; set; } = new();
    public DriftSeverity MinimumAlertSeverity { get; set; } = DriftSeverity.Medium;
    public int AlertCooldownMinutes { get; set; } = 60;
}

/// <summary>
/// Drift severity levels
/// </summary>
public enum DriftSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Drift level classifications
/// </summary>
public enum DriftLevel
{
    None = 0,
    Low = 1,
    Moderate = 2,
    High = 3,
    Severe = 4
}

/// <summary>
/// Statistical methods for drift detection
/// </summary>
public enum DriftDetectionMethod
{
    KS_Test,           // Kolmogorov-Smirnov test
    Wasserstein,       // Wasserstein distance
    PSI,              // Population Stability Index
    ChiSquare,        // Chi-square test
    JensenShannon     // Jensen-Shannon divergence
}

/// <summary>
/// Interface for data drift detection service
/// </summary>
public interface IDataDriftService
{
    /// <summary>
    /// Detects drift between reference and current data
    /// </summary>
    /// <param name="referenceData">Baseline/reference data</param>
    /// <param name="currentData">Current/production data</param>
    /// <param name="thresholds">Drift detection thresholds</param>
    /// <param name="method">Statistical method to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Drift detection result</returns>
    Task<DriftDetectionResult> DetectDriftAsync(
        Dictionary<string, double[]> referenceData,
        Dictionary<string, double[]> currentData,
        DriftThresholds thresholds,
        DriftDetectionMethod method = DriftDetectionMethod.KS_Test,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitors drift over time for specific features
    /// </summary>
    /// <param name="modelName">Name/identifier of the model</param>
    /// <param name="featureData">Feature data stream</param>
    /// <param name="windowSize">Sliding window size for comparison</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Continuous drift monitoring result</returns>
    Task<DriftDetectionResult> MonitorDriftAsync(
        string modelName,
        Dictionary<string, IEnumerable<double>> featureData,
        int windowSize = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical drift metrics for a model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="startTime">Start time for historical data</param>
    /// <param name="endTime">End time for historical data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical drift metrics</returns>
    Task<IEnumerable<DriftMetrics>> GetDriftHistoryAsync(
        string modelName,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current drift status for all monitored models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of model names and their drift status</returns>
    Task<Dictionary<string, DriftDetectionResult>> GetCurrentDriftStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures drift detection thresholds for a model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="thresholds">Drift thresholds configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ConfigureThresholdsAsync(
        string modelName,
        DriftThresholds thresholds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current drift thresholds for a model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current drift thresholds</returns>
    Task<DriftThresholds> GetThresholdsAsync(
        string modelName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates drift report
    /// </summary>
    /// <param name="modelName">Model identifier (optional - if null, all models)</param>
    /// <param name="period">Time period for report</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Drift report data</returns>
    Task<DriftReport> GenerateDriftReportAsync(
        string? modelName,
        TimeSpan period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if drift alert should be triggered
    /// </summary>
    /// <param name="result">Drift detection result</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Whether alert should be sent</returns>
    Task<bool> ShouldTriggerAlertAsync(
        DriftDetectionResult result,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Drift report data structure
/// </summary>
public class DriftReport
{
    public string ReportId { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public TimeSpan Period { get; set; }
    public Dictionary<string, DriftDetectionResult> ModelResults { get; set; } = new();
    public SummaryStatistics Summary { get; set; } = new();
    public List<DriftAlert> Alerts { get; set; } = new();
}

/// <summary>
/// Summary statistics for drift report
/// </summary>
public class SummaryStatistics
{
    public int TotalModelsMonitored { get; set; }
    public int ModelsWithDrift { get; set; }
    public double AverageDriftScore { get; set; }
    public Dictionary<DriftLevel, int> DriftLevelDistribution { get; set; } = new();
    public List<string> TopDriftedFeatures { get; set; } = new();
}

/// <summary>
/// Drift alert information
/// </summary>
public class DriftAlert
{
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string ModelName { get; set; } = string.Empty;
    public DriftSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset TriggeredAt { get; set; } = DateTimeOffset.UtcNow;
    public DriftMetrics Metrics { get; set; } = new();
    public bool Acknowledged { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
}