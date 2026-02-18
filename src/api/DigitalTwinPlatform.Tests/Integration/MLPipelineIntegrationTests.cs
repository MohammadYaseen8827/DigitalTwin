using System.Net;
using System.Net.Http.Json;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Entities.Enums;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinPlatform.Tests.Integration;

[Trait("Category", "Integration")]
[Collection("Sequential")]
public class MLPipelineIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly IntegrationTestFixture _fixture;

    public MLPipelineIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Complete_ML_Pipeline_From_Training_To_Prediction()
    {
        // Arrange - Create machine and generate training data
        var machineId = await CreateTestMachine();
        await GenerateTrainingData(machineId, 200); // Generate sufficient training data

        var trainingRequest = new
        {
            MachineId = machineId,
            TrainingParameters = new
            {
                Epochs = 50,
                LearningRate = 0.001,
                BatchSize = 32,
                ValidationSplit = 0.2
            },
            Features = new[] { "temperature", "vibration", "pressure", "operating_hours" },
            Target = "remaining_useful_life"
        };

        // Act & Assert - Model Training
        var trainResponse = await _client.PostAsJsonAsync("/api/ml/train", trainingRequest);
        trainResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var trainResult = await trainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        trainResult.Should().NotBeNull();
        trainResult!.ModelId.Should().NotBeEmpty();
        trainResult.Status.Should().Be("Training");

        var modelId = trainResult.ModelId;

        // Wait for training completion
        await WaitForTrainingCompletion(modelId);

        // Act & Assert - Verify trained model
        var modelResponse = await _client.GetAsync($"/api/ml/models/{modelId}");
        modelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var modelDetails = await modelResponse.Content.ReadFromJsonAsync<MLModelDetails>();
        modelDetails.Should().NotBeNull();
        modelDetails!.ModelId.Should().Be(modelId);
        modelDetails.Status.Should().Be("Trained");
        modelDetails.TrainingMetrics.Should().NotBeNull();
        modelDetails.TrainingMetrics!["mape"].Should().BeOfType<double>();

        // Act & Assert - Generate Prediction
        var predictionRequest = new
        {
            ModelId = modelId,
            Features = new Dictionary<string, double>
            {
                ["temperature"] = 75.5,
                ["vibration"] = 0.4,
                ["pressure"] = 48.2,
                ["operating_hours"] = 1250.0
            }
        };

        var predictResponse = await _client.PostAsJsonAsync("/api/ml/predict", predictionRequest);
        predictResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var prediction = await predictResponse.Content.ReadFromJsonAsync<MLPredictionResult>();
        prediction.Should().NotBeNull();
        prediction!.PredictedValue.Should().BeGreaterThan(0);
        prediction.Confidence.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(1);
        prediction.FeatureContributions.Should().NotBeEmpty();

        // Act & Assert - Validate Against Benchmark (MAPE < 15%)
        var validationResponse = await _client.PostAsJsonAsync("/api/benchmark-validation/validate", new
        {
            ModelId = modelId,
            BenchmarkDataset = "NASA_CMAPSS_FD001",
            ValidationParameters = new
            {
                TestSplit = 0.3,
                Metrics = new[] { "mape", "rmse", "r2" }
            }
        });

        validationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var validationResult = await validationResponse.Content.ReadFromJsonAsync<BenchmarkValidationResult>();
        validationResult.Should().NotBeNull();
        validationResult!.MeetsRequirement.Should().BeTrue(); // MAPE < 15%
        validationResult.MAPE.Should().BeLessThan(0.15);
    }

    [Fact]
    public async Task Model_Lifecycle_Management_Workflow()
    {
        // Arrange - Train initial model
        var machineId = await CreateTestMachine();
        await GenerateTrainingData(machineId, 150);

        var initialTrainingRequest = new
        {
            MachineId = machineId,
            ModelName = "Initial RUL Model",
            TrainingParameters = new
            {
                Epochs = 30,
                LearningRate = 0.001
            },
            Features = new[] { "temperature", "vibration" },
            Target = "rul"
        };

        // Act & Assert - Initial Training and Registration
        var trainResponse = await _client.PostAsJsonAsync("/api/ml/train", initialTrainingRequest);
        var trainResult = await trainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        var initialModelId = trainResult!.ModelId;

        await WaitForTrainingCompletion(initialModelId);

        // Act & Assert - Register Model Version
        var registerResponse = await _client.PostAsJsonAsync("/api/model-lifecycle/register", new
        {
            ModelId = initialModelId,
            Version = "1.0.0",
            Description = "Initial RUL prediction model",
            PerformanceMetrics = new
            {
                MAPE = 0.12,
                RMSE = 8.5,
                R2 = 0.88
            }
        });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registration = await registerResponse.Content.ReadFromJsonAsync<ModelRegistration>();
        registration.Should().NotBeNull();
        registration!.Version.Should().Be("1.0.0");
        registration.Status.Should().Be(ModelStatus.Development);

        var versionId = registration.VersionId;

        // Act & Assert - Promote to Staging
        var promoteToStagingResponse = await _client.PostAsync($"/api/model-lifecycle/{versionId}/promote/staging", null);
        promoteToStagingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stagingStatus = await GetModelVersionStatus(versionId);
        stagingStatus.Should().Be(ModelStatus.Staging);

        // Act & Assert - Performance Testing in Staging
        await SimulateStagingPerformanceTesting(versionId, machineId);

        // Act & Assert - Promote to Production
        var promoteToProdResponse = await _client.PostAsync($"/api/model-lifecycle/{versionId}/promote/production", null);
        promoteToProdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var prodStatus = await GetModelVersionStatus(versionId);
        prodStatus.Should().Be(ModelStatus.Production);

        // Act & Assert - Verify Production Model Serving
        var prodModelResponse = await _client.GetAsync($"/api/ml/models/production?machineType=CNC");
        prodModelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var prodModels = await prodModelResponse.Content.ReadFromJsonAsync<List<MLModelDetails>>();
        prodModels.Should().NotBeNull();
        prodModels!.Should().Contain(m => m.VersionId == versionId);

        // Act & Assert - Model Replacement Workflow
        var newTrainingRequest = new
        {
            MachineId = machineId,
            ModelName = "Improved RUL Model",
            TrainingParameters = new
            {
                Epochs = 40,
                LearningRate = 0.0005
            },
            Features = new[] { "temperature", "vibration", "pressure" }, // Additional feature
            Target = "rul"
        };

        var newTrainResponse = await _client.PostAsJsonAsync("/api/ml/train", newTrainingRequest);
        var newTrainResult = await newTrainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        var newModelId = newTrainResult!.ModelId;

        await WaitForTrainingCompletion(newModelId);

        // Register and promote new model
        var newRegisterResponse = await _client.PostAsJsonAsync("/api/model-lifecycle/register", new
        {
            ModelId = newModelId,
            Version = "1.1.0",
            ParentVersionId = versionId,
            Description = "Enhanced model with pressure feature",
            PerformanceMetrics = new
            {
                MAPE = 0.09, // Better performance
                RMSE = 6.2,
                R2 = 0.92
            }
        });

        var newRegistration = await newRegisterResponse.Content.ReadFromJsonAsync<ModelRegistration>();
        var newVersionId = newRegistration!.VersionId;

        await _client.PostAsync($"/api/model-lifecycle/{newVersionId}/promote/staging", null);
        await SimulateStagingPerformanceTesting(newVersionId, machineId);
        await _client.PostAsync($"/api/model-lifecycle/{newVersionId}/promote/production", null);

        // Verify new model is now serving
        var updatedProdResponse = await _client.GetAsync($"/api/ml/models/production?machineType=CNC");
        var updatedProdModels = await updatedProdResponse.Content.ReadFromJsonAsync<List<MLModelDetails>>();
        updatedProdModels.Should().Contain(m => m.VersionId == newVersionId);
        updatedProdModels.Should().NotContain(m => m.VersionId == versionId); // Old model should be deprecated
    }

    [Fact]
    public async Task SHAP_Explanation_Integration_Works_Correctly()
    {
        // Arrange - Train model and generate prediction
        var machineId = await CreateTestMachine();
        await GenerateTrainingData(machineId, 100);

        var trainingRequest = new
        {
            MachineId = machineId,
            Features = new[] { "temperature", "vibration", "pressure" },
            Target = "rul"
        };

        var trainResponse = await _client.PostAsJsonAsync("/api/ml/train", trainingRequest);
        var trainResult = await trainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        var modelId = trainResult!.ModelId;

        await WaitForTrainingCompletion(modelId);

        var predictionRequest = new
        {
            ModelId = modelId,
            Features = new Dictionary<string, double>
            {
                ["temperature"] = 78.0,
                ["vibration"] = 0.5,
                ["pressure"] = 52.0
            }
        };

        var predictResponse = await _client.PostAsJsonAsync("/api/ml/predict", predictionRequest);
        var prediction = await predictResponse.Content.ReadFromJsonAsync<MLPredictionResult>();

        // Act - Get SHAP Explanation
        var shapRequest = new
        {
            ModelId = modelId,
            PredictionId = prediction!.PredictionId,
            ExplanationType = "SHAP",
            BackgroundDataSize = 50
        };

        var explanationResponse = await _client.PostAsJsonAsync("/api/xai/explain", shapRequest);
        explanationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var explanation = await explanationResponse.Content.ReadFromJsonAsync<XAIExplanation>();
        explanation.Should().NotBeNull();
        explanation!.ExplanationType.Should().Be("SHAP");
        explanation.FeatureImportance.Should().NotBeEmpty();
        explanation.BaseValue.Should().BeGreaterThan(0);

        // Verify feature importance structure
        explanation.FeatureImportance.Should().ContainKey("temperature");
        explanation.FeatureImportance.Should().ContainKey("vibration");
        explanation.FeatureImportance.Should().ContainKey("pressure");

        // Sum of SHAP values should approximately equal prediction - base value
        var shapSum = explanation.FeatureImportance.Values.Sum();
        var predictionDifference = Math.Abs(prediction.PredictedValue - (explanation.BaseValue + shapSum));
        predictionDifference.Should().BeLessThan(1.0); // Allow small numerical differences

        // Act - Get Global Feature Importance
        var globalImportanceResponse = await _client.GetAsync($"/api/xai/global-importance/{modelId}");
        globalImportanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var globalImportance = await globalImportanceResponse.Content.ReadFromJsonAsync<GlobalFeatureImportance>();
        globalImportance.Should().NotBeNull();
        globalImportance!.FeatureImportance.Should().NotBeEmpty();
        globalImportance.SampleSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Synthetic_Data_Generation_Integrates_With_ML_Training()
    {
        // Arrange - Generate synthetic data using degradation models
        var machineId = await CreateTestMachine();
        
        var synthDataRequest = new
        {
            MachineId = machineId,
            GenerationParameters = new
            {
                SampleCount = 500,
                TimeHorizonHours = 1000,
                SamplingFrequencyHz = 0.1,
                NoiseLevel = 0.05
            },
            DegradationModels = new object[]
            {
                new
                {
                    Name = "bearing_degradation",
                    Type = "Exponential",
                    Parameters = new
                    {
                        Lambda = 0.0008,
                        EnvironmentalFactor = 1.2
                    }
                },
                new
                {
                    Name = "thermal_degradation",
                    Type = "Linear",
                    Parameters = new
                    {
                        Rate = 0.0002,
                        Threshold = 80.0
                    }
                }
            }
        };

        // Act & Assert - Generate Synthetic Data
        var generateResponse = await _client.PostAsJsonAsync("/api/synthetic-data/generate", synthDataRequest);
        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var generateResult = await generateResponse.Content.ReadFromJsonAsync<SyntheticDataGenerationResult>();
        generateResult.Should().NotBeNull();
        generateResult!.DatasetId.Should().NotBeEmpty();
        generateResult.GeneratedSamples.Should().Be(500);

        var datasetId = generateResult.DatasetId;

        // Act & Assert - Validate Synthetic Data Quality
        var validateResponse = await _client.PostAsJsonAsync("/api/synthetic-data/validate", new
        {
            DatasetId = datasetId,
            ValidationCriteria = new
            {
                StatisticalTests = new[] { "ks_test", "anderson_darling" },
                DistributionSimilarity = 0.8,
                TemporalConsistency = true
            }
        });

        validateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var validationResult = await validateResponse.Content.ReadFromJsonAsync<SyntheticDataValidationResult>();
        validationResult.Should().NotBeNull();
        validationResult!.IsValid.Should().BeTrue();
        validationResult.QualityScore.Should().BeGreaterOrEqualTo(0.8);

        // Act & Assert - Use Synthetic Data for ML Training
        var trainingRequest = new
        {
            DatasetId = datasetId, // Use synthetic dataset
            MachineId = machineId,
            TrainingParameters = new
            {
                Epochs = 25,
                ValidationSplit = 0.25
            },
            Features = new[] { "temperature", "vibration", "health_indicator" },
            Target = "remaining_hours"
        };

        var trainResponse = await _client.PostAsJsonAsync("/api/ml/train-from-dataset", trainingRequest);
        trainResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var trainResult = await trainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        trainResult.Should().NotBeNull();
        trainResult!.Status.Should().Be("Training");

        await WaitForTrainingCompletion(trainResult.ModelId);

        // Verify model trained successfully on synthetic data
        var modelResponse = await _client.GetAsync($"/api/ml/models/{trainResult.ModelId}");
        var modelDetails = await modelResponse.Content.ReadFromJsonAsync<MLModelDetails>();
        modelDetails.Should().NotBeNull();
        modelDetails!.TrainingMetrics.Should().ContainKey("mape");
        modelDetails.TrainingMetrics!["mape"].Should().BeOfType<double>();
    }

    [Fact]
    public async Task Model_Drift_Detection_And_Retraining_Workflow()
    {
        // Arrange - Train initial model
        var machineId = await CreateTestMachine();
        await GenerateTrainingData(machineId, 200);

        var initialTrainResponse = await _client.PostAsJsonAsync("/api/ml/train", new
        {
            MachineId = machineId,
            Features = new[] { "temperature", "vibration" },
            Target = "rul"
        });

        var initialTrainResult = await initialTrainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        var initialModelId = initialTrainResult!.ModelId;
        await WaitForTrainingCompletion(initialModelId);

        // Register and promote to production
        await _client.PostAsJsonAsync("/api/model-lifecycle/register", new
        {
            ModelId = initialModelId,
            Version = "1.0.0"
        });

        var registerResponse = await _client.PostAsJsonAsync("/api/model-lifecycle/register", new
        {
            ModelId = initialModelId,
            Version = "1.0.0",
            PerformanceMetrics = new { MAPE = 0.12 }
        });

        var registration = await registerResponse.Content.ReadFromJsonAsync<ModelRegistration>();
        await _client.PostAsync($"/api/model-lifecycle/{registration!.VersionId}/promote/production", null);

        // Act - Simulate data drift by generating different distribution data
        await GenerateDriftedData(machineId, 100); // Different data distribution

        // Act & Assert - Detect Model Drift
        var driftDetectionRequest = new
        {
            MachineId = machineId,
            ModelId = initialModelId,
            ReferencePeriodDays = 30,
            CurrentPeriodDays = 7,
            DriftThresholds = new
            {
                KS_Test = 0.05,
                Wasserstein = 0.1,
                PSI = 0.1
            }
        };

        var driftResponse = await _client.PostAsJsonAsync("/api/drift/detect", driftDetectionRequest);
        driftResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var driftResult = await driftResponse.Content.ReadFromJsonAsync<DriftDetectionResult>();
        driftResult.Should().NotBeNull();
        driftResult!.IsDriftDetected.Should().BeTrue();
        driftResult.DriftSeverity.Should().Be(DriftSeverity.High);
        driftResult.RecommendedAction.Should().Be("Retrain_Model");

        // Act & Assert - Trigger Automated Retraining
        var retrainResponse = await _client.PostAsJsonAsync("/api/ml/retrain", new
        {
            MachineId = machineId,
            Trigger = "Drift_Detected",
            DriftDetectionId = driftResult.DetectionId,
            TrainingParameters = new
            {
                Epochs = 35,
                IncludeHistoricalData = true
            }
        });

        retrainResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrainResult = await retrainResponse.Content.ReadFromJsonAsync<ModelTrainingResult>();
        retrainResult.Should().NotBeNull();
        retrainResult!.ModelId.Should().NotBe(initialModelId); // Should be new model

        await WaitForTrainingCompletion(retrainResult.ModelId);

        // Verify new model performs better on drifted data
        var newModelResponse = await _client.GetAsync($"/api/ml/models/{retrainResult.ModelId}");
        var newModelDetails = await newModelResponse.Content.ReadFromJsonAsync<MLModelDetails>();
        
        var oldModelResponse = await _client.GetAsync($"/api/ml/models/{initialModelId}");
        var oldModelDetails = await oldModelResponse.Content.ReadFromJsonAsync<MLModelDetails>();

        // New model should have better MAPE on current data
        var newMape = (double)newModelDetails!.TrainingMetrics!["mape"];
        var oldMape = (double)oldModelDetails!.TrainingMetrics!["mape"];
        newMape.Should().BeLessThan(oldMape);
    }

    #region Helper Methods

    private async Task<Guid> CreateTestMachine()
    {
        var machine = new
        {
            Name = "ML Test Machine " + DateTime.Now.Ticks,
            Type = "CNC",
            SerialNumber = "ML-" + Guid.NewGuid().ToString("N")[..8],
            Manufacturer = "TestCorp",
            Model = "TC-ML-1000",
            InstallationDate = DateTime.UtcNow.AddYears(-1),
            Location = "ML Lab",
            Criticality = 4,
            Status = "Operational"
        };

        var response = await _client.PostAsJsonAsync("/api/machines", machine);
        response.EnsureSuccessStatusCode();
        
        var createdMachine = await response.Content.ReadFromJsonAsync<MachineDto>();
        return createdMachine!.Id;
    }

    private async Task GenerateTrainingData(Guid machineId, int sampleCount)
    {
        var random = new Random();
        
        for (int i = 0; i < sampleCount; i++)
        {
            var hours = i * 10; // Incremental hours
            var baseTemp = 70 + (hours / 1000.0) * 5; // Gradually increasing temperature
            
            var telemetry = new
            {
                MachineId = machineId,
                Timestamp = DateTime.UtcNow.AddHours(-sampleCount * 10).AddHours(i * 10),
                Temperature = baseTemp + random.NextDouble() * 10 - 5, // ±5° variation
                Vibration = 0.2 + (hours / 5000.0) + random.NextDouble() * 0.3, // Increasing vibration
                Pressure = 45 + random.NextDouble() * 10,
                OperatingHours = (double)hours,
                RUL_Label = Math.Max(0, 200 - hours / 8.0) // Synthetic RUL label
            };

            await _client.PostAsJsonAsync("/api/telemetry", telemetry);
        }
        
        await Task.Delay(1000); // Allow processing time
    }

    private async Task GenerateDriftedData(Guid machineId, int sampleCount)
    {
        var random = new Random();
        
        for (int i = 0; i < sampleCount; i++)
        {
            // Different distribution pattern to simulate drift
            var telemetry = new
            {
                MachineId = machineId,
                Timestamp = DateTime.UtcNow.AddHours(-sampleCount).AddHours(i),
                Temperature = 85 + random.NextDouble() * 15, // Higher baseline temperature
                Vibration = 0.8 + random.NextDouble() * 0.5, // Higher baseline vibration
                Pressure = 55 + random.NextDouble() * 15, // Higher pressure
                OperatingHours = (double)(i * 15),
                RUL_Label = Math.Max(0, 150 - i * 2.0) // Faster degradation
            };

            await _client.PostAsJsonAsync("/api/telemetry", telemetry);
        }
        
        await Task.Delay(1000);
    }

    private async Task WaitForTrainingCompletion(Guid modelId)
    {
        var maxWaitTime = TimeSpan.FromSeconds(60);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var response = await _client.GetAsync($"/api/ml/models/{modelId}");
            if (response.IsSuccessStatusCode)
            {
                var modelDetails = await response.Content.ReadFromJsonAsync<MLModelDetails>();
                if (modelDetails?.Status == "Trained" || modelDetails?.Status == "Failed")
                    return;
            }
            
            await Task.Delay(2000);
        }

        throw new TimeoutException($"Model training did not complete within timeout period");
    }

    private async Task<ModelStatus> GetModelVersionStatus(Guid versionId)
    {
        var response = await _client.GetAsync($"/api/model-lifecycle/version/{versionId}");
        response.EnsureSuccessStatusCode();
        var version = await response.Content.ReadFromJsonAsync<ModelVersionDetails>();
        return version!.Status;
    }

    private async Task SimulateStagingPerformanceTesting(Guid versionId, Guid machineId)
    {
        // Simulate performance testing in staging environment
        await Task.Delay(3000); // Simulate testing time
        
        // Log some test predictions
        await _client.PostAsJsonAsync("/api/ml/test-predictions", new
        {
            VersionId = versionId,
            TestCases = 50,
            MachineId = machineId
        });
    }

    #endregion
}

#region DTO Classes for ML Testing

public class ModelTrainingResult
{
    public Guid ModelId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string? Message { get; set; }
}

public class MLModelDetails
{
    public Guid ModelId { get; set; }
    public Guid VersionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> TrainingMetrics { get; set; } = [];
    public DateTime TrainedAt { get; set; }
}

public class MLPredictionResult
{
    public Guid PredictionId { get; set; }
    public double PredictedValue { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, double> FeatureContributions { get; set; } = [];
    public PredictionIntervals Intervals { get; set; } = new();
}

public class PredictionIntervals
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double ConfidenceLevel { get; set; }
}

public class BenchmarkValidationResult
{
    public double MAPE { get; set; }
    public double RMSE { get; set; }
    public double R2 { get; set; }
    public bool MeetsRequirement { get; set; }
    public Dictionary<string, object> DetailedMetrics { get; set; } = [];
}

public class ModelRegistration
{
    public Guid VersionId { get; set; }
    public string Version { get; set; } = string.Empty;
    public ModelStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
}

public enum ModelStatus
{
    Development,
    Staging,
    Production,
    Deprecated
}

public class ModelVersionDetails
{
    public Guid VersionId { get; set; }
    public string Version { get; set; } = string.Empty;
    public ModelStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ParentVersionId { get; set; }
}

public class XAIExplanation
{
    public string ExplanationType { get; set; } = string.Empty;
    public Dictionary<string, double> FeatureImportance { get; set; } = [];
    public double BaseValue { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class GlobalFeatureImportance
{
    public Dictionary<string, double> FeatureImportance { get; set; } = [];
    public int SampleSize { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class SyntheticDataGenerationResult
{
    public Guid DatasetId { get; set; }
    public int GeneratedSamples { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class SyntheticDataValidationResult
{
    public bool IsValid { get; set; }
    public double QualityScore { get; set; }
    public Dictionary<string, object> TestResults { get; set; } = [];
}

public class DriftDetectionResult
{
    public Guid DetectionId { get; set; }
    public bool IsDriftDetected { get; set; }
    public DriftSeverity DriftSeverity { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public Dictionary<string, double> DriftMetrics { get; set; } = [];
}

public enum DriftSeverity
{
    Low,
    Medium,
    High,
    Critical
}

#endregion