using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class TelemetryRepository(DigitalTwinDbContext context)
    : Repository<TelemetryData>(context), ITelemetryRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<TelemetryData>> GetForMachineAsync(Guid machineId, DateTime? since = null, int take = 500, CancellationToken ct = default)
    {
        IQueryable<TelemetryData> query = _context.TelemetryData.Where(t => t.MachineId == machineId);

        if (since.HasValue)
        {
            query = query.Where(t => t.Timestamp >= since.Value);
        }

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TelemetryData>> GetRecentAsync(DateTime? since = null, Guid? machineId = null, int limit = 100, CancellationToken ct = default)
    {
        IQueryable<TelemetryData> query = _context.TelemetryData;

        if (machineId.HasValue)
        {
            query = query.Where(t => t.MachineId == machineId.Value);
        }

        if (since.HasValue)
        {
            query = query.Where(t => t.Timestamp >= since.Value);
        }

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public Task<IEnumerable<TelemetryData>> SearchAsync(string query, DateTime? since = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

