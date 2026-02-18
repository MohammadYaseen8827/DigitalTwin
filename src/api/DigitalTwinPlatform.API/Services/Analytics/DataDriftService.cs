using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Numerics;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class DriftServiceOptions
{
    public int DefaultWindowSize { get; set; } = 1000;
    public double DefaultFeatureThreshold { get; set; } = 0.1;
    public double DefaultPredictionThreshold { get; set; } = 0.15;
    public Dictionary<string, DriftThresholds> ModelSpecificThresholds { get; set; } = new();
    public bool EnableAlerts { get; set; } = true;
    public int AlertCooldownMinutes { get; set; } = 60;
}

public class DataDriftService : IDataDriftService
{
    private readonly ILogger<DataDriftService> _logger;
    private readonly DriftServiceOptions _options;
    private readonly Dictionary<string, DriftThresholds> _modelThresholds;
    private readonly Dictionary<string, List<DriftMetrics>> _driftHistory;
    private readonly Dictionary<string, DateTimeOffset> _lastAlertTimes;

    public DataDriftService(
        ILogger<DataDriftService> logger,
        IOptions<DriftServiceOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _modelThresholds = new Dictionary<string, DriftThresholds>();
        _driftHistory = new Dictionary<string, List<DriftMetrics>>();
        _lastAlertTimes = new Dictionary<string, DateTimeOffset>();

        // Initialize default thresholds
        InitializeDefaultThresholds();
    }

    private void InitializeDefaultThresholds()
    {
        foreach (var kvp in _options.ModelSpecificThresholds)
        {
            _modelThresholds[kvp.Key] = kvp.Value;
        }
    }

    public async Task<DriftDetectionResult> DetectDriftAsync(
        Dictionary<string, double[]> referenceData,
        Dictionary<string, double[]> currentData,
        DriftThresholds thresholds,
        DriftDetectionMethod method = DriftDetectionMethod.KS_Test,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new DriftDetectionResult
            {
                Metrics = new DriftMetrics
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["method"] = method.ToString(),
                        ["reference_size"] = referenceData.FirstOrDefault().Value?.Length ?? 0,
                        ["current_size"] = currentData.FirstOrDefault().Value?.Length ?? 0
                    }
                }
            };

            var driftedFeatures = new List<string>();

            // Calculate drift for each feature
            foreach (var feature in referenceData.Keys)
            {
                if (!currentData.ContainsKey(feature))
                {
                    _logger.LogWarning("Feature {Feature} not found in current data", feature);
                    continue;
                }

                var referenceValues = referenceData[feature];
                var currentValues = currentData[feature];

                double driftScore;
                switch (method)
                {
                    case DriftDetectionMethod.KS_Test:
                        driftScore = CalculateKSTest(referenceValues, currentValues);
                        break;
                    case DriftDetectionMethod.Wasserstein:
                        driftScore = CalculateWassersteinDistance(referenceValues, currentValues);
                        break;
                    case DriftDetectionMethod.PSI:
                        driftScore = CalculatePSI(referenceValues, currentValues);
                        break;
                    default:
                        driftScore = CalculateKSTest(referenceValues, currentValues);
                        break;
                }

                result.Metrics.FeatureWiseDrift[feature] = driftScore;

                var featureThreshold = thresholds.FeatureSpecificThresholds.GetValueOrDefault(
                    feature, thresholds.FeatureDriftThreshold);

                if (driftScore > featureThreshold)
                {
                    driftedFeatures.Add(feature);
                }
            }

            // Calculate overall drift scores
            if (result.Metrics.FeatureWiseDrift.Any())
            {
                result.Metrics.FeatureDriftScore = result.Metrics.FeatureWiseDrift.Values.Average();
            }

            // Determine drift level
            result.Metrics.DriftLevel = DetermineDriftLevel(result.Metrics.FeatureDriftScore, thresholds);
            result.IsDriftDetected = driftedFeatures.Any();
            result.DriftedFeatures = driftedFeatures;

            // Generate recommendation
            result.Recommendation = GenerateRecommendation(result);

            _logger.LogInformation(
                "Drift detection completed. Drift detected: {IsDriftDetected}, Features drifted: {DriftedCount}",
                result.IsDriftDetected, driftedFeatures.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting drift");
            throw;
        }
    }

    public async Task<DriftDetectionResult> MonitorDriftAsync(
        string modelName,
        Dictionary<string, IEnumerable<double>> featureData,
        int windowSize = 1000,
        CancellationToken cancellationToken = default)
    {
        // Convert enumerable data to arrays for processing
        var currentData = featureData.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Take(windowSize).ToArray());

        var referenceData = GetReferenceData(modelName, windowSize);

        var thresholds = await GetThresholdsAsync(modelName, cancellationToken);
        
        var result = await DetectDriftAsync(
            referenceData, currentData, thresholds, 
            DriftDetectionMethod.KS_Test, cancellationToken);

        // Store result in history
        StoreDriftResult(modelName, result.Metrics);

        return result;
    }

    public async Task<IEnumerable<DriftMetrics>> GetDriftHistoryAsync(
        string modelName,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        if (_driftHistory.TryGetValue(modelName, out var history))
        {
            return history
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .OrderBy(m => m.Timestamp);
        }

        return Enumerable.Empty<DriftMetrics>();
    }

    public async Task<Dictionary<string, DriftDetectionResult>> GetCurrentDriftStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, DriftDetectionResult>();

        foreach (var modelName in _driftHistory.Keys)
        {
            var latestMetrics = _driftHistory[modelName]
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            if (latestMetrics != null)
            {
                results[modelName] = new DriftDetectionResult
                {
                    IsDriftDetected = latestMetrics.DriftLevel > DriftLevel.None,
                    Metrics = latestMetrics
                };
            }
        }

        return results;
    }

    public async Task ConfigureThresholdsAsync(
        string modelName,
        DriftThresholds thresholds,
        CancellationToken cancellationToken = default)
    {
        _modelThresholds[modelName] = thresholds;
        _logger.LogInformation("Configured drift thresholds for model {ModelName}", modelName);
    }

    public async Task<DriftThresholds> GetThresholdsAsync(
        string modelName,
        CancellationToken cancellationToken = default)
    {
        return _modelThresholds.GetValueOrDefault(modelName, CreateDefaultThresholds());
    }

    public async Task<DriftReport> GenerateDriftReportAsync(
        string? modelName,
        TimeSpan period,
        CancellationToken cancellationToken = default)
    {
        var endTime = DateTimeOffset.UtcNow;
        var startTime = endTime - period;

        var report = new DriftReport
        {
            Period = period,
            Summary = new SummaryStatistics()
        };

        var modelNames = modelName != null ? new[] { modelName } : _driftHistory.Keys.ToArray();

        foreach (var model in modelNames)
        {
            var history = await GetDriftHistoryAsync(model, startTime, endTime, cancellationToken);
            if (history.Any())
            {
                var latestResult = new DriftDetectionResult
                {
                    Metrics = history.OrderByDescending(h => h.Timestamp).First(),
                    IsDriftDetected = history.Any(h => h.DriftLevel > DriftLevel.None)
                };
                report.ModelResults[model] = latestResult;
            }
        }

        // Generate summary statistics
        report.Summary.TotalModelsMonitored = report.ModelResults.Count;
        report.Summary.ModelsWithDrift = report.ModelResults.Count(r => r.Value.IsDriftDetected);
        report.Summary.AverageDriftScore = report.ModelResults.Any() ?
            report.ModelResults.Average(r => r.Value.Metrics.FeatureDriftScore) : 0;

        return report;
    }

    public async Task<bool> ShouldTriggerAlertAsync(
        DriftDetectionResult result,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableAlerts)
            return false;

        if (!result.IsDriftDetected)
            return false;

        // Check cooldown period
        var modelName = "default"; // In a real implementation, this would come from context
        
        if (_lastAlertTimes.TryGetValue(modelName, out var lastAlert) && 
            DateTimeOffset.UtcNow - lastAlert < TimeSpan.FromMinutes(_options.AlertCooldownMinutes))
        {
            return false;
        }

        // Check severity threshold
        var severity = DetermineSeverity(result.Metrics.DriftLevel);
        return severity >= DriftSeverity.Medium;
    }

    #region Private Helper Methods

    private Dictionary<string, double[]> GetReferenceData(string modelName, int windowSize)
    {
        // In a real implementation, this would fetch from a reference dataset
        // For now, we'll create reference data from historical data or training dataset
        var referenceData = new Dictionary<string, double[]>();
        
        try
        {
            // Attempt to get historical reference data for the model
            // This could come from the training dataset or a reference period
            var historicalData = GetHistoricalDataForModel(modelName, windowSize);
            
            if (historicalData != null && historicalData.Any())
            {
                foreach (var kvp in historicalData)
                {
                    // Take only the specified window size of data
                    var data = kvp.Value.Take(windowSize).ToArray();
                    if (data.Length > 0)
                    {
                        referenceData[kvp.Key] = data;
                    }
                }
            }
            
            // If no historical data was found, we could potentially use training data
            // as a fallback, but for now we'll log that we're using a minimal reference set
            if (!referenceData.Any())
            {
                _logger.LogInformation("No historical reference data found for model {ModelName}, using default reference data", modelName);
                // Create minimal reference data based on common feature names
                referenceData = CreateDefaultReferenceData(windowSize);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve reference data for model {ModelName}, using default reference data", modelName);
            // Provide default reference data when historical data retrieval fails
            referenceData = CreateDefaultReferenceData(windowSize);
        }
        
        return referenceData;
    }
    
    private Dictionary<string, double[]> GetHistoricalDataForModel(string modelName, int windowSize)
    {
        // In a real implementation, this would query a historical data store or training dataset
        // For now, we'll return an empty dictionary, but we'll log that this is a placeholder
        _logger.LogWarning("Historical data retrieval for model {ModelName} is not implemented - this is a placeholder", modelName);
        return new Dictionary<string, double[]>();
    }
    
    private Dictionary<string, double[]> CreateDefaultReferenceData(int windowSize)
    {
        // Create default reference data with typical feature names and synthetic values
        // This ensures the drift detection can still function even without historical data
        var defaultData = new Dictionary<string, double[]>();
        
        // Common features for machine data
        var commonFeatures = new[] { "temperature", "vibration", "pressure", "rpm", "load" };
        
        foreach (var feature in commonFeatures)
        {
            // Generate synthetic reference data with reasonable values for each feature
            var values = new double[windowSize];
            var random = new Random();
            
            for (int i = 0; i < windowSize; i++)
            {
                switch (feature)
                {
                    case "temperature":
                        values[i] = 65.0 + random.NextDouble() * 20.0; // 65-85 range
                        break;
                    case "vibration":
                        values[i] = random.NextDouble() * 3.0; // 0-3 range
                        break;
                    case "pressure":
                        values[i] = 90.0 + random.NextDouble() * 20.0; // 90-110 range
                        break;
                    case "rpm":
                        values[i] = 1000.0 + random.NextDouble() * 500.0; // 1000-1500 range
                        break;
                    case "load":
                        values[i] = 70.0 + random.NextDouble() * 20.0; // 70-90 range
                        break;
                    default:
                        values[i] = random.NextDouble() * 100.0; // 0-100 range
                        break;
                }
            }
            
            defaultData[feature] = values;
        }
        
        return defaultData;
    }

    private void StoreDriftResult(string modelName, DriftMetrics metrics)
    {
        if (!_driftHistory.ContainsKey(modelName))
        {
            _driftHistory[modelName] = new List<DriftMetrics>();
        }

        _driftHistory[modelName].Add(metrics);

        // Keep only recent history (last 1000 entries)
        if (_driftHistory[modelName].Count > 1000)
        {
            _driftHistory[modelName] = _driftHistory[modelName]
                .OrderByDescending(m => m.Timestamp)
                .Take(1000)
                .ToList();
        }
    }

    private double CalculateKSTest(double[] reference, double[] current)
    {
        // Kolmogorov-Smirnov test approximation
        var refSorted = reference.OrderBy(x => x).ToArray();
        var curSorted = current.OrderBy(x => x).ToArray();
        
        var maxDiff = 0.0;
        var i = 0;
        var j = 0;
        
        while (i < refSorted.Length && j < curSorted.Length)
        {
            var diff = Math.Abs(
                (double)i / refSorted.Length - 
                (double)j / curSorted.Length);
            maxDiff = Math.Max(maxDiff, diff);
            
            if (refSorted[i] <= curSorted[j])
                i++;
            else
                j++;
        }
        
        return maxDiff;
    }

    private double CalculateWassersteinDistance(double[] reference, double[] current)
    {
        // Simplified Wasserstein distance calculation
        var refSorted = reference.OrderBy(x => x).ToArray();
        var curSorted = current.OrderBy(x => x).ToArray();
        
        var sum = 0.0;
        var minLength = Math.Min(refSorted.Length, curSorted.Length);
        
        for (int i = 0; i < minLength; i++)
        {
            sum += Math.Abs(refSorted[i] - curSorted[i]);
        }
        
        return sum / minLength;
    }

    private double CalculatePSI(double[] reference, double[] current)
    {
        // Population Stability Index calculation
        const int buckets = 10;
        var psi = 0.0;
        
        var refMin = reference.Min();
        var refMax = reference.Max();
        var bucketWidth = (refMax - refMin) / buckets;
        
        for (int i = 0; i < buckets; i++)
        {
            var lowerBound = refMin + (i * bucketWidth);
            var upperBound = refMin + ((i + 1) * bucketWidth);
            
            var refCount = reference.Count(x => x >= lowerBound && x < upperBound);
            var curCount = current.Count(x => x >= lowerBound && x < upperBound);
            
            var refPercent = (double)refCount / reference.Length;
            var curPercent = (double)curCount / current.Length;
            
            if (refPercent > 0 && curPercent > 0)
            {
                psi += (curPercent - refPercent) * Math.Log(curPercent / refPercent);
            }
        }
        
        return psi;
    }

    private DriftLevel DetermineDriftLevel(double driftScore, DriftThresholds thresholds)
    {
        if (driftScore <= thresholds.FeatureDriftThreshold * 0.5) return DriftLevel.None;
        if (driftScore <= thresholds.FeatureDriftThreshold) return DriftLevel.Low;
        if (driftScore <= thresholds.FeatureDriftThreshold * 2) return DriftLevel.Moderate;
        if (driftScore <= thresholds.FeatureDriftThreshold * 3) return DriftLevel.High;
        return DriftLevel.Severe;
    }

    private DriftSeverity DetermineSeverity(DriftLevel level)
    {
        return level switch
        {
            DriftLevel.None => DriftSeverity.Low,
            DriftLevel.Low => DriftSeverity.Low,
            DriftLevel.Moderate => DriftSeverity.Medium,
            DriftLevel.High => DriftSeverity.High,
            DriftLevel.Severe => DriftSeverity.Critical,
            _ => DriftSeverity.Low
        };
    }

    private string GenerateRecommendation(DriftDetectionResult result)
    {
        if (!result.IsDriftDetected)
            return "No significant drift detected. Model performance should remain stable.";

        var severity = DetermineSeverity(result.Metrics.DriftLevel);
        
        return severity switch
        {
            DriftSeverity.Low => "Minor drift detected. Monitor model performance closely.",
            DriftSeverity.Medium => "Moderate drift detected. Consider reviewing model inputs and potentially retraining.",
            DriftSeverity.High => "Significant drift detected. Model retraining recommended soon.",
            DriftSeverity.Critical => "Severe drift detected. Immediate model retraining required.",
            _ => "Drift detected. Review model performance metrics."
        };
    }

    private DriftThresholds CreateDefaultThresholds()
    {
        return new DriftThresholds
        {
            FeatureDriftThreshold = _options.DefaultFeatureThreshold,
            PredictionDriftThreshold = _options.DefaultPredictionThreshold,
            LabelDriftThreshold = 0.2
        };
    }

    #endregion
}