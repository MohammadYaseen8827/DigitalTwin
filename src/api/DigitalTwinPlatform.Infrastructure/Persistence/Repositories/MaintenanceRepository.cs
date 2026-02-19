using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class MaintenanceRepository(DigitalTwinDbContext context) 
    : Repository<MaintenanceRecord>(context), IMaintenanceRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<MaintenanceRecord>> SearchAsync(string query, string? statusFilter = null, CancellationToken ct = default)
    {
        IQueryable<MaintenanceRecord> queryable = _context.MaintenanceRecords;

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (Enum.TryParse<MaintenanceStatus>(statusFilter, true, out var status))
            {
                queryable = queryable.Where(m => m.Status == status);
            }
        }

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            queryable = queryable.Where(m => 
                m.Type.ToString().ToLower().Contains(normalizedQuery) ||
                m.Notes != null && m.Notes.ToLower().Contains(normalizedQuery) ||
                m.PerformedBy != null && m.PerformedBy.ToLower().Contains(normalizedQuery) ||
                m.MachineId.ToString().Contains(normalizedQuery) ||
                m.Id.ToString().Contains(normalizedQuery)
            );
        }

        // Order by planned date descending and return results
        return await queryable
            .OrderByDescending(m => m.PlannedDate)
            .ToListAsync(ct);
    }
}