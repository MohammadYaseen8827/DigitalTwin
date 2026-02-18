using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.Application.Abstractions.Repositories;

public interface IModelVersionRepository
{
    Task<ModelVersion?> GetAsync(Guid id, CancellationToken ct = default);
    Task<ModelVersion?> GetByVersionAsync(string modelType, string version, CancellationToken ct = default);
    Task<ModelVersion?> GetProductionVersionAsync(string modelType, CancellationToken ct = default);
    Task<List<ModelVersion>> GetByModelTypeAsync(string modelType, CancellationToken ct = default);
    Task<List<ModelVersion>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ModelVersion modelVersion, CancellationToken ct = default);
    Task UpdateAsync(ModelVersion modelVersion, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
