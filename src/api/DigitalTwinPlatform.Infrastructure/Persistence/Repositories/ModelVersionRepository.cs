using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class ModelVersionRepository : IModelVersionRepository
{
    private readonly DigitalTwinDbContext _context;

    public ModelVersionRepository(DigitalTwinDbContext context)
    {
        _context = context;
    }

    public async Task<ModelVersion?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ModelVersion>()
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<ModelVersion?> GetByVersionAsync(string modelType, string version, CancellationToken ct = default)
    {
        return await _context.Set<ModelVersion>()
            .FirstOrDefaultAsync(m => m.ModelType == modelType && m.Version == version, ct);
    }

    public async Task<ModelVersion?> GetProductionVersionAsync(string modelType, CancellationToken ct = default)
    {
        return await _context.Set<ModelVersion>()
            .FirstOrDefaultAsync(m => m.ModelType == modelType && m.Status == ModelStatus.Production, ct);
    }

    public async Task<List<ModelVersion>> GetByModelTypeAsync(string modelType, CancellationToken ct = default)
    {
        return await _context.Set<ModelVersion>()
            .Where(m => m.ModelType == modelType)
            .OrderByDescending(m => m.TrainedAt)
            .ToListAsync(ct);
    }

    public async Task<List<ModelVersion>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<ModelVersion>()
            .OrderByDescending(m => m.TrainedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ModelVersion modelVersion, CancellationToken ct = default)
    {
        await _context.Set<ModelVersion>().AddAsync(modelVersion, ct);
    }

    public async Task UpdateAsync(ModelVersion modelVersion, CancellationToken ct = default)
    {
        modelVersion.UpdatedAt = DateTime.UtcNow;
        _context.Set<ModelVersion>().Update(modelVersion);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var modelVersion = await GetAsync(id, ct);
        if (modelVersion != null)
        {
            _context.Set<ModelVersion>().Remove(modelVersion);
        }
        await Task.CompletedTask;
    }
}
