using Asp.Versioning;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Analytics.Advanced.Models;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Analytics.Advanced;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for managing RUL (Remaining Useful Life) predictions and health classification.
/// Provides endpoints for predictions, health classification, and model training.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class PredictionsController(
    IMediator mediator,
    IPredictionService predictionService,
    IAdvancedPredictiveService advancedPredictiveService,
    ILogger<PredictionsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets RUL prediction for a specific machine with full details.
    /// Returns prediction, confidence intervals, and contributing factors.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>RUL prediction result with confidence and contributing factors.</returns>
    [HttpPost("rul/{machineId}")]
    [ProducesResponseType(typeof(RulPredictionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RulPredictionResult>> GetRulPrediction(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting RUL prediction for machine {MachineId}", machineId);

            var result = await predictionService.GetRulPredictionWithDetailsAsync(machineId, ct);

            logger.LogInformation(
                "RUL prediction for {MachineId}: {RUL} {Unit} (confidence: {Confidence:P2})",
                machineId, result.Rul, result.RulUnit, result.Confidence);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Insufficient telemetry data for machine {MachineId}", machineId);
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting RUL prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to get RUL prediction due to an internal error" });
        }
    }

    /// <summary>
    /// Gets health classification for a specific machine with full details.
    /// Returns health status, probability, and contributing factors.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Health classification result with recommendations.</returns>
    [HttpGet("health/{machineId}")]
    [ProducesResponseType(typeof(HealthClassificationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HealthClassificationResult>> GetHealthClassification(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting health classification for machine {MachineId}", machineId);

            var result = await predictionService.GetHealthClassificationWithDetailsAsync(machineId, ct);

            logger.LogInformation(
                "Health classification for {MachineId}: {Status} (probability: {Probability:P2})",
                machineId, result.HealthStatus, result.HealthProbability);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Insufficient telemetry data for machine {MachineId}", machineId);
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting health classification for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to classify health due to an internal error" });
        }
    }

    /// <summary>
    /// Gets quick RUL summary for a machine (lightweight endpoint).
    /// </summary>
    [HttpGet("rul/{machineId}/summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetRulSummary(Guid machineId, CancellationToken ct)
    {
        try
        {
            var summary = await predictionService.GetDetailedRulSummaryAsync(machineId, ct);
            return Ok(summary);
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new { Error = "Insufficient telemetry data" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting RUL summary for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to get RUL summary due to an internal error" });
        }
    }

    /// <summary>
    /// Triggers model retraining for RUL and health models.
    /// </summary>
    [HttpPost("train")]
    [ProducesResponseType(typeof(TrainingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TrainingResultDto>> TrainModels(
        [FromBody] TrainModelRequestDto? request,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Starting model training. Force retrain: {ForceRetrain}",
                request?.ForceRetrain ?? false);

            var result = await mediator.Send(new TrainModelCommand
            {
                ForceRetrain = request?.ForceRetrain ?? false,
                ModelType = request?.ModelType ?? "all"
            }, ct);

            logger.LogInformation("Model training completed. Samples used: {SampleCount}, R2: {R2:F4}",
                result.SamplesUsed, result.R2Score);

            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            logger.LogWarning("Training validation failed");
            return BadRequest(new { Error = "Model training validation failed. Please check training data density." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Model training failed");
            return StatusCode(500, new { Error = "Failed to train models due to an internal error" });
        }
    }

    /// <summary>
    /// Gets model status and information.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ModelStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ModelStatusDto>> GetModelStatus()
    {
        var status = await predictionService.GetModelStatus();
        return Ok(status);
    }

    /// <summary>
    /// Requests a new prediction for a machine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PredictionDto>> RequestPrediction([FromBody] PredictionRequestDto request, CancellationToken ct)
    {
        try
        {
            var result = await predictionService.PredictAsync(request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting prediction for machine {MachineId}", request.MachineId);
            return StatusCode(500, new { Error = "Failed to request prediction due to an internal error" });
        }
    }

    /// <summary>
    /// Gets list of predictions for all machines or a specific machine.
    /// </summary>
    [HttpGet]
    [HttpGet("{machineId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PredictionDto>>> GetPredictions(
        Guid? machineId = null,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Getting predictions list for machine {MachineId}", machineId);

            if (machineId.HasValue)
            {
                var history = await predictionService.GetPredictionHistoryAsync(machineId.Value, limit, ct);
                return Ok(history);
            }
            else
            {
                var search = await predictionService.SearchPredictionsAsync(string.Empty, null, ct);
                return Ok(search.Take(limit));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting predictions list");
            return StatusCode(500, new { Error = "Failed to retrieve predictions due to an internal error" });
        }
    }

    /// <summary>
    /// Gets anomaly prediction for a specific machine.
    /// </summary>
    [HttpGet("anomaly/{machineId}")]
    [ProducesResponseType(typeof(AnomalyDetectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AnomalyDetectionResult>> GetAnomalyPrediction(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting anomaly prediction for machine {MachineId}", machineId);

            var result = await advancedPredictiveService.DetectAnomaliesAsync(machineId, null, null, ct);

            logger.LogInformation(
                "Anomaly detection for {MachineId}: {TotalAnomalies} anomalies (score: {RiskScore:P2})",
                machineId, result.TotalAnomalies, result.OverallRiskScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting anomaly prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to detect anomalies due to an internal error" });
        }
    }

    /// <summary>
    /// Searches prediction data across all machines based on a text query.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<RulPredictionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RulPredictionResult>>> Search(
        [FromQuery] string query,
        [FromQuery] Guid? machineId = null,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Searching predictions with query: {Query} for machine: {MachineId}", query, machineId);

            // Use the prediction service to search predictions
            var predictionDtos = await predictionService.SearchPredictionsAsync(query, machineId, ct);

            // Convert PredictionDto to RulPredictionResult
            var results = predictionDtos.Select(dto => new RulPredictionResult
            {
                MachineId = dto.MachineId.ToString(),
                Rul = dto.RemainingUsefulLifeDays,
                RulUnit = "days",
                Confidence = 0.8, // Default confidence since PredictionDto doesn't have it
                LowerBound = dto.RulLowerBound,
                UpperBound = dto.RulUpperBound,
                PredictionTime = dto.CreatedAt,
                ModelVersion = dto.ModelVersion
            }).ToList();

            return Ok(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching predictions");
            return StatusCode(500, new { Error = "Failed to search predictions due to an internal error" });
        }
    }

}
