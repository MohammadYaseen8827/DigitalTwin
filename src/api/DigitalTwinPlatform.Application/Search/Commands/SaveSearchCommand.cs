using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Search.Models;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Search.Commands;

/// <summary>
/// Command for saving a search query.
/// </summary>
public class SaveSearchCommand(SaveSearchRequestDto request) : IRequest<SavedSearchDto>
{
    public SaveSearchRequestDto Request { get; } = request;
}

/// <summary>
/// Handler for SaveSearchCommand.
/// </summary>
public class SaveSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository,
    ILogger<SaveSearchCommandHandler> logger) : IRequestHandler<SaveSearchCommand, SavedSearchDto>
{
    public async Task<SavedSearchDto> Handle(SaveSearchCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        logger.LogInformation("Saving search: {Name} with query: {Query}", request.Name, request.Query);

        var savedSearch = new SavedSearch
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Query = request.Query,
            EntityTypes = request.EntityTypes ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            LastUsed = null
        };

        await savedSearchRepository.AddAsync(savedSearch, cancellationToken);
        await savedSearchRepository.SaveChangesAsync(cancellationToken);

        return new SavedSearchDto
        {
            Id = savedSearch.Id.ToString(),
            Name = savedSearch.Name,
            Query = savedSearch.Query,
            EntityTypes = savedSearch.EntityTypes,
            CreatedAt = savedSearch.CreatedAt,
            LastUsed = savedSearch.LastUsed
        };
    }
}
