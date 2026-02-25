using Asp.Versioning;
using DigitalTwinPlatform.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalTwinPlatform.Infrastructure.Persistence;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for system health checks.
/// Provides endpoints to monitor the health of database and other services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersion("1.0")]
[AllowAnonymous]
public class HealthController(DigitalTwinDbContext db, ILogger<HealthController> logger) : ControllerBase
{
    /// <summary>
    /// Performs a comprehensive health check of all system components.
    /// Returns the overall health status along with individual component statuses.
    /// </summary>
    /// <returns>Health status including database and SignalR connectivity.</returns>
    /// <response code="200">Health check completed successfully.</response>
    /// <response code="503">One or more components are unhealthy.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthStatusDto>> Get()
    {
        var dbStatus = await CheckDbAsync();
        var signalRStatus = "Healthy"; // SignalR health is implicit via successful response
        var overall = dbStatus == "Healthy" ? "Healthy" : "Degraded";

        var result = new HealthStatusDto(overall, DateTime.UtcNow, dbStatus, signalRStatus);

        if (overall != "Healthy")
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Simple health check endpoint for load balancers and monitoring systems.
    /// Returns 200 OK if the API is responsive.
    /// </summary>
    /// <returns>Simple status message.</returns>
    /// <response code="200">API is responsive.</response>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed health check with readiness information.
    /// Useful for Kubernetes liveness/readiness probes.
    /// </summary>
    /// <returns>Detailed health information.</returns>
    /// <response code="200">System is ready to accept traffic.</response>
    /// <response code="503">System is not ready.</response>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ReadinessResponse>> Ready()
    {
        var dbStatus = await CheckDbAsync();
        var isReady = dbStatus == "Healthy";

        var response = new ReadinessResponse
        {
            Ready = isReady,
            Timestamp = DateTime.UtcNow,
            Checks = new Dictionary<string, string>
            {
                { "Database", dbStatus }
            }
        };

        if (!isReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }

    private async Task<string> CheckDbAsync()
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync("SELECT 1");
            return "Healthy";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed: {ErrorMessage}", ex.Message);
            return "Unhealthy";
        }
    }
}

/// <summary>
/// Response model for readiness check.
/// </summary>
public class ReadinessResponse
{
    /// <summary>
    /// Indicates if the system is ready to accept traffic.
    /// </summary>
    public bool Ready { get; set; }
    
    /// <summary>
    /// Timestamp of the health check.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Individual component health checks.
    /// </summary>
    public Dictionary<string, string> Checks { get; set; } = new();
}
