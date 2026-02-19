using Asp.Versioning;
using DigitalTwinPlatform.Application.Telemetry.Commands;
using DigitalTwinPlatform.Application.Telemetry.Models;
using DigitalTwinPlatform.Application.Telemetry.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for managing machine telemetry data.
/// Handles real-time sensor data ingestion and retrieval for predictive maintenance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class TelemetryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Ingests new telemetry data into the system.
    /// Accepts sensor readings from machines for processing and storage.
    /// </summary>
    /// <param name="payload">The telemetry data containing machine ID, data type, and sensor readings.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Accepted status on successful ingestion.</returns>
    /// <example>
    /// {
    ///   "machineId": "123e4567-e89b-12d3-a456-426614174000",
    ///   "dataType": "sensor_reading",
    ///   "data": {
    ///     "temperature": 72.5,
    ///     "vibration": 0.023,
    ///     "pressure": 101.3,
    ///     "rpm": 1450
    ///   },
    ///   "timestamp": "2024-01-15T10:30:00Z"
    /// }
    /// </example>
    /// <response code="202">Telemetry data accepted for processing.</response>
    /// <response code="400">Invalid telemetry data format.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error during processing.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Ingest([FromBody] TelemetryIngestDto payload, CancellationToken ct)
    {
        await mediator.Send(new IngestTelemetryCommand(payload), ct);
        return Accepted();
    }

    private static DateTime? ResolveSince(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return null;
        }

        return range.ToLowerInvariant() switch
        {
            "1h" => DateTime.UtcNow.AddHours(-1),
            "6h" => DateTime.UtcNow.AddHours(-6),
            "12h" => DateTime.UtcNow.AddHours(-12),
            "24h" => DateTime.UtcNow.AddHours(-24),
            _ => null
        };
    }

    /// <summary>
    /// Retrieves telemetry data for a specific machine within a time range.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="range">Optional time range filter (1h, 6h, 12h, 24h).</param>
    /// <param name="take">Maximum number of records to return (default: 500).</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of telemetry readings for the specified machine.</returns>
    /// <response code="200">Returns telemetry data successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{machineId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TelemetryDto>>> GetForMachine(
        Guid machineId,
        [FromQuery] string? range = null,
        [FromQuery] int take = 500,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetTelemetryForMachineQuery(machineId, ResolveSince(range), take), ct));

    /// <summary>
    /// Retrieves the most recent telemetry data across all machines or a specific machine.
    /// </summary>
    /// <param name="range">Optional time range filter (1h, 6h, 12h, 24h).</param>
    /// <param name="machineId">Optional machine ID filter to get recent data for specific machine.</param>
    /// <param name="limit">Maximum number of records to return (default: 100).</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of most recent telemetry readings.</returns>
    /// <response code="200">Returns recent telemetry data successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TelemetryDto>>> GetRecent(
        [FromQuery] string? range = null,
        [FromQuery] Guid? machineId = null,
        [FromQuery] int limit = 100,
        CancellationToken ct = default)
        => Ok(await mediator.Send(new GetRecentTelemetryQuery(ResolveSince(range), machineId, limit), ct));

    /// <summary>
    /// Retrieves the latest telemetry reading for a specific machine.
    /// Useful for dashboard status indicators and real-time monitoring.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The most recent telemetry reading for the machine.</returns>
    /// <response code="200">Returns latest telemetry successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found or no telemetry data available.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{machineId:guid}/latest")]
    [ProducesResponseType(typeof(TelemetryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TelemetryDto>> GetLatest(
        Guid machineId,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRecentTelemetryQuery(null, machineId, 1), ct);
        return result.FirstOrDefault() is { } latest 
            ? Ok(latest) 
            : NotFound(new { Message = $"No telemetry data found for machine {machineId}" });
    }

    /// <summary>
    /// Retrieves the latest flattened telemetry metrics for a specific machine.
    /// Returns a flat object with temperature, vibration, pressure, etc. for direct consumption by the frontend.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The most recent flattened telemetry metrics for the machine.</returns>
    /// <response code="200">Returns latest telemetry metrics successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found or no telemetry data available.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{machineId:guid}/latest/metrics")]
    [ProducesResponseType(typeof(TelemetryMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TelemetryMetricsDto>> GetLatestMetrics(
        Guid machineId,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRecentTelemetryQuery(null, machineId, 1), ct);
        var latest = result.FirstOrDefault();
        
        if (latest is null)
        {
            return NotFound(new { Message = $"No telemetry data found for machine {machineId}" });
        }
        
        return Ok(latest.ToMetrics());
    }

    /// <summary>
    /// Searches telemetry data across all machines based on a text query.
    /// </summary>
    /// <param name="query">Text query to search in telemetry data fields.</param>
    /// <param name="range">Optional time range filter (1h, 6h, 12h, 24h).</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of telemetry readings matching the search criteria.</returns>
    /// <response code="200">Returns search results successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<TelemetryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TelemetryDto>>> Search(
        [FromQuery] string query,
        [FromQuery] string? range = null,
        CancellationToken ct = default)
    {
        var since = ResolveSince(range);
        var result = await mediator.Send(new GetTelemetrySearchQuery(query, since), ct);
        return Ok(result);
    }
}
