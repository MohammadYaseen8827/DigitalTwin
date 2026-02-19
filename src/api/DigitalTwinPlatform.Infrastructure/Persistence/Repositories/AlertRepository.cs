using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalTwinPlatform.Infrastructure.Persistence.Repositories;

public class AlertRepository(DigitalTwinDbContext context) 
    : Repository<Alert>(context), IAlertRepository
{
    private readonly DigitalTwinDbContext _context = context;

    public async Task<IEnumerable<Alert>> SearchAsync(string query, string? statusFilter = null, string? severityFilter = null, CancellationToken ct = default)
    {
        IQueryable<Alert> queryable = _context.Alerts;

        // Apply status filter if provided
        if (!string.IsNullOrEmpty(statusFilter))
        {
            queryable = queryable.Where(a => a.Status.ToLower().Contains(statusFilter.ToLower()));
        }

        // Apply severity filter if provided
        if (!string.IsNullOrEmpty(severityFilter))
        {
            if (Enum.TryParse<AlertSeverity>(severityFilter, true, out var severity))
            {
                queryable = queryable.Where(a => a.Severity == severity);
            }
        }

        // Apply text search across relevant fields if query is provided
        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLowerInvariant();
            queryable = queryable.Where(a => 
                a.Message.ToLower().Contains(normalizedQuery) ||
                a.Title.ToLower().Contains(normalizedQuery) ||
                a.Description.ToLower().Contains(normalizedQuery) ||
                a.Category != null && a.Category.ToLower().Contains(normalizedQuery) ||
                a.RecommendedAction != null && a.RecommendedAction.ToLower().Contains(normalizedQuery) ||
                a.MachineId.ToString().Contains(normalizedQuery)
            );
        }

        // Order by creation date descending and return results
        return await queryable
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }
}