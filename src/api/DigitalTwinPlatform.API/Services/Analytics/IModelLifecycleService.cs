using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IModelLifecycleService
{
    Task<ModelVersion> RegisterModelVersionAsync(
        string modelType,
        string modelPath,
        Dictionary<string, double> metrics,
        string? trainingDatasetHash = null,
        string? notes = null,
        CancellationToken ct = default);
    
    Task<ModelVersion> PromoteModelAsync(
        Guid modelVersionId,
        ModelStatus targetStatus,
        string? notes = null,
        CancellationToken ct = default);
    
    Task<List<ModelVersion>> GetModelVersionsAsync(
        string? modelType = null,
        CancellationToken ct = default);
    
    Task<ModelVersion?> GetProductionVersionAsync(
        string modelType,
        CancellationToken ct = default);
    
    Task<ModelComparisonResult> CompareModelsAsync(
        Guid modelVersionId1,
        Guid modelVersionId2,
        CancellationToken ct = default);
    
    Task<ModelPerformanceSummary> GetModelPerformanceAsync(
        Guid modelVersionId,
        CancellationToken ct = default);
}

public class ModelComparisonResult
{
    public ModelVersion Model1 { get; set; } = null!;
    public ModelVersion Model2 { get; set; } = null!;
    public Dictionary<string, ComparisonMetric> Metrics { get; set; } = new();
    public string Winner { get; set; } = string.Empty;
    public List<string> Differences { get; set; } = new();
}

public class ComparisonMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value1 { get; set; }
    public double Value2 { get; set; }
    public double Difference { get; set; }
    public double DifferencePercentage { get; set; }
    public bool IsBetter { get; set; }
    public string BetterModel { get; set; } = string.Empty;
}

public class ModelPerformanceSummary
{
    public ModelVersion ModelVersion { get; set; } = null!;
    public int TotalPredictions { get; set; }
    public double AverageRulError { get; set; }
    public double AverageFailureProbability { get; set; }
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public DateTime? LastUsedAt { get; set; }
}
