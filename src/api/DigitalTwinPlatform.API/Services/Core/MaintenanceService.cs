using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Core;

public class MaintenanceService : IMaintenanceService
{
    private readonly IRepository<MaintenanceRecord> _maintenanceRepository;
    private readonly IRepository<Alert> _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        IRepository<MaintenanceRecord> maintenanceRepository,
        IRepository<Alert> alertRepository,
        IUnitOfWork unitOfWork,
        ILogger<MaintenanceService> logger)
    {
        _maintenanceRepository = maintenanceRepository;
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MaintenanceRecord> PlanMaintenanceAsync(Guid machineId, MaintenanceType type,
        DateTime plannedDate, string notes, Guid? alertId = null)
    {
        var record = new MaintenanceRecord
        {
            MachineId = machineId,
            Type = type,
            PlannedDate = plannedDate,
            Notes = notes,
            AlertId = alertId,
            Status = MaintenanceStatus.Planned
        };

        await _maintenanceRepository.AddAsync(record);

        if (alertId.HasValue)
        {
            var alert = await _alertRepository.GetAsync(alertId.Value);
            if (alert != null && !alert.IsAcknowledged)
            {
                alert.IsAcknowledged = true;
                alert.AcknowledgedAt = DateTime.UtcNow;
                alert.AcknowledgedBy = "SYSTEM_PLANNED_MAINTENANCE";
            }
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Maintenance planned for machine {MachineId} on {PlannedDate}", machineId, plannedDate);
        return record;
    }

    public async Task<MaintenanceRecord> StartMaintenanceAsync(Guid recordId)
    {
        var record = await _maintenanceRepository.GetAsync(recordId);
        if (record == null) throw new KeyNotFoundException("Maintenance record not found");

        record.Status = MaintenanceStatus.InProgress;
        record.Date = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Maintenance started for record {RecordId}", recordId);
        return record;
    }

    public async Task<MaintenanceRecord> CompleteMaintenanceAsync(Guid recordId, string technician, string finalNotes)
    {
        var record = await _maintenanceRepository.GetAsync(recordId);
        if (record == null) throw new KeyNotFoundException("Maintenance record not found");

        record.Status = MaintenanceStatus.Completed;
        record.CompletionDate = DateTime.UtcNow;
        record.Technician = technician;
        record.Notes += "\nCompletion Notes: " + finalNotes;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Maintenance completed for record {RecordId} by {Technician}", recordId, technician);
        return record;
    }

    public async Task<MaintenanceRecord> CancelMaintenanceAsync(Guid recordId, string reason)
    {
        var record = await _maintenanceRepository.GetAsync(recordId);
        if (record == null) throw new KeyNotFoundException("Maintenance record not found");

        record.Status = MaintenanceStatus.Cancelled;
        record.Notes += "\nCancellation Reason: " + reason;

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Maintenance cancelled for record {RecordId}. Reason: {Reason}", recordId, reason);
        return record;
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceHistoryAsync(Guid machineId)
    {
        return await _maintenanceRepository.GetAllAsync(r => r.MachineId == machineId);
    }

    public async Task<IEnumerable<MaintenanceRecord>> GetActiveMaintenanceAsync()
    {
        return await _maintenanceRepository.GetAllAsync(r => r.Status == MaintenanceStatus.InProgress || r.Status == MaintenanceStatus.Planned);
    }

    public async Task<IEnumerable<MaintenanceRecord>> SearchMaintenanceAsync(string query, string? status = null, Guid? machineId = null, CancellationToken ct = default)
    {
        // Cast to IMaintenanceRepository to access the SearchAsync method
        var repo = _unitOfWork.Repository<MaintenanceRecord>() as IMaintenanceRepository;
        if (repo != null)
        {
            return await repo.SearchAsync(query, status, ct);
        }

        // Fallback: Search without status filter if the specific repository interface isn't available
        var results = await _maintenanceRepository.GetAllAsync();

        // Apply filters manually
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MaintenanceStatus>(status, true, out var statusEnum))
        {
            results = results.Where(r => r.Status == statusEnum);
        }

        if (machineId.HasValue)
        {
            results = results.Where(r => r.MachineId == machineId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            results = results.Where(r =>
                r.Type.ToString().ToLower().Contains(normalizedQuery) ||
                r.Notes.ToLower().Contains(normalizedQuery) ||
                r.Technician.ToLower().Contains(normalizedQuery) ||
                r.MachineId.ToString().Contains(normalizedQuery) ||
                r.Id.ToString().Contains(normalizedQuery)
            );
        }

        return results;
    }
}
