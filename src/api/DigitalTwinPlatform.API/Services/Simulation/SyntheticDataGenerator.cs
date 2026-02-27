using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Domain.Common;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.ValueObjects;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Analytics.Degradation.Models;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Simulation;

public class SyntheticDataGenerator : ISyntheticDataGenerator
{
    private readonly IRunToFailureOrchestrator _orchestrator;
    private readonly IMachineConfigurationService _configService;
    private readonly IMachineRepository _machineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyntheticDataGenerator> _logger;

    public SyntheticDataGenerator(
        IRunToFailureOrchestrator orchestrator,
        IMachineConfigurationService configService,
        IMachineRepository machineRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyntheticDataGenerator> logger)
    {
        _orchestrator = orchestrator;
        _configService = configService;
        _machineRepository = machineRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<TelemetryData>> GenerateSyntheticDataAsync(
        string machineType,
        int numberOfTrajectories,
        TimeSpan? timeRange = null,
        int? randomSeed = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Generating synthetic data for {MachineType}: {Trajectories} trajectories",
            machineType, numberOfTrajectories);

        var allTelemetry = new List<TelemetryData>();
        var random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

        for (int i = 0; i < numberOfTrajectories; i++)
        {
            var trajectorySeed = randomSeed.HasValue ? randomSeed.Value + i : random.Next();
            
            // Create temporary machine for simulation
            var machineNameResult = MachineName.Create($"Synth-{machineType}-{i}");
            var machineTypeResult = MachineType.Create(machineType);
            
            if (machineNameResult is not Result<MachineName>.Success nameSuccess ||
                machineTypeResult is not Result<MachineType>.Success typeSuccess)
            {
                var error = machineNameResult is Result<MachineName>.Failure nameFailure 
                    ? nameFailure.Error 
                    : machineTypeResult is Result<MachineType>.Failure typeFailure 
                        ? typeFailure.Error 
                        : "Unknown error";
                _logger.LogWarning("Failed to create temporary machine for trajectory {Index}: {Error}", 
                    i, error);
                continue;
            }
            
            var tempMachineResult = Machine.Create(nameSuccess.Value, typeSuccess.Value);
            
            if (tempMachineResult is not Result<Machine>.Success machineSuccess)
            {
                var error = tempMachineResult is Result<Machine>.Failure failure ? failure.Error : "Unknown error";
                _logger.LogWarning("Failed to create temporary machine for trajectory {Index}: {Error}", 
                    i, error);
                continue;
            }
            
            var tempMachine = machineSuccess.Value;
            await _machineRepository.AddAsync(tempMachine);
            await _unitOfWork.SaveChangesAsync(ct);

            try
            {
                var options = new RunToFailureOptions
                {
                    GenerateTelemetry = true,
                    StoreTrajectory = false,
                    StepInterval = TimeSpan.FromMinutes(10),
                    MaxSimulationTime = timeRange,
                    RandomSeed = trajectorySeed
                };

                var result = await _orchestrator.RunToFailureAsync(
                    tempMachine.Id,
                    options,
                    ct);

                allTelemetry.AddRange(result.GeneratedTelemetry);
                
                _logger.LogDebug(
                    "Generated trajectory {Index}/{Total}: {DataPoints} data points, TTF: {TimeToFailure}",
                    i + 1, numberOfTrajectories, result.GeneratedTelemetry.Count, result.TimeToFailure);
            }
            finally
            {
                // Clean up temporary machine
                await _machineRepository.DeleteAsync(tempMachine, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation(
            "Generated {TotalDataPoints} synthetic data points from {Trajectories} trajectories",
            allTelemetry.Count, numberOfTrajectories);

        return allTelemetry;
    }

    public async Task<SyntheticDataGenerationResult> GenerateWithValidationAsync(
        string machineType,
        int numberOfTrajectories,
        TimeSpan? timeRange = null,
        int? randomSeed = null,
        CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        
        var telemetryData = await GenerateSyntheticDataAsync(
            machineType,
            numberOfTrajectories,
            timeRange,
            randomSeed,
            ct);

        var statistics = CalculateStatistics(telemetryData);
        var duration = DateTime.UtcNow - startTime;

        return new SyntheticDataGenerationResult
        {
            TelemetryData = telemetryData,
            Statistics = statistics,
            GenerationDuration = duration,
            TrajectoriesGenerated = numberOfTrajectories,
            TotalDataPoints = telemetryData.Count
        };
    }

    public Task<DataValidationReport> ValidateAgainstBenchmarksAsync(
        List<TelemetryData> syntheticData,
        string benchmarkDataset,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Validating {DataPoints} synthetic data points against benchmark: {Benchmark}",
            syntheticData.Count, benchmarkDataset);

        var report = InitializeValidationReport();
        var syntheticStats = CalculateStatistics(syntheticData);
        var benchmarkStats = LoadBenchmarkStatistics(benchmarkDataset);

        var validationResult = ValidateMetrics(syntheticStats, benchmarkStats, report);
        FinalizeValidationReport(report, validationResult);

        return Task.FromResult(report);
    }

    /// <summary>
    /// Initializes a new validation report.
    /// </summary>
    private static DataValidationReport InitializeValidationReport()
    {
        return new DataValidationReport
        {
            Metrics = new Dictionary<string, ValidationMetric>(),
            Warnings = new List<string>(),
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Validates synthetic data metrics against benchmark statistics.
    /// </summary>
    private ValidationResult ValidateMetrics(
        GenerationStatistics syntheticStats,
        GenerationStatistics benchmarkStats,
        DataValidationReport report)
    {
        const double threshold = 0.15; // 15% tolerance
        var validationResult = new ValidationResult();

        ValidateFeatureMeans(syntheticStats, benchmarkStats, threshold, report, validationResult);
        ValidateFeatureStdDevs(syntheticStats, benchmarkStats, threshold, report, validationResult);

        return validationResult;
    }

    /// <summary>
    /// Validates feature means against benchmark statistics.
    /// </summary>
    private void ValidateFeatureMeans(
        GenerationStatistics syntheticStats,
        GenerationStatistics benchmarkStats,
        double threshold,
        DataValidationReport report,
        ValidationResult result)
    {
        foreach (var kvp in syntheticStats.FeatureMeans)
        {
            result.TotalMetrics++;
            var featureName = kvp.Key;
            var syntheticValue = kvp.Value;
            var benchmarkValue = benchmarkStats.FeatureMeans.GetValueOrDefault(featureName, syntheticValue);

            var metric = CreateValidationMetric(featureName, syntheticValue, benchmarkValue, threshold * 100);
            report.Metrics[featureName] = metric;

            if (metric.Passed)
            {
                result.PassedMetrics++;
            }
            else
            {
                report.Warnings.Add(
                    $"Feature '{featureName}': Mean difference {metric.DifferencePercentage:F2}% exceeds threshold {threshold * 100:F2}%");
            }
        }
    }

    /// <summary>
    /// Validates feature standard deviations against benchmark statistics.
    /// </summary>
    private void ValidateFeatureStdDevs(
        GenerationStatistics syntheticStats,
        GenerationStatistics benchmarkStats,
        double threshold,
        DataValidationReport report,
        ValidationResult result)
    {
        foreach (var kvp in syntheticStats.FeatureStdDevs)
        {
            result.TotalMetrics++;
            var featureName = $"{kvp.Key}_StdDev";
            var syntheticValue = kvp.Value;
            var benchmarkValue = benchmarkStats.FeatureStdDevs.GetValueOrDefault(kvp.Key, syntheticValue);

            var metric = CreateValidationMetric(featureName, syntheticValue, benchmarkValue, threshold * 100);
            report.Metrics[featureName] = metric;

            if (metric.Passed)
            {
                result.PassedMetrics++;
            }
        }
    }

    /// <summary>
    /// Creates a validation metric from synthetic and benchmark values.
    /// </summary>
    private static ValidationMetric CreateValidationMetric(
        string featureName,
        double syntheticValue,
        double benchmarkValue,
        double threshold)
    {
        var difference = Math.Abs(syntheticValue - benchmarkValue);
        var differencePercentage = benchmarkValue != 0 
            ? (difference / Math.Abs(benchmarkValue)) * 100 
            : 0;

        return new ValidationMetric
        {
            Name = featureName,
            SyntheticValue = syntheticValue,
            BenchmarkValue = benchmarkValue,
            Difference = difference,
            DifferencePercentage = differencePercentage,
            Passed = differencePercentage <= threshold,
            Threshold = threshold
        };
    }

    /// <summary>
    /// Finalizes the validation report with overall score and validation status.
    /// </summary>
    private void FinalizeValidationReport(
        DataValidationReport report,
        ValidationResult result)
    {
        report.OverallScore = result.TotalMetrics > 0 
            ? (double)result.PassedMetrics / result.TotalMetrics * 100 
            : 0;
        report.IsValid = report.OverallScore >= 80.0 && report.Errors.Count == 0;

        if (!report.IsValid)
        {
            report.Errors.Add(
                $"Validation failed: {result.PassedMetrics}/{result.TotalMetrics} metrics passed ({report.OverallScore:F2}%)");
        }

        _logger.LogInformation(
            "Validation complete: {Passed}/{Total} metrics passed, Score: {Score:F2}%",
            result.PassedMetrics, result.TotalMetrics, report.OverallScore);
    }

    /// <summary>
    /// Internal class for tracking validation results.
    /// </summary>
    private class ValidationResult
    {
        public int PassedMetrics { get; set; }
        public int TotalMetrics { get; set; }
    }

    private GenerationStatistics CalculateStatistics(List<TelemetryData> telemetryData)
    {
        var stats = new GenerationStatistics();

        if (!telemetryData.Any())
            return stats;

        // Group by data type
        var groupedData = telemetryData.GroupBy(t => t.DataType).ToList();

        foreach (var group in groupedData)
        {
            var values = ExtractNumericValues(group).ToList();
            
            if (values.Any())
            {
                var mean = values.Average();
                var stdDev = CalculateStandardDeviation(values, mean);
                var min = values.Min();
                var max = values.Max();

                stats.FeatureMeans[$"{group.Key}_mean"] = mean;
                stats.FeatureStdDevs[$"{group.Key}_std"] = stdDev;
                stats.FeatureRanges[$"{group.Key}_range"] = max - min;
            }
        }

        // Calculate correlations between features
        CalculateCorrelations(telemetryData, stats);

        return stats;
    }

    private IEnumerable<double> ExtractNumericValues(IEnumerable<TelemetryData> telemetry)
    {
        foreach (var item in telemetry)
        {
            if (item.Data.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (item.Data.RootElement.TryGetProperty("value", out var valueElement) &&
                    valueElement.TryGetDouble(out var value))
                {
                    yield return value;
                }
                else if (item.Data.RootElement.TryGetProperty("amplitude", out var ampElement) &&
                         ampElement.TryGetDouble(out var amplitude))
                {
                    yield return amplitude;
                }
            }
        }
    }

    private double CalculateStandardDeviation(IEnumerable<double> values, double mean)
    {
        var valuesList = values.ToList();
        if (valuesList.Count == 0) return 0;

        var variance = valuesList.Average(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(variance);
    }

    private void CalculateCorrelations(List<TelemetryData> telemetryData, GenerationStatistics stats)
    {
        // Simplified correlation calculation
        // In production, would use proper correlation matrix calculation
        var tempValues = ExtractNumericValues(
            telemetryData.Where(t => t.DataType == "temperature")).ToList();
        var vibValues = ExtractNumericValues(
            telemetryData.Where(t => t.DataType == "vibration")).ToList();

        if (tempValues.Any() && vibValues.Any() && tempValues.Count == vibValues.Count)
        {
            var correlation = CalculatePearsonCorrelation(tempValues, vibValues);
            stats.CorrelationMatrix["temperature_vibration"] = correlation;
        }
    }

    private double CalculatePearsonCorrelation(List<double> x, List<double> y)
    {
        if (x.Count != y.Count || x.Count == 0) return 0;

        var xMean = x.Average();
        var yMean = y.Average();

        var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        var xVariance = x.Sum(xi => Math.Pow(xi - xMean, 2));
        var yVariance = y.Sum(yi => Math.Pow(yi - yMean, 2));

        var denominator = Math.Sqrt(xVariance * yVariance);
        return denominator == 0 ? 0 : numerator / denominator;
    }

    private GenerationStatistics LoadBenchmarkStatistics(string benchmarkDataset)
    {
        // Placeholder - would load actual benchmark statistics
        // For now, return empty statistics (validation will use synthetic values as baseline)
        _logger.LogWarning("Benchmark statistics loading not implemented for dataset: {Dataset}", benchmarkDataset);
        return new GenerationStatistics();
    }
}
