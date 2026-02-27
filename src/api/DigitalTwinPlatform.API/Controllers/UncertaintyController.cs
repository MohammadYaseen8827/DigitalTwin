using DigitalTwinPlatform.API.Services.Analytics;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Uncertainty.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UncertaintyController : ControllerBase
{
    private readonly IUncertaintyQuantificationService _uncertaintyService;
    private readonly ILogger<UncertaintyController> _logger;

    public UncertaintyController(
        IUncertaintyQuantificationService uncertaintyService,
        ILogger<UncertaintyController> logger)
    {
        _uncertaintyService = uncertaintyService;
        _logger = logger;
    }

    /// <summary>
    /// Perform Monte Carlo uncertainty analysis for machine predictions
    /// </summary>
    [HttpPost("{machineId}/monte-carlo")]
    public async Task<ActionResult<UncertaintyAnalysisResponse>> MonteCarloAnalysis(
        Guid machineId,
        [FromBody] MonteCarloRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing Monte Carlo analysis for machine {MachineId}", machineId);

            var result = await _uncertaintyService.PerformMonteCarloSimulationAsync(
                request.BaseFeatures,
                request.Iterations,
                request.NoiseLevel,
                ct);

            var response = new UncertaintyAnalysisResponse
            {
                AnalysisType = "Monte Carlo",
                MachineId = machineId,
                MeanPrediction = result.MeanPrediction,
                StandardDeviation = result.StandardDeviation,
                ConfidenceInterval = new ConfidenceIntervalDto
                {
                    LowerBound = result.ConfidenceIntervalLower,
                    UpperBound = result.ConfidenceIntervalUpper,
                    ConfidenceLevel = 0.95
                },
                FeatureUncertainties = result.FeatureUncertainties,
                PredictionVariance = result.PredictionVariance,
                SampleCount = result.Samples.Length,
                Timestamp = result.Timestamp
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing Monte Carlo analysis for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to perform Monte Carlo analysis" });
        }
    }

    /// <summary>
    /// Perform Bayesian uncertainty inference
    /// </summary>
    [HttpPost("{machineId}/bayesian")]
    public async Task<ActionResult<UncertaintyAnalysisResponse>> BayesianAnalysis(
        Guid machineId,
        [FromBody] BayesianRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing Bayesian analysis for machine {MachineId}", machineId);

            var result = await _uncertaintyService.PerformBayesianInferenceAsync(
                request.ObservedData,
                request.PriorDistributions,
                request.Samples,
                ct);

            var response = new UncertaintyAnalysisResponse
            {
                AnalysisType = "Bayesian",
                MachineId = machineId,
                MeanPrediction = result.MeanPrediction,
                StandardDeviation = result.StandardDeviation,
                ConfidenceInterval = new ConfidenceIntervalDto
                {
                    LowerBound = result.ConfidenceIntervalLower,
                    UpperBound = result.ConfidenceIntervalUpper,
                    ConfidenceLevel = 0.95
                },
                PredictionVariance = result.PredictionVariance,
                SampleCount = result.Samples.Length,
                Timestamp = result.Timestamp
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing Bayesian analysis for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to perform Bayesian analysis" });
        }
    }

    /// <summary>
    /// Calculate bootstrap confidence intervals for predictions
    /// </summary>
    [HttpGet("{machineId}/bootstrap-intervals")]
    public async Task<ActionResult<ConfidenceIntervalDto>> BootstrapIntervals(
        Guid machineId,
        [FromQuery] int samples = 1000,
        [FromQuery] double confidenceLevel = 0.95,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calculating bootstrap intervals for machine {MachineId}", machineId);

            var interval = await _uncertaintyService.CalculateBootstrapIntervalsAsync(
                machineId,
                samples,
                confidenceLevel,
                ct);

            var response = new ConfidenceIntervalDto
            {
                LowerBound = interval.LowerBound,
                UpperBound = interval.UpperBound,
                ConfidenceLevel = interval.ConfidenceLevel,
                CoverageProbability = interval.CoverageProbability,
                Timestamp = interval.Timestamp
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating bootstrap intervals for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to calculate bootstrap intervals" });
        }
    }

    /// <summary>
    /// Quantify model uncertainty using ensemble methods
    /// </summary>
    [HttpPost("{machineId}/model-uncertainty")]
    public async Task<ActionResult<ModelUncertaintyResponse>> ModelUncertainty(
        Guid machineId,
        [FromBody] ModelUncertaintyRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Quantifying model uncertainty for machine {MachineId}", machineId);

            var result = await _uncertaintyService.QuantifyModelUncertaintyAsync(
                machineId,
                request.Features,
                ct);

            var response = new ModelUncertaintyResponse
            {
                MachineId = machineId,
                AleatoricUncertainty = result.AleatoricUncertainty,
                EpistemicUncertainty = result.EpistemicUncertainty,
                TotalUncertainty = result.TotalUncertainty,
                FeatureImportanceWithUncertainty = result.FeatureImportanceWithUncertainty,
                ModelConfidence = result.ModelConfidence,
                UncertaintyComponents = new UncertaintyComponentsDto
                {
                    DataNoise = result.AleatoricUncertainty,
                    ModelVariance = result.EpistemicUncertainty,
                    Total = result.TotalUncertainty
                },
                Timestamp = result.Timestamp
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error quantifying model uncertainty for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to quantify model uncertainty" });
        }
    }

    /// <summary>
    /// Get comprehensive uncertainty report combining all methods
    /// </summary>
    [HttpGet("{machineId}/comprehensive-report")]
    public async Task<ActionResult<ComprehensiveUncertaintyReport>> ComprehensiveReport(
        Guid machineId,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Generating comprehensive uncertainty report for machine {MachineId}", machineId);

            // Get recent telemetry for base features
            // In practice, this would integrate with the telemetry service
            var baseFeatures = new Dictionary<string, double>
            {
                ["temperature"] = 72.5,
                ["vibration"] = 0.3,
                ["pressure"] = 45.2,
                ["operating_hours"] = 1250
            };

            // Perform all analyses
            var monteCarloResult = await _uncertaintyService.PerformMonteCarloSimulationAsync(
                baseFeatures, 1000, 0.1, ct);

            var modelUncertaintyResult = await _uncertaintyService.QuantifyModelUncertaintyAsync(
                machineId, baseFeatures, ct);

            var bootstrapInterval = await _uncertaintyService.CalculateBootstrapIntervalsAsync(
                machineId, 1000, 0.95, ct);

            var report = new ComprehensiveUncertaintyReport
            {
                MachineId = machineId,
                GeneratedAt = DateTime.UtcNow,
                MonteCarloAnalysis = new UncertaintyAnalysisResponse
                {
                    AnalysisType = "Monte Carlo",
                    MachineId = machineId,
                    MeanPrediction = monteCarloResult.MeanPrediction,
                    StandardDeviation = monteCarloResult.StandardDeviation,
                    ConfidenceInterval = new ConfidenceIntervalDto
                    {
                        LowerBound = monteCarloResult.ConfidenceIntervalLower,
                        UpperBound = monteCarloResult.ConfidenceIntervalUpper,
                        ConfidenceLevel = 0.95
                    },
                    FeatureUncertainties = monteCarloResult.FeatureUncertainties,
                    PredictionVariance = monteCarloResult.PredictionVariance,
                    SampleCount = monteCarloResult.Samples.Length,
                    Timestamp = monteCarloResult.Timestamp
                },
                ModelUncertainty = new ModelUncertaintyResponse
                {
                    MachineId = machineId,
                    AleatoricUncertainty = modelUncertaintyResult.AleatoricUncertainty,
                    EpistemicUncertainty = modelUncertaintyResult.EpistemicUncertainty,
                    TotalUncertainty = modelUncertaintyResult.TotalUncertainty,
                    FeatureImportanceWithUncertainty = modelUncertaintyResult.FeatureImportanceWithUncertainty,
                    ModelConfidence = modelUncertaintyResult.ModelConfidence,
                    UncertaintyComponents = new UncertaintyComponentsDto
                    {
                        DataNoise = modelUncertaintyResult.AleatoricUncertainty,
                        ModelVariance = modelUncertaintyResult.EpistemicUncertainty,
                        Total = modelUncertaintyResult.TotalUncertainty
                    },
                    Timestamp = modelUncertaintyResult.Timestamp
                },
                BootstrapInterval = new ConfidenceIntervalDto
                {
                    LowerBound = bootstrapInterval.LowerBound,
                    UpperBound = bootstrapInterval.UpperBound,
                    ConfidenceLevel = bootstrapInterval.ConfidenceLevel,
                    CoverageProbability = bootstrapInterval.CoverageProbability,
                    Timestamp = bootstrapInterval.Timestamp
                },
                RiskAssessment = GenerateRiskAssessment(
                    monteCarloResult.MeanPrediction,
                    monteCarloResult.StandardDeviation,
                    modelUncertaintyResult.ModelConfidence)
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive uncertainty report for machine {MachineId}", machineId);
            return StatusCode(500, new { Error = "Failed to generate uncertainty report" });
        }
    }

    private RiskAssessment GenerateRiskAssessment(double meanRul, double stdDev, double confidence)
    {
        var riskScore = CalculateRiskScore(meanRul, stdDev, confidence);
        var riskLevel = riskScore switch
        {
            <= 0.3 => RiskLevel.Low,
            <= 0.6 => RiskLevel.Medium,
            <= 0.8 => RiskLevel.High,
            _ => RiskLevel.Critical
        };

        return new RiskAssessment
        {
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            Recommendation = riskLevel switch
            {
                RiskLevel.Low => "Continue normal operations with scheduled maintenance",
                RiskLevel.Medium => "Increase monitoring frequency and prepare maintenance schedule",
                RiskLevel.High => "Schedule preventive maintenance within 30 days",
                RiskLevel.Critical => "Immediate maintenance required - high failure probability",
                _ => "Maintain current operational procedures"
            },
            Factors = new Dictionary<string, double>
            {
                ["Remaining_Useful_Life"] = meanRul,
                ["Uncertainty_Level"] = stdDev,
                ["Model_Confidence"] = confidence
            }
        };
    }

    private double CalculateRiskScore(double meanRul, double stdDev, double confidence)
    {
        // Normalize factors to 0-1 scale
        var rulFactor = Math.Min(1.0, meanRul / 365.0); // Normalize to 1 year
        var uncertaintyFactor = Math.Min(1.0, stdDev / 100.0); // Normalize uncertainty
        var confidenceFactor = 1.0 - confidence; // Invert confidence (lower confidence = higher risk)

        // Weighted combination
        return (rulFactor * 0.4) + (uncertaintyFactor * 0.4) + (confidenceFactor * 0.2);
    }
}