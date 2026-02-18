using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Search.Models;
using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Search.Queries;

/// <summary>
/// Query for performing unified search across all entity types.
/// </summary>
public class SearchQuery(SearchRequestDto request) : IRequest<SearchResultDto>
{
    public SearchRequestDto Request { get; } = request;
}

/// <summary>
/// Handler for SearchQuery.
/// </summary>
public class SearchQueryHandler(
    IMachineRepository machineRepository,
    ITelemetryRepository telemetryRepository,
    IMaintenanceRepository maintenanceRepository,
    IAlertRepository alertRepository,
    IProductionLineRepository productionLineRepository,
    ILogger<SearchQueryHandler> logger) : IRequestHandler<SearchQuery, SearchResultDto>
{
    public async Task<SearchResultDto> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var results = new List<SearchResultItemDto>();
        var searchQuery = request.Request.Query.ToLowerInvariant();

        logger.LogInformation("Performing search for query: {Query}, entity types: {EntityTypes}", 
            searchQuery, string.Join(",", request.Request.EntityTypes ?? ["all"]));

        // Search machines
        if (ShouldSearchEntityType("machines", request.Request))
        {
            var machines = await SearchMachines(searchQuery, request.Request, cancellationToken);
            results.AddRange(machines);
        }

        // Search telemetry data
        if (ShouldSearchEntityType("telemetry", request.Request))
        {
            var telemetry = await SearchTelemetry(searchQuery, request.Request, cancellationToken);
            results.AddRange(telemetry);
        }

        // Search maintenance records
        if (ShouldSearchEntityType("maintenance", request.Request))
        {
            var maintenance = await SearchMaintenance(searchQuery, request.Request, cancellationToken);
            results.AddRange(maintenance);
        }

        // Search alerts
        if (ShouldSearchEntityType("alerts", request.Request))
        {
            var alerts = await SearchAlerts(searchQuery, request.Request, cancellationToken);
            results.AddRange(alerts);
        }

        // Search production lines
        if (ShouldSearchEntityType("production_lines", request.Request))
        {
            var productionLines = await SearchProductionLines(searchQuery, request.Request, cancellationToken);
            results.AddRange(productionLines);
        }

        // Sort results
        results = SortResults(results, request.Request.SortBy);

        // Paginate results
        var totalCount = results.Count;
        var pagedResults = results
            .Skip(request.Request.Page * request.Request.PageSize)
            .Take(request.Request.PageSize)
            .ToList();

        var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

        logger.LogInformation("Search completed: {TotalCount} results in {ExecutionTime}ms", 
            totalCount, executionTime);

        return new SearchResultDto
        {
            Items = pagedResults,
            TotalCount = totalCount,
            Page = request.Request.Page,
            PageSize = request.Request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.Request.PageSize),
            ExecutionTimeMs = executionTime
        };
    }

    private bool ShouldSearchEntityType(string entityType, SearchRequestDto request)
    {
        return request.EntityTypes == null || 
               request.EntityTypes.Count == 0 || 
               request.EntityTypes.Contains(entityType);
    }

    private async Task<List<SearchResultItemDto>> SearchMachines(string query, SearchRequestDto request, CancellationToken ct)
    {
        var machines = await machineRepository.SearchAsync(query, request.StatusFilter, ct);
        
        return machines.Select(machine => new SearchResultItemDto
        {
            Id = machine.Id.ToString(),
            Type = "machine",
            Title = machine.Name.Value,
            Subtitle = $"ID: {machine.Id} | Type: {machine.Type.Value}",
            Status = machine.Status.ToString(),
            CreatedAt = machine.CreatedAt,
            MatchScore = CalculateMatchScore(machine.Name.Value, query) * 0.9,
            Metadata = new Dictionary<string, object>
            {
                ["location"] = machine.Location ?? "",
                ["type"] = machine.Type.Value,
                ["healthStatus"] = machine.HealthStatus?.ToString() ?? ""
            }
        }).ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchTelemetry(string query, SearchRequestDto request, CancellationToken ct)
    {
        var since = ResolveDateRange(request.DateRange);
        var telemetry = await telemetryRepository.SearchAsync(query, since, ct);
        
        // Apply advanced filters
        if (request.AdvancedFilters != null)
        {
            telemetry = ApplyAdvancedTelemetryFilters(telemetry.ToList(), request.AdvancedFilters);
        }

        return telemetry.Select(t => new SearchResultItemDto
        {
            Id = t.Id.ToString(),
            Type = "telemetry",
            Title = $"Telemetry Reading - {GetMachineDisplayName(t.Machine)}",
            Subtitle = $"Temp: {t.Temperature:F1}°C | Vibration: {t.Vibration:F2} mm/s | Pressure: {t.Pressure:F1} PSI",
            Status = GetTelemetryStatus(t),
            CreatedAt = t.Timestamp,
            MatchScore = CalculateMatchScore($"Temperature {t.Temperature} Vibration {t.Vibration} Pressure {t.Pressure}", query) * 0.8,
            Metadata = new Dictionary<string, object>
            {
                ["machineId"] = t.MachineId.ToString(),
                ["temperature"] = t.Temperature ?? 0,
                ["vibration"] = t.Vibration ?? 0,
                ["pressure"] = t.Pressure ?? 0,
                ["rpm"] = t.Rpm ?? 0,
                ["healthScore"] = t.HealthScore ?? 0
            }
        }).ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchMaintenance(string query, SearchRequestDto request, CancellationToken ct)
    {
        var maintenance = await maintenanceRepository.SearchAsync(query, request.StatusFilter, ct);
        
        return maintenance.Select(m => new SearchResultItemDto
        {
            Id = m.Id.ToString(),
            Type = "maintenance",
            Title = $"Maintenance Record - {GetMachineDisplayName(m.Machine)}",
            Subtitle = $"Type: {m.Type} | Scheduled: {m.PlannedDate:yyyy-MM-dd}",
            Status = m.Status.ToString(),
            CreatedAt = m.Date,
            MatchScore = CalculateMatchScore($"{m.Type} {m.Description}", query) * 0.7,
            Metadata = new Dictionary<string, object>
            {
                ["machineId"] = m.MachineId.ToString(),
                ["type"] = m.Type.ToString(),
                ["plannedDate"] = m.PlannedDate,
                ["completedDate"] = m.CompletionDate
            }
        }).ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchAlerts(string query, SearchRequestDto request, CancellationToken ct)
    {
        var alerts = await alertRepository.SearchAsync(query, request.StatusFilter, ct);
        
        return alerts.Select(alert => new SearchResultItemDto
        {
            Id = alert.Id.ToString(),
            Type = "alert",
            Title = $"{alert.Severity} Alert - {GetMachineDisplayName(alert.Machine)}",
            Subtitle = alert.Message,
            Status = alert.IsAcknowledged ? "acknowledged" : "active",
            CreatedAt = alert.CreatedAt,
            MatchScore = CalculateMatchScore($"{alert.Severity} {alert.Message}", query) * 0.85,
            Metadata = new Dictionary<string, object>
            {
                ["machineId"] = alert.MachineId.ToString(),
                ["severity"] = alert.Severity.ToString(),
                ["message"] = alert.Message
            }
        }).ToList();
    }

    private async Task<List<SearchResultItemDto>> SearchProductionLines(string query, SearchRequestDto request, CancellationToken ct)
    {
        var productionLines = await productionLineRepository.SearchAsync(query, ct);
        
        return productionLines.Select(pl => new SearchResultItemDto
        {
            Id = pl.Id.ToString(),
            Type = "production_line",
            Title = pl.Name,
            Subtitle = $"Production Line | Machines: {pl.Machines.Count}",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            MatchScore = CalculateMatchScore(pl.Name, query) * 0.75,
            Metadata = new Dictionary<string, object>
            {
                ["machineCount"] = pl.Machines.Count
            }
        }).ToList();
    }

    private static List<SearchResultItemDto> SortResults(List<SearchResultItemDto> results, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "date_desc" => results.OrderByDescending(r => r.CreatedAt).ToList(),
            "date_asc" => results.OrderBy(r => r.CreatedAt).ToList(),
            "name_asc" => results.OrderBy(r => r.Title).ToList(),
            "name_desc" => results.OrderByDescending(r => r.Title).ToList(),
            "relevance" or _ => results.OrderByDescending(r => r.MatchScore).ToList()
        };
    }

    private static double CalculateMatchScore(string text, string query)
    {
        if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(text))
            return 0;

        var textLower = text.ToLowerInvariant();
        var queryLower = query.ToLowerInvariant();

        // Exact match gets highest score
        if (textLower.Contains(queryLower))
            return 1.0;

        // Partial matches based on word boundaries
        var queryWords = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var textWords = textLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var matches = 0;
        foreach (var queryWord in queryWords)
        {
            if (textWords.Any(textWord => textWord.Contains(queryWord)))
                matches++;
        }

        return queryWords.Length > 0 ? (double)matches / queryWords.Length : 0;
    }

    private static DateTime? ResolveDateRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
            return null;

        return range.ToLowerInvariant() switch
        {
            "1h" => DateTime.UtcNow.AddHours(-1),
            "6h" => DateTime.UtcNow.AddHours(-6),
            "12h" => DateTime.UtcNow.AddHours(-12),
            "24h" => DateTime.UtcNow.AddHours(-24),
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            "1y" => DateTime.UtcNow.AddYears(-1),
            _ => null
        };
    }

    private static List<TelemetryData> ApplyAdvancedTelemetryFilters(List<TelemetryData> telemetry, AdvancedFiltersDto filters)
    {
        return telemetry.Where(t =>
            (!filters.MinTemperature.HasValue || t.Temperature >= filters.MinTemperature.Value) &&
            (!filters.MaxTemperature.HasValue || t.Temperature <= filters.MaxTemperature.Value) &&
            (!filters.MinPressure.HasValue || t.Pressure >= filters.MinPressure.Value) &&
            (!filters.MaxPressure.HasValue || t.Pressure <= filters.MaxPressure.Value)
        ).ToList();
    }

    private static string GetMachineDisplayName(Machine? machine)
    {
        return machine?.Name.Value ?? "Unknown Machine";
    }

    private static string GetTelemetryStatus(TelemetryData telemetry)
    {
        if (telemetry.HealthScore.HasValue)
        {
            return telemetry.HealthScore.Value switch
            {
                >= 80 => "good",
                >= 60 => "warning",
                _ => "critical"
            };
        }
        return "unknown";
    }
}
