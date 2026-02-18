namespace DigitalTwinPlatform.Domain.ValueObjects;

public record MaintenanceWindow
{
    public DateTime ScheduledDate { get; init; }
    public double EstimatedCost { get; init; }
    public double RiskScore { get; init; } // Probability of failure before this date
    public string Recommendation { get; init; } = string.Empty;

    public MaintenanceWindow(DateTime scheduledDate, double estimatedCost, double riskScore, string recommendation)
    {
        ScheduledDate = scheduledDate;
        EstimatedCost = estimatedCost;
        RiskScore = riskScore;
        Recommendation = recommendation;
    }
}
