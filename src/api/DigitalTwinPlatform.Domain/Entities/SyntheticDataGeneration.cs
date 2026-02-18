using DigitalTwinPlatform.Domain.Enums;
using System.Collections.Generic;

namespace DigitalTwinPlatform.Domain.Entities;

/// <summary>
/// Entity for tracking synthetic data generation sessions.
/// Stores metadata about generated synthetic data for validation and statistics.
/// </summary>
public class SyntheticDataGeneration
{
    public Guid Id { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public int NumberOfTrajectories { get; set; }
    public TimeSpan? TimeRange { get; set; }
    public int RandomSeed { get; set; }
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public GenerationStatistics Statistics { get; set; } = new GenerationStatistics();
    public DataValidationReport ValidationReport { get; set; } = new DataValidationReport();
    public List<SyntheticDataPoint> DataPoints { get; set; } = new List<SyntheticDataPoint>();
}

/// <summary>
/// Statistics for synthetic data generation session.
/// </summary>
public class GenerationStatistics
{
    public int TotalGenerations { get; set; }
    public int TotalDataPoints { get; set; }
    public double AverageTrajectoriesPerGeneration { get; set; }
    public double AverageDataPointsPerTrajectory { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public DateTime LastGeneratedAt { get; set; }
}

/// <summary>
/// Validation report for synthetic data.
/// </summary>
public class DataValidationReport
{
    public double OverallScore { get; set; }
    public double KolmogorovSmirnovStatistic { get; set; }
    public double MaximumMeanDiscrepancy { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public string BenchmarkDataset { get; set; } = string.Empty;
    public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
    public List<string> Recommendations { get; set; } = new List<string>();
}

/// <summary>
/// Synthetic data point for generation.
/// </summary>
public class SyntheticDataPoint
{
    public DateTime Timestamp { get; set; }
    public double? Temperature { get; set; }
    public double? Vibration { get; set; }
    public double? Pressure { get; set; }
    public double? Rpm { get; set; }
    public double? HealthScore { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Status of synthetic data generation.
/// </summary>
public enum GenerationStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}
