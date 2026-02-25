using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Search.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Search.Queries;

/// <summary>
/// Query for getting search suggestions for autocomplete functionality.
/// </summary>
public class GetSearchSuggestionsQuery(string query, string? entityType = null, int limit = 10) : IRequest<IEnumerable<SearchSuggestionDto>>
{
    public string Query { get; } = query;
    public string? EntityType { get; } = entityType;
    public int Limit { get; } = limit;
}

/// <summary>
/// Handler for GetSearchSuggestionsQuery.
/// </summary>
public class GetSearchSuggestionsQueryHandler(
    IMachineRepository machineRepository,
    ITelemetryRepository _telemetryRepository,
    ILogger<GetSearchSuggestionsQueryHandler> logger) : IRequestHandler<GetSearchSuggestionsQuery, IEnumerable<SearchSuggestionDto>>
{
    public async Task<IEnumerable<SearchSuggestionDto>> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        _ = _telemetryRepository; // Reserved for future telemetry-based suggestions
        var suggestions = new List<SearchSuggestionDto>();
        var queryLower = request.Query.ToLowerInvariant();

        logger.LogInformation("Getting search suggestions for query: {Query}, entity type: {EntityType}", 
            request.Query, request.EntityType);

        // Get machine name suggestions
        if (string.IsNullOrEmpty(request.EntityType) || request.EntityType == "machines")
        {
            var machines = await machineRepository.SearchAsync(queryLower, null, cancellationToken);
            suggestions.AddRange(machines.Take(3).Select(machine => new SearchSuggestionDto
            {
                Text = machine.Name.Value,
                EntityType = "machine",
                Score = 1.0
            }));
        }

        // Add common search patterns
        if (queryLower.Length < 3)
        {
            suggestions.AddRange(new[]
            {
                new SearchSuggestionDto { Text = "temperature", EntityType = "telemetry", Score = 0.8 },
                new SearchSuggestionDto { Text = "vibration", EntityType = "telemetry", Score = 0.8 },
                new SearchSuggestionDto { Text = "pressure", EntityType = "telemetry", Score = 0.8 },
                new SearchSuggestionDto { Text = "maintenance", EntityType = "maintenance", Score = 0.7 },
                new SearchSuggestionDto { Text = "alert", EntityType = "alerts", Score = 0.7 }
            });
        }

        return suggestions
            .OrderByDescending(s => s.Score)
            .Take(request.Limit)
            .ToList();
    }
}
