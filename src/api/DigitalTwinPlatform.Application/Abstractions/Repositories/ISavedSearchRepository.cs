using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface ISavedSearchRepository : IRepository<SavedSearch>
{
    Task<IEnumerable<SavedSearch>> GetByUserAsync(string userId, CancellationToken ct = default);
}
