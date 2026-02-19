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

    public async Task<IEnumerable<TelemetryData>> SearchAsync(string query, DateTime? since = null, CancellationToken ct = default)
    {
        IQueryable<TelemetryData> queryable = _context.TelemetryData;

        // Apply date filter if provided
        if (since.HasValue)
        {
            queryable = queryable.Where(t => t.Timestamp >= since.Value);
        }

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            queryable = queryable.Where(t => 
                t.DataType.ToLower().Contains(normalizedQuery) ||
                (t.Data != null && t.Data.ToString().ToLower().Contains(normalizedQuery)) ||
                t.MachineId.ToString().Contains(normalizedQuery) ||
                (t.Metadata != null && t.Metadata.ToString().ToLower().Contains(normalizedQuery))
            );
        }

        // Order by timestamp descending and return results
        return await queryable
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(ct);
    }
}

