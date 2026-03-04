using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork(DigitalTwinDbContext context, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(type);
            var repository = Activator.CreateInstance(repositoryType, context)
                ?? throw new InvalidOperationException($"Failed to create repository for {type.Name}");
            _repositories[type] = repository;
        }
        return (IRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        const int maxRetries = 3;
        int attempt = 0;

        while (true)
        {
            try
            {
                return await context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                attempt++;
                if (attempt >= maxRetries)
                {
                    logger.LogWarning("Concurrency failed after {MaxRetries} attempts for entity: {EntityName}",
                        maxRetries, ex.Entries.FirstOrDefault()?.Entity.GetType().Name);
                    throw;
                }

                // Reload all modified entities from the database
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync(ct);
                }

                // Give a small delay before retrying
                await Task.Delay(50 * attempt, ct);
            }
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();
            }
        }
        finally
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(ct);
            }
        }
        finally
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
        await context.DisposeAsync();
        _disposed = true;
    }
}
