namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IBenchmarkValidationService
{
    Task<BenchmarkValidationResult> ValidateAgainstBenchmarkAsync(
        string benchmarkDataset,
        string modelVersionId,
        CancellationToken ct = default);
    
    Task<BenchmarkValidationResult> ValidateModelAsync(
        string benchmarkDataset,
        string modelType,
        string modelPath,
        CancellationToken ct = default);
    
    Task<List<string>> GetAvailableBenchmarksAsync(CancellationToken ct = default);
    
    Task<BenchmarkDatasetInfo> GetBenchmarkInfoAsync(
        string benchmarkDataset,
        CancellationToken ct = default);
}

public class BenchmarkValidationResult
{
    public string BenchmarkDataset { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public double MAPE { get; set; }
    public double RMSE { get; set; }
    public double R2 { get; set; }
    public double MAE { get; set; }
    public bool MeetsRequirement { get; set; } // MAPE < 15%
    public Dictionary<string, object> DetailedMetrics { get; set; } = new();
    public List<PredictionComparison> Predictions { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}

public class PredictionComparison
{
    public int SampleIndex { get; set; }
    public double ActualRul { get; set; }
    public double PredictedRul { get; set; }
    public double AbsoluteError { get; set; }
    public double PercentageError { get; set; }
}

public class BenchmarkDatasetInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public Dictionary<string, double> Statistics { get; set; } = new();
    public bool IsAvailable { get; set; }
    public string? DataPath { get; set; }
}
