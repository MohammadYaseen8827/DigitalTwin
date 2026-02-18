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
}

