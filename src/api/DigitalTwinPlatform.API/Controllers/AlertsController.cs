using DigitalTwinPlatform.API.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Asp.Versioning;
using DigitalTwinPlatform.Application.Alerts.Models;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for managing system alerts.
/// Provides endpoints for viewing, acknowledging, and resolving alerts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class AlertsController(IAlertService alertService) : ControllerBase
{
    /// <summary>
    /// Retrieves all active alerts, optionally filtered by machine.
    /// Returns alerts that have not been resolved or acknowledged.
    /// </summary>
    /// <param name="machineId">Optional machine ID filter to get alerts for specific machine.</param>
    /// <returns>List of active alerts sorted by severity and creation time.</returns>
    /// <response code="200">Returns list of alerts successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetActiveAlerts([FromQuery] Guid? machineId)
    {
        var alerts = await alertService.GetActiveAlertsAsync(machineId);
        return Ok(alerts.Select(AlertDto.FromEntity));
    }

    /// <summary>
    /// Retrieves a specific alert by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the alert.</param>
    /// <returns>The full alert details including acknowledgment status.</returns>
    /// <response code="200">Returns alert details successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlertDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AlertDto>> GetAlert(Guid id)
    {
        var alert = await alertService.GetAlertByIdAsync(id);
        if (alert == null)
        {
            return NotFound(new { Message = $"Alert with ID {id} not found" });
        }
        return Ok(AlertDto.FromEntity(alert));
    }

    /// <summary>
    /// Acknowledges an alert, marking it as seen by the current user.
    /// </summary>
    /// <param name="id">The unique identifier of the alert to acknowledge.</param>
    /// <returns>No content on successful acknowledgment.</returns>
    /// <response code="204">Alert acknowledged successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await alertService.AcknowledgeAlertAsync(id, userId);
        return NoContent();
    }

    /// <summary>
    /// Resolves (deletes) an alert from the system.
    /// Used when alerts have been addressed and are no longer relevant.
    /// </summary>
    /// <param name="id">The unique identifier of the alert to resolve.</param>
    /// <returns>No content on successful resolution.</returns>
    /// <response code="204">Alert resolved successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Alert not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await alertService.ResolveAlertAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Retrieves all alerts including acknowledged ones for a machine.
    /// </summary>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <returns>List of all alerts for the specified machine.</returns>
    /// <response code="200">Returns list of all alerts successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AlertDto>>> GetAllAlerts([FromQuery] Guid? machineId)
    {
        var alerts = await alertService.GetAllAlertsAsync(machineId);
        return Ok(alerts.Select(AlertDto.FromEntity));
    }

    /// <summary>
    /// Gets alert statistics for dashboard display.
    /// </summary>
    /// <returns>Counts of alerts by severity and status.</returns>
    /// <response code="200">Returns alert statistics successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AlertStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AlertStatsDto>> GetStats()
    {
        var stats = await alertService.GetAlertStatsAsync();
        return Ok(new AlertStatsDto(
            stats.TotalActive,
            stats.TotalAcknowledged,
            stats.TotalResolved,
            stats.CriticalCount,
            stats.WarningCount,
            stats.InfoCount
        ));
    }

    /// <summary>
    /// Searches alerts based on a text query.
    /// </summary>
    /// <param name="query">Text query to search in alert fields.</param>
    /// <param name="status">Optional status filter (active, acknowledged, resolved).</param>
    /// <param name="severity">Optional severity filter (info, warning, critical, error).</param>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of alerts matching the search criteria.</returns>
    /// <response code="200">Returns search results successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AlertDto>>> Search(
        [FromQuery] string query,
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        [FromQuery] Guid? machineId = null,
        CancellationToken ct = default)
    {
        var alerts = await alertService.SearchAlertsAsync(query, status, severity, machineId, ct);
        return Ok(alerts.Select(AlertDto.FromEntity));
    }
}

/// <summary>
/// DTO for alert statistics response.
/// </summary>
public record AlertStatsDto(
    int TotalActive,
    int TotalAcknowledged,
    int TotalResolved,
    int CriticalCount,
    int WarningCount,
    int InfoCount
);
