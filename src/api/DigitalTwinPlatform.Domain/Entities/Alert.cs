namespace DigitalTwinPlatform.Domain.Entities;

public enum AlertSeverity
{
    Info,
    Warning,
    Critical,
    Emergency
}

public class Alert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MachineId { get; set; }
    public Machine? Machine { get; set; }
    
    // Alert content
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    
    // Related prediction (for RUL-based alerts)
    public Guid? RelatedPredictionId { get; set; }
    public Prediction? RelatedPrediction { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Acknowledgment
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Creates a new alert
    /// </summary>
    public static Alert Create(
        Guid machineId,
        string message,
        AlertSeverity severity,
        Guid? relatedPredictionId = null)
    {
        return new Alert
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            Message = message,
            Severity = severity,
            RelatedPredictionId = relatedPredictionId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Acknowledges the alert
    /// </summary>
    public void Acknowledge(string acknowledgedBy)
    {
        if (IsAcknowledged)
            return;

        IsAcknowledged = true;
        AcknowledgedBy = acknowledgedBy;
        AcknowledgedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if alert is critical and needs immediate attention
    /// </summary>
    public bool IsCritical()
    {
        return Severity == AlertSeverity.Critical && !IsAcknowledged;
    }

    /// <summary>
    /// Checks if alert is still active (not too old)
    /// </summary>
    public bool IsActive(TimeSpan maxAge)
    {
        return !IsAcknowledged && (DateTime.UtcNow - CreatedAt) < maxAge;
    }
}
