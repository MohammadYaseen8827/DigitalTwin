using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using DigitalTwinPlatform.Application.ML.Models;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.ML
{
    public class FastForestPredictor
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private readonly ILogger<FastForestPredictor> _logger;

        public FastForestPredictor(ILogger<FastForestPredictor> logger)
        {
            _mlContext = new MLContext(seed: 42);
            _logger = logger;
        }

        public ITransformer? Model => _model;

        public Dictionary<string, double> Train(List<ModelTrainingData> trainingData)
        {
            _logger.LogInformation("Training FastForest model with {Count} samples", trainingData.Count);

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Split into train/test
            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainSet = trainTestSplit.TrainSet;
            var testSet = trainTestSplit.TestSet;

            var pipeline = _mlContext.Transforms.Concatenate("Features", 
                nameof(ModelTrainingData.Temperature), 
                nameof(ModelTrainingData.Vibration), 
                nameof(ModelTrainingData.Pressure), 
                nameof(ModelTrainingData.Rpm),
                nameof(ModelTrainingData.Age),
                nameof(ModelTrainingData.CycleCount))
                .Append(_mlContext.Regression.Trainers.FastForest(
                    labelColumnName: "Label",
                    featureColumnName: "Features",
                    numberOfLeaves: 20,
                    numberOfTrees: 100));

            var model = pipeline.Fit(trainSet);
            _model = model;

            // Evaluate
            var predictions = model.Transform(testSet);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            _logger.LogInformation("FastForest model training completed. R2: {R2}, MAE: {MAE}", metrics.RSquared, metrics.MeanAbsoluteError);

            return new Dictionary<string, double>
            {
                ["RSquared"] = metrics.RSquared,
                ["MeanAbsoluteError"] = metrics.MeanAbsoluteError,
                ["MeanSquaredError"] = metrics.MeanSquaredError,
                ["RootMeanSquaredError"] = metrics.RootMeanSquaredError
            };
        }

        public ModelPredictionDto Predict(ModelInputData input)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Model must be trained before prediction");
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInputData, RulPrediction>(_model);
            var prediction = predictionEngine.Predict(input);

            // Extract feature importance if it's a FastForest model
            var featureImportance = ExtractFeatureImportance();

            return new ModelPredictionDto
            {
                RemainingUsefulLife = prediction.Score,
                Confidence = 0.95, // Default confidence, will be updated by QuantileRegression
                RiskLevel = prediction.Score < 30 ? "high" : prediction.Score < 90 ? "medium" : "low",
                FeatureImportance = featureImportance,
                PredictionTime = DateTime.UtcNow
            };
        }

        private Dictionary<string, object> ExtractFeatureImportance()
        {
            var importance = new Dictionary<string, object>();
            
            if (_model is TransformerChain<RegressionPredictionTransformer<FastForestRegressionModelParameters>> chain)
            {
                var modelParameters = chain.LastTransformer.Model;
                //var stats = modelParameters.FeatureContributions;
                
                // This is a simplified extraction. In a real scenario, we'd map indices back to names.
                string[] featureNames = { "Temperature", "Vibration", "Pressure", "Rpm", "Age", "CycleCount" };
                
                // For FastForest, we can get Gain-based importance
                // Note: ML.NET FastForest importance extraction is slightly more complex
                // Log when mock importance is returned
                _logger.LogWarning("Using default uniform feature importance as proper extraction not available");
                for (int i = 0; i < featureNames.Length; i++)
                {
                    importance[featureNames[i]] = 1.0 / featureNames.Length;
                }
            }

            return importance;
        }

        public byte[] SaveModel()
        {
            if (_model == null) throw new InvalidOperationException("No model to save");

            using var ms = new MemoryStream();
            _mlContext.Model.Save(_model, null, ms);
            return ms.ToArray();
        }

        public void LoadModel(byte[] modelData)
        {
            using var ms = new MemoryStream(modelData);
            _model = _mlContext.Model.Load(ms, out _);
        }
    }

    public class ModelTrainingData
    {
        public float Temperature { get; set; }
        public float Vibration { get; set; }
        public float Pressure { get; set; }
        public float Rpm { get; set; }
        public float Age { get; set; }
        public float CycleCount { get; set; }
        
        [ColumnName("Label")]
        public float Label { get; set; }
    }

    public class ModelInputData
    {
        public float Temperature { get; set; }
        public float Vibration { get; set; }
        public float Pressure { get; set; }
        public float Rpm { get; set; }
        public float Age { get; set; }
        public float CycleCount { get; set; }
    }

    public class RulPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
