using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> SearchAsync(string query, string? statusFilter = null, string? severityFilter = null, CancellationToken ct = default);
}
