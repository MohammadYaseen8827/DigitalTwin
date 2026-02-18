using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

/// <summary>
/// Interface for RUL prediction service.
/// </summary>
public interface IRulPredictor
{
    /// <summary>
    /// Gets RUL prediction with full details including confidence intervals and contributing factors.
    /// </summary>
    RulPredictionResult PredictWithDetails(string machineId, Dictionary<string, double> features);

    /// <summary>
    /// Predicts RUL from feature dictionary.
    /// </summary>
    double PredictWithFeatures(Dictionary<string, double> features);

    /// <summary>
    /// Predicts RUL with confidence interval bounds.
    /// </summary>
    (double Prediction, double LowerBound, double UpperBound) PredictWithRange(Dictionary<string, double> features);

    /// <summary>
    /// Gets contributing factors for the prediction.
    /// </summary>
    List<ContributingFactor> GetContributingFactors(Dictionary<string, double> features);

    /// <summary>
    /// Validates if model can make predictions.
    /// </summary>
    bool IsModelLoaded { get; }
}

/// <summary>
/// Result of RUL prediction with full details.
/// </summary>
public class RulPredictionResult
{
    public string MachineId { get; set; } = string.Empty;
    public double Rul { get; set; }
    public string RulUnit { get; set; } = "hours";
    public double Confidence { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public DateTime PredictionTime { get; set; }
    public List<ContributingFactor> ContributingFactors { get; set; } = new();
    public string ModelVersion { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    
    // Additional fields for frontend compatibility
    public double ConfidenceScore => Confidence; // Map Confidence to ConfidenceScore
    public double DegradationRate { get; set; } = 0.0;
    public string Trend { get; set; } = "stable"; // Options: "improving", "declining", "stable"
    public DateTime EstimatedFailureDate { get; set; }
    public string ModelType { get; set; } = "rul";
    public DateTime LastUpdated => PredictionTime;
    public double HealthScore { get; set; } = 0.0;
}

/// <summary>
/// Represents a contributing factor to the RUL prediction.
/// </summary>
public class ContributingFactor
{
    public string Feature { get; set; } = string.Empty;
    public double Impact { get; set; }
    public string Direction { get; set; } = "positive";
}

/// <summary>
/// Interface for health classification service.
/// </summary>
public interface IHealthClassifier
{
    /// <summary>
    /// Gets health classification with full details including confidence and contributing factors.
    /// </summary>
    HealthClassificationResult ClassifyWithDetails(string machineId, Dictionary<string, double> features);

    /// <summary>
    /// Classifies health status from feature dictionary.
    /// </summary>
    (double probability, HealthClassification health) ClassifyWithFeatures(Dictionary<string, double> features);

    /// <summary>
    /// Gets contributing factors for health classification.
    /// </summary>
    List<HealthContributingFactor> GetContributingFactors(Dictionary<string, double> features);

    /// <summary>
    /// Validates if model is loaded.
    /// </summary>
    bool IsModelLoaded { get; }

    /// <summary>
    /// Gets overall risk score (0-1, higher = more risk).
    /// </summary>
    double GetRiskScore(Dictionary<string, double> features);
}

/// <summary>
/// Result of health classification with full details.
/// </summary>
public class HealthClassificationResult
{
    public string MachineId { get; set; } = string.Empty;
    public HealthClassification HealthStatus { get; set; }
    public double HealthProbability { get; set; }
    public double Confidence { get; set; }
    public List<HealthContributingFactor> ContributingFactors { get; set; } = new();
    public DateTime ClassificationTime { get; set; }
    public long ProcessingTimeMs { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public bool IsFallback { get; set; }
}

/// <summary>
/// Represents a contributing factor to health classification.
/// </summary>
public class HealthContributingFactor
{
    public string Feature { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Impact { get; set; }
    public double Threshold { get; set; }
    public bool IsWithinThreshold { get; set; }
}
