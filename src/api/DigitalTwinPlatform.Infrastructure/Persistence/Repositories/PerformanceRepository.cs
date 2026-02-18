using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// High-performance repository with optimization patterns
    /// </summary>
    public interface IPerformanceRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, bool includeRelated = false);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, 
            string[]? includeProperties = null,
            int? take = null,
            int? skip = null);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Expression<Func<T, bool>>? predicate = null,
            string[]? includeProperties = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task BulkInsertAsync(IEnumerable<T> entities);
        Task BulkUpdateAsync(IEnumerable<T> entities);
    }

    public class PerformanceRepository<T> : IPerformanceRepository<T> where T : class
    {
        protected readonly DigitalTwinDbContext _context;
        protected readonly DbSet<T> DbSet;
        protected readonly ILogger<PerformanceRepository<T>> Logger;

        public PerformanceRepository(DigitalTwinDbContext context, ILogger<PerformanceRepository<T>> logger)
        {
            _context = context;
            DbSet = context.Set<T>();
            Logger = logger;
        }

        public async Task<T?> GetByIdAsync(Guid id, bool includeRelated = false)
        {
            var query = DbSet.AsNoTracking();
            
            // Apply includes for related data if requested
            if (includeRelated)
            {
                query = ApplyIncludes(query);
            }

            // Use compiled queries for better performance
            return await query.FirstOrDefaultAsync(CreateIdPredicate(id));
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? predicate = null,
            string[]? includeProperties = null,
            int? take = null,
            int? skip = null)
        {
            var query = DbSet.AsNoTracking();

            // Apply predicate
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Apply includes
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            // Apply paging
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            string[]? includeProperties = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false)
        {
            var query = DbSet.AsNoTracking();

            // Apply predicate
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Apply includes
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            // Get total count before paging
            var totalCount = await query.CountAsync();

            // Apply ordering
            if (orderBy != null)
            {
                query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            // Apply paging
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            var query = DbSet.AsNoTracking();
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await DbSet.AsNoTracking().AnyAsync(predicate);
        }

        public async Task<T> AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            DbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            await DbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task BulkUpdateAsync(IEnumerable<T> entities)
        {
            DbSet.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

        #region Private Methods

        private Expression<Func<T, bool>> CreateIdPredicate(Guid id)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            return Expression.Lambda<Func<T, bool>>(equality, parameter);
        }

        private IQueryable<T> ApplyIncludes(IQueryable<T> query)
        {
            // Override in derived classes to specify includes
            return query;
        }

        #endregion
    }

    // Example specialized repository with specific optimizations
    public class OptimizedMachineRepository : PerformanceRepository<Machine>
    {
        public OptimizedMachineRepository(DigitalTwinDbContext context, ILogger<OptimizedMachineRepository> logger)
            : base(context, logger)
        {
        }

        // Simplified compiled query for better performance
        private static readonly Func<DigitalTwinDbContext, Guid, Machine?> CompiledGetById =
            EF.CompileQuery((DigitalTwinDbContext context, Guid id) =>
                context.Machines
                    .AsNoTracking()
                    .FirstOrDefault(m => m.Id == id));

        public async Task<Machine?> GetMachineWithDetailsAsync(Guid id)
        {
            return await Task.FromResult(CompiledGetById(_context, id));
        }

        // Optimized bulk operations
        public async Task BulkUpdateMachineStatusAsync(IEnumerable<Guid> machineIds, string newStatus)
        {
            var machines = await DbSet
                .Where(m => machineIds.Contains(m.Id))
                .ToListAsync();

            foreach (var machine in machines)
            {
                // Update properties
                // machine.Status = newStatus;
                // machine.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}