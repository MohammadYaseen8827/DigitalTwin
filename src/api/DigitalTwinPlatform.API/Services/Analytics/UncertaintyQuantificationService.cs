using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Analytics.Advanced;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Domain.Entities;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class UncertaintyQuantificationService(
    IPredictiveAnalyticsService predictiveService,
    ITelemetryRepository telemetryRepository,
    IRepository<Prediction> predictionRepository,
    ILogger<UncertaintyQuantificationService> logger)
    : IUncertaintyQuantificationService
{
    private readonly Random _random = new();

    public async Task<UncertaintyResult> PerformMonteCarloSimulationAsync(
        Dictionary<string, double> baseFeatures,
        int iterations = 1000,
        double noiseLevel = 0.1,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        logger.LogInformation("Starting Monte Carlo simulation with {Iterations} iterations", iterations);

        var samples = new double[iterations];
        var featureVariances = new Dictionary<string, double>();

        // Calculate variance for each feature based on historical data or defaults
        foreach (var feature in baseFeatures)
        {
            featureVariances[feature.Key] = Math.Abs(feature.Value * noiseLevel);
        }

        // Perform Monte Carlo sampling
        for (int i = 0; i < iterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            // Perturb features with Gaussian noise
            var perturbedFeatures = new Dictionary<string, double>();
            foreach (var feature in baseFeatures)
            {
                var noise = SampleNormal(0, featureVariances[feature.Key]);
                perturbedFeatures[feature.Key] = feature.Value + noise;
            }

            // Make prediction with perturbed features
            // Note: This is a simplified approach - in practice, we'd need to convert features to the format expected by the prediction service
            try
            {
                // For demonstration, we'll simulate the prediction
                var simulatedRul = SimulatePrediction(perturbedFeatures);
                samples[i] = simulatedRul;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Prediction failed for iteration {Iteration}", i);
                samples[i] = baseFeatures.GetValueOrDefault("rul_estimate", 100.0); // Fallback
            }
        }

        // Calculate statistics
        var mean = samples.Average();
        var variance = samples.Select(x => Math.Pow(x - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);

        // Calculate confidence intervals (95%)
        Array.Sort(samples);
        var lowerIndex = (int)(0.025 * iterations);
        var upperIndex = (int)(0.975 * iterations);
        var confidenceLower = samples[lowerIndex];
        var confidenceUpper = samples[upperIndex];

        // Calculate feature uncertainties (simplified sensitivity analysis)
        var featureUncertainties = new Dictionary<string, double>();
        foreach (var feature in baseFeatures)
        {
            var sensitivity = CalculateFeatureSensitivity(feature.Key, baseFeatures, samples, iterations);
            featureUncertainties[feature.Key] = sensitivity * featureVariances[feature.Key];
        }

        logger.LogInformation("Monte Carlo simulation completed. Mean: {Mean:F2}, StdDev: {StdDev:F2}", mean, stdDev);

        return new UncertaintyResult
        {
            MeanPrediction = mean,
            StandardDeviation = stdDev,
            Samples = samples,
            FeatureUncertainties = featureUncertainties,
            ConfidenceIntervalLower = confidenceLower,
            ConfidenceIntervalUpper = confidenceUpper,
            PredictionVariance = variance
        };
    }

    public async Task<UncertaintyResult> PerformBayesianInferenceAsync(
        Dictionary<string, double> observedData,
        Dictionary<string, (double mean, double stdDev)> priorDistributions,
        int samples = 2000,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        logger.LogInformation("Starting Bayesian inference with {Samples} samples", samples);

        // Initialize posterior samples
        var posteriorSamples = new double[samples];

        // For each sample, perform Bayesian updating
        for (int i = 0; i < samples; i++)
        {
            ct.ThrowIfCancellationRequested();

            // Sample from prior distributions
            var parameters = new Dictionary<string, double>();
            foreach (var prior in priorDistributions)
            {
                parameters[prior.Key] = SampleNormal(prior.Value.mean, prior.Value.stdDev);
            }

            // Likelihood calculation (simplified)
            var likelihood = CalculateLikelihood(observedData, parameters);
            
            // Accept-reject sampling (Metropolis-Hastings simplified)
            if (i == 0 || _random.NextDouble() < likelihood)
            {
                // Convert parameters to RUL prediction
                posteriorSamples[i] = parameters.GetValueOrDefault("rul", 100.0);
            }
            else
            {
                posteriorSamples[i] = posteriorSamples[i - 1]; // Repeat previous sample
            }
        }

        // Calculate posterior statistics
        var mean = posteriorSamples.Average();
        var variance = posteriorSamples.Select(x => Math.Pow(x - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);

        Array.Sort(posteriorSamples);
        var lowerIndex = (int)(0.025 * samples);
        var upperIndex = (int)(0.975 * samples);
        var confidenceLower = posteriorSamples[lowerIndex];
        var confidenceUpper = posteriorSamples[upperIndex];

        logger.LogInformation("Bayesian inference completed. Posterior mean: {Mean:F2}", mean);

        return new UncertaintyResult
        {
            MeanPrediction = mean,
            StandardDeviation = stdDev,
            Samples = posteriorSamples,
            ConfidenceIntervalLower = confidenceLower,
            ConfidenceIntervalUpper = confidenceUpper,
            PredictionVariance = variance
        };
    }

    public async Task<ConfidenceInterval> CalculateBootstrapIntervalsAsync(
        Guid machineId,
        int bootstrapSamples = 1000,
        double confidenceLevel = 0.95,
        CancellationToken ct = default)
    {
        logger.LogInformation("Calculating bootstrap confidence intervals for machine {MachineId}", machineId);

        // Get historical predictions
        var historicalPredictions = (await predictionRepository.GetAllAsync(
            p => p.MachineId == machineId && p.CreatedAt > DateTime.UtcNow.AddDays(-30), ct)).ToList();

        if (!historicalPredictions.Any())
        {
            throw new InvalidOperationException($"No historical predictions found for machine {machineId}");
        }

        var rulValues = historicalPredictions.Select(p => p.RemainingUsefulLifeDays).ToArray();
        var predictionErrors = historicalPredictions.Select(p => 
            Math.Abs(p.RemainingUsefulLifeDays - (p.RulLowerBound + p.RulUpperBound) / 2)).ToArray();

        var bootstrapRuls = new double[bootstrapSamples];
        var bootstrapErrors = new double[bootstrapSamples];

        // Bootstrap resampling
        for (int i = 0; i < bootstrapSamples; i++)
        {
            ct.ThrowIfCancellationRequested();

            // Resample with replacement
            var rulSample = Resample(rulValues);
            var errorSample = Resample(predictionErrors);

            bootstrapRuls[i] = rulSample.Average();
            bootstrapErrors[i] = errorSample.Average();
        }

        // Calculate confidence intervals
        Array.Sort(bootstrapRuls);
        Array.Sort(bootstrapErrors);

        var alpha = 1 - confidenceLevel;
        var lowerPercentile = alpha / 2;
        var upperPercentile = 1 - alpha / 2;

        var lowerIndex = (int)(lowerPercentile * bootstrapSamples);
        var upperIndex = (int)(upperPercentile * bootstrapSamples);

        var interval = new ConfidenceInterval
        {
            LowerBound = bootstrapRuls[lowerIndex],
            UpperBound = bootstrapRuls[upperIndex],
            ConfidenceLevel = confidenceLevel,
            CoverageProbability = CalculateCoverageProbability(
                bootstrapRuls[lowerIndex], 
                bootstrapRuls[upperIndex], 
                rulValues)
        };

        logger.LogInformation("Bootstrap intervals calculated: [{Lower:F2}, {Upper:F2}]", 
            interval.LowerBound, interval.UpperBound);

        return interval;
    }

    public async Task<ModelUncertaintyResult> QuantifyModelUncertaintyAsync(
        Guid machineId,
        Dictionary<string, double> features,
        CancellationToken ct = default)
    {
        logger.LogInformation("Quantifying model uncertainty for machine {MachineId}", machineId);

        // Get recent telemetry for ensemble analysis
        var recentTelemetry = (await telemetryRepository.GetForMachineAsync(
            machineId, DateTime.UtcNow.AddDays(-7), 100, ct)).ToList();

        if (!recentTelemetry.Any())
        {
            throw new InvalidOperationException($"No recent telemetry found for machine {machineId}");
        }

        // Convert TelemetryData to FlatTelemetry for processing
        var flatTelemetry = TelemetryConverter.Convert(recentTelemetry).ToList();
        
        // Calculate aleatoric uncertainty (data noise)
        var temperatureValues = flatTelemetry.Select(t => t.Temperature).ToArray();
        var vibrationValues = flatTelemetry.Select(t => t.Vibration).ToArray();
        
        var tempStd = CalculateStandardDeviation(temperatureValues);
        var vibStd = CalculateStandardDeviation(vibrationValues);
        var aleatoricUncertainty = (tempStd + vibStd) / 2;

        // Calculate epistemic uncertainty (model uncertainty)
        // Using ensemble approach - predict with slightly different feature sets
        var ensemblePredictions = new List<double>();
        var basePrediction = await predictiveService.PredictAsync(machineId);

        for (int i = 0; i < 10; i++) // 10 ensemble members
        {
            ct.ThrowIfCancellationRequested();

            // Perturb features slightly
            var perturbedFeatures = new Dictionary<string, double>();
            foreach (var feature in features)
            {
                var noise = SampleNormal(0, feature.Value * 0.05); // 5% noise
                perturbedFeatures[feature.Key] = Math.Max(0, feature.Value + noise);
            }

            // In practice, we'd make predictions with different models
            // For now, we'll simulate ensemble variation
            var ensemblePred = basePrediction.RemainingUsefulLifeDays + SampleNormal(0, 5);
            ensemblePredictions.Add(Math.Max(0, ensemblePred));
        }

        var epistemicUncertainty = CalculateStandardDeviation(ensemblePredictions.ToArray());
        var totalUncertainty = Math.Sqrt(Math.Pow(aleatoricUncertainty, 2) + Math.Pow(epistemicUncertainty, 2));

        // Feature importance with uncertainty
        var featureImportance = new Dictionary<string, double>
        {
            ["Temperature"] = 0.3,
            ["Vibration"] = 0.4,
            ["Pressure"] = 0.2,
            ["TimeSinceMaintenance"] = 0.1
        };

        // Add uncertainty to feature importance
        var featureImportanceWithUncertainty = featureImportance.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value + SampleNormal(0, 0.05)); // ±5% uncertainty

        var result = new ModelUncertaintyResult
        {
            AleatoricUncertainty = aleatoricUncertainty,
            EpistemicUncertainty = epistemicUncertainty,
            TotalUncertainty = totalUncertainty,
            FeatureImportanceWithUncertainty = featureImportanceWithUncertainty,
            ModelConfidence = 1.0 / (1.0 + totalUncertainty / 100.0) // Scaled confidence
        };

        logger.LogInformation("Model uncertainty quantified - Aleatoric: {Aleatoric:F2}, Epistemic: {Epistemic:F2}", 
            aleatoricUncertainty, epistemicUncertainty);

        return result;
    }

    #region Private Helper Methods

    private double SampleNormal(double mean, double stdDev)
    {
        // Box-Muller transform for normal distribution sampling
        var u1 = _random.NextDouble();
        var u2 = _random.NextDouble();
        var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * z0;
    }

    private double SimulatePrediction(Dictionary<string, double> features)
    {
        // Simplified prediction simulation
        // In practice, this would call the actual ML model
        var temp = features.GetValueOrDefault("temperature", 70);
        var vib = features.GetValueOrDefault("vibration", 0.5);
        var hours = features.GetValueOrDefault("operating_hours", 1000);

        // Simple degradation model
        var degradationRate = (temp - 60) * 0.1 + vib * 10;
        var remainingHours = Math.Max(0, 5000 - hours - degradationRate * (hours / 1000));
        return remainingHours / 24.0; // Convert to days
    }

    private double CalculateFeatureSensitivity(string featureName, 
        Dictionary<string, double> baseFeatures, 
        double[] samples, 
        int iterations)
    {
        // Simplified sensitivity analysis
        var baselineMean = samples.Average();
        var perturbation = baseFeatures[featureName] * 0.1; // 10% perturbation
        
        // Count how many samples changed significantly
        var significantChanges = samples.Count(s => Math.Abs(s - baselineMean) > perturbation * 0.5);
        return (double)significantChanges / iterations;
    }

    private double CalculateLikelihood(Dictionary<string, double> observed, Dictionary<string, double> parameters)
    {
        // Simplified likelihood calculation
        var logLikelihood = 0.0;
        foreach (var obs in observed)
        {
            if (parameters.ContainsKey(obs.Key))
            {
                var diff = obs.Value - parameters[obs.Key];
                logLikelihood -= Math.Pow(diff, 2) / 2; // Gaussian likelihood
            }
        }
        return Math.Exp(logLikelihood);
    }

    private T[] Resample<T>(T[] data)
    {
        var resampled = new T[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            var index = _random.Next(data.Length);
            resampled[i] = data[index];
        }
        return resampled;
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        
        var mean = values.Average();
        var variance = values.Select(x => Math.Pow(x - mean, 2)).Average();
        return Math.Sqrt(variance);
    }

    private double CalculateCoverageProbability(double lower, double upper, double[] actualValues)
    {
        var covered = actualValues.Count(v => v >= lower && v <= upper);
        return (double)covered / actualValues.Length;
    }

    #endregion
}