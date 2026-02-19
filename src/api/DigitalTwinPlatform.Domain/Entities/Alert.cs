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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Status { get; set; } = "active"; // active, acknowledged, resolved
    
    // Categories and recommendations
    public string? Category { get; set; }
    public string? RecommendedAction { get; set; }
    public string? SuggestedActions { get; set; }
    
    // Related prediction (for RUL-based alerts)
    public Guid? RelatedPredictionId { get; set; }
    public Prediction? RelatedPrediction { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Acknowledgment
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// Creates a new alert
    /// </summary>
    public static Alert Create(
        Guid machineId,
        string message,
        AlertSeverity severity,
        Guid? relatedPredictionId = null,
        string? category = null,
        string? recommendedAction = null)
    {
        // Parse message to extract title and description
        // Assuming message format: "TITLE: Description" or just "Message"
        var colonIndex = message.IndexOf(':');
        var title = colonIndex > -1 ? message.Substring(0, colonIndex).Trim() : message;
        var description = colonIndex > -1 ? message.Substring(colonIndex + 1).Trim() : message;

        return new Alert
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            Message = message,
            Title = title,
            Description = description,
            Severity = severity,
            Status = "active",
            Category = category,
            RecommendedAction = recommendedAction,
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
        Status = "acknowledged";
    }

    /// <summary>
    /// Resolves the alert
    /// </summary>
    public void Resolve()
    {
        ResolvedAt = DateTime.UtcNow;
        Status = "resolved";
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
