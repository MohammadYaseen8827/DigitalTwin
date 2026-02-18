using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Search.Commands;

/// <summary>
/// Command for deleting a saved search.
/// </summary>
public class DeleteSavedSearchCommand(Guid id) : IRequest
{
    public Guid Id { get; } = id;
}

/// <summary>
/// Handler for DeleteSavedSearchCommand.
/// </summary>
public class DeleteSavedSearchCommandHandler(
    ISavedSearchRepository savedSearchRepository,
    ILogger<DeleteSavedSearchCommandHandler> logger) : IRequestHandler<DeleteSavedSearchCommand>
{
    public async Task Handle(DeleteSavedSearchCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting saved search with ID: {Id}", command.Id);

        var savedSearch = await savedSearchRepository.GetAsync(command.Id, cancellationToken);
        if (savedSearch == null)
        {
            throw new KeyNotFoundException($"Saved search with ID {command.Id} not found");
        }

        await savedSearchRepository.DeleteAsync(savedSearch, cancellationToken);
        await savedSearchRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully deleted saved search with ID: {Id}", command.Id);
    }
}
