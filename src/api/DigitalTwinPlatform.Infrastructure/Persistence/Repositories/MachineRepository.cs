using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class MachineRepository(DigitalTwinDbContext context) : Repository<Machine>(context), IMachineRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<Machine>> GetByProductionLineAsync(Guid productionLineId, CancellationToken ct = default)
        => await _context.Machines.Where(m => m.ProductionLineId == productionLineId).ToListAsync(ct);

    public Task<IEnumerable<Machine>> SearchAsync(string query, string? statusFilter = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}

