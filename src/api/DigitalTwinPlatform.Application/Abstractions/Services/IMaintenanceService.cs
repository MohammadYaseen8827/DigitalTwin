using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface IMaintenanceService
{
    Task<MaintenanceRecord> PlanMaintenanceAsync(Guid machineId, MaintenanceType type, DateTime plannedDate,
        string notes, Guid? alertId = null);
    Task<MaintenanceRecord> StartMaintenanceAsync(Guid recordId);
    Task<MaintenanceRecord> CompleteMaintenanceAsync(Guid recordId, string performedBy, string finalNotes);
    Task<MaintenanceRecord> CancelMaintenanceAsync(Guid recordId, string reason);
    Task<IEnumerable<MaintenanceRecord>> GetMaintenanceHistoryAsync(Guid machineId);
    Task<IEnumerable<MaintenanceRecord>> GetActiveMaintenanceAsync();
}
