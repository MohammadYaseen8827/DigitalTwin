using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IMachineRepository : IRepository<Machine>
{
    Task<IEnumerable<Machine>> GetByProductionLineAsync(Guid productionLineId, CancellationToken ct = default);
    Task<IEnumerable<Machine>> SearchAsync(string query, string? statusFilter = null, CancellationToken ct = default);
}
