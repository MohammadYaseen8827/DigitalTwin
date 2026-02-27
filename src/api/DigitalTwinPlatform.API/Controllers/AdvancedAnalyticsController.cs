using DigitalTwinPlatform.Application.Analytics.Advanced;
using DigitalTwinPlatform.Application.Analytics.Advanced.Models;
using DigitalTwinPlatform.Application.Predictions.Models;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AdvancedAnalyticsController : ControllerBase
{
    private readonly IAdvancedPredictiveService _advancedPredictiveService;
    private readonly IPrescriptiveAnalyticsService _prescriptiveAnalyticsService;
    private readonly ILogger<AdvancedAnalyticsController> _logger;

    public AdvancedAnalyticsController(
        IAdvancedPredictiveService advancedPredictiveService,
        IPrescriptiveAnalyticsService prescriptiveAnalyticsService,
        ILogger<AdvancedAnalyticsController> logger)
    {
        _advancedPredictiveService = advancedPredictiveService;
        _prescriptiveAnalyticsService = prescriptiveAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Perform ensemble prediction combining multiple ML models
    /// </summary>
    [HttpPost("predictions/{machineId}/ensemble")]
    public async Task<ActionResult<PredictionDto>> EnsemblePrediction(
        Guid machineId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing ensemble prediction for machine {MachineId}", machineId);
            
            var prediction = await _advancedPredictiveService.EnsemblePredictionAsync(machineId, ct);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing ensemble prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to perform ensemble prediction due to an internal error" });
        }
    }

    /// <summary>
    /// Perform deep learning-based prediction using time series analysis
    /// </summary>
    [HttpPost("predictions/{machineId}/deep-learning")]
    public async Task<ActionResult<PredictionDto>> DeepLearningPrediction(
        Guid machineId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing deep learning prediction for machine {MachineId}", machineId);
            
            var prediction = await _advancedPredictiveService.DeepLearningPredictionAsync(machineId, ct);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing deep learning prediction for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to perform deep learning prediction due to an internal error" });
        }
    }

    /// <summary>
    /// Detect anomalies in machine telemetry data
    /// </summary>
    [HttpPost("anomaly-detection/{machineId}")]
    public async Task<ActionResult<AnomalyDetectionResult>> DetectAnomalies(
        Guid machineId,
        [FromBody] AnomalyDetectionRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Detecting anomalies for machine {MachineId}", machineId);
            
            var result = await _advancedPredictiveService.DetectAnomaliesAsync(
                machineId,
                request.StartTime,
                request.EndTime,
                ct);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting anomalies for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to detect anomalies due to an internal error" });
        }
    }

    /// <summary>
    /// Perform time series forecasting for machine metrics
    /// </summary>
    [HttpPost("forecasting/{machineId}/{metric}")]
    public async Task<ActionResult<ForecastResult>> ForecastTimeSeries(
        Guid machineId,
        string metric,
        [FromBody] ForecastRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Forecasting {Metric} for machine {MachineId}", metric, machineId);
            
            var result = await _advancedPredictiveService.ForecastTimeSeriesAsync(
                machineId,
                metric,
                request.ForecastHorizon,
                ct);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting {Metric} for machine {MachineId}", metric, machineId);
            return StatusCode(500, new { Error = "Failed to perform forecasting due to an internal error" });
        }
    }

    /// <summary>
    /// Generate prescriptive maintenance recommendations
    /// </summary>
    [HttpPost("prescriptive/maintenance/{machineId}")]
    public async Task<ActionResult<PrescriptiveRecommendation>> GenerateMaintenanceRecommendation(
        Guid machineId,
        [FromBody] MaintenanceRecommendationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating maintenance recommendation for machine {MachineId}", machineId);
            
            var criteria = new MaintenanceOptimizationCriteria
            {
                CostFactors = request.CostFactors,
                BusinessImpact = request.BusinessImpact,
                TimeConstraints = request.TimeConstraints
            };

            var recommendation = await _prescriptiveAnalyticsService.GenerateMaintenanceRecommendationAsync(
                machineId,
                criteria,
                ct);
            
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating maintenance recommendation for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to generate maintenance recommendation due to an internal error" });
        }
    }

    /// <summary>
    /// Optimize production scheduling based on equipment health
    /// </summary>
    [HttpPost("prescriptive/scheduling")]
    public async Task<ActionResult<SchedulingRecommendation>> OptimizeProductionSchedule(
        [FromBody] SchedulingOptimizationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Optimizing production schedule for {MachineCount} machines", 
                request.MachineIds.Count());
            
            var recommendation = await _prescriptiveAnalyticsService.OptimizeProductionScheduleAsync(
                request.MachineIds,
                request.PlanningHorizon,
                ct);
            
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing production schedule");
            return StatusCode(500, new { Error = "Failed to optimize production schedule due to an internal error" });
        }
    }

    /// <summary>
    /// Optimize resource allocation for maintenance activities
    /// </summary>
    [HttpPost("prescriptive/resource-allocation")]
    public async Task<ActionResult<ResourceAllocationPlan>> OptimizeResourceAllocation(
        [FromBody] ResourceAllocationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Optimizing resource allocation for {MachineCount} machines", 
                request.MachineIds.Count());
            
            var plan = await _prescriptiveAnalyticsService.OptimizeResourceAllocationAsync(
                request.MachineIds,
                request.PlanningPeriod,
                ct);
            
            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing resource allocation");
            return StatusCode(500, new { Error = "Failed to optimize resource allocation due to an internal error" });
        }
    }

    /// <summary>
    /// Optimize maintenance costs within budget constraints
    /// </summary>
    [HttpPost("prescriptive/cost-optimization")]
    public async Task<ActionResult<CostOptimizationResult>> OptimizeMaintenanceCosts(
        [FromBody] CostOptimizationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Optimizing maintenance costs with budget {Budget}", request.Budget.TotalBudget);
            
            var result = await _prescriptiveAnalyticsService.OptimizeMaintenanceCostsAsync(
                request.MachineIds,
                request.Budget,
                ct);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing maintenance costs");
            return StatusCode(500, new { Error = "Failed to optimize maintenance costs due to an internal error" });
        }
    }

    /// <summary>
    /// Get comprehensive advanced analytics dashboard data
    /// </summary>
    [HttpGet("dashboard/{machineId}")]
    public async Task<ActionResult<AdvancedAnalyticsDashboard>> GetAdvancedAnalyticsDashboard(
        Guid machineId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating advanced analytics dashboard for machine {MachineId}", machineId);

            // Get ensemble prediction
            var prediction = await _advancedPredictiveService.EnsemblePredictionAsync(machineId, ct);

            // Get anomaly detection results
            var anomalyResult = await _advancedPredictiveService.DetectAnomaliesAsync(
                machineId,
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow,
                ct);

            // Get maintenance recommendation
            var criteria = new MaintenanceOptimizationCriteria();
            var recommendation = await _prescriptiveAnalyticsService.GenerateMaintenanceRecommendationAsync(
                machineId,
                criteria,
                ct);

            var dashboard = new AdvancedAnalyticsDashboard
            {
                MachineId = machineId,
                Prediction = prediction,
                AnomalyDetection = anomalyResult,
                MaintenanceRecommendation = recommendation,
                GeneratedAt = DateTime.UtcNow,
                HealthScore = _advancedPredictiveService.CalculateHealthScore(prediction, anomalyResult, recommendation)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced analytics dashboard for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to generate analytics dashboard due to an internal error" });
        }
    }
}