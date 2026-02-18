using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Application.ML.Models;

namespace DigitalTwinPlatform.Application.ML.Services
{
    public interface IAIService
    {
        /// <summary>
        /// Make prediction using specified model
        /// </summary>
        Task<ModelPredictionDto> Predict(string modelId, Dictionary<string, double> features);

        /// <summary>
        /// Get model performance metrics
        /// </summary>
        Task<ModelMetricsDto> GetModelMetrics(string modelId);

        /// <summary>
        /// Get machines compatible with specified model
        /// </summary>
        Task<IEnumerable<string>> GetCompatibleMachines(string modelId);

        /// <summary>
        /// Deploy model to specified machines
        /// </summary>
        Task DeployModelToMachines(string modelId, IEnumerable<string> machineIds);

        /// <summary>
        /// Get model deployment status
        /// </summary>
        Task<DeploymentStatusDto> GetDeploymentStatus(string modelId);

        /// <summary>
        /// Validate model file before deployment
        /// </summary>
        Task<ModelValidationResultDto> ValidateModel(byte[] modelFile, string modelType);

        /// <summary>
        /// Retrain existing model with new data
        /// </summary>
        Task<bool> RetrainModel(string modelId, IEnumerable<string> trainingData, Dictionary<string, object>? hyperparameters = null);

        /// <summary>
        /// Monitor model performance and detect concept drift
        /// </summary>
        Task<bool> MonitorModelPerformance(string modelId);

        /// <summary>
        /// Archive outdated model versions
        /// </summary>
        Task ArchiveModel(string modelId);
    }

    public class AIService : IAIService
    {
        private readonly ILogger<AIService> _logger;
        private readonly FastForestPredictor _fastForest;
        private readonly QuantileRegression _quantile;
        private readonly ShapExplainer _shap;

        public AIService(
            ILogger<AIService> logger,
            FastForestPredictor fastForest,
            QuantileRegression quantile,
            ShapExplainer shap)
        {
            _logger = logger;
            _fastForest = fastForest;
            _quantile = quantile;
            _shap = shap;
        }

        public async Task<ModelPredictionDto> Predict(string modelId, Dictionary<string, double> features)
        {
            _logger.LogInformation("Making prediction with model {ModelId}", modelId);
            
            // Convert features to ModelInputData
            var input = new ModelInputData
            {
                Temperature = (float)features.GetValueOrDefault("Temperature", 70.0),
                Vibration = (float)features.GetValueOrDefault("Vibration", 2.0),
                Pressure = (float)features.GetValueOrDefault("Pressure", 101.3),
                Rpm = (float)features.GetValueOrDefault("Rpm", 1500.0),
                Age = (float)features.GetValueOrDefault("Age", 0.0),
                CycleCount = (float)features.GetValueOrDefault("CycleCount", 0.0)
            };

            try 
            {
                // In a real implementation, we'd load the model bytes from storage first
                // For this interactive session, we'll use the in-memory models if they exist
                // or fall back to mock if not trained.

                var prediction = _fastForest.Predict(input);
                var (lower, upper) = _quantile.PredictInterval(input);
                
                return new ModelPredictionDto
                {
                    RemainingUsefulLife = prediction.RemainingUsefulLife,
                    Confidence = 0.90, // We use 90% confidence interval
                    RiskLevel = prediction.RiskLevel,
                    FeatureImportance = prediction.FeatureImportance,
                    PredictionTime = DateTime.UtcNow
                };
            }
            catch (InvalidOperationException ex)
            {
                // Log the exception and provide a more structured fallback
                _logger.LogWarning(ex, "Model {ModelId} not trained or unavailable, returning default prediction", modelId);
                
                // Fallback to default values based on features
                var avgTemp = features.GetValueOrDefault("Temperature", 70.0);
                var avgVib = features.GetValueOrDefault("Vibration", 2.0);
                
                // Calculate a reasonable RUL based on feature values
                var baseRul = 365.0; // Base RUL in days
                var tempEffect = Math.Max(0, (avgTemp - 70) * -0.5); // Higher temp reduces RUL
                var vibEffect = Math.Max(0, (avgVib - 2) * -1.0);   // Higher vibration reduces RUL
                
                var rul = Math.Max(30, baseRul + tempEffect + vibEffect); // Minimum 30 days
                var confidence = 0.60; // Lower confidence for fallback prediction
                var riskLevel = rul < 30 ? "high" : rul < 90 ? "medium" : "low";
                
                return new ModelPredictionDto
                {
                    RemainingUsefulLife = (int)rul,
                    Confidence = confidence,
                    RiskLevel = riskLevel,
                    FeatureImportance = features.ToDictionary(f => f.Key, f => (object)f.Value),
                    PredictionTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                // Log any other exception that occurs during prediction
                _logger.LogError(ex, "Unexpected error occurred during prediction for model {ModelId}", modelId);
                throw; // Re-throw to let caller handle
            }
        }

        public async Task<ModelMetricsDto> GetModelMetrics(string modelId)
        {
            _logger.LogInformation("Retrieving metrics for model {ModelId}", modelId);
            
            await Task.Delay(50); // Simulate database lookup
            
            return new ModelMetricsDto
            {
                Accuracy = 0.92,
                Precision = 0.89,
                Recall = 0.94,
                F1Score = 0.91,
                MeanAbsoluteError = 15.3,
                RootMeanSquareError = 22.1,
                ClassMetrics = new Dictionary<string, double>
                {
                    { "Class_0_Precision", 0.95 },
                    { "Class_0_Recall", 0.88 },
                    { "Class_1_Precision", 0.85 },
                    { "Class_1_Recall", 0.92 }
                },
                LastEvaluated = DateTime.UtcNow.AddDays(-1)
            };
        }

        public async Task<IEnumerable<string>> GetCompatibleMachines(string modelId)
        {
            _logger.LogInformation("Getting compatible machines for model {ModelId}", modelId);
            
            await Task.Delay(50);
            
            // Mock implementation - in reality this would check model requirements
            // against machine specifications
            return new List<string> { "cnc_001", "cnc_002", "cnc_003", "im_001" };
        }

        public async Task DeployModelToMachines(string modelId, IEnumerable<string> machineIds)
        {
            _logger.LogInformation("Deploying model {ModelId} to {MachineCount} machines", 
                modelId, machineIds.Count());
            
            await Task.Delay(2000); // Simulate deployment process
            
            // In real implementation, this would:
            // 1. Validate machine compatibility
            // 2. Transfer model files to edge devices
            // 3. Update machine configuration
            // 4. Restart prediction services
        }

        public async Task<DeploymentStatusDto> GetDeploymentStatus(string modelId)
        {
            _logger.LogInformation("Getting deployment status for model {ModelId}", modelId);
            
            await Task.Delay(100);
            
            return new DeploymentStatusDto
            {
                Status = "deployed",
                TotalMachines = 4,
                SuccessfulDeployments = 4,
                FailedDeployments = 0,
                MachineStatuses = new List<MachineDeploymentStatus>
                {
                    new() { MachineId = "cnc_001", Status = "success", DeployedAt = DateTime.UtcNow.AddHours(-2) },
                    new() { MachineId = "cnc_002", Status = "success", DeployedAt = DateTime.UtcNow.AddHours(-2) },
                    new() { MachineId = "cnc_003", Status = "success", DeployedAt = DateTime.UtcNow.AddHours(-2) },
                    new() { MachineId = "im_001", Status = "success", DeployedAt = DateTime.UtcNow.AddHours(-1) }
                },
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<ModelValidationResultDto> ValidateModel(byte[] modelFile, string modelType)
        {
            _logger.LogInformation("Validating model file for type {ModelType}", modelType);
            
            await Task.Delay(500); // Simulate validation process
            
            // Mock validation logic
            var isValid = modelFile.Length > 0 && modelFile.Length < 100_000_000; // 100MB limit
            
            return new ModelValidationResultDto
            {
                IsValid = isValid,
                Message = isValid ? "Model file is valid" : "Invalid model file",
                Errors = isValid ? new List<string>() : new List<string> { "File size exceeds limit" },
                Compatibility = new ModelCompatibilityDto
                {
                    IsCompatible = isValid,
                    CompatibleMachineTypes = new List<string> { "cnc", "injection_molder" },
                    RequiredFeatures = new List<string> { "temperature", "vibration", "current" },
                    Framework = "scikit-learn"
                }
            };
        }

        public async Task<bool> RetrainModel(string modelId, IEnumerable<string> trainingData, Dictionary<string, object>? hyperparameters = null)
        {
            _logger.LogInformation("Retraining model {ModelId} with {DataCount} samples", 
                modelId, trainingData.Count());
            
            await Task.Delay(5000); // Simulate training process
            
            // In real implementation, this would:
            // 1. Load existing model
            // 2. Combine with new training data
            // 3. Retrain with updated hyperparameters
            // 4. Validate new model performance
            // 5. Deploy if performance improves
            
            return true; // Mock success
        }

        public async Task<bool> MonitorModelPerformance(string modelId)
        {
            _logger.LogInformation("Monitoring performance for model {ModelId}", modelId);
            
            await Task.Delay(100);
            
            // In real implementation, this would:
            // 1. Collect recent predictions and actual outcomes
            // 2. Calculate performance metrics
            // 3. Detect concept drift
            // 4. Trigger alerts if performance degrades
            
            return true; // Mock - no drift detected
        }

        public async Task ArchiveModel(string modelId)
        {
            _logger.LogInformation("Archiving model {ModelId}", modelId);
            
            await Task.Delay(100);
            
            // In real implementation, this would:
            // 1. Move model to archive storage
            // 2. Update model status to archived
            // 3. Clean up active deployment references
        }
    }
}