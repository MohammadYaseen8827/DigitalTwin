using DigitalTwinPlatform.Application.Mathematics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using DigitalTwinPlatform.Application.Services;
using ConfidenceInterval = DigitalTwinPlatform.Application.Mathematics.ConfidenceInterval;
using ModelComparisonResult = DigitalTwinPlatform.API.Services.Analytics.ModelComparisonResult;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for mathematical degradation modeling and simulation.
/// Provides endpoints for configuring degradation models, running simulations,
/// and estimating parameters from historical data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DegradationModelingController(
    DigitalTwinPlatform.Application.Services.IODESolver odeSolver,
    IParameterEstimation parameterEstimation,
    ILogger<DegradationModelingController> logger)
    : ControllerBase
{
    /// <summary>
    /// Solves an exponential degradation ODE.
    /// </summary>
    [HttpPost("solve/exponential")]
    public async Task<ActionResult<ODESolution>> SolveExponentialDegradation(
        [FromBody] ExponentialDegradationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Solving exponential degradation model for machine {MachineType}", request.MachineType);

            var problem = odeSolver.CreateExponentialDegradationProblem(
                request.InitialValue,
                request.DegradationRate,
                request.TimeHorizon,
                request.TimePoints);

            var solution = await odeSolver.SolveODEAsync(problem, request.SolverMethod, cancellationToken);

            logger.LogInformation("Successfully solved exponential degradation model with {SolutionPoints} points", 
                solution.TimePoints.Count);

            return Ok(solution);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to solve exponential degradation model");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Solves a power-law degradation ODE.
    /// </summary>
    [HttpPost("solve/powerlaw")]
    public async Task<ActionResult<ODESolution>> SolvePowerLawDegradation(
        [FromBody] PowerLawDegradationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Solving power-law degradation model for machine {MachineType}", request.MachineType);

            var problem = odeSolver.CreatePowerLawDegradationProblem(
                request.InitialValue,
                request.DegradationRate,
                request.Power,
                request.TimeHorizon,
                request.TimePoints);

            var solution = await odeSolver.SolveODEAsync(problem, request.SolverMethod, cancellationToken);

            logger.LogInformation("Successfully solved power-law degradation model with {SolutionPoints} points", 
                solution.TimePoints.Count);

            return Ok(solution);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to solve power-law degradation model");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Solves a multi-variable degradation ODE system.
    /// </summary>
    [HttpPost("solve/multivariable")]
    public async Task<ActionResult<ODESolution>> SolveMultiVariableDegradation(
        [FromBody] MultiVariableDegradationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Solving multi-variable degradation model for machine {MachineType}", request.MachineType);

            var problem = odeSolver.CreateMultiVariableDegradationProblem(
                request.InitialValues,
                request.Parameters,
                request.TimeHorizon,
                request.TimePoints);

            var solution = await odeSolver.SolveODEAsync(problem, request.SolverMethod, cancellationToken);

            logger.LogInformation("Successfully solved multi-variable degradation model with {SolutionPoints} points", 
                solution.TimePoints.Count);

            return Ok(solution);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to solve multi-variable degradation model");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Solves a stochastic degradation model with confidence intervals.
    /// </summary>
    [HttpPost("solve/stochastic")]
    public async Task<ActionResult<StochasticSolutionResult>> SolveStochasticDegradation(
        [FromBody] StochasticDegradationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Solving stochastic degradation model with {Simulations} simulations", 
                request.NumberOfSimulations);

            var problem = request.ModelType.ToLower() switch
            {
                "exponential" => odeSolver.CreateExponentialDegradationProblem(
                    request.InitialValue, request.DegradationRate, request.TimeHorizon, request.TimePoints),
                "powerlaw" => odeSolver.CreatePowerLawDegradationProblem(
                    request.InitialValue, request.DegradationRate, request.Power, request.TimeHorizon, request.TimePoints),
                _ => throw new ArgumentException($"Unsupported model type: {request.ModelType}")
            };

            var solution = await odeSolver.SolveSDEAsync(
                problem, 
                request.DiffusionCoefficient, 
                request.NumberOfSimulations, 
                cancellationToken);

            // Calculate confidence intervals
            var confidenceIntervals = CalculateConfidenceIntervals(solution, request.ConfidenceLevel);

            var result = new StochasticSolutionResult
            {
                MeanSolution = solution,
                ConfidenceIntervals = confidenceIntervals,
                NumberOfSimulations = request.NumberOfSimulations,
                ConfidenceLevel = request.ConfidenceLevel,
                DiffusionCoefficient = request.DiffusionCoefficient
            };

            logger.LogInformation("Successfully solved stochastic degradation model with {Simulations} simulations", 
                request.NumberOfSimulations);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to solve stochastic degradation model");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Estimates exponential degradation parameters from historical data.
    /// </summary>
    [HttpPost("estimate/exponential")]
    public async Task<ActionResult<ParameterEstimationResult>> EstimateExponentialParameters(
        [FromBody] ParameterEstimationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Estimating exponential parameters from {DataCount} data points", 
                request.HistoricalData.Count);

            var result = await parameterEstimation.EstimateExponentialParametersAsync(
                request.HistoricalData,
                request.InitialGuess,
                cancellationToken);

            logger.LogInformation("Successfully estimated exponential parameters with log-likelihood: {LogLikelihood}", 
                result.LogLikelihood);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to estimate exponential parameters");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Estimates power-law degradation parameters from historical data.
    /// </summary>
    [HttpPost("estimate/powerlaw")]
    public async Task<ActionResult<ParameterEstimationResult>> EstimatePowerLawParameters(
        [FromBody] ParameterEstimationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Estimating power-law parameters from {DataCount} data points", 
                request.HistoricalData.Count);

            var result = await parameterEstimation.EstimatePowerLawParametersAsync(
                request.HistoricalData,
                request.InitialGuess,
                cancellationToken);

            logger.LogInformation("Successfully estimated power-law parameters with log-likelihood: {LogLikelihood}", 
                result.LogLikelihood);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to estimate power-law parameters");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Estimates multi-variable degradation parameters from historical data.
    /// </summary>
    [HttpPost("estimate/multivariable")]
    public async Task<ActionResult<ParameterEstimationResult>> EstimateMultiVariableParameters(
        [FromBody] MultiVariableEstimationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Estimating multi-variable parameters for {VariableCount} variables", 
                request.VariableNames.Count);

            var result = await parameterEstimation.EstimateMultiVariableParametersAsync(
                request.HistoricalData,
                request.VariableNames,
                request.InitialGuess,
                cancellationToken);

            logger.LogInformation("Successfully estimated multi-variable parameters with log-likelihood: {LogLikelihood}", 
                result.LogLikelihood);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to estimate multi-variable parameters");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Compares multiple degradation models using information criteria.
    /// </summary>
    [HttpPost("compare")]
    public async Task<ActionResult<ModelComparisonResult>> CompareModels(
        [FromBody] ModelComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Comparing {ModelCount} degradation models", request.ModelTypes.Count);

            var result = await parameterEstimation.CompareModelsAsync(
                request.HistoricalData,
                request.ModelTypes,
                cancellationToken);

            logger.LogInformation("Successfully compared models. Best model: {BestModel}", 
                result.BestModel?.EstimationMetadata.GetValueOrDefault("ModelType"));

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to compare degradation models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates estimated parameters using cross-validation.
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ParameterValidationResult>> ValidateParameters(
        [FromBody] ParameterValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Validating parameters with {Folds}-fold cross-validation", request.Folds);

            var result = await parameterEstimation.ValidateParametersAsync(
                request.EstimatedParameters,
                request.HistoricalData,
                request.Folds,
                cancellationToken);

            logger.LogInformation("Parameter validation completed with mean score: {MeanScore:F3}", 
                result.MeanValidationScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate parameters");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates an ODE solution for numerical stability.
    /// </summary>
    [HttpPost("validate/solution")]
    public ActionResult<ODEValidationResult> ValidateSolution(
        [FromBody] SolutionValidationRequest request)
    {
        try
        {
            logger.LogInformation("Validating ODE solution for {VariableCount} variables", 
                request.Solution.Values.Length);

            // Create a mock problem for validation
            var problem = new ODEProblem
            {
                InitialConditions = request.Solution.Values.Select(v => v.First()).ToArray(),
                Parameters = request.ValidationParameters ?? new Dictionary<string, double>()
            };

            var result = odeSolver.ValidateSolution(request.Solution, problem);

            logger.LogInformation("Solution validation completed. Is valid: {IsValid}", result.IsValid);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate ODE solution");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #region Private Methods

    private Dictionary<string, ConfidenceInterval> CalculateConfidenceIntervals(
        ODESolution solution, double confidenceLevel)
    {
        var intervals = new Dictionary<string, ConfidenceInterval>();

        for (int i = 0; i < solution.Values.Length; i++)
        {
            var values = solution.Values[i];
            var mean = values.Average();
            var stdDev = Math.Sqrt(values.Select(v => Math.Pow(v - mean, 2)).Average());
            
            // Calculate confidence interval bounds
            var alpha = 1 - confidenceLevel;
            var zScore = GetZScore(confidenceLevel);
            var margin = zScore * stdDev;

            intervals[$"Variable_{i}"] = new ConfidenceInterval
            {
                LowerBound = mean - margin,
                UpperBound = mean + margin,
                Mean = mean,
                ConfidenceLevel = confidenceLevel,
                SampleValues = values.ToList()
            };
        }

        return intervals;
    }

    private double GetZScore(double confidenceLevel)
    {
        // Simplified z-score lookup for common confidence levels
        return confidenceLevel switch
        {
            0.90 => 1.645,
            0.95 => 1.96,
            0.99 => 2.576,
            _ => 1.96 // Default to 95%
        };
    }

    #endregion
}

#region Request/Response Models

public record ExponentialDegradationRequest(
    [Required] string MachineType,
    [Range(0.1, 1000)] double InitialValue,
    [Range(1e-6, 10)] double DegradationRate,
    [Range(1, 365)] double TimeHorizon,
    [Range(10, 10000)] int TimePoints = 100,
    ODESolverMethod SolverMethod = ODESolverMethod.RungeKutta4);

public record PowerLawDegradationRequest(
    [Required] string MachineType,
    [Range(0.1, 1000)] double InitialValue,
    [Range(1e-6, 10)] double DegradationRate,
    [Range(0.1, 10)] double Power,
    [Range(1, 365)] double TimeHorizon,
    [Range(10, 10000)] int TimePoints = 100,
    ODESolverMethod SolverMethod = ODESolverMethod.RungeKutta4);

public record MultiVariableDegradationRequest(
    [Required] string MachineType,
    [Required] double[] InitialValues,
    [Required] Dictionary<string, double> Parameters,
    [Range(1, 365)] double TimeHorizon,
    [Range(10, 10000)] int TimePoints = 100,
    ODESolverMethod SolverMethod = ODESolverMethod.RungeKutta4);

public record StochasticDegradationRequest(
    [Required] string MachineType,
    [Required] string ModelType,
    [Range(0.1, 1000)] double InitialValue,
    [Range(1e-6, 10)] double DegradationRate,
    [Range(1e-6, 1)] double DiffusionCoefficient,
    [Range(0.1, 10)] double Power = 1.0,
    [Range(1, 365)] double TimeHorizon = 30,
    [Range(10, 10000)] int TimePoints = 100,
    [Range(10, 10000)] int NumberOfSimulations = 100,
    [Range(0.8, 0.99)] double ConfidenceLevel = 0.95);

public record ParameterEstimationRequest(
    [Required] List<SyntheticDataPoint> HistoricalData,
    Dictionary<string, double>? InitialGuess = null);

public record MultiVariableEstimationRequest(
    [Required] List<SyntheticDataPoint> HistoricalData,
    [Required] List<string> VariableNames,
    Dictionary<string, double>? InitialGuess = null);

public record ModelComparisonRequest(
    [Required] List<SyntheticDataPoint> HistoricalData,
    [Required] List<string> ModelTypes);

public record ParameterValidationRequest(
    [Required] ParameterEstimationResult EstimatedParameters,
    [Required] List<SyntheticDataPoint> HistoricalData,
    [Range(2, 10)] int Folds = 5);

public record SolutionValidationRequest(
    [Required] ODESolution Solution,
    Dictionary<string, double>? ValidationParameters = null);

public class StochasticSolutionResult
{
    public ODESolution MeanSolution { get; set; } = null!;
    public Dictionary<string, ConfidenceInterval> ConfidenceIntervals { get; set; } = new();
    public int NumberOfSimulations { get; set; }
    public double ConfidenceLevel { get; set; }
    public double DiffusionCoefficient { get; set; }
}

#endregion
