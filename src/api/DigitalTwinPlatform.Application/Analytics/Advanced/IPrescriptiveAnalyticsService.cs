using DigitalTwinPlatform.Application.Analytics.Advanced.Models;

namespace DigitalTwinPlatform.Application.Analytics.Advanced;

public interface IPrescriptiveAnalyticsService
{
    Task<PrescriptiveRecommendation> GenerateMaintenanceRecommendationAsync(Guid machineId, MaintenanceOptimizationCriteria criteria, CancellationToken ct = default);
    Task<SchedulingRecommendation> OptimizeProductionScheduleAsync(IEnumerable<Guid> machineIds, DateTime planningHorizon, CancellationToken ct = default);
    Task<ResourceAllocationPlan> OptimizeResourceAllocationAsync(IEnumerable<Guid> machineIds, DateTime planningPeriod, CancellationToken ct = default);
    Task<CostOptimizationResult> OptimizeMaintenanceCostsAsync(IEnumerable<Guid> machineIds, BudgetConstraints budget, CancellationToken ct = default);
}
