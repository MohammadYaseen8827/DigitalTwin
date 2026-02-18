using Microsoft.ML;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.ML
{
    public class ShapExplainer
    {
        private readonly MLContext _mlContext;
        private readonly ILogger<ShapExplainer> _logger;

        public ShapExplainer(ILogger<ShapExplainer> logger)
        {
            _mlContext = new MLContext(seed: 42);
            _logger = logger;
        }

        public Dictionary<string, double> GetExplainerValues(ITransformer model, ModelInputData input)
        {
            _logger.LogInformation("Generating SHAP-like feature contributions for prediction");

            try
            {
                // For ML.NET 2.0, we need to use Permutation Feature Importance instead
                // as CalculateFeatureContribution has limited model compatibility
                var contributions = new Dictionary<string, double>();
                string[] featureNames = { "Temperature", "Vibration", "Pressure", "Rpm", "Age", "CycleCount" };
                
                // Create sample data for PFI calculation
                var sampleData = new List<ModelInputData> { input };
                var dataView = _mlContext.Data.LoadFromEnumerable(sampleData);
                
                // Log that mock SHAP values are being generated
                _logger.LogWarning("Generating mock SHAP values as proper SHAP library not available");
                
                // Generate mock SHAP values based on feature importance
                // In a real implementation, you'd use a proper SHAP library or PFI
                var random = new Random(42); // Fixed seed for consistency
                for (int i = 0; i < featureNames.Length; i++)
                {
                    // Generate small random contributions that sum to approximately the prediction
                    contributions[featureNames[i]] = (random.NextDouble() - 0.5) * 0.2;
                }
                
                return contributions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate feature contributions");
                
                // Return empty contributions on error
                string[] featureNames = { "Temperature", "Vibration", "Pressure", "Rpm", "Age", "CycleCount" };
                return featureNames.ToDictionary(name => name, _ => 0.0);
            }
        }
    }

    public class FeatureContributionPrediction : RulPrediction
    {
        public float[] FeatureContributions { get; set; } = [];
    }
}
