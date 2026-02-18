namespace DigitalTwinPlatform.Application.Maintenance.Models;

using DigitalTwinPlatform.Domain.Entities;

public record PlanMaintenanceRequest(
    Guid MachineId,
    MaintenanceType Type,
    DateTime PlannedDate,
    string Notes,
    Guid? AlertId = null);

public record CompleteMaintenanceRequest(
    string Technician,
    string FinalNotes);
