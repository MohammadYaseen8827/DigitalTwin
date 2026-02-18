using DigitalTwinPlatform.API.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriftController(IDataDriftService driftService) : ControllerBase
{
    /// <summary>
    /// Gets current drift status for all monitored models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of model names and drift status</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(Dictionary<string, DriftDetectionResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, DriftDetectionResult>>> GetCurrentDriftStatus(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await driftService.GetCurrentDriftStatusAsync(cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve drift status", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets drift history for a specific model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="hours">Hours of history to retrieve (default: 24)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical drift metrics</returns>
    [HttpGet("history/{modelName}")]
    [ProducesResponseType(typeof(IEnumerable<DriftMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DriftMetrics>>> GetDriftHistory(
        string modelName,
        [FromQuery] int hours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var endTime = DateTimeOffset.UtcNow;
            var startTime = endTime.AddHours(-hours);

            var history = await driftService.GetDriftHistoryAsync(
                modelName, startTime, endTime, cancellationToken);

            if (!history.Any())
            {
                return NotFound(new { error = $"No drift history found for model '{modelName}'" });
            }

            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve drift history", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets drift thresholds configuration for a model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current drift thresholds</returns>
    [HttpGet("thresholds/{modelName}")]
    [ProducesResponseType(typeof(DriftThresholds), StatusCodes.Status200OK)]
    public async Task<ActionResult<DriftThresholds>> GetThresholds(
        string modelName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var thresholds = await driftService.GetThresholdsAsync(modelName, cancellationToken);
            return Ok(thresholds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve thresholds", details = ex.Message });
        }
    }

    /// <summary>
    /// Configures drift detection thresholds for a model
    /// </summary>
    /// <param name="modelName">Model identifier</param>
    /// <param name="thresholds">New thresholds configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success confirmation</returns>
    [HttpPut("thresholds/{modelName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConfigureThresholds(
        string modelName,
        [FromBody] DriftThresholds thresholds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate thresholds
            if (thresholds.FeatureDriftThreshold <= 0 || thresholds.FeatureDriftThreshold > 1)
            {
                return BadRequest(new { error = "Feature drift threshold must be between 0 and 1" });
            }

            if (thresholds.PredictionDriftThreshold <= 0 || thresholds.PredictionDriftThreshold > 1)
            {
                return BadRequest(new { error = "Prediction drift threshold must be between 0 and 1" });
            }

            await driftService.ConfigureThresholdsAsync(modelName, thresholds, cancellationToken);
            
            return Ok(new { message = $"Thresholds configured successfully for model '{modelName}'" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to configure thresholds", details = ex.Message });
        }
    }

    /// <summary>
    /// Generates drift report for specified period
    /// </summary>
    /// <param name="modelName">Model identifier (optional - if null, all models)</param>
    /// <param name="days">Period in days (default: 7)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Drift report</returns>
    [HttpGet("report")]
    [ProducesResponseType(typeof(DriftReport), StatusCodes.Status200OK)]
    public async Task<ActionResult<DriftReport>> GenerateReport(
        [FromQuery] string? modelName = null,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var period = TimeSpan.FromDays(days);
            var report = await driftService.GenerateDriftReportAsync(
                modelName, period, cancellationToken);

            return Ok(report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate drift report", details = ex.Message });
        }
    }

    /// <summary>
    /// Manually triggers drift detection for a model
    /// </summary>
    /// <param name="request">Drift detection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Drift detection result</returns>
    [HttpPost("detect")]
    [ProducesResponseType(typeof(DriftDetectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DriftDetectionResult>> DetectDrift(
        [FromBody] DriftDetectionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ReferenceData == null || request.CurrentData == null)
            {
                return BadRequest(new { error = "Both reference and current data are required" });
            }

            if (!request.ReferenceData.Any() || !request.CurrentData.Any())
            {
                return BadRequest(new { error = "Data arrays cannot be empty" });
            }

            var result = await driftService.DetectDriftAsync(
                request.ReferenceData,
                request.CurrentData,
                request.Thresholds ?? new DriftThresholds(),
                request.Method,
                cancellationToken);

            // Check if alert should be triggered
            var shouldAlert = await driftService.ShouldTriggerAlertAsync(result, cancellationToken);
            if (shouldAlert)
            {
                // In a real implementation, you would send alerts here
                // For now, we'll just log it
                Console.WriteLine($"DRIFT ALERT: {result.Recommendation}");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to detect drift", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets drift monitoring configuration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Monitoring configuration</returns>
    [HttpGet("monitoring/config")]
    [ProducesResponseType(typeof(DriftMonitoringConfig), StatusCodes.Status200OK)]
    public ActionResult<DriftMonitoringConfig> GetMonitoringConfig(
        CancellationToken cancellationToken = default)
    {
        // Return current monitoring configuration
        var config = new DriftMonitoringConfig
        {
            Enabled = true,
            MonitoringIntervalMinutes = 60,
            AlertEnabled = true,
            AlertCooldownMinutes = 60
        };

        return Ok(config);
    }
}

/// <summary>
/// Request model for manual drift detection
/// </summary>
public class DriftDetectionRequest
{
    public Dictionary<string, double[]> ReferenceData { get; set; } = new();
    public Dictionary<string, double[]> CurrentData { get; set; } = new();
    public DriftThresholds? Thresholds { get; set; }
    public DriftDetectionMethod Method { get; set; } = DriftDetectionMethod.KS_Test;
}

/// <summary>
/// Drift monitoring configuration
/// </summary>
public class DriftMonitoringConfig
{
    public bool Enabled { get; set; }
    public int MonitoringIntervalMinutes { get; set; }
    public bool AlertEnabled { get; set; }
    public int AlertCooldownMinutes { get; set; }
    public List<string> MonitoredModels { get; set; } = new();
}