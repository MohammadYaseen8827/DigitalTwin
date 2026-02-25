using Asp.Versioning;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Search.Commands;
using DigitalTwinPlatform.Application.Search.Models;
using DigitalTwinPlatform.Application.Search.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for searching across machines, telemetry, maintenance records, and alerts.
/// Provides unified search functionality with advanced filtering and sorting capabilities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class SearchController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Performs a unified search across all entity types with advanced filtering.
    /// </summary>
    /// <param name="request">The search request containing query, filters, and pagination.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Paginated search results with relevance scoring.</returns>
    /// <response code="200">Returns search results successfully.</response>
    /// <response code="400">Invalid search request parameters.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromBody] SearchRequestDto request,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new SearchQuery(request), ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Internal server error during search" });
        }
    }

    /// <summary>
    /// Gets search suggestions for autocomplete functionality.
    /// </summary>
    /// <param name="query">Partial search query.</param>
    /// <param name="entityType">Optional entity type filter.</param>
    /// <param name="limit">Maximum number of suggestions to return (default: 10).</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of search suggestions.</returns>
    /// <response code="200">Returns search suggestions successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(IEnumerable<SearchSuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SearchSuggestionDto>>> GetSuggestions(
        [FromQuery] string query,
        [FromQuery] string? entityType = null,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        try
        {
            var result = await mediator.Send(new GetSearchSuggestionsQuery(query, entityType, limit), ct);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Internal server error while getting suggestions" });
        }
    }

    /// <summary>
    /// Gets saved searches for the current user.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of saved searches.</returns>
    /// <response code="200">Returns saved searches successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("saved")]
    [ProducesResponseType(typeof(IEnumerable<SavedSearchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SavedSearchDto>>> GetSavedSearches(CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetSavedSearchesQuery(), ct);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Internal server error while getting saved searches" });
        }
    }

    /// <summary>
    /// Saves a search query for future use.
    /// </summary>
    /// <param name="request">The save search request.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The saved search details.</returns>
    /// <response code="201">Search saved successfully.</response>
    /// <response code="400">Invalid save search request.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("saved")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SavedSearchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SavedSearchDto>> SaveSearch(
        [FromBody] SaveSearchRequestDto request,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new SaveSearchCommand(request), ct);
            return CreatedAtAction(nameof(GetSavedSearches), new { }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Internal server error while saving search" });
        }
    }

    /// <summary>
    /// Deletes a saved search.
    /// </summary>
    /// <param name="id">The ID of the saved search to delete.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <response code="204">Search deleted successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Saved search not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("saved/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSavedSearch(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DeleteSavedSearchCommand(id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { Message = "Saved search not found" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Message = "Internal server error while deleting saved search" });
        }
    }
}
