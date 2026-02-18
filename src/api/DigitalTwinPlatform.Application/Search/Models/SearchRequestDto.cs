namespace DigitalTwinPlatform.Application.Search.Models;

/// <summary>
/// Request DTO for unified search across all entity types.
/// </summary>
public class SearchRequestDto
{
    /// <summary>
    /// The search query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Entity types to search (machines, telemetry, maintenance, alerts, production_lines).
    /// If null or empty, searches across all entity types.
    /// </summary>
    public List<string>? EntityTypes { get; set; }

    /// <summary>
    /// Date range filter (1h, 6h, 12h, 24h, 7d, 30d, 1y).
    /// </summary>
    public string? DateRange { get; set; }

    /// <summary>
    /// Status filter for entities that support status.
    /// </summary>
    public string? StatusFilter { get; set; }

    /// <summary>
    /// Advanced filters for specific fields.
    /// </summary>
    public AdvancedFiltersDto? AdvancedFilters { get; set; }

    /// <summary>
    /// Sort order (relevance, date_desc, date_asc, name_asc, name_desc).
    /// </summary>
    public string SortBy { get; set; } = "relevance";

    /// <summary>
    /// Page number for pagination (0-based).
    /// </summary>
    public int Page { get; set; } = 0;

    /// <summary>
    /// Number of results per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Advanced filters for specific entity fields.
/// </summary>
public class AdvancedFiltersDto
{
    /// <summary>
    /// Minimum temperature filter for telemetry data.
    /// </summary>
    public double? MinTemperature { get; set; }

    /// <summary>
    /// Maximum temperature filter for telemetry data.
    /// </summary>
    public double? MaxTemperature { get; set; }

    /// <summary>
    /// Minimum pressure filter for telemetry data.
    /// </summary>
    public double? MinPressure { get; set; }

    /// <summary>
    /// Maximum pressure filter for telemetry data.
    /// </summary>
    public double? MaxPressure { get; set; }

    /// <summary>
    /// Location filter for machines.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Manufacturer filter for machines.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model filter for machines.
    /// </summary>
    public string? Model { get; set; }
}

/// <summary>
/// Search result containing paginated results with metadata.
/// </summary>
public class SearchResultDto
{
    /// <summary>
    /// List of search result items.
    /// </summary>
    public List<SearchResultItemDto> Items { get; set; } = [];

    /// <summary>
    /// Total number of results matching the query.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of results per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Query execution time in milliseconds.
    /// </summary>
    public double ExecutionTimeMs { get; set; }
}

/// <summary>
/// Individual search result item.
/// </summary>
public class SearchResultItemDto
{
    /// <summary>
    /// Unique identifier of the result item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Entity type (machine, telemetry, maintenance, alert, production_line).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Title of the result item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Subtitle or description of the result item.
    /// </summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Status of the entity (if applicable).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Creation timestamp of the entity.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Relevance score (0.0 to 1.0).
    /// </summary>
    public double MatchScore { get; set; }

    /// <summary>
    /// Additional metadata for the result item.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Search suggestion for autocomplete.
/// </summary>
public class SearchSuggestionDto
{
    /// <summary>
    /// Suggestion text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Entity type the suggestion applies to.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Estimated relevance score.
    /// </summary>
    public double Score { get; set; }
}

/// <summary>
/// Saved search for quick access.
/// </summary>
public class SavedSearchDto
{
    /// <summary>
    /// Unique identifier of the saved search.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the saved search.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Search query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Entity types filter.
    /// </summary>
    public List<string>? EntityTypes { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last used timestamp.
    /// </summary>
    public DateTime? LastUsed { get; set; }
}

/// <summary>
/// Request DTO for saving a search.
/// </summary>
public class SaveSearchRequestDto
{
    /// <summary>
    /// Display name for the saved search.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Search query string.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Entity types filter.
    /// </summary>
    public List<string>? EntityTypes { get; set; }
}
