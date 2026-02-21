using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class PredictionRepository(DigitalTwinDbContext context) : Repository<Prediction>(context), IPredictionRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<Prediction>> GetByMachineIdAsync(Guid machineId, int take = 100, CancellationToken ct = default)
    {
        return await _context.Predictions
            .Where(p => p.MachineId == machineId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Prediction>> SearchAsync(string query, Guid? machineId = null, CancellationToken ct = default)
    {
        var queryable = _context.Predictions.AsQueryable();

        // Apply machine filter if specified
        if (machineId.HasValue)
        {
            queryable = queryable.Where(p => p.MachineId == machineId.Value);
        }

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.ToLowerInvariant();
            // Use simple search without null-propagating operators in expression tree
            queryable = queryable.Where(p =>
                p.HealthStatus.ToString().ToLowerInvariant().Contains(lowerQuery) ||
                p.ModelVersion.ToLowerInvariant().Contains(lowerQuery) ||
                p.Id.ToString().Contains(lowerQuery)
            );
        }

        // Order by creation date descending and return results
        return await queryable
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }
}

