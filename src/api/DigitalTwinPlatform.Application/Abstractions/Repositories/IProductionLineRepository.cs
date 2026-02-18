using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IProductionLineRepository : IRepository<ProductionLine>
{
    Task<IEnumerable<ProductionLine>> SearchAsync(string query, CancellationToken ct = default);
}
