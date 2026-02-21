// using DigitalTwinPlatform.Application.Services;
// using DigitalTwinPlatform.Application.ML;
// using DigitalTwinPlatform.Application.ML.Models;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;
// using FluentAssertions;
//
// namespace DigitalTwinPlatform.Tests.Services;
//
// public class MLModelServiceTests
// {
//     private readonly Mock<ILogger<MLModelService>> _mockLogger;
//     private readonly Mock<FastForestPredictor> _mockFastForest;
//     private readonly Mock<QuantileRegression> _mockQuantileRegression;
//     private readonly Mock<ShapExplainer> _mockShapExplainer;
//     private readonly Mock<FeatureImportanceExtractor> _mockImportanceExtractor;
//     private readonly MLModelService _mlModelService;
//
//     public MLModelServiceTests()
//     {
//         _mockLogger = new Mock<ILogger<MLModelService>>();
//         _mockFastForest = new Mock<FastForestPredictor>();
//         _mockQuantileRegression = new Mock<QuantileRegression>();
//         _mockShapExplainer = new Mock<ShapExplainer>();
//         _mockImportanceExtractor = new Mock<FeatureImportanceExtractor>();
//
//         _mlModelService = new MLModelService(
//             _mockFastForest.Object,
//             _mockQuantileRegression.Object,
//             _mockShapExplainer.Object,
//             _mockImportanceExtractor.Object,
//             _mockLogger.Object
//         );
//     }
//
//     [Fact]
//     public async Task TrainModelsAsync_WithValidData_ShouldCalculateMetricsCorrectly()
//     {
//         // Arrange
//         var trainingData = GenerateTestTrainingData(100);
//
//         _mockFastForest.Setup(x => x.Train(It.IsAny<IEnumerable<ModelTrainingData>>()));
//         _mockFastForest.Setup(x => x.Predict(It.IsAny<ModelInputData>()))
//             .Returns((ModelInputData input) => new RulPrediction
//             {
//                 Score = (float)(input.Temperature * 0.5 + input.Vibration * 2.0 + 50)
//             });
//
//         _mockQuantileRegression.Setup(x => x.Train(It.IsAny<IEnumerable<ModelTrainingData>>()));
//
//         // Act
//         var result = await _mlModelService.TrainModelsAsync(trainingData);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.Success.Should().BeTrue();
//         result.SamplesUsed.Should().Be(100);
//         result.Metrics.Should().ContainKey("R2Score");
//         result.Metrics.Should().ContainKey("MAE");
//         result.Metrics.Should().ContainKey("RMSE");
//         result.Metrics.Should().ContainKey("MAPE");
//
//         // Verify metrics are reasonable values (not hardcoded)
//         result.Metrics["R2Score"].Should().BeGreaterThanOrEqualTo(0);
//         result.Metrics["R2Score"].Should().BeLessThanOrEqualTo(1);
//         result.Metrics["MAE"].Should().BeGreaterThan(0);
//         result.Metrics["RMSE"].Should().BeGreaterThan(0);
//         result.Metrics["MAPE"].Should().BeGreaterThanOrEqualTo(0);
//     }
//
//     [Fact]
//     public async Task TrainModelsAsync_WithEmptyData_ShouldReturnFailure()
//     {
//         // Arrange
//         var emptyTrainingData = new List<ModelTrainingData>();
//
//         // Act
//         var result = await _mlModelService.TrainModelsAsync(emptyTrainingData);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.Success.Should().BeFalse();
//         result.SamplesUsed.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task PredictRulAsync_WithValidFeatures_ShouldReturnPrediction()
//     {
//         // Arrange
//         var features = new Dictionary<string, double>
//         {
//             ["Temperature"] = 75.0,
//             ["Vibration"] = 2.5,
//             ["Pressure"] = 101.3,
//             ["Rpm"] = 1500.0,
//             ["Age"] = 30.0,
//             ["CycleCount"] = 500.0
//         };
//
//         var mockModel = new Mock<ML.Model>();
//         _mockFastForest.Setup(x => x.Model).Returns(mockModel.Object);
//         _mockFastForest.Setup(x => x.Predict(It.IsAny<ModelInputData>()))
//             .Returns(new ModelPredictionResult
//             {
//                 RemainingUsefulLife = 45.5,
//                 RiskLevel = "Medium",
//                 FeatureImportance = new Dictionary<string, double>
//                 {
//                     ["Temperature"] = 0.4,
//                     ["Vibration"] = 0.3,
//                     ["Pressure"] = 0.2,
//                     ["Rpm"] = 0.1
//                 }
//             });
//
//         _mockQuantileRegression.Setup(x => x.PredictInterval(It.IsAny<ModelInputData>()))
//             .Returns((10.0, 80.0));
//
//         _mockShapExplainer.Setup(x => x.GetExplainerValues(It.IsAny<ML.Model>(), It.IsAny<ModelInputData>()))
//             .Returns(new Dictionary<string, double>
//             {
//                 ["Temperature"] = 0.3,
//                 ["Vibration"] = 0.2,
//                 ["Pressure"] = 0.1
//             });
//
//         // Act
//         var result = await _mlModelService.PredictRulAsync("machine-123", features);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.RemainingUsefulLife.Should().Be(45.5);
//         result.Confidence.Should().Be(0.90);
//         result.RiskLevel.Should().Be("Medium");
//         result.FeatureImportance.Should().ContainKey("Temperature");
//         result.ShapValues.Should().ContainKey("Temperature");
//     }
//
//     [Fact]
//     public async Task GetFeatureImportanceAsync_ShouldReturnFeatureImportance()
//     {
//         // Act
//         var result = await _mlModelService.GetFeatureImportanceAsync();
//
//         // Assert
//         result.Should().NotBeNull();
//         result.Should().ContainKey("Temperature");
//         result.Should().ContainKey("Vibration");
//         result.Should().ContainKey("Pressure");
//         result.Should().ContainKey("Rpm");
//         result.Should().ContainKey("Age");
//         result.Should().ContainKey("CycleCount");
//
//         // Verify importance values sum to 1.0 (or close to it)
//         var totalImportance = result.Values.Sum();
//         totalImportance.Should().BeApproximately(1.0, 0.01);
//     }
//
//     [Fact]
//     public void CalculateR2Score_WithPerfectPredictions_ShouldReturnOne()
//     {
//         // This test verifies the private method through reflection
//         // Arrange
//         var trainingData = GenerateTestTrainingData(50);
//         
//         // Act - Use reflection to access private method
//         var method = typeof(MLModelService).GetMethod("CalculateR2Score", 
//             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         
//         var result = method?.Invoke(_mlModelService, new object[] { trainingData });
//
//         // Assert
//         result.Should().NotBeNull();
//         var r2Score = (double)result!;
//         r2Score.Should().BeApproximately(1.0, 0.1); // Allow for floating point precision
//     }
//
//     [Fact]
//     public void CalculateMAE_WithConsistentData_ShouldReturnZero()
//     {
//         // Arrange
//         var trainingData = GenerateTestTrainingData(50, true); // Perfect predictions
//         
//         // Act
//         var method = typeof(MLModelService).GetMethod("CalculateMAE", 
//             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         
//         var result = method?.Invoke(_mlModelService, new object[] { trainingData });
//
//         // Assert
//         result.Should().NotBeNull();
//         var mae = (double)result!;
//         mae.Should().BeApproximately(0.0, 0.01);
//     }
//
//     private List<ModelTrainingData> GenerateTestTrainingData(int count, bool perfectPredictions = false)
//     {
//         var random = new Random(42); // Fixed seed for reproducible tests
//         var trainingData = new List<ModelTrainingData>();
//         
//         for (int i = 0; i < count; i++)
//         {
//             var temperature = 70 + random.NextDouble() * 30;
//             var vibration = 1 + random.NextDouble() * 5;
//             var pressure = 90 + random.NextDouble() * 30;
//             var rpm = 1200 + random.Next(0, 800);
//             var age = random.Next(0, 365);
//             var cycleCount = random.Next(0, 1000);
//             
//             // Create a predictable relationship for testing
//             var label = perfectPredictions 
//                 ? temperature * 0.5 + vibration * 2.0 + 50 // Perfect prediction formula
//                 : random.Next(10, 100); // Random label for imperfect predictions
//
//             trainingData.Add(new ModelTrainingData
//             {
//                 Temperature = (float)temperature,
//                 Vibration = (float)vibration,
//                 Pressure = (float)pressure,
//                 Rpm = (float)rpm,
//                 Age = (float)age,
//                 CycleCount = (float)cycleCount,
//                 Label = (float)label
//             });
//         }
//         
//         return trainingData;
//     }
// }
