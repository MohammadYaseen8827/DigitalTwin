using DigitalTwinPlatform.Application.Maintenance;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.ValueObjects;
using DigitalTwinPlatform.Application.Predictions.Models;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Maintenance;

public class PrescriptiveService : IPrescriptiveService
{
    private readonly IRepository<Prediction> _predictionRepository;
    private readonly ILogger<PrescriptiveService> _logger;

    // Constants for cost modeling (would typically be configurable per machine type)
    private const double PreventiveMaintenanceCost = 500.0;
    private const double ReactiveFailureCost = 5000.0;
    private const double DowntimeCostPerHour = 200.0;

    public PrescriptiveService(
        IRepository<Prediction> predictionRepository,
        ILogger<PrescriptiveService> logger)
    {
        _predictionRepository = predictionRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MaintenanceWindow>> RunWhatIfAnalysisAsync(Guid machineId, int daysToSimulate = 30)
    {
        _logger.LogInformation("Running what-if analysis for machine {MachineId} over {Days} days", machineId, daysToSimulate);

        var latestPrediction = (await _predictionRepository.GetAllAsync(p => p.MachineId == machineId))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        if (latestPrediction == null)
            return Enumerable.Empty<MaintenanceWindow>();

        var results = new List<MaintenanceWindow>();
        var rul = latestPrediction.RemainingUsefulLifeDays;

        for (int day = 0; day <= daysToSimulate; day += 2)
        {
            var scheduledDate = DateTime.UtcNow.AddDays(day);
            
            // Simplified probability of failure: increases as we approach RUL
            // P(Failure) ≈ 1 - exp(- (day / RUL)^beta) where beta is shape parameter (Weibull)
            double beta = 2.0; 
            double riskScore = 1.0 - Math.Exp(-Math.Pow(day / Math.Max(1.0, rul), beta));
            
            // Total expected cost = Cost(Preventive) + P(Failure) * Cost(Reactive)
            double expectedCost = PreventiveMaintenanceCost + (riskScore * ReactiveFailureCost);

            results.Add(new MaintenanceWindow(
                scheduledDate,
                expectedCost,
                riskScore,
                GetRecommendation(day, rul, riskScore)
            ));
        }

        return results;
    }

    public async Task<MaintenanceWindow> GetOptimalMaintenanceDateAsync(Guid machineId)
    {
        var analysis = await RunWhatIfAnalysisAsync(machineId, 60);
        return analysis.OrderBy(m => m.EstimatedCost).FirstOrDefault() 
            ?? new MaintenanceWindow(DateTime.UtcNow, 0, 0, "No data");
    }

    private string GetRecommendation(int day, double rul, double risk)
    {
        if (day > rul) return "CRITICAL: Past predicted failure date.";
        if (risk > 0.4) return "REPLACE: High failure probability.";
        if (risk > 0.1) return "PLAN: Schedule maintenance soon.";
        return "MONITOR: Healthy operational window.";
    }
}
