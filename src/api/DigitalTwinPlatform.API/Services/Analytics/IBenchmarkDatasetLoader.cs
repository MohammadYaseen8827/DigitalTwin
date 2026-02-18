namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IBenchmarkDatasetLoader
{
    Task<List<BenchmarkDataPoint>> LoadDatasetAsync(
        string datasetName,
        CancellationToken ct = default);
    
    Task<List<string>> GetAvailableDatasetsAsync(CancellationToken ct = default);
    
    Task<BenchmarkDatasetInfo> GetDatasetInfoAsync(
        string datasetName,
        CancellationToken ct = default);
}

public class BenchmarkDataPoint
{
    public int Index { get; set; }
    public Dictionary<string, double> Features { get; set; } = new();
    public double ActualRul { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
