using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities.Enums;
using DigitalTwinPlatform.Domain.Enums;

namespace DigitalTwinPlatform.Domain.Entities;

public class Prediction
{
    public Guid Id { get; set; }
    public Guid MachineId { get; set; }
    public Machine? Machine { get; set; }
    
    // RUL (Remaining Useful Life) predictions
    public double RemainingUsefulLifeDays { get; set; }
    public double RulLowerBound { get; set; }
    public double RulUpperBound { get; set; }
    
    // Confidence metrics
    public double Confidence { get; set; }
    
    // Failure probability
    public double FailureProbability { get; set; }
    
    // Health classification based on prediction
    public HealthClassification HealthStatus { get; set; }
    
    // Contributing factors (JSONB for flexible storage)
    public Dictionary<string, double>? ContributingFactors { get; set; }
    public Dictionary<string, double>? FeatureContributions { get; set; }
    
    // Model metadata
    public string ModelVersion { get; set; } = "v0";
    public Guid? ModelVersionId { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PredictionTime { get; set; }

    /// <summary>
    /// Creates a new prediction record
    /// </summary>
    public static Prediction Create(
        Guid machineId,
        double rul,
        double confidence,
        double failureProbability,
        HealthClassification healthStatus,
        Dictionary<string, double>? contributingFactors = null)
    {
        return new Prediction
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            RemainingUsefulLifeDays = rul,
            Confidence = Math.Clamp(confidence, 0, 1),
            FailureProbability = Math.Clamp(failureProbability, 0, 1),
            HealthStatus = healthStatus,
            ContributingFactors = contributingFactors ?? new Dictionary<string, double>(),
            CreatedAt = DateTime.UtcNow,
            PredictionTime = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Computes confidence interval bounds based on confidence level
    /// </summary>
    public void ComputeConfidenceBounds()
    {
        // Simple confidence interval calculation
        // Higher confidence = wider interval
        double uncertaintyFactor = 1.0 - Confidence;
        double marginOfError = RemainingUsefulLifeDays * uncertaintyFactor * 0.3;
        
        RulLowerBound = Math.Max(0, RemainingUsefulLifeDays - marginOfError);
        RulUpperBound = RemainingUsefulLifeDays + marginOfError;
    }

    /// <summary>
    /// Gets the prediction interval width (measure of uncertainty)
    /// </summary>
    public double GetPredictionIntervalWidth()
    {
        return RulUpperBound - RulLowerBound;
    }

    /// <summary>
    /// Checks if prediction is still valid (within confidence bounds)
    /// </summary>
    public bool IsValid()
    {
        return Confidence >= 0.5 && RemainingUsefulLifeDays >= 0;
    }
}
