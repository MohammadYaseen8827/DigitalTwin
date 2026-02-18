using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IMaintenanceRepository : IRepository<MaintenanceRecord>
{
    Task<IEnumerable<MaintenanceRecord>> SearchAsync(string query, string? statusFilter = null, CancellationToken ct = default);
}
