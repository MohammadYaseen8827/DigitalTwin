using DigitalTwinPlatform.API.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceMetricsController : ControllerBase
{
    private readonly IPerformanceMetricsCollector _metricsCollector;
    private readonly ILogger<PerformanceMetricsController> _logger;

    public PerformanceMetricsController(
        IPerformanceMetricsCollector metricsCollector,
        ILogger<PerformanceMetricsController> logger)
    {
        _metricsCollector = metricsCollector;
        _logger = logger;
    }

    /// <summary>
    /// Get performance metrics with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PerformanceMetrics>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PerformanceMetrics>>> GetMetrics(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] string? operationName = null)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-1);
        var end = endTime ?? DateTime.UtcNow;
        
        var metrics = await _metricsCollector.GetMetricsAsync(start, end, operationName);
        return Ok(metrics);
    }

    /// <summary>
    /// Get aggregated performance statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(PerformanceStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<PerformanceStatistics>> GetStatistics(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] string? operationName = null)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-1);
        var end = endTime ?? DateTime.UtcNow;
        
        var statistics = await _metricsCollector.GetStatisticsAsync(start, end, operationName);
        return Ok(statistics);
    }

    /// <summary>
    /// Get threshold violations
    /// </summary>
    [HttpGet("threshold-violations")]
    [ProducesResponseType(typeof(List<PerformanceMetrics>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PerformanceMetrics>>> GetThresholdViolations(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var start = startTime ?? DateTime.UtcNow.AddHours(-1);
        var end = endTime ?? DateTime.UtcNow;
        
        var allMetrics = await _metricsCollector.GetMetricsAsync(start, end);
        var violations = allMetrics.Where(m => m.ExceededThreshold).ToList();
        
        return Ok(violations);
    }
}
