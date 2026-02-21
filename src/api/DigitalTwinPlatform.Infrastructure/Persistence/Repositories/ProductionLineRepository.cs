using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class ProductionLineRepository(DigitalTwinDbContext context) 
    : Repository<ProductionLine>(context), IProductionLineRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<ProductionLine>> SearchAsync(string query, CancellationToken ct = default)
    {
        IQueryable<ProductionLine> queryable = _context.ProductionLines;

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            queryable = queryable.Where(p => 
                p.Name.ToLowerInvariant().Contains(normalizedQuery) ||
                p.Id.ToString().Contains(normalizedQuery)
            );
        }

        // Order by name and return results
        return await queryable
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}