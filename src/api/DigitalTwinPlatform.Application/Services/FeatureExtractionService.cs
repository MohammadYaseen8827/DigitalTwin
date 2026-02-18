using DigitalTwinPlatform.Domain.Entities;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Services;

public interface IFeatureExtractionService
{
    Dictionary<string, double> ExtractFeatures(IEnumerable<TelemetryData> telemetry);
}

public class FeatureExtractionService(ILogger<FeatureExtractionService> logger) : IFeatureExtractionService
{
    public Dictionary<string, double> ExtractFeatures(IEnumerable<TelemetryData> telemetry)
    {
        var telemetryList = telemetry.ToList();
        var features = new Dictionary<string, double>();

        if (telemetryList.Count == 0) return features;

        var tempData = telemetryList.Where(t => t.DataType == "temperature").ToList();
        if (tempData.Count > 0)
        {
            var temps = tempData.Select(t => ExtractDouble(t.Data, "value")).ToList();
            features["temp_mean"] = temps.Average();
            features["temp_std"] = CalculateStdDev(temps);
            features["temp_trend"] = CalculateTrend(temps);
        }

        var vibData = telemetryList.Where(t => t.DataType == "vibration").ToList();
        if (vibData.Count > 0)
        {
            var vibes = vibData.Select(t => ExtractDouble(t.Data, "amplitude")).ToList();
            features["vib_mean"] = vibes.Average();
            features["vib_std"] = CalculateStdDev(vibes);
            features["vib_rms"] = Math.Sqrt(vibes.Average(v => v * v));
            features["vib_trend"] = CalculateTrend(vibes);
        }

        var pressureData = telemetryList.Where(t => t.DataType == "pressure").ToList();
        if (pressureData.Count > 0)
        {
            var pressures = pressureData.Select(t => ExtractDouble(t.Data, "value")).ToList();
            features["pressure_mean"] = pressures.Average();
        }

        features["data_points"] = telemetryList.Count;
        features["time_span_hours"] = telemetryList.Count > 0
            ? (telemetryList.Max(t => t.Timestamp) - telemetryList.Min(t => t.Timestamp)).TotalHours
            : 0;

        return features;
    }

    private double ExtractDouble(JsonDocument doc, string propertyName)
    {
        try 
        {
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty(propertyName, out var element))
            {
                 if (element.ValueKind == JsonValueKind.Number && element.TryGetDouble(out var value))
                     return value;
            }
            return 0;
        }
        catch (Exception ex)
        {
            // Log at Information level when fallback is used, so support can see that fallback was used
            logger.LogInformation(ex, "Could not extract property '{PropertyName}' from telemetry data, using default value 0", propertyName);
            return 0;
        }
    }

    private static double CalculateStdDev(List<double> values)
    {
        if (values.Count < 2) return 0;
        var mean = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));
    }

    private static double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;
        // Simple linear regression slope
        var n = values.Count;
        var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
        var y = values.ToArray();
        var xMean = x.Average();
        var yMean = y.Average();
        
        var numerator = 0.0;
        var denominator = 0.0;
        
        for (int i = 0; i < n; i++)
        {
            var xDiff = x[i] - xMean;
            numerator += xDiff * (y[i] - yMean);
            denominator += xDiff * xDiff;
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }
}
