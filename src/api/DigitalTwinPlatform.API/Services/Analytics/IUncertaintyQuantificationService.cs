using DigitalTwinPlatform.Application.Predictions.Models;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IUncertaintyQuantificationService
{
    /// <summary>
    /// Performs Monte Carlo simulation to quantify prediction uncertainty
    /// </summary>
    Task<UncertaintyResult> PerformMonteCarloSimulationAsync(
        Dictionary<string, double> baseFeatures,
        int iterations = 1000,
        double noiseLevel = 0.1,
        CancellationToken ct = default);

    /// <summary>
    /// Performs Bayesian inference for uncertainty quantification
    /// </summary>
    Task<UncertaintyResult> PerformBayesianInferenceAsync(
        Dictionary<string, double> observedData,
        Dictionary<string, (double mean, double stdDev)> priorDistributions,
        int samples = 2000,
        CancellationToken ct = default);

    /// <summary>
    /// Calculates prediction intervals using bootstrap resampling
    /// </summary>
    Task<ConfidenceInterval> CalculateBootstrapIntervalsAsync(
        Guid machineId,
        int bootstrapSamples = 1000,
        double confidenceLevel = 0.95,
        CancellationToken ct = default);

    /// <summary>
    /// Quantifies model uncertainty using ensemble methods
    /// </summary>
    Task<ModelUncertaintyResult> QuantifyModelUncertaintyAsync(
        Guid machineId,
        Dictionary<string, double> features,
        CancellationToken ct = default);
}

public class UncertaintyResult
{
    public double MeanPrediction { get; set; }
    public double StandardDeviation { get; set; }
    public double[] Samples { get; set; } = [];
    public Dictionary<string, double> FeatureUncertainties { get; set; } = [];
    public double ConfidenceIntervalLower { get; set; }
    public double ConfidenceIntervalUpper { get; set; }
    public double PredictionVariance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConfidenceInterval
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double ConfidenceLevel { get; set; }
    public double CoverageProbability { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ModelUncertaintyResult
{
    public double AleatoricUncertainty { get; set; }  // Data uncertainty
    public double EpistemicUncertainty { get; set; }  // Model uncertainty
    public double TotalUncertainty { get; set; }
    public Dictionary<string, double> FeatureImportanceWithUncertainty { get; set; } = [];
    public double ModelConfidence { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}