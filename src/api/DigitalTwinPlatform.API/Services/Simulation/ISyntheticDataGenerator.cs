using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.API.Services.Simulation;

public interface ISyntheticDataGenerator
{
    Task<List<TelemetryData>> GenerateSyntheticDataAsync(
        string machineType,
        int numberOfTrajectories,
        TimeSpan? timeRange = null,
        int? randomSeed = null,
        CancellationToken ct = default);
    
    Task<SyntheticDataGenerationResult> GenerateWithValidationAsync(
        string machineType,
        int numberOfTrajectories,
        TimeSpan? timeRange = null,
        int? randomSeed = null,
        CancellationToken ct = default);
    
    Task<DataValidationReport> ValidateAgainstBenchmarksAsync(
        List<TelemetryData> syntheticData,
        string benchmarkDataset,
        CancellationToken ct = default);
}

public class SyntheticDataGenerationResult
{
    public List<TelemetryData> TelemetryData { get; set; } = new();
    public List<DegradationTrajectory> Trajectories { get; set; } = new();
    public GenerationStatistics Statistics { get; set; } = new();
    public TimeSpan GenerationDuration { get; set; }
    public int TrajectoriesGenerated { get; set; }
    public int TotalDataPoints { get; set; }
}

public class GenerationStatistics
{
    public Dictionary<string, double> FeatureMeans { get; set; } = new();
    public Dictionary<string, double> FeatureStdDevs { get; set; } = new();
    public Dictionary<string, double> FeatureRanges { get; set; } = new();
    public Dictionary<string, double> CorrelationMatrix { get; set; } = new();
}

public class DataValidationReport
{
    public bool IsValid { get; set; }
    public Dictionary<string, ValidationMetric> Metrics { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public double OverallScore { get; set; }
}

public class ValidationMetric
{
    public string Name { get; set; } = string.Empty;
    public double SyntheticValue { get; set; }
    public double BenchmarkValue { get; set; }
    public double Difference { get; set; }
    public double DifferencePercentage { get; set; }
    public bool Passed { get; set; }
    public double Threshold { get; set; }
}

