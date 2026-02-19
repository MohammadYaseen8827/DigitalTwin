using DigitalTwinPlatform.Application.Predictions.Models;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface IPredictionService
{
    Task<PredictionDto> PredictAsync(PredictionRequestDto request, CancellationToken ct = default);
    
    Task<PredictionDto> CreatePredictionAsync(PredictionRequestDto dto, CancellationToken cancellationToken = default);
    Task<PredictionDto?> GetLatestPredictionAsync(Guid machineId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PredictionDto>> GetPredictionHistoryAsync(Guid machineId, int limit = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<PredictionDto>> GetHighConfidencePredictionsAsync(double minConfidence = 0.8, CancellationToken cancellationToken = default);
    Task CleanupOldPredictionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PredictionDto>> SearchPredictionsAsync(string query, Guid? machineId = null, CancellationToken cancellationToken = default);
}
