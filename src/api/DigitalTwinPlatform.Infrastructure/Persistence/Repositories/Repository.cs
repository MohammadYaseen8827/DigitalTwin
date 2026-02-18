using System.Linq.Expressions;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class Repository<T>(DigitalTwinDbContext context) : IRepository<T>
    where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default) 
        => await _set.FindAsync(new object[] { id }, cancellationToken: ct);

    public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default,
        int take = 0,
        bool asNoTracking = false)
    {
        IQueryable<T> query = predicate is null ? _set : _set.Where(predicate);
        if (asNoTracking)
            query = query.AsNoTracking();
        if (take > 0)
            query = query.Take(take);
        return await query.ToListAsync(ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _set.AddRangeAsync(entities, ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        _set.RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
