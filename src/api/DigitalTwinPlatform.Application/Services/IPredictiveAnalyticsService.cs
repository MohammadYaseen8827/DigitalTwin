using DigitalTwinPlatform.Application.Predictions.Models;

namespace DigitalTwinPlatform.Application.Services;

public interface IPredictiveAnalyticsService
{
    Task<PredictionDto> PredictAsync(Guid machineId);
    Task<IEnumerable<PredictionDto>> GetHistoryAsync(Guid machineId, int take = 100);
}
