using DigitalTwinPlatform.Application.Predictions.Models;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IPredictiveAnalyticsService
{
    Task<PredictionDto> PredictAsync(Guid machineId);
    Task<IEnumerable<PredictionDto>> GetHistoryAsync(Guid machineId, int take = 100);
}
