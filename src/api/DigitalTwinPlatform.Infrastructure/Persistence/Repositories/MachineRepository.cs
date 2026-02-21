using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class MachineRepository(DigitalTwinDbContext context) : Repository<Machine>(context), IMachineRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<Machine>> GetByProductionLineAsync(Guid productionLineId, CancellationToken ct = default)
        => await _context.Machines.Where(m => m.ProductionLineId == productionLineId).ToListAsync(ct);

    public async Task<IEnumerable<Machine>> SearchAsync(string query, string? statusFilter = null, CancellationToken ct = default)
    {
        IQueryable<Machine> queryable = _context.Machines;

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(statusFilter))
        {
            var statuses = statusFilter.Split(',', StringSplitOptions.RemoveEmptyEntries);
            queryable = queryable.Where(m => statuses.Contains(m.Status.ToString()));
        }

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            queryable = queryable.Where(m => 
                m.Name.Value.ToLowerInvariant().Contains(normalizedQuery) ||
                m.Type.ToString().ToLowerInvariant().Contains(normalizedQuery) ||
                m.Location.ToLowerInvariant().Contains(normalizedQuery) ||
                m.Id.ToString().Contains(normalizedQuery)
            );
        }

        // Order by name and return results
        return await queryable
            .OrderBy(m => m.Name)
            .ToListAsync(ct);
    }
}

