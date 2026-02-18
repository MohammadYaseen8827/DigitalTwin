using DigitalTwinPlatform.Application.ML;
using DigitalTwinPlatform.Application.ML.Models;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Services
{
    public interface IMLModelService
    {
        Task<ModelPredictionDto> PredictRulAsync(string machineId, Dictionary<string, double> features);
        Task<TrainingResultDto> TrainModelsAsync(IEnumerable<ModelTrainingData> trainingData);
        Task<Dictionary<string, double>> GetFeatureImportanceAsync();
    }

    public class MLModelService : IMLModelService
    {
        private readonly FastForestPredictor _fastForest;
        private readonly QuantileRegression _quantileRegression;
        private readonly ShapExplainer _shapExplainer;
        private readonly FeatureImportanceExtractor _importanceExtractor;
        private readonly ILogger<MLModelService> _logger;

        public MLModelService(
            FastForestPredictor fastForest,
            QuantileRegression quantileRegression,
            ShapExplainer shapExplainer,
            FeatureImportanceExtractor importanceExtractor,
            ILogger<MLModelService> logger)
        {
            _fastForest = fastForest;
            _quantileRegression = quantileRegression;
            _shapExplainer = shapExplainer;
            _importanceExtractor = importanceExtractor;
            _logger = logger;
        }

        public async Task<ModelPredictionDto> PredictRulAsync(string machineId, Dictionary<string, double> features)
        {
            _logger.LogInformation("Predicting RUL for machine {MachineId}", machineId);

            var input = new ModelInputData
            {
                Temperature = (float)features.GetValueOrDefault("Temperature", 70.0),
                Vibration = (float)features.GetValueOrDefault("Vibration", 2.0),
                Pressure = (float)features.GetValueOrDefault("Pressure", 101.3),
                Rpm = (float)features.GetValueOrDefault("Rpm", 1500.0),
                Age = (float)features.GetValueOrDefault("Age", 0.0),
                CycleCount = (float)features.GetValueOrDefault("CycleCount", 0.0)
            };

            // 1. Get base prediction from FastForest
            var prediction = _fastForest.Predict(input);

            // 2. Get confidence intervals from Quantile Regression
            var (lower, upper) = _quantileRegression.PredictInterval(input);
            
            // 3. Get local explanation (SHAP-like)
            var shapValues = new Dictionary<string, double>();
            if (_fastForest.Model != null)
            {
                shapValues = _shapExplainer.GetExplainerValues(_fastForest.Model, input);
            }

            return new ModelPredictionDto
            {
                RemainingUsefulLife = prediction.RemainingUsefulLife,
                Confidence = 0.90, // We calculated 90% interval
                RiskLevel = prediction.RiskLevel,
                PredictionTime = DateTime.UtcNow,
                FeatureImportance = prediction.FeatureImportance.ToDictionary(k => k.Key, v => v.Value),
                ShapValues = shapValues
            };
        }

        public async Task<TrainingResultDto> TrainModelsAsync(IEnumerable<ModelTrainingData> trainingData)
        {
            _logger.LogInformation("Starting ML pipeline training");

            try
            {
                var dataList = trainingData.ToList();
                
                // Train main predictor
                _fastForest.Train(dataList);
                
                // Train quantile models
                _quantileRegression.Train(dataList);

                return new TrainingResultDto
                {
                    Success = true,
                    SamplesUsed = dataList.Count,
                    TrainedAt = DateTime.UtcNow,
                    Message = "ML models (FastForest + Quantile) trained successfully",
                    Metrics = new Dictionary<string, double>
                    {
                        ["R2Score"] = CalculateR2Score(dataList),
                        ["MAE"] = CalculateMAE(dataList),
                        ["RMSE"] = CalculateRMSE(dataList),
                        ["MAPE"] = CalculateMAPE(dataList)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ML training failed");
                return new TrainingResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    TrainedAt = DateTime.UtcNow
                };
            }
        }

        public Task<Dictionary<string, double>> GetFeatureImportanceAsync()
        {
            // Usually we'd extract this from the trained model
            return Task.FromResult(new Dictionary<string, double>
            {
                ["Temperature"] = 0.35,
                ["Vibration"] = 0.25,
                ["Pressure"] = 0.15,
                ["Rpm"] = 0.10,
                ["Age"] = 0.10,
                ["CycleCount"] = 0.05
            });
        }

        /// <summary>
        /// Calculates R² score (coefficient of determination) for model evaluation
        /// </summary>
        private double CalculateR2Score(IEnumerable<ModelTrainingData> data)
        {
            var dataList = data.ToList();
            if (dataList.Count < 2) return 0.0;

            var actualValues = dataList.Select(d => d.Label).ToList();
            var meanActual = actualValues.Average();
            
            var totalSumSquares = actualValues.Sum(actual => Math.Pow(actual - meanActual, 2));
            var residualSumSquares = 0.0;

            foreach (var dataPoint in dataList)
            {
                var prediction = _fastForest.Predict(new ModelInputData
                {
                    Temperature = dataPoint.Temperature,
                    Vibration = dataPoint.Vibration,
                    Pressure = dataPoint.Pressure,
                    Rpm = dataPoint.Rpm,
                    Age = dataPoint.Age,
                    CycleCount = dataPoint.CycleCount
                });
                
                residualSumSquares += Math.Pow(dataPoint.Label - prediction.RemainingUsefulLife, 2);
            }

            return totalSumSquares == 0 ? 1.0 : 1.0 - (residualSumSquares / totalSumSquares);
        }

        /// <summary>
        /// Calculates Mean Absolute Error (MAE) for model evaluation
        /// </summary>
        private double CalculateMAE(IEnumerable<ModelTrainingData> data)
        {
            var dataList = data.ToList();
            if (dataList.Count == 0) return 0.0;

            var totalAbsoluteError = 0.0;
            foreach (var dataPoint in dataList)
            {
                var prediction = _fastForest.Predict(new ModelInputData
                {
                    Temperature = dataPoint.Temperature,
                    Vibration = dataPoint.Vibration,
                    Pressure = dataPoint.Pressure,
                    Rpm = dataPoint.Rpm,
                    Age = dataPoint.Age,
                    CycleCount = dataPoint.CycleCount
                });
                
                totalAbsoluteError += Math.Abs(dataPoint.Label - prediction.RemainingUsefulLife);
            }

            return totalAbsoluteError / dataList.Count;
        }

        /// <summary>
        /// Calculates Root Mean Square Error (RMSE) for model evaluation
        /// </summary>
        private double CalculateRMSE(IEnumerable<ModelTrainingData> data)
        {
            var dataList = data.ToList();
            if (dataList.Count == 0) return 0.0;

            var totalSquaredError = 0.0;
            foreach (var dataPoint in dataList)
            {
                var prediction = _fastForest.Predict(new ModelInputData
                {
                    Temperature = dataPoint.Temperature,
                    Vibration = dataPoint.Vibration,
                    Pressure = dataPoint.Pressure,
                    Rpm = dataPoint.Rpm,
                    Age = dataPoint.Age,
                    CycleCount = dataPoint.CycleCount
                });
                
                totalSquaredError += Math.Pow(dataPoint.Label - prediction.RemainingUsefulLife, 2);
            }

            return Math.Sqrt(totalSquaredError / dataList.Count);
        }

        /// <summary>
        /// Calculates Mean Absolute Percentage Error (MAPE) for model evaluation
        /// </summary>
        private double CalculateMAPE(IEnumerable<ModelTrainingData> data)
        {
            var dataList = data.ToList();
            if (dataList.Count == 0) return 0.0;

            var totalPercentageError = 0.0;
            var validCount = 0;

            foreach (var dataPoint in dataList)
            {
                if (Math.Abs(dataPoint.Label) < 0.001) continue; // Avoid division by zero

                var prediction = _fastForest.Predict(new ModelInputData
                {
                    Temperature = dataPoint.Temperature,
                    Vibration = dataPoint.Vibration,
                    Pressure = dataPoint.Pressure,
                    Rpm = dataPoint.Rpm,
                    Age = dataPoint.Age,
                    CycleCount = dataPoint.CycleCount
                });
                
                totalPercentageError += Math.Abs((dataPoint.Label - prediction.RemainingUsefulLife) / dataPoint.Label);
                validCount++;
            }

            return validCount == 0 ? 0.0 : (totalPercentageError / validCount) * 100.0;
        }
    }
}
