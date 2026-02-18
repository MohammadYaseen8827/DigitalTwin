using DigitalTwinPlatform.Application.Abstractions.Repositories;

namespace DigitalTwinPlatform.Application.Abstractions.UnitOfWork;

/// <summary>
/// Unit of Work pattern for managing transactional boundaries.
/// All repositories share the same DbContext and transaction.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Gets a repository for the specified entity type.
    /// All repositories share the same DbContext and transaction.
    /// </summary>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Saves all changes to the database.
    /// Must be called after all operations are complete.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins a new transaction. All operations will be atomic.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken ct = default);
}
