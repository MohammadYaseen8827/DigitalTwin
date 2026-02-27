using DigitalTwinPlatform.Application.Analytics.Advanced.Models;
using DigitalTwinPlatform.Application.Predictions.Models;

namespace DigitalTwinPlatform.Application.Analytics.Advanced;

public interface IAdvancedPredictiveService
{
    Task<PredictionDto> EnsemblePredictionAsync(Guid machineId, CancellationToken ct = default);
    Task<PredictionDto> DeepLearningPredictionAsync(Guid machineId, CancellationToken ct = default);
    Task<AnomalyDetectionResult> DetectAnomaliesAsync(Guid machineId, DateTime? startTime, DateTime? endTime, CancellationToken ct = default);
    Task<ForecastResult> ForecastTimeSeriesAsync(Guid machineId, string metric, int forecastHorizon, CancellationToken ct = default);
    double CalculateHealthScore(PredictionDto prediction, AnomalyDetectionResult anomalies, PrescriptiveRecommendation recommendation);
}
