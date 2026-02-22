using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class SavedSearchRepository(DigitalTwinDbContext context) : Repository<SavedSearch>(context), ISavedSearchRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<SavedSearch>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await _context.SavedSearches.Where(s => s.UserId == userId).ToListAsync(ct);
}
