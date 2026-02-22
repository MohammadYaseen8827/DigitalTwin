using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Application.Services;

namespace DigitalTwinPlatform.API.Services.Analytics;

/// <summary>
/// Adapter to bridge IPredictiveAnalyticsService to IPredictionService interface.
/// </summary>
public class PredictionServiceAdapter : IPredictionService
{
    private readonly IPredictiveAnalyticsService _predictiveAnalyticsService;
    private readonly PredictionService _predictionService;

    public PredictionServiceAdapter(
        IPredictiveAnalyticsService predictiveAnalyticsService,
        PredictionService predictionService)
    {
        _predictiveAnalyticsService = predictiveAnalyticsService;
        _predictionService = predictionService;
    }

    public async Task<PredictionDto> PredictAsync(PredictionRequestDto request, CancellationToken ct = default)
    {
        return await _predictiveAnalyticsService.PredictAsync(request.MachineId);
    }

    public async Task<PredictionDto> CreatePredictionAsync(PredictionRequestDto dto, CancellationToken cancellationToken = default)
    {
        return await _predictionService.CreatePredictionAsync(dto, cancellationToken);
    }

    public async Task<PredictionDto?> GetLatestPredictionAsync(Guid machineId, CancellationToken cancellationToken = default)
    {
        return await _predictionService.GetLatestPredictionAsync(machineId, cancellationToken);
    }

    public async Task<IEnumerable<PredictionDto>> GetPredictionHistoryAsync(Guid machineId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _predictionService.GetPredictionHistoryAsync(machineId, limit, cancellationToken);
    }

    public async Task<IEnumerable<PredictionDto>> GetHighConfidencePredictionsAsync(double minConfidence = 0.8, CancellationToken cancellationToken = default)
    {
        return await _predictionService.GetHighConfidencePredictionsAsync(minConfidence, cancellationToken);
    }

    public async Task CleanupOldPredictionsAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        await _predictionService.CleanupOldPredictionsAsync(cutoffDate, cancellationToken);
    }

    public async Task<IEnumerable<PredictionDto>> SearchPredictionsAsync(string query, Guid? machineId = null, CancellationToken cancellationToken = default)
    {
        return await _predictionService.SearchPredictionsAsync(query, machineId, cancellationToken);
    }
}
