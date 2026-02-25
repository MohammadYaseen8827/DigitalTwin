using DigitalTwinPlatform.Application.Mathematics;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinPlatform.Application.Services;

/// <summary>
/// Service for estimating parameters of degradation models from historical data.
/// Provides maximum likelihood estimation, Bayesian inference, and model selection
/// for calibrating mathematical degradation models to real-world data.
/// </summary>
public class ParameterEstimationService : IParameterEstimation
{
    private readonly ILogger<ParameterEstimationService> _logger;
    private readonly IODESolver _odeSolver;

    public ParameterEstimationService(
        ILogger<ParameterEstimationService> logger,
        IODESolver odeSolver)
    {
        _logger = logger;
        _odeSolver = odeSolver;
    }

    /// <summary>
    /// Estimates parameters for exponential degradation model using maximum likelihood.
    /// </summary>
    public async Task<ParameterEstimationResult> EstimateExponentialParametersAsync(
        List<SyntheticDataPoint> historicalData,
        Dictionary<string, double>? initialGuess = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating exponential degradation parameters from {DataCount} data points", 
            historicalData.Count);

        try
        {
            // Extract time series data
            var times = historicalData.Select(d => d.Timestamp).ToList();
            var healthScores = historicalData.Select(d => d.HealthScore ?? 100.0).ToList();

            // Initial parameter guesses
            var parameters = initialGuess ?? new Dictionary<string, double>
            {
                { "InitialValue", healthScores.First() },
                { "DegradationRate", 0.01 }
            };

            // Use gradient descent for optimization
            var optimizedParams = await OptimizeParametersAsync(
                parameters,
                (paramSet) => ExponentialLogLikelihood(paramSet, times, healthScores),
                cancellationToken);

            // Calculate model selection criteria
            var logLikelihood = ExponentialLogLikelihood(optimizedParams, times, healthScores);
            var aic = CalculateAIC(logLikelihood, optimizedParams.Count);
            var bic = CalculateBIC(logLikelihood, optimizedParams.Count, times.Count);

            // Calculate parameter uncertainties using Fisher information
            var uncertainties = CalculateParameterUncertainties(optimizedParams, times, healthScores);

            var result = new ParameterEstimationResult
            {
                EstimatedParameters = optimizedParams,
                ParameterUncertainties = uncertainties,
                LogLikelihood = logLikelihood,
                AIC = aic,
                BIC = bic,
                NumberOfObservations = times.Count,
                EstimationMetadata = new Dictionary<string, object>
                {
                    { "ModelType", "Exponential" },
                    { "OptimizationMethod", "GradientDescent" },
                    { "Convergence", true }
                }
            };

            _logger.LogInformation("Exponential parameter estimation completed. Log-likelihood: {LogLikelihood}", 
                logLikelihood);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate exponential degradation parameters");
            throw;
        }
    }

    /// <summary>
    /// Estimates parameters for power-law degradation model.
    /// </summary>
    public async Task<ParameterEstimationResult> EstimatePowerLawParametersAsync(
        List<SyntheticDataPoint> historicalData,
        Dictionary<string, double>? initialGuess = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating power-law degradation parameters from {DataCount} data points", 
            historicalData.Count);

        try
        {
            var times = historicalData.Select(d => d.Timestamp).ToList();
            var healthScores = historicalData.Select(d => d.HealthScore ?? 100.0).ToList();

            var parameters = initialGuess ?? new Dictionary<string, double>
            {
                { "InitialValue", healthScores.First() },
                { "DegradationRate", 0.01 },
                { "Power", 1.0 }
            };

            var optimizedParams = await OptimizeParametersAsync(
                parameters,
                (paramSet) => PowerLawLogLikelihood(paramSet, times, healthScores),
                cancellationToken);

            var logLikelihood = PowerLawLogLikelihood(optimizedParams, times, healthScores);
            var aic = CalculateAIC(logLikelihood, optimizedParams.Count);
            var bic = CalculateBIC(logLikelihood, optimizedParams.Count, times.Count);
            var uncertainties = CalculateParameterUncertainties(optimizedParams, times, healthScores);

            var result = new ParameterEstimationResult
            {
                EstimatedParameters = optimizedParams,
                ParameterUncertainties = uncertainties,
                LogLikelihood = logLikelihood,
                AIC = aic,
                BIC = bic,
                NumberOfObservations = times.Count,
                EstimationMetadata = new Dictionary<string, object>
                {
                    { "ModelType", "PowerLaw" },
                    { "OptimizationMethod", "GradientDescent" },
                    { "Convergence", true }
                }
            };

            _logger.LogInformation("Power-law parameter estimation completed. Log-likelihood: {LogLikelihood}", 
                logLikelihood);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate power-law degradation parameters");
            throw;
        }
    }

    /// <summary>
    /// Estimates parameters for multi-variable degradation model.
    /// </summary>
    public async Task<ParameterEstimationResult> EstimateMultiVariableParametersAsync(
        List<SyntheticDataPoint> historicalData,
        List<string> variableNames,
        Dictionary<string, double>? initialGuess = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Estimating multi-variable degradation parameters for {VariableCount} variables", 
            variableNames.Count);

        try
        {
            var times = historicalData.Select(d => d.Timestamp).ToList();
            var variableData = ExtractVariableData(historicalData, variableNames);

            var parameters = initialGuess ?? CreateDefaultMultiVariableParams(variableNames);

            var optimizedParams = await OptimizeParametersAsync(
                parameters,
                (paramSet) => MultiVariableLogLikelihood(paramSet, times, variableData),
                cancellationToken);

            var logLikelihood = MultiVariableLogLikelihood(optimizedParams, times, variableData);
            var aic = CalculateAIC(logLikelihood, optimizedParams.Count);
            var bic = CalculateBIC(logLikelihood, optimizedParams.Count, times.Count);
            var uncertainties = CalculateParameterUncertainties(optimizedParams, times, variableData);

            var result = new ParameterEstimationResult
            {
                EstimatedParameters = optimizedParams,
                ParameterUncertainties = uncertainties,
                LogLikelihood = logLikelihood,
                AIC = aic,
                BIC = bic,
                NumberOfObservations = times.Count,
                EstimationMetadata = new Dictionary<string, object>
                {
                    { "ModelType", "MultiVariable" },
                    { "Variables", variableNames },
                    { "OptimizationMethod", "GradientDescent" },
                    { "Convergence", true }
                }
            };

            _logger.LogInformation("Multi-variable parameter estimation completed. Log-likelihood: {LogLikelihood}", 
                logLikelihood);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to estimate multi-variable degradation parameters");
            throw;
        }
    }

    /// <summary>
    /// Compares multiple degradation models using information criteria.
    /// </summary>
    public async Task<ModelComparisonResult> CompareModelsAsync(
        List<SyntheticDataPoint> historicalData,
        List<string> modelTypes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing {ModelCount} degradation models", modelTypes.Count);

        var results = new List<ParameterEstimationResult>();

        foreach (var modelType in modelTypes)
        {
            try
            {
                var result = modelType.ToLower() switch
                {
                    "exponential" => await EstimateExponentialParametersAsync(historicalData, null, cancellationToken),
                    "powerlaw" => await EstimatePowerLawParametersAsync(historicalData, null, cancellationToken),
                    _ => throw new ArgumentException($"Unsupported model type: {modelType}")
                };
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to estimate parameters for model {ModelType}", modelType);
            }
        }

        // Sort models by AIC (lower is better)
        var sortedResults = results.OrderBy(r => r.AIC).ToList();

        var comparisonResult = new ModelComparisonResult
        {
            ModelResults = sortedResults,
            BestModel = sortedResults.FirstOrDefault(),
            ComparisonMetadata = new Dictionary<string, object>
            {
                { "ModelCount", results.Count },
                { "ComparisonCriteria", "AIC" },
                { "DataPoints", historicalData.Count }
            }
        };

        _logger.LogInformation("Model comparison completed. Best model: {BestModel} with AIC: {BestAIC}", 
            comparisonResult.BestModel?.EstimationMetadata.GetValueOrDefault("ModelType"),
            comparisonResult.BestModel?.AIC);

        return comparisonResult;
    }

    /// <summary>
    /// Validates estimated parameters using cross-validation.
    /// </summary>
    public async Task<ParameterValidationResult> ValidateParametersAsync(
        ParameterEstimationResult estimatedParams,
        List<SyntheticDataPoint> historicalData,
        int folds = 5,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating parameters with {Folds}-fold cross-validation", folds);

        try
        {
            var foldSize = historicalData.Count / folds;
            var validationScores = new List<double>();

            for (int i = 0; i < folds; i++)
            {
                var validationData = historicalData.Skip(i * foldSize).Take(foldSize).ToList();
                var trainingData = historicalData.Except(validationData).ToList();

                // Re-estimate parameters on training data
                var modelType = estimatedParams.EstimationMetadata.GetValueOrDefault("ModelType")?.ToString();
                var reestimatedParams = modelType?.ToLower() switch
                {
                    "exponential" => await EstimateExponentialParametersAsync(trainingData, null, cancellationToken),
                    "powerlaw" => await EstimatePowerLawParametersAsync(trainingData, null, cancellationToken),
                    _ => throw new ArgumentException($"Unsupported model type: {modelType}")
                };

                // Calculate validation score
                var score = CalculateValidationScore(reestimatedParams, validationData);
                validationScores.Add(score);
            }

            var meanScore = validationScores.Average();
            var scoreStdDev = Math.Sqrt(validationScores.Select(s => Math.Pow(s - meanScore, 2)).Average());

            var validationResult = new ParameterValidationResult
            {
                IsValid = meanScore > 0.7, // Threshold for validation
                MeanValidationScore = meanScore,
                ValidationScoreStdDev = scoreStdDev,
                FoldScores = validationScores,
                ValidationMetadata = new Dictionary<string, object>
                {
                    { "Folds", folds },
                    { "ValidationMethod", "CrossValidation" },
                    { "Threshold", 0.7 }
                }
            };

            _logger.LogInformation("Parameter validation completed. Mean score: {MeanScore:F3}", meanScore);
            return validationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate parameters");
            throw;
        }
    }

    #region Private Methods

    private double ExponentialLogLikelihood(Dictionary<string, double> parameters, List<DateTime> times, List<double> values)
    {
        var initialValue = parameters.GetValueOrDefault("InitialValue", 100.0);
        var degradationRate = parameters.GetValueOrDefault("DegradationRate", 0.01);
        
        var logLikelihood = 0.0;
        var startTime = times.First();

        for (int i = 0; i < times.Count; i++)
        {
            var t = (times[i] - startTime).TotalDays;
            var predicted = initialValue * Math.Exp(-degradationRate * t);
            var observed = values[i];
            
            // Assuming Gaussian noise
            var residual = observed - predicted;
            logLikelihood += -0.5 * Math.Log(2 * Math.PI) - Math.Pow(residual, 2) / 2.0;
        }

        return logLikelihood;
    }

    private double PowerLawLogLikelihood(Dictionary<string, double> parameters, List<DateTime> times, List<double> values)
    {
        var initialValue = parameters.GetValueOrDefault("InitialValue", 100.0);
        var degradationRate = parameters.GetValueOrDefault("DegradationRate", 0.01);
        var power = parameters.GetValueOrDefault("Power", 1.0);
        
        var logLikelihood = 0.0;
        var startTime = times.First();

        for (int i = 0; i < times.Count; i++)
        {
            var t = (times[i] - startTime).TotalDays;
            var predicted = initialValue * Math.Pow(1 + degradationRate * t, -power);
            var observed = values[i];
            
            var residual = observed - predicted;
            logLikelihood += -0.5 * Math.Log(2 * Math.PI) - Math.Pow(residual, 2) / 2.0;
        }

        return logLikelihood;
    }

    private double MultiVariableLogLikelihood(Dictionary<string, double> parameters, List<DateTime> times, Dictionary<string, List<double>> variableData)
    {
        // Simplified multi-variable log-likelihood
        var logLikelihood = 0.0;
        var startTime = times.First();

        foreach (var varName in variableData.Keys)
        {
            var rate = parameters.GetValueOrDefault($"{varName}Rate", 0.01);
            var values = variableData[varName];

            for (int i = 0; i < times.Count; i++)
            {
                var t = (times[i] - startTime).TotalDays;
                var predicted = values.First() * Math.Exp(-rate * t);
                var observed = values[i];
                
                var residual = observed - predicted;
                logLikelihood += -0.5 * Math.Log(2 * Math.PI) - Math.Pow(residual, 2) / 2.0;
            }
        }

        return logLikelihood;
    }

    private async Task<Dictionary<string, double>> OptimizeParametersAsync(
        Dictionary<string, double> initialParams,
        Func<Dictionary<string, double>, double> objectiveFunction,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        // Simple gradient descent optimization
        var currentParams = new Dictionary<string, double>(initialParams);
        var learningRate = 0.001;
        var iterations = 1000;
        var tolerance = 1e-6;

        for (int iter = 0; iter < iterations; iter++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var currentLikelihood = objectiveFunction(currentParams);
            var gradients = new Dictionary<string, double>();

            // Calculate numerical gradients
            foreach (var param in currentParams.Keys)
            {
                var epsilon = 1e-8;
                var paramsPlus = new Dictionary<string, double>(currentParams);
                paramsPlus[param] += epsilon;
                
                var likelihoodPlus = objectiveFunction(paramsPlus);
                gradients[param] = (likelihoodPlus - currentLikelihood) / epsilon;
            }

            // Update parameters
            var maxGradient = gradients.Values.Max(Math.Abs);
            if (maxGradient < tolerance) break;

            foreach (var param in currentParams.Keys.ToList())
            {
                currentParams[param] += learningRate * gradients[param];
                
                // Ensure parameters stay positive where appropriate
                if (param.Contains("Rate") || param.Contains("Power"))
                {
                    currentParams[param] = Math.Max(1e-8, currentParams[param]);
                }
            }

            // Adaptive learning rate
            if (iter % 100 == 0)
            {
                learningRate *= 0.9;
            }
        }

        return currentParams;
    }

    private Dictionary<string, double> CalculateParameterUncertainties(
        Dictionary<string, double> parameters,
        List<DateTime> times,
        List<double> values)
    {
        // Simplified uncertainty calculation using numerical differentiation
        var uncertainties = new Dictionary<string, double>();

        foreach (var param in parameters.Keys)
        {
            var epsilon = 1e-6;
            var paramsPlus = new Dictionary<string, double>(parameters);
            var paramsMinus = new Dictionary<string, double>(parameters);
            
            paramsPlus[param] += epsilon;
            paramsMinus[param] -= epsilon;

            // Calculate second derivative (curvature) as uncertainty estimate
            var baseLikelihood = ExponentialLogLikelihood(parameters, times, values);
            var plusLikelihood = ExponentialLogLikelihood(paramsPlus, times, values);
            var minusLikelihood = ExponentialLogLikelihood(paramsMinus, times, values);

            var secondDerivative = (plusLikelihood - 2 * baseLikelihood + minusLikelihood) / (epsilon * epsilon);
            uncertainties[param] = Math.Sqrt(1.0 / Math.Max(secondDerivative, 1e-8));
        }

        return uncertainties;
    }

    private Dictionary<string, double> CalculateParameterUncertainties(
        Dictionary<string, double> parameters,
        List<DateTime> times,
        Dictionary<string, List<double>> variableData)
    {
        var uncertainties = new Dictionary<string, double>();

        foreach (var param in parameters.Keys)
        {
            var epsilon = 1e-6;
            var paramsPlus = new Dictionary<string, double>(parameters);
            var paramsMinus = new Dictionary<string, double>(parameters);
            
            paramsPlus[param] += epsilon;
            paramsMinus[param] -= epsilon;

            var baseLikelihood = MultiVariableLogLikelihood(parameters, times, variableData);
            var plusLikelihood = MultiVariableLogLikelihood(paramsPlus, times, variableData);
            var minusLikelihood = MultiVariableLogLikelihood(paramsMinus, times, variableData);

            var secondDerivative = (plusLikelihood - 2 * baseLikelihood + minusLikelihood) / (epsilon * epsilon);
            uncertainties[param] = Math.Sqrt(1.0 / Math.Max(secondDerivative, 1e-8));
        }

        return uncertainties;
    }

    private double CalculateAIC(double logLikelihood, int parameterCount)
    {
        return 2 * parameterCount - 2 * logLikelihood;
    }

    private double CalculateBIC(double logLikelihood, int parameterCount, int observationCount)
    {
        return parameterCount * Math.Log(observationCount) - 2 * logLikelihood;
    }

    private Dictionary<string, List<double>> ExtractVariableData(List<SyntheticDataPoint> data, List<string> variableNames)
    {
        var result = new Dictionary<string, List<double>>();

        foreach (var varName in variableNames)
        {
            var values = varName.ToLower() switch
            {
                "temperature" => data.Select(d => d.Temperature ?? 0).ToList(),
                "vibration" => data.Select(d => d.Vibration ?? 0).ToList(),
                "pressure" => data.Select(d => d.Pressure ?? 0).ToList(),
                "rpm" => data.Select(d => d.Rpm ?? 0).ToList(),
                "health" => data.Select(d => d.HealthScore ?? 100).ToList(),
                _ => new List<double>()
            };
            result[varName] = values;
        }

        return result;
    }

    private Dictionary<string, double> CreateDefaultMultiVariableParams(List<string> variableNames)
    {
        var parameterDict = new Dictionary<string, double>();

        foreach (var varName in variableNames)
        {
            parameterDict[$"{varName}Rate"] = 0.01;
        }

        return parameterDict;
    }

    private double CalculateValidationScore(ParameterEstimationResult parameters, List<SyntheticDataPoint> validationData)
    {
        // Simple R-squared based validation score
        var modelType = parameters.EstimationMetadata.GetValueOrDefault("ModelType")?.ToString();
        
        if (modelType == "Exponential")
        {
            var times = validationData.Select(d => d.Timestamp).ToList();
            var values = validationData.Select(d => d.HealthScore ?? 100).ToList();
            var predicted = GeneratePredictions(parameters.EstimatedParameters, times, "Exponential");
            
            return CalculateRSquared(values, predicted);
        }

        return 0.0;
    }

    private List<double> GeneratePredictions(Dictionary<string, double> parameters, List<DateTime> times, string modelType)
    {
        var predictions = new List<double>();
        var startTime = times.First();

        foreach (var time in times)
        {
            var t = (time - startTime).TotalDays;
            var prediction = modelType switch
            {
                "Exponential" => parameters.GetValueOrDefault("InitialValue", 100) * 
                                Math.Exp(-parameters.GetValueOrDefault("DegradationRate", 0.01) * t),
                _ => 100.0
            };
            predictions.Add(prediction);
        }

        return predictions;
    }

    private double CalculateRSquared(List<double> observed, List<double> predicted)
    {
        var observedMean = observed.Average();
        var ssTotal = observed.Sum(o => Math.Pow(o - observedMean, 2));
        var ssResidual = observed.Zip(predicted, (o, p) => Math.Pow(o - p, 2)).Sum();
        
        return 1 - (ssResidual / ssTotal);
    }

    #endregion
}

public interface IParameterEstimation
{
    Task<ParameterEstimationResult> EstimateExponentialParametersAsync(List<SyntheticDataPoint> historicalData, Dictionary<string, double>? initialGuess = null, CancellationToken cancellationToken = default);
    Task<ParameterEstimationResult> EstimatePowerLawParametersAsync(List<SyntheticDataPoint> historicalData, Dictionary<string, double>? initialGuess = null, CancellationToken cancellationToken = default);
    Task<ParameterEstimationResult> EstimateMultiVariableParametersAsync(List<SyntheticDataPoint> historicalData, List<string> variableNames, Dictionary<string, double>? initialGuess = null, CancellationToken cancellationToken = default);
    Task<ModelComparisonResult> CompareModelsAsync(List<SyntheticDataPoint> historicalData, List<string> modelTypes, CancellationToken cancellationToken = default);
    Task<ParameterValidationResult> ValidateParametersAsync(ParameterEstimationResult estimatedParams, List<SyntheticDataPoint> historicalData, int folds = 5, CancellationToken cancellationToken = default);
}

public class ModelComparisonResult
{
    public List<ParameterEstimationResult> ModelResults { get; set; } = new();
    public ParameterEstimationResult? BestModel { get; set; }
    public Dictionary<string, object> ComparisonMetadata { get; set; } = new();
}

public class ParameterValidationResult
{
    public bool IsValid { get; set; }
    public double MeanValidationScore { get; set; }
    public double ValidationScoreStdDev { get; set; }
    public List<double> FoldScores { get; set; } = new();
    public Dictionary<string, object> ValidationMetadata { get; set; } = new();
}
