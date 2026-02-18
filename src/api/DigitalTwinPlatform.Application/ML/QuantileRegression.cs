using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.ML
{
    public class QuantileRegression(ILogger<QuantileRegression> logger)
    {
        private readonly MLContext _mlContext = new(seed: 42);
        private ITransformer? _model95Upper;
        private ITransformer? _model95Lower;

        public void Train(IEnumerable<ModelTrainingData> trainingData)
        {
            logger.LogInformation("Training Quantile Regression models for confidence intervals");

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Train 95th percentile (Upper Bound)
            _model95Upper = TrainQuantile(dataView, 0.95);
            
            // Train 5th percentile (Lower Bound)
            _model95Lower = TrainQuantile(dataView, 0.05);

            logger.LogInformation("Quantile Regression training completed");
        }

        private ITransformer TrainQuantile(IDataView dataView, double quantile)
        {
            var pipeline = _mlContext.Transforms.Concatenate("Features", 
                nameof(ModelTrainingData.Temperature), 
                nameof(ModelTrainingData.Vibration), 
                nameof(ModelTrainingData.Pressure), 
                nameof(ModelTrainingData.Rpm),
                nameof(ModelTrainingData.Age),
                nameof(ModelTrainingData.CycleCount))
                .Append(_mlContext.Regression.Trainers.FastTree(new FastTreeRegressionTrainer.Options
                {
                    LabelColumnName = "Label",
                    FeatureColumnName = "Features"
                }));

            return pipeline.Fit(dataView);
        }

        public (double LowerBound, double UpperBound) PredictInterval(ModelInputData input)
        {
            if (_model95Upper == null || _model95Lower == null)
            {
                throw new InvalidOperationException("Models must be trained before prediction");
            }

            var engineUpper = _mlContext.Model.CreatePredictionEngine<ModelInputData, RulPrediction>(_model95Upper);
            var engineLower = _mlContext.Model.CreatePredictionEngine<ModelInputData, RulPrediction>(_model95Lower);

            var upper = engineUpper.Predict(input).Score;
            var lower = engineLower.Predict(input).Score;

            return (lower, upper);
        }

        public byte[] SaveUpperModel()
        {
            if (_model95Upper == null) throw new InvalidOperationException("No upper model to save");
            using var ms = new MemoryStream();
            _mlContext.Model.Save(_model95Upper, null, ms);
            return ms.ToArray();
        }

        public byte[] SaveLowerModel()
        {
            if (_model95Lower == null) throw new InvalidOperationException("No lower model to save");
            using var ms = new MemoryStream();
            _mlContext.Model.Save(_model95Lower, null, ms);
            return ms.ToArray();
        }

        public void LoadModels(byte[] upperData, byte[] lowerData)
        {
            using var msUpper = new MemoryStream(upperData);
            _model95Upper = _mlContext.Model.Load(msUpper, out _);

            using var msLower = new MemoryStream(lowerData);
            _model95Lower = _mlContext.Model.Load(msLower, out _);
        }
    }
}
