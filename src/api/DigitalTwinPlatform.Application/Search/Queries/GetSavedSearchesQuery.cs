using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Search.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Search.Queries;

/// <summary>
/// Query for getting saved searches for the current user.
/// </summary>
public class GetSavedSearchesQuery : IRequest<IEnumerable<SavedSearchDto>>
{
}

/// <summary>
/// Handler for GetSavedSearchesQuery.
/// </summary>
public class GetSavedSearchesQueryHandler(
    ISavedSearchRepository savedSearchRepository,
    ILogger<GetSavedSearchesQueryHandler> logger) : IRequestHandler<GetSavedSearchesQuery, IEnumerable<SavedSearchDto>>
{
    public async Task<IEnumerable<SavedSearchDto>> Handle(GetSavedSearchesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting saved searches for user");

        // For now, get all saved searches (in a real app, this would be filtered by user)
        var savedSearches = await savedSearchRepository.GetAllAsync(null, cancellationToken);

        return savedSearches.Select(search => new SavedSearchDto
        {
            Id = search.Id.ToString(),
            Name = search.Name,
            Query = search.Query,
            EntityTypes = search.EntityTypes,
            CreatedAt = search.CreatedAt,
            LastUsed = search.LastUsed
        }).OrderByDescending(s => s.LastUsed ?? s.CreatedAt);
    }
}
