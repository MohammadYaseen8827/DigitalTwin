using Asp.Versioning;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.Services;

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
    [ProducesResponseType(typeof(IEnumerable<RulPredictionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RulPredictionResult>>> GetPredictions(
        [FromQuery] Guid? machineId = null,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Getting predictions list for machine {MachineId}", machineId);

            // In a real implementation, this would fetch from a predictions repository
            // For now, return empty list as placeholder
            var predictions = new List<RulPredictionResult>();
            
            if (machineId.HasValue)
            {
                // Return predictions for specific machine
                // This would involve querying the prediction history
                var telemetry = await GetTelemetryForMachine(machineId.Value, ct);
                
                if (telemetry.Count >= 20)
                {
                    var features = featureExtractor.ExtractFeatures(telemetry);
                    var result = rulPredictor.PredictWithDetails(machineId.Value.ToString(), features);
                    predictions.Add(result);
                }
            }
            else
            {
                // Return predictions for all machines
                // This would involve querying all machines and their predictions
                logger.LogWarning("Prediction list endpoint not fully implemented - returning empty list");
            }

            return Ok(predictions);
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

            var telemetry = await GetTelemetryForMachine(machineId, ct);

            if (telemetry.Count < 20)
            {
                logger.LogWarning("Insufficient telemetry data for machine {MachineId}: {Count} points", machineId, telemetry.Count);
                return BadRequest(new { Message = $"Insufficient telemetry data. Required: 20, Available: {telemetry.Count}" });
            }

            // In a real implementation, this would call an anomaly detection service
            // For now, we'll return a mock result based on the existing AdvancedPredictiveService
            var anomalyResult = new AnomalyDetectionResult
            {
                MachineId = machineId,
                TotalAnomalies = 0,
                CriticalAnomalies = 0,
                HighAnomalies = 0,
                MediumAnomalies = 0,
                LowAnomalies = 0,
                Anomalies = new List<Anomaly>(),
                DetectionPeriod = new DateTimeRange { Start = DateTime.UtcNow.AddDays(-7), End = DateTime.UtcNow },
                OverallRiskScore = 0.1
            };

            logger.LogInformation(
                "Anomaly detection for {MachineId}: {TotalAnomalies} anomalies (score: {RiskScore:P2})",
                machineId, anomalyResult.TotalAnomalies, anomalyResult.OverallRiskScore);

            return Ok(anomalyResult);
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
                MachineId = dto.MachineId,
                Rul = dto.RemainingUsefulLifeDays,
                RulUnit = "days",
                Confidence = dto.Confidence,
                FailureProbability = dto.FailureProbability,
                HealthStatus = dto.HealthStatus,
                FeatureContributions = dto.FeatureContributions,
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
}

/// <summary>
/// Request DTO for model training.
/// </summary>
public class TrainModelRequestDto
{
    /// <summary>
    /// Force retraining even if existing model is valid.
    /// </summary>
    public bool ForceRetrain { get; set; }

    /// <summary>
    /// Type of model to train (rul, health, or all).
    /// </summary>
    public string ModelType { get; set; } = "all";
}

/// <summary>
/// Result DTO for model training.
/// </summary>
public class TrainingResultDto
{
    public bool Success { get; init; }
    public int SamplesUsed { get; init; }
    public double R2Score { get; init; }
    public double Mape { get; init; }
    public string ModelPath { get; init; } = string.Empty;
    public DateTime TrainedAt { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Status DTO for ML models.
/// </summary>
public class ModelStatusDto
{
    public bool RulModelLoaded { get; init; }
    public bool HealthModelLoaded { get; init; }
    public string ModelVersion { get; init; } = string.Empty;
    public DateTime LastUpdated { get; init; }
}

#region Supporting Classes

public class AnomalyDetectionResult
{
    public Guid MachineId { get; set; }
    public int TotalAnomalies { get; set; }
    public int CriticalAnomalies { get; set; }
    public int HighAnomalies { get; set; }
    public int MediumAnomalies { get; set; }
    public int LowAnomalies { get; set; }
    public List<Anomaly> Anomalies { get; set; } = new();
    public DateTimeRange DetectionPeriod { get; set; } = new();
    public double OverallRiskScore { get; set; }
}

public class Anomaly
{
    public Guid Id { get; set; }
    public string? Metric { get; set; }
    public double Value { get; set; }
    public double ExpectedValue { get; set; }
    public double Deviation { get; set; }
    public AnomalySeverity Severity { get; set; }
    public double SeverityScore { get; set; }
    public DateTime Timestamp { get; set; }
    public AnomalyType Type { get; set; }
}

public enum AnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AnomalyType
{
    Statistical,
    Temporal,
    Multivariate,
    Threshold
}

public class DateTimeRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

#endregion
