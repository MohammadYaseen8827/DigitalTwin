using System.Text.Json;

namespace DigitalTwinPlatform.Domain.Entities;

public enum MaintenanceStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}

public enum MaintenanceType
{
    Preventive,
    Corrective,
    Predictive,
    Emergency
}

public class MaintenanceRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MachineId { get; set; }
    public Machine? Machine { get; set; }
    public Guid? AlertId { get; set; }
    public Alert? Alert { get; set; }
    
    // Maintenance details
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? PlannedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Planned;
    public MaintenanceType Type { get; set; } = MaintenanceType.Preventive;
    public string Description { get; set; } = string.Empty;
    
    // Technician information
    public string Technician { get; set; } = string.Empty;
    
    // Parts and cost tracking
    public List<string> PartsReplaced { get; set; } = new();
    public decimal Cost { get; set; }
    public string? CostCurrency { get; set; } = "USD";
    
    // Additional notes
    public string Notes { get; set; } = string.Empty;
    public JsonDocument? WorkOrderDetails { get; set; }

    /// <summary>
    /// Creates a new maintenance record
    /// </summary>
    public static MaintenanceRecord Create(
        Guid machineId,
        MaintenanceType type,
        string description,
        DateTime? plannedDate = null,
        string technician = "")
    {
        return new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            Type = type,
            Description = description,
            PlannedDate = plannedDate,
            Technician = technician,
            Date = DateTime.UtcNow,
            Status = MaintenanceStatus.Planned
        };
    }

    /// <summary>
    /// Marks maintenance as started
    /// </summary>
    public void Start()
    {
        if (Status != MaintenanceStatus.Planned)
            return;

        Status = MaintenanceStatus.InProgress;
    }

    /// <summary>
    /// Completes the maintenance
    /// </summary>
    public void Complete(List<string> partsReplaced, decimal cost, string? notes = null)
    {
        Status = MaintenanceStatus.Completed;
        CompletionDate = DateTime.UtcNow;
        PartsReplaced = partsReplaced;
        Cost = cost;
        if (!string.IsNullOrEmpty(notes))
            Notes = notes;
    }

    /// <summary>
    /// Cancels the maintenance
    /// </summary>
    public void Cancel(string reason)
    {
        Status = MaintenanceStatus.Cancelled;
        Notes = $"{Notes}\nCancellation reason: {reason}".Trim();
    }

    /// <summary>
    /// Gets the total duration of maintenance (if completed)
    /// </summary>
    public TimeSpan? GetDuration()
    {
        if (CompletionDate.HasValue && Status == MaintenanceStatus.Completed)
        {
            return CompletionDate.Value - Date;
        }
        return null;
    }

    /// <summary>
    /// Checks if maintenance is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return PlannedDate.HasValue && 
               PlannedDate.Value < DateTime.UtcNow && 
               Status == MaintenanceStatus.Planned;
    }
}
