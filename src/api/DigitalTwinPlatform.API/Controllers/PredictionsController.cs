using Asp.Versioning;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Abstractions.Services;
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
    IRulPredictor rulPredictor,
    IHealthClassifier healthClassifier,
    ITelemetryRepository telemetryRepository,
    IFeatureExtractionService featureExtractor,
    IPredictionService predictionService,
    DigitalTwinPlatform.API.Services.Analytics.Advanced.IAdvancedPredictiveService advancedPredictiveService,
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RulPredictionResult>> GetRulPrediction(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting RUL prediction for machine {MachineId}", machineId);

            var telemetry = await GetTelemetryForMachine(machineId, ct);

            if (telemetry.Count < 20)
            {
                logger.LogWarning("Insufficient telemetry data for machine {MachineId}: {Count} points", machineId, telemetry.Count);
                return BadRequest(new { Message = $"Insufficient telemetry data. Required: 20, Available: {telemetry.Count}" });
            }

            var features = featureExtractor.ExtractFeatures(telemetry);
            var result = rulPredictor.PredictWithDetails(machineId.ToString(), features);

            logger.LogInformation(
                "RUL prediction for {MachineId}: {RUL} {Unit} (confidence: {Confidence:P2})",
                machineId, result.Rul, result.RulUnit, result.Confidence);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting RUL prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Message = "Internal server error during prediction" });
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HealthClassificationResult>> GetHealthClassification(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting health classification for machine {MachineId}", machineId);

            var telemetry = await GetTelemetryForMachine(machineId, ct);

            if (telemetry.Count < 20)
            {
                logger.LogWarning("Insufficient telemetry data for machine {MachineId}: {Count} points", machineId, telemetry.Count);
                return BadRequest(new { Message = $"Insufficient telemetry data. Required: 20, Available: {telemetry.Count}" });
            }

            var features = featureExtractor.ExtractFeatures(telemetry);
            var result = healthClassifier.ClassifyWithDetails(machineId.ToString(), features);

            logger.LogInformation(
                "Health classification for {MachineId}: {Status} (probability: {Probability:P2})",
                machineId, result.HealthStatus, result.HealthProbability);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting health classification for machine {MachineId}", machineId);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets quick RUL summary for a machine (lightweight endpoint).
    /// </summary>
    [HttpGet("rul/{machineId}/summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetRulSummary(Guid machineId, CancellationToken ct)
    {
        var telemetry = await GetTelemetryForMachine(machineId, ct);

        if (telemetry.Count < 20)
        {
            return BadRequest(new { Message = "Insufficient telemetry data" });
        }

        var features = featureExtractor.ExtractFeatures(telemetry);
        var result = rulPredictor.PredictWithDetails(machineId.ToString(), features);
        var healthResult = healthClassifier.ClassifyWithDetails(machineId.ToString(), features);

        return Ok(new
        {
            machineId,
            rul = result.Rul,
            rulUnit = result.RulUnit,
            confidence = result.Confidence,
            healthStatus = healthResult.HealthStatus.ToString(),
            predictionTime = result.PredictionTime
        });
    }

    /// <summary>
    /// Triggers model retraining for RUL and health models.
    /// </summary>
    [HttpPost("train")]
    [ProducesResponseType(typeof(TrainingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Training validation failed");
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Model training failed");
            return StatusCode(500, new { Message = "Internal server error during training" });
        }
    }

    /// <summary>
    /// Triggers retraining via MediatR command (for backward compatibility).
    /// </summary>
    [HttpPost("ai/train")]
    [ProducesResponseType(typeof(TrainingResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TrainingResultDto>> TrainModelsAiEndpoint(
        [FromBody] TrainModelRequestDto? request,
        CancellationToken ct)
    {
        return await TrainModels(request, ct);
    }

    /// <summary>
    /// Gets model status and information.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ModelStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ModelStatusDto> GetModelStatus()
    {
        return Ok(new ModelStatusDto
        {
            RulModelLoaded = rulPredictor.IsModelLoaded,
            HealthModelLoaded = healthClassifier.IsModelLoaded,
            ModelVersion = "1.0.0",
            LastUpdated = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Requests a new prediction for a machine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PredictionDto>> RequestPrediction([FromBody] PredictionRequestDto request, CancellationToken ct)
    {
        var result = await predictionService.PredictAsync(request, ct);
        return Ok(result);
    }

    private async Task<List<TelemetryData>> GetTelemetryForMachine(Guid machineId, CancellationToken ct)
    {
        var telemetry = await telemetryRepository.GetRecentAsync(
            machineId: machineId,
            limit: 100,
            ct: ct);

        return telemetry.ToList();
    }

    /// <summary>
    /// Gets list of predictions for all machines or a specific machine.
    /// </summary>
    [HttpGet]
    [HttpGet("{machineId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets RUL prediction for a specific machine using GET method.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>RUL prediction result with confidence and contributing factors.</returns>
    [HttpGet("rul/{machineId}")] 
    [ProducesResponseType(typeof(RulPredictionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RulPredictionResult>> GetRulPredictionGet(
        Guid machineId,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Getting RUL prediction (GET) for machine {MachineId}", machineId);

            var telemetry = await GetTelemetryForMachine(machineId, ct);

            if (telemetry.Count < 20)
            {
                logger.LogWarning("Insufficient telemetry data for machine {MachineId}: {Count} points", machineId, telemetry.Count);
                return BadRequest(new { Message = $"Insufficient telemetry data. Required: 20, Available: {telemetry.Count}" });
            }

            var features = featureExtractor.ExtractFeatures(telemetry);
            var result = rulPredictor.PredictWithDetails(machineId.ToString(), features);

            logger.LogInformation(
                "RUL prediction for {MachineId}: {RUL} {Unit} (confidence: {Confidence:P2})",
                machineId, result.Rul, result.RulUnit, result.Confidence);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting RUL prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Message = "Internal server error during prediction" });
        }
    }

    /// <summary>
    /// Gets anomaly prediction for a specific machine.
    /// </summary>
    /// <param name="machineId">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>Anomaly detection result.</returns>
    [HttpGet("anomaly/{machineId}")]
    [ProducesResponseType(typeof(DigitalTwinPlatform.API.Services.Analytics.Advanced.AnomalyDetectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DigitalTwinPlatform.API.Services.Analytics.Advanced.AnomalyDetectionResult>> GetAnomalyPrediction(
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
            return StatusCode(500, new { Message = "Internal server error during anomaly detection" });
        }
    }

    /// <summary>
    /// Searches prediction data across all machines based on a text query.
    /// </summary>
    /// <param name="query">Text query to search in prediction data fields.</param>
    /// <param name="machineId">Optional machine ID filter.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>List of predictions matching the search criteria.</returns>
    /// <response code="200">Returns search results successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<RulPredictionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return StatusCode(500, new { Message = "Internal server error during search" });
        }
    }

    /// <summary>
    /// Manually requests a new prediction for a machine.
    /// </summary>
    [HttpPost("manual")]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PredictionDto>> RequestPredictionManual(
        [FromBody] PredictionRequestDto request,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Manually requesting prediction for machine {MachineId}", request.MachineId);
            var result = await predictionService.PredictAsync(request, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error requesting prediction for machine {MachineId}", request.MachineId);
            return StatusCode(500, new { Message = "Internal server error during manual prediction request" });
        }
    }
}
