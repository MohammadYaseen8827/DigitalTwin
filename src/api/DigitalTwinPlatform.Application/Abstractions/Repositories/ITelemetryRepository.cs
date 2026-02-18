using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface ITelemetryRepository : IRepository<TelemetryData>
{
    Task<IEnumerable<TelemetryData>> GetForMachineAsync(Guid machineId, DateTime? since = null, int take = 500, CancellationToken ct = default);
    Task<IEnumerable<TelemetryData>> GetRecentAsync(DateTime? since = null, Guid? machineId = null, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<TelemetryData>> SearchAsync(string query, DateTime? since = null, CancellationToken ct = default);
}
