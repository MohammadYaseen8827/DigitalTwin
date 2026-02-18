namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IShapService
{
    Task<ShapResult> CalculateShapValuesAsync(
        Dictionary<string, double> features,
        Dictionary<string, double>[] backgroundData,
        string modelPath,
        CancellationToken ct = default);
    
    Task<bool> UpdateBackgroundDataAsync(
        string modelPath,
        Dictionary<string, double>[] backgroundData,
        CancellationToken ct = default);
    
    Task<bool> IsServiceAvailableAsync(CancellationToken ct = default);
}

public class ShapResult
{
    public Dictionary<string, double> Contributions { get; set; } = new();
    public double BaseValue { get; set; }
    public List<double> ShapValues { get; set; } = new();
}
