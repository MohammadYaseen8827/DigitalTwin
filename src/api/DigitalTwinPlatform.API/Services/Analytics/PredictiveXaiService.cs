using DigitalTwinPlatform.Application.Predictions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IPredictiveXaiService
{
    Task<Dictionary<string, double>> CalculateContributionsAsync(
        Dictionary<string, double> features, 
        Func<Dictionary<string, double>, double> predictionFunc,
        double baselinePrediction,
        string? modelPath = null,
        Dictionary<string, double>[]? backgroundData = null,
        CancellationToken ct = default);
}

public class PredictiveXaiService(
    IShapService? shapService,
    IOptions<ShapServiceOptions>? shapOptions,
    ILogger<PredictiveXaiService> logger)
    : IPredictiveXaiService
{
    private readonly ShapServiceOptions? _shapOptions = shapOptions?.Value;

    public async Task<Dictionary<string, double>> CalculateContributionsAsync(
        Dictionary<string, double> features, 
        Func<Dictionary<string, double>, double> predictionFunc,
        double baselinePrediction,
        string? modelPath = null,
        Dictionary<string, double>[]? backgroundData = null,
        CancellationToken ct = default)
    {
        // Try SHAP first if available and enabled
        if (shapService != null && _shapOptions?.Enabled == true && !string.IsNullOrEmpty(modelPath))
        {
            try
            {
                var isAvailable = await shapService.IsServiceAvailableAsync(ct);
                if (isAvailable)
                {
                    // Prepare background data if not provided
                    if (backgroundData == null || backgroundData.Length == 0)
                    {
                        backgroundData = GenerateBackgroundData(features, 100);
                    }

                    var shapResult = await shapService.CalculateShapValuesAsync(
                        features,
                        backgroundData,
                        modelPath,
                        ct);

                    if (shapResult.Contributions.Any())
                    {
                        logger.LogInformation("Using SHAP for XAI contributions");
                        return shapResult.Contributions;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "SHAP service failed, falling back to perturbation-based method");
            }
        }

        // Fallback to perturbation-based method
        logger.LogDebug("Using perturbation-based XAI method");
        return CalculateContributionsPerturbation(features, predictionFunc, baselinePrediction);
    }

    private Dictionary<string, double> CalculateContributionsPerturbation(
        Dictionary<string, double> features, 
        Func<Dictionary<string, double>, double> predictionFunc,
        double baselinePrediction)
    {
        var contributions = new Dictionary<string, double>();
        var keys = features.Keys.ToList();

        foreach (var key in keys)
        {
            // Perturb feature by 10% (if it's not 0, else add a small constant)
            var originalValue = features[key];
            var delta = originalValue != 0 ? originalValue * 0.1 : 1.0;
            
            var perturbedFeatures = new Dictionary<string, double>(features);
            perturbedFeatures[key] = originalValue + delta;

            var newPrediction = predictionFunc(perturbedFeatures);
            var impact = Math.Abs(newPrediction - baselinePrediction);

            if (impact > 0)
            {
                contributions[GetHumanReadableName(key)] = impact;
            }
        }

        // Add small baseline for remaining features
        if (!contributions.Any())
        {
            contributions["Baseline"] = 1.0;
        }

        // Normalize contributions to sum to 1.0
        var total = contributions.Values.Sum();
        if (total > 0)
        {
            foreach (var key in contributions.Keys.ToList())
            {
                contributions[key] /= total;
            }
        }

        return contributions;
    }

    private Dictionary<string, double>[] GenerateBackgroundData(
        Dictionary<string, double> features, 
        int sampleCount)
    {
        var background = new List<Dictionary<string, double>>();
        var random = new Random();

        for (int i = 0; i < sampleCount; i++)
        {
            var sample = new Dictionary<string, double>();
            foreach (var kvp in features)
            {
                // Generate samples around the feature value with some variance
                var mean = kvp.Value;
                var std = Math.Max(Math.Abs(mean) * 0.2, 1.0);
                var value = mean + (random.NextDouble() - 0.5) * std * 2;
                sample[kvp.Key] = value;
            }
            background.Add(sample);
        }

        return background.ToArray();
    }

    private string GetHumanReadableName(string internalKey)
    {
        return internalKey switch
        {
            "temp_mean" => "Average Temperature",
            "temp_std" => "Temperature Volatility",
            "temp_trend" => "Temperature Trend",
            "vib_rms" => "Vibration RMS",
            "vib_trend" => "Vibration Trend",
            "pressure_mean" => "Average Pressure",
            _ => internalKey.Replace("_", " ").ToTitleCase()
        };
    }
}

public static class StringExtensions
{
    public static string ToTitleCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }
}
