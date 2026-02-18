using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.API.Services.Analytics.ML;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class BenchmarkValidationService : IBenchmarkValidationService
{
    private readonly IModelVersionRepository _modelVersionRepository;
    private readonly IBenchmarkDatasetLoader _datasetLoader;
    private readonly ILogger<BenchmarkValidationService> _logger;

    public BenchmarkValidationService(
        IModelVersionRepository modelVersionRepository,
        IBenchmarkDatasetLoader datasetLoader,
        ILogger<BenchmarkValidationService> logger)
    {
        _modelVersionRepository = modelVersionRepository;
        _datasetLoader = datasetLoader;
        _logger = logger;
    }

    public async Task<BenchmarkValidationResult> ValidateAgainstBenchmarkAsync(
        string benchmarkDataset,
        string modelVersionId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(modelVersionId, out var versionGuid))
        {
            return new BenchmarkValidationResult
            {
                ErrorMessage = $"Invalid model version ID: {modelVersionId}"
            };
        }

        var modelVersion = await _modelVersionRepository.GetAsync(versionGuid, ct);
        if (modelVersion == null)
        {
            return new BenchmarkValidationResult
            {
                ErrorMessage = $"Model version {modelVersionId} not found"
            };
        }

        return await ValidateModelAsync(
            benchmarkDataset,
            modelVersion.ModelType,
            modelVersion.ModelPath,
            ct);
    }

    public async Task<BenchmarkValidationResult> ValidateModelAsync(
        string benchmarkDataset,
        string modelType,
        string modelPath,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Validating {ModelType} model against benchmark: {Benchmark}",
            modelType, benchmarkDataset);

        var result = new BenchmarkValidationResult
        {
            BenchmarkDataset = benchmarkDataset,
            ModelType = modelType,
            ModelVersion = Path.GetFileName(modelPath)
        };

        try
        {
            // Load benchmark dataset
            var benchmarkData = await _datasetLoader.LoadDatasetAsync(benchmarkDataset, ct);
            if (benchmarkData == null || !benchmarkData.Any())
            {
                result.ErrorMessage = $"Failed to load benchmark dataset: {benchmarkDataset}";
                return result;
            }

            // Load ML.NET model
            if (!File.Exists(modelPath))
            {
                result.ErrorMessage = $"Model file not found: {modelPath}";
                return result;
            }

            var mlContext = new MLContext(seed: 0);
            var model = mlContext.Model.Load(modelPath, out var schema);

            // Prepare test data from benchmark
            var testData = PrepareTestData(benchmarkData, modelType);
            var testDataView = mlContext.Data.LoadFromEnumerable(testData);

            // Make predictions
            var predictions = model.Transform(testDataView);
            
            // Extract actual and predicted values
            var actualValues = predictions.GetColumn<float>("RulDays").ToArray();
            var predictedValues = predictions.GetColumn<float>("Score").ToArray();

            // Calculate metrics
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "RulDays");
            var mape = CalculateMape(actualValues, predictedValues);
            var mae = metrics.MeanAbsoluteError;
            var rmse = metrics.RootMeanSquaredError;
            var r2 = metrics.RSquared;

            // Create prediction comparisons
            var comparisons = new List<PredictionComparison>();
            for (int i = 0; i < actualValues.Length; i++)
            {
                var actual = actualValues[i];
                var predicted = predictedValues[i];
                var absError = Math.Abs(actual - predicted);
                var pctError = actual != 0 ? Math.Abs((actual - predicted) / actual) * 100 : 0;

                comparisons.Add(new PredictionComparison
                {
                    SampleIndex = i,
                    ActualRul = actual,
                    PredictedRul = predicted,
                    AbsoluteError = absError,
                    PercentageError = pctError
                });
            }

            result.MAPE = mape * 100; // Convert to percentage
            result.RMSE = rmse;
            result.R2 = r2;
            result.MAE = mae;
            result.MeetsRequirement = result.MAPE < 15.0; // MAPE < 15%
            result.Predictions = comparisons;
            result.DetailedMetrics = new Dictionary<string, object>
            {
                ["TotalSamples"] = actualValues.Length,
                ["MeanAbsoluteError"] = mae,
                ["RootMeanSquaredError"] = rmse,
                ["RSquared"] = r2,
                ["MeanAbsolutePercentageError"] = result.MAPE,
                ["MaxError"] = comparisons.Max(c => c.AbsoluteError),
                ["MinError"] = comparisons.Min(c => c.AbsoluteError),
                ["MedianError"] = comparisons.OrderBy(c => c.AbsoluteError)
                    .Skip(comparisons.Count / 2)
                    .First().AbsoluteError
            };

            _logger.LogInformation(
                "Validation complete: MAPE={MAPE:F2}%, RMSE={RMSE:F2}, R²={R2:F4}, MeetsRequirement={MeetsRequirement}",
                result.MAPE, result.RMSE, result.R2, result.MeetsRequirement);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating model against benchmark");
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public Task<List<string>> GetAvailableBenchmarksAsync(CancellationToken ct = default)
    {
        return _datasetLoader.GetAvailableDatasetsAsync(ct);
    }

    public Task<BenchmarkDatasetInfo> GetBenchmarkInfoAsync(
        string benchmarkDataset,
        CancellationToken ct = default)
    {
        return _datasetLoader.GetDatasetInfoAsync(benchmarkDataset, ct);
    }

    private List<RulModelInput> PrepareTestData(
        List<BenchmarkDataPoint> benchmarkData,
        string modelType)
    {
        // Convert benchmark data to model input format
        // This is a simplified conversion - actual implementation would depend on benchmark format
        var testData = new List<RulModelInput>();

        foreach (var point in benchmarkData)
        {
            testData.Add(new RulModelInput
            {
                Feature1 = (float)(point.Features.GetValueOrDefault("Feature1", 0)),
                Feature2 = (float)(point.Features.GetValueOrDefault("Feature2", 0)),
                Feature3 = (float)(point.Features.GetValueOrDefault("Feature3", 0)),
                Feature4 = (float)(point.Features.GetValueOrDefault("Feature4", 0)),
                Feature5 = (float)(point.Features.GetValueOrDefault("Feature5", 0)),
                Feature6 = (float)(point.Features.GetValueOrDefault("Feature6", 0)),
                Feature7 = (float)(point.Features.GetValueOrDefault("Feature7", 0)),
                Feature8 = (float)(point.Features.GetValueOrDefault("Feature8", 0)),
                Feature9 = (float)(point.Features.GetValueOrDefault("Feature9", 0)),
                Feature10 = (float)(point.Features.GetValueOrDefault("Feature10", 0))
            });
        }

        return testData;
    }

    private double CalculateMape(float[] actual, float[] predicted)
    {
        if (actual.Length != predicted.Length || actual.Length == 0)
            return 0;

        double sumPercentageError = 0;
        int count = 0;

        for (int i = 0; i < actual.Length; i++)
        {
            if (Math.Abs(actual[i]) > 0.001f) // Avoid division by zero
            {
                sumPercentageError += Math.Abs((actual[i] - predicted[i]) / actual[i]);
                count++;
            }
        }

        return count > 0 ? sumPercentageError / count : 0;
    }
}
