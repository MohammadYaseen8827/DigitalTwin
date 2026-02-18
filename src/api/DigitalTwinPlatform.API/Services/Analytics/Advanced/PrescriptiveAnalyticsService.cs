using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Domain.Entities.Enums;

namespace DigitalTwinPlatform.API.Services.Analytics.Advanced;

public interface IPrescriptiveAnalyticsService
{
    /// <summary>
    /// Generate optimization-based maintenance recommendations
    /// </summary>
    Task<PrescriptiveRecommendation> GenerateMaintenanceRecommendationAsync(
        Guid machineId,
        MaintenanceOptimizationCriteria criteria,
        CancellationToken ct = default);

    /// <summary>
    /// Optimize production scheduling based on equipment health
    /// </summary>
    Task<SchedulingRecommendation> OptimizeProductionScheduleAsync(
        List<Guid> machineIds,
        DateTime planningHorizon,
        CancellationToken ct = default);

    /// <summary>
    /// Resource allocation optimization for maintenance activities
    /// </summary>
    Task<ResourceAllocationPlan> OptimizeResourceAllocationAsync(
        List<Guid> machineIds,
        DateTime planningPeriod,
        CancellationToken ct = default);

    /// <summary>
    /// Predictive cost optimization for maintenance strategies
    /// </summary>
    Task<CostOptimizationResult> OptimizeMaintenanceCostsAsync(
        IEnumerable<Guid> machineIds,
        BudgetConstraints budget,
        CancellationToken ct = default);
}

public class PrescriptiveAnalyticsService : IPrescriptiveAnalyticsService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IRepository<Prediction> _predictionRepository;
    private readonly IRepository<Machine> _machineRepository;
    private readonly IAdvancedPredictiveService _advancedPredictiveService;
    private readonly ILogger<PrescriptiveAnalyticsService> _logger;

    public PrescriptiveAnalyticsService(
        ITelemetryRepository telemetryRepository,
        IRepository<Prediction> predictionRepository,
        IRepository<Machine> machineRepository,
        IAdvancedPredictiveService advancedPredictiveService,
        ILogger<PrescriptiveAnalyticsService> logger)
    {
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _machineRepository = machineRepository;
        _advancedPredictiveService = advancedPredictiveService;
        _logger = logger;
    }

    public async Task<PrescriptiveRecommendation> GenerateMaintenanceRecommendationAsync(
        Guid machineId,
        MaintenanceOptimizationCriteria criteria,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Generating maintenance recommendation for machine {MachineId}", machineId);

        // Get current machine state and predictions
        var machine = await _machineRepository.GetAsync(machineId, ct);
        if (machine == null)
        {
            throw new InvalidOperationException($"Machine {machineId} not found");
        }

        var recentPredictions = await _predictionRepository.GetAllAsync(
            p => p.MachineId == machineId && p.CreatedAt > DateTime.UtcNow.AddDays(-30), ct);
        
        var latestPrediction = recentPredictions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        
        if (latestPrediction == null)
        {
            // Generate new prediction if none exists
            var ensemblePrediction = await _advancedPredictiveService.EnsemblePredictionAsync(machineId, ct);
            latestPrediction = new Prediction
            {
                Id = ensemblePrediction.Id,
                MachineId = machineId,
                RemainingUsefulLifeDays = ensemblePrediction.RemainingUsefulLifeDays,
                FailureProbability = ensemblePrediction.FailureProbability,
                HealthStatus = ensemblePrediction.HealthStatus,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Calculate optimal maintenance timing using cost optimization
        var optimalTiming = CalculateOptimalMaintenanceTiming(
            latestPrediction, 
            machine, 
            criteria.CostFactors);

        // Extract Criticality from Properties JSON (default to 3 if not found)
        var criticality = 3;
        try
        {
            if (machine.Properties.RootElement.TryGetProperty("criticality", out var criticalityElement) &&
                criticalityElement.TryGetInt32(out var critValue))
            {
                criticality = critValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read criticality from machine properties; using default value of {DefaultCriticality}", criticality);
        }

        // Generate priority score
        var priorityScore = CalculateMaintenancePriority(
            latestPrediction.FailureProbability,
            latestPrediction.RemainingUsefulLifeDays,
            criticality,
            criteria.BusinessImpact);

        // Determine maintenance type
        var maintenanceType = DetermineMaintenanceType(
            latestPrediction.HealthStatus,
            latestPrediction.FailureProbability,
            latestPrediction.RemainingUsefulLifeDays);

        // Calculate expected benefits
        var expectedBenefits = CalculateExpectedBenefits(
            latestPrediction,
            machine,
            maintenanceType,
            criteria.BusinessImpact);

        var recommendation = new PrescriptiveRecommendation
        {
            MachineId = machineId,
            RecommendedAction = maintenanceType,
            PriorityScore = priorityScore,
            RecommendedTiming = optimalTiming,
            ExpectedBenefits = expectedBenefits,
            RiskMitigation = CalculateRiskMitigation(latestPrediction),
            CostEstimate = CalculateMaintenanceCost(maintenanceType),
            ConfidenceScore = 0.85, // Based on model accuracy
            GeneratedAt = DateTime.UtcNow,
            SupportingEvidence = new EvidenceSummary
            {
                PredictionConfidence = latestPrediction.FeatureContributions?.Values.Average() ?? 0.8,
                DataQualityScore = 0.9,
                ModelAccuracy = 0.85,
                RecentAnomalies = await GetRecentAnomalyCount(machineId, ct)
            }
        };

        _logger.LogInformation("Maintenance recommendation generated: {Action} with priority {Priority}", 
            maintenanceType, priorityScore);

        return recommendation;
    }

    public async Task<SchedulingRecommendation> OptimizeProductionScheduleAsync(
        List<Guid> machineIds,
        DateTime planningHorizon,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Optimizing production schedule for {MachineCount} machines", machineIds.Count());

        var machineStates = new List<MachineState>();
        var maintenanceWindows = new List<MaintenanceWindow>();

        // Collect machine states and predictions
        foreach (var machineId in machineIds)
        {
            var machine = await _machineRepository.GetAsync(machineId, ct);
            if (machine == null) continue;

            var prediction = await GetLatestPrediction(machineId, ct);
            
            // Extract Criticality from Properties JSON (default to 3 if not found)
            var criticality = 3;
            try {
                if (machine.Properties.RootElement.TryGetProperty("criticality", out var critElement) &&
                    critElement.TryGetInt32(out var critValue)) {
                    criticality = critValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read criticality from machine properties for {MachineId}; using default value of {DefaultCriticality}", machineId, criticality);
            }
            
            var state = new MachineState
            {
                MachineId = machineId,
                HealthStatus = prediction?.HealthStatus ?? HealthClassification.Healthy,
                RemainingUsefulLife = prediction?.RemainingUsefulLifeDays ?? 365,
                FailureProbability = prediction?.FailureProbability ?? 0.0,
                Criticality = criticality,
                CurrentLoad = 1.0 // Simplified assumption
            };

            machineStates.Add(state);

            // Generate maintenance windows for high-risk machines
            if (state.FailureProbability > 0.3 || state.RemainingUsefulLife < 30)
            {
                var window = GenerateMaintenanceWindow(state, planningHorizon);
                maintenanceWindows.Add(window);
            }
        }

        // Optimize schedule using genetic algorithm
        var optimizedSchedule = OptimizeScheduleUsingGeneticAlgorithm(
            machineStates, 
            maintenanceWindows, 
            planningHorizon);

        var recommendation = new SchedulingRecommendation
        {
            PlanningHorizon = planningHorizon,
            OptimizedSchedule = optimizedSchedule,
            EfficiencyGain = CalculateEfficiencyGain(optimizedSchedule),
            RiskReduction = CalculateScheduleRiskReduction(machineStates, optimizedSchedule),
            ResourceUtilization = CalculateResourceUtilization(optimizedSchedule),
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Production schedule optimization completed with {WindowCount} maintenance windows", 
            maintenanceWindows.Count);

        return recommendation;
    }

    public async Task<ResourceAllocationPlan> OptimizeResourceAllocationAsync(
        List<Guid> machineIds,
        DateTime planningPeriod,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Optimizing resource allocation for {MachineCount} machines", machineIds.Count());

        // Get maintenance requirements
        var maintenanceRequirements = new List<MaintenanceRequirement>();
        foreach (var machineId in machineIds)
        {
            var requirement = await CalculateMaintenanceRequirement(machineId, planningPeriod, ct);
            if (requirement != null)
            {
                maintenanceRequirements.Add(requirement);
            }
        }

        // Get available resources
        var availableResources = await GetAvailableResources(planningPeriod, ct);

        // Optimize allocation using linear programming approach
        var allocationPlan = OptimizeResourceAllocation(
            maintenanceRequirements, 
            availableResources);

        var plan = new ResourceAllocationPlan
        {
            PlanningPeriod = planningPeriod,
            Allocations = allocationPlan,
            ResourceUtilizationRate = CalculateResourceUtilizationRate(allocationPlan, availableResources),
            BottleneckResources = IdentifyBottleneckResources(allocationPlan, availableResources),
            CostEfficiency = CalculateAllocationCostEfficiency(allocationPlan),
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Resource allocation optimization completed with {AllocationCount} allocations", 
            allocationPlan.Count);

        return plan;
    }

    public async Task<CostOptimizationResult> OptimizeMaintenanceCostsAsync(
        IEnumerable<Guid> machineIds,
        BudgetConstraints budget,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Optimizing maintenance costs within budget {Budget}", budget.TotalBudget);

        var costScenarios = new List<CostScenario>();

        // Generate different maintenance strategies
        var strategies = GenerateMaintenanceStrategies(machineIds, budget);
        
        foreach (var strategy in strategies)
        {
            var scenario = await EvaluateCostScenario(strategy, budget, ct);
            costScenarios.Add(scenario);
        }

        // Select optimal strategy based on cost-benefit analysis
        var optimalStrategy = SelectOptimalStrategy(costScenarios, budget);

        var result = new CostOptimizationResult
        {
            Budget = budget,
            OptimalStrategy = optimalStrategy,
            AlternativeScenarios = costScenarios.Where(s => s.StrategyId != optimalStrategy.StrategyId).ToList(),
            CostBenefitAnalysis = PerformCostBenefitAnalysis(optimalStrategy, costScenarios),
            RiskAdjustedROI = CalculateRiskAdjustedROI(optimalStrategy),
            GeneratedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Maintenance cost optimization completed with ROI {ROI:P2}", 
            result.RiskAdjustedROI);

        return result;
    }

    #region Private Helper Methods

    private DateTime CalculateOptimalMaintenanceTiming(
        Prediction prediction, 
        Machine machine, 
        CostFactors costFactors)
    {
        // Economic life optimization model
        var failureCost = costFactors.FailureCostMultiplier * 50000; // Default machine value
        var maintenanceCost = costFactors.PreventiveCostMultiplier * 50000; // Default machine value
        
        // Optimal timing balances failure costs vs maintenance costs
        var optimalDays = Math.Sqrt(maintenanceCost / (failureCost * prediction.FailureProbability / 365));
        var recommendedDate = DateTime.UtcNow.AddDays(Math.Min(optimalDays, prediction.RemainingUsefulLifeDays * 0.8));
        
        return recommendedDate;
    }

    private double CalculateMaintenancePriority(
        double failureProbability,
        double remainingRul,
        int criticality,
        BusinessImpact businessImpact)
    {
        // Weighted priority calculation
        var probabilityScore = failureProbability;
        var urgencyScore = Math.Max(0, 1 - (remainingRul / 180)); // Normalized to 180 days
        var criticalityScore = criticality / 5.0; // Assuming criticality scale 1-5
        var businessImpactScore = businessImpact.RevenueImpact / 100.0;

        return (probabilityScore * 0.4 + urgencyScore * 0.3 + criticalityScore * 0.2 + businessImpactScore * 0.1);
    }

    private MaintenanceAction DetermineMaintenanceType(
        HealthClassification healthStatus,
        double failureProbability,
        double remainingRul)
    {
        return (healthStatus, failureProbability, remainingRul) switch
        {
            (HealthClassification.FailureImminent, _, _) => MaintenanceAction.ImmediateRepair,
            (HealthClassification.SignificantDegradation, _, _) or (_, > 0.5, _) => MaintenanceAction.PreventiveMaintenance,
            (HealthClassification.MinorDegradation, _, _) or (_, > 0.3, < 60) => MaintenanceAction.Inspection,
            _ => MaintenanceAction.Monitoring
        };
    }

    private ExpectedBenefits CalculateExpectedBenefits(
        Prediction prediction,
        Machine machine,
        MaintenanceAction action,
        BusinessImpact businessImpact)
    {
        var avoidedFailures = action != MaintenanceAction.Monitoring ? 0.8 : 0.2;
        var uptimeImprovement = action switch
        {
            MaintenanceAction.ImmediateRepair => 0.95,
            MaintenanceAction.PreventiveMaintenance => 0.90,
            MaintenanceAction.Inspection => 0.70,
            _ => 0.50
        };

        return new ExpectedBenefits
        {
            AvoidedDowntimeHours = avoidedFailures * (prediction.RemainingUsefulLifeDays * 0.1 * 24),
            RevenueProtection = avoidedFailures * businessImpact.RevenueImpact,
            SafetyImprovement = uptimeImprovement,
            EquipmentLifeExtension = action != MaintenanceAction.Monitoring ? 0.15 : 0.05
        };
    }

    private RiskMitigation CalculateRiskMitigation(Prediction prediction)
    {
        return new RiskMitigation
        {
            FailureRiskReduction = Math.Min(0.9, prediction.FailureProbability * 0.8),
            SafetyRiskReduction = 0.75,
            EnvironmentalRiskReduction = 0.70,
            OperationalRiskReduction = 0.80
        };
    }

    private decimal CalculateMaintenanceCost(MaintenanceAction action)
    {
        return action switch
        {
            MaintenanceAction.ImmediateRepair => 5000m,
            MaintenanceAction.PreventiveMaintenance => 2000m,
            MaintenanceAction.Inspection => 500m,
            MaintenanceAction.Monitoring => 100m,
            _ => 0m
        };
    }

    private async Task<int> GetRecentAnomalyCount(Guid machineId, CancellationToken ct)
    {
        var anomalyResult = await _advancedPredictiveService.DetectAnomaliesAsync(
            machineId, 
            DateTime.UtcNow.AddDays(-7), 
            DateTime.UtcNow, 
            ct);
        return anomalyResult.TotalAnomalies;
    }

    private async Task<Prediction?> GetLatestPrediction(Guid machineId, CancellationToken ct)
    {
        var predictions = await _predictionRepository.GetAllAsync(
            p => p.MachineId == machineId, ct);
        return predictions.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
    }

    private MaintenanceWindow GenerateMaintenanceWindow(MachineState state, DateTime planningHorizon)
    {
        var duration = state.HealthStatus switch
        {
            HealthClassification.FailureImminent => TimeSpan.FromHours(8),
            HealthClassification.SignificantDegradation => TimeSpan.FromHours(4),
            _ => TimeSpan.FromHours(2)
        };

        return new MaintenanceWindow
        {
            MachineId = state.MachineId,
            WindowStart = DateTime.UtcNow.AddDays(7), // Default 1 week out
            WindowEnd = DateTime.UtcNow.AddDays(7).Add(duration),
            Duration = duration,
            Priority = state.Criticality
        };
    }

    private List<ScheduledActivity> OptimizeScheduleUsingGeneticAlgorithm(
        List<MachineState> machineStates,
        List<MaintenanceWindow> maintenanceWindows,
        DateTime planningHorizon)
    {
        // Simplified genetic algorithm implementation
        var population = InitializePopulation(maintenanceWindows, 50);
        var generations = 100;

        for (int gen = 0; gen < generations; gen++)
        {
            var fitnessScores = population.Select(schedule => 
                CalculateScheduleFitness(schedule, machineStates)).ToArray();
            
            var selected = SelectParents(population, fitnessScores, 25);
            var offspring = CreateOffspring(selected, maintenanceWindows);
            population = offspring.Concat(selected).Take(50).ToList();
        }

        return population.OrderByDescending(schedule => 
            CalculateScheduleFitness(schedule, machineStates)).First();
    }

    private double CalculateScheduleFitness(List<ScheduledActivity> schedule, List<MachineState> machineStates)
    {
        var totalEfficiency = 0.0;
        var totalRisk = 0.0;

        foreach (var activity in schedule)
        {
            var machineState = machineStates.FirstOrDefault(m => m.MachineId == activity.MachineId);
            if (machineState != null)
            {
                // Higher efficiency for critical machines maintained early
                totalEfficiency += machineState.Criticality * (1.0 - activity.StartTime.Day / 30.0);
                // Lower risk for high-probability failure machines
                totalRisk += machineState.FailureProbability;
            }
        }

        return totalEfficiency - totalRisk;
    }

    private List<List<ScheduledActivity>> InitializePopulation(List<MaintenanceWindow> windows, int populationSize)
    {
        var population = new List<List<ScheduledActivity>>();
        var random = new Random();

        for (int i = 0; i < populationSize; i++)
        {
            var schedule = new List<ScheduledActivity>();
            foreach (var window in windows)
            {
                schedule.Add(new ScheduledActivity
                {
                    MachineId = window.MachineId,
                    ActivityType = "Maintenance",
                    StartTime = window.WindowStart.AddDays(random.Next(0, 14)),
                    Duration = window.Duration,
                    Resources = ["Technician"]
                });
            }
            population.Add(schedule);
        }

        return population;
    }

    private List<List<ScheduledActivity>> SelectParents(
        List<List<ScheduledActivity>> population, 
        double[] fitnessScores, 
        int parentCount)
    {
        var parents = new List<List<ScheduledActivity>>();
        var totalFitness = fitnessScores.Sum();
        
        for (int i = 0; i < parentCount; i++)
        {
            var randomValue = new Random().NextDouble() * totalFitness;
            var cumulative = 0.0;
            
            for (int j = 0; j < population.Count; j++)
            {
                cumulative += fitnessScores[j];
                if (cumulative >= randomValue)
                {
                    parents.Add(population[j]);
                    break;
                }
            }
        }

        return parents;
    }

    private List<List<ScheduledActivity>> CreateOffspring(
        List<List<ScheduledActivity>> parents, 
        List<MaintenanceWindow> windows)
    {
        var offspring = new List<List<ScheduledActivity>>();
        var random = new Random();

        for (int i = 0; i < parents.Count - 1; i += 2)
        {
            var parent1 = parents[i];
            var parent2 = parents[i + 1];

            // Crossover
            var child1 = new List<ScheduledActivity>();
            var child2 = new List<ScheduledActivity>();

            for (int j = 0; j < parent1.Count; j++)
            {
                if (random.NextDouble() < 0.5)
                {
                    child1.Add(parent1[j]);
                    child2.Add(parent2[j]);
                }
                else
                {
                    child1.Add(parent2[j]);
                    child2.Add(parent1[j]);
                }
            }

            // Mutation
            MutateSchedule(child1, windows);
            MutateSchedule(child2, windows);

            offspring.Add(child1);
            offspring.Add(child2);
        }

        return offspring;
    }

    private void MutateSchedule(List<ScheduledActivity> schedule, List<MaintenanceWindow> windows)
    {
        var random = new Random();
        if (random.NextDouble() < 0.1) // 10% mutation rate
        {
            var index = random.Next(schedule.Count);
            var window = windows.FirstOrDefault(w => w.MachineId == schedule[index].MachineId);
            if (window != null)
            {
                schedule[index].StartTime = window.WindowStart.AddDays(random.Next(0, 14));
            }
        }
    }

    private double CalculateEfficiencyGain(List<ScheduledActivity> optimizedSchedule)
    {
        // Simplified efficiency calculation
        return Math.Min(0.95, optimizedSchedule.Count * 0.05);
    }

    private double CalculateScheduleRiskReduction(List<MachineState> machineStates, List<ScheduledActivity> schedule)
    {
        var totalRiskBefore = machineStates.Sum(m => m.FailureProbability);
        var maintainedMachines = schedule.Select(s => s.MachineId).ToHashSet();
        var remainingRisk = machineStates.Where(m => !maintainedMachines.Contains(m.MachineId))
            .Sum(m => m.FailureProbability);
        
        return totalRiskBefore > 0 ? (totalRiskBefore - remainingRisk) / totalRiskBefore : 0;
    }

    private ResourceUtilization CalculateResourceUtilization(List<ScheduledActivity> schedule)
    {
        var totalHours = schedule.Sum(s => s.Duration.TotalHours);
        var technicianHours = schedule.Count * 8.0; // Assuming 8-hour shifts
        var equipmentHours = totalHours;

        return new ResourceUtilization
        {
            TechnicianUtilization = Math.Min(1.0, technicianHours / (schedule.Count * 40.0)), // 5-day work week
            EquipmentUtilization = Math.Min(1.0, equipmentHours / (schedule.Count * 168.0)), // 168 hours/week
            OverallEfficiency = (technicianHours + equipmentHours) / (2 * totalHours)
        };
    }

    private async Task<MaintenanceRequirement?> CalculateMaintenanceRequirement(
        Guid machineId, 
        DateTime planningPeriod, 
        CancellationToken ct)
    {
        var prediction = await GetLatestPrediction(machineId, ct);
        if (prediction == null) return null;

        return new MaintenanceRequirement
        {
            MachineId = machineId,
            RequiredActions = DetermineRequiredActions(prediction),
            EstimatedDuration = TimeSpan.FromHours(8),
            RequiredSkills = ["Mechanical Technician"],
            PartsNeeded = ["Filters", "Belts"]
        };
    }

    private List<string> DetermineRequiredActions(Prediction prediction)
    {
        return prediction.HealthStatus switch
        {
            HealthClassification.FailureImminent => ["Emergency Repair", "Component Replacement"],
            HealthClassification.SignificantDegradation => ["Preventive Maintenance", "Calibration"],
            HealthClassification.MinorDegradation => ["Inspection", "Lubrication"],
            _ => ["Routine Check"]
        };
    }

    private async Task<List<Resource>> GetAvailableResources(DateTime planningPeriod, CancellationToken ct)
    {
        // Simplified resource availability
        return new List<Resource>
        {
            new() { Id = "tech_001", Type = "Technician", AvailableHours = 40 },
            new() { Id = "tech_002", Type = "Technician", AvailableHours = 32 },
            new() { Id = "equip_001", Type = "Equipment", AvailableHours = 168 }
        };
    }

    private List<ResourceAllocation> OptimizeResourceAllocation(
        List<MaintenanceRequirement> requirements,
        List<Resource> availableResources)
    {
        var allocations = new List<ResourceAllocation>();
        var remainingResources = availableResources.ToDictionary(r => r.Id, r => r.AvailableHours);

        foreach (var requirement in requirements)
        {
            var allocation = new ResourceAllocation
            {
                RequirementId = requirement.MachineId,
                AllocatedResources = new List<AllocatedResource>(),
                StartTime = DateTime.UtcNow.AddDays(3), // Default scheduling
                Duration = requirement.EstimatedDuration
            };

            // Allocate technicians
            var techResource = availableResources.FirstOrDefault(r => 
                r.Type == "Technician" && remainingResources[r.Id] >= 8);
            if (techResource != null)
            {
                allocation.AllocatedResources.Add(new AllocatedResource
                {
                    ResourceId = techResource.Id,
                    Hours = 8
                });
                remainingResources[techResource.Id] -= 8;
            }

            allocations.Add(allocation);
        }

        return allocations;
    }

    private double CalculateResourceUtilizationRate(
        List<ResourceAllocation> allocations, 
        List<Resource> availableResources)
    {
        var allocatedHours = allocations.Sum(a => a.AllocatedResources.Sum(r => r.Hours));
        var totalAvailableHours = availableResources.Sum(r => r.AvailableHours);
        return totalAvailableHours > 0 ? allocatedHours / totalAvailableHours : 0;
    }

    private List<BottleneckResource> IdentifyBottleneckResources(
        List<ResourceAllocation> allocations, 
        List<Resource> availableResources)
    {
        var bottlenecks = new List<BottleneckResource>();
        
        foreach (var resource in availableResources)
        {
            var allocatedHours = allocations
                .SelectMany(a => a.AllocatedResources)
                .Where(ar => ar.ResourceId == resource.Id)
                .Sum(ar => ar.Hours);
            
            var utilization = resource.AvailableHours > 0 ? allocatedHours / resource.AvailableHours : 0;
            
            if (utilization > 0.8) // 80% threshold for bottleneck
            {
                bottlenecks.Add(new BottleneckResource
                {
                    ResourceId = resource.Id,
                    ResourceType = resource.Type,
                    UtilizationRate = utilization,
                    RequiredAdditionalCapacity = allocatedHours - resource.AvailableHours * 0.8
                });
            }
        }

        return bottlenecks;
    }

    private double CalculateAllocationCostEfficiency(List<ResourceAllocation> allocations)
    {
        // Simplified cost efficiency calculation
        var totalAllocations = allocations.Count;
        var efficientAllocations = allocations.Count(a => 
            a.AllocatedResources.Count >= 1 && a.Duration.TotalHours <= 24);
        
        return totalAllocations > 0 ? (double)efficientAllocations / totalAllocations : 0;
    }

    private List<MaintenanceStrategy> GenerateMaintenanceStrategies(
        IEnumerable<Guid> machineIds, 
        BudgetConstraints budget)
    {
        return new List<MaintenanceStrategy>
        {
            new MaintenanceStrategy
            {
                StrategyId = "preventive_optimal",
                Name = "Optimal Preventive",
                Description = "Balanced preventive maintenance focusing on high-risk equipment",
                MachinePriorities = machineIds.Take(5).ToList(), // Top 5 priority machines
                BudgetAllocation = budget.TotalBudget * 0.7m
            },
            new MaintenanceStrategy
            {
                StrategyId = "reactive_minimal",
                Name = "Minimal Reactive",
                Description = "Minimal intervention, reactive maintenance only",
                MachinePriorities = machineIds.Take(2).ToList(), // Only critical machines
                BudgetAllocation = budget.TotalBudget * 0.3m
            },
            new MaintenanceStrategy
            {
                StrategyId = "comprehensive",
                Name = "Comprehensive",
                Description = "Full maintenance program for all equipment",
                MachinePriorities = machineIds.ToList(),
                BudgetAllocation = budget.TotalBudget
            }
        };
    }

    private async Task<CostScenario> EvaluateCostScenario(
        MaintenanceStrategy strategy, 
        BudgetConstraints budget, 
        CancellationToken ct)
    {
        var totalCost = 0m;
        var totalBenefit = 0m;
        var avoidedFailures = 0;

        foreach (var machineId in strategy.MachinePriorities)
        {
            var prediction = await GetLatestPrediction(machineId, ct);
            if (prediction != null)
            {
                var maintenanceCost = CalculateMaintenanceCost(MaintenanceAction.PreventiveMaintenance);
                var avoidedFailureValue = (decimal)prediction.FailureProbability * 10000m; // Avoided failure cost
                
                totalCost += maintenanceCost;
                totalBenefit += avoidedFailureValue;
                avoidedFailures += prediction.FailureProbability > 0.3 ? 1 : 0;
            }
        }

        return new CostScenario
        {
            StrategyId = strategy.StrategyId,
            TotalCost = totalCost,
            TotalBenefit = totalBenefit,
            NetBenefit = totalBenefit - totalCost,
            ROI = totalCost > 0 ? (double)(totalBenefit / totalCost) : 0,
            AvoidedFailures = avoidedFailures,
            BudgetUtilization = (double)(totalCost / budget.TotalBudget)
        };
    }

    private CostScenario SelectOptimalStrategy(List<CostScenario> scenarios, BudgetConstraints budget)
    {
        // Select strategy with best risk-adjusted ROI within budget
        return scenarios
            .Where(s => s.BudgetUtilization <= 1.0)
            .OrderByDescending(s => s.ROI * (1 - s.BudgetUtilization))
            .FirstOrDefault() ?? scenarios.First();
    }

    private CostBenefitAnalysis PerformCostBenefitAnalysis(
        CostScenario optimalScenario, 
        List<CostScenario> allScenarios)
    {
        return new CostBenefitAnalysis
        {
            OptimalScenario = optimalScenario,
            OpportunityCost = allScenarios.Max(s => s.NetBenefit) - optimalScenario.NetBenefit,
            BreakEvenPoint = CalculateBreakEvenPoint(optimalScenario),
            SensitivityAnalysis = PerformSensitivityAnalysis(allScenarios)
        };
    }

    private double CalculateBreakEvenPoint(CostScenario scenario)
    {
        return scenario.TotalCost > 0 ? (double)(scenario.TotalBenefit / scenario.TotalCost) : 0;
    }

    private Dictionary<string, double> PerformSensitivityAnalysis(List<CostScenario> scenarios)
    {
        var analysis = new Dictionary<string, double>();
        
        // Calculate variance in ROI across scenarios
        var rois = scenarios.Select(s => s.ROI).ToArray();
        var meanRoi = rois.Average();
        var roiVariance = rois.Select(roi => Math.Pow(roi - meanRoi, 2)).Average();
        
        analysis["ROI_Variance"] = roiVariance;
        analysis["ROI_Stability"] = 1.0 / (1.0 + roiVariance); // Higher is more stable
        
        return analysis;
    }

    private double CalculateRiskAdjustedROI(CostScenario scenario)
    {
        // Adjust ROI based on risk factors
        var riskAdjustment = 0.8; // Conservative adjustment
        return scenario.ROI * riskAdjustment;
    }

    #endregion
}

#region Data Models

public class MaintenanceOptimizationCriteria
{
    public CostFactors CostFactors { get; set; } = new();
    public BusinessImpact BusinessImpact { get; set; } = new();
    public TimeConstraints TimeConstraints { get; set; } = new();
}

public class CostFactors
{
    public double PreventiveCostMultiplier { get; set; } = 0.1;
    public double FailureCostMultiplier { get; set; } = 1.0;
    public double DowntimeCostPerHour { get; set; } = 1000;
}

public class BusinessImpact
{
    public double RevenueImpact { get; set; } = 50000;
    public double SafetyImpact { get; set; } = 0.8;
    public double EnvironmentalImpact { get; set; } = 0.6;
}

public class TimeConstraints
{
    public DateTime EarliestStart { get; set; } = DateTime.UtcNow;
    public DateTime LatestCompletion { get; set; } = DateTime.UtcNow.AddDays(30);
    public List<DateTime[]> PreferredWindows { get; set; } = [];
}

public class PrescriptiveRecommendation
{
    public Guid MachineId { get; set; }
    public MaintenanceAction RecommendedAction { get; set; }
    public double PriorityScore { get; set; }
    public DateTime RecommendedTiming { get; set; }
    public ExpectedBenefits ExpectedBenefits { get; set; } = new();
    public RiskMitigation RiskMitigation { get; set; } = new();
    public decimal CostEstimate { get; set; }
    public double ConfidenceScore { get; set; }
    public DateTime GeneratedAt { get; set; }
    public EvidenceSummary SupportingEvidence { get; set; } = new();
}

public enum MaintenanceAction
{
    Monitoring,
    Inspection,
    PreventiveMaintenance,
    ImmediateRepair
}

public class ExpectedBenefits
{
    public double AvoidedDowntimeHours { get; set; }
    public double RevenueProtection { get; set; }
    public double SafetyImprovement { get; set; }
    public double EquipmentLifeExtension { get; set; }
}

public class RiskMitigation
{
    public double FailureRiskReduction { get; set; }
    public double SafetyRiskReduction { get; set; }
    public double EnvironmentalRiskReduction { get; set; }
    public double OperationalRiskReduction { get; set; }
}

public class EvidenceSummary
{
    public double PredictionConfidence { get; set; }
    public double DataQualityScore { get; set; }
    public double ModelAccuracy { get; set; }
    public int RecentAnomalies { get; set; }
}

public class MachineState
{
    public Guid MachineId { get; set; }
    public HealthClassification HealthStatus { get; set; }
    public double RemainingUsefulLife { get; set; }
    public double FailureProbability { get; set; }
    public int Criticality { get; set; }
    public double CurrentLoad { get; set; }
}

public class MaintenanceWindow
{
    public Guid MachineId { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public TimeSpan Duration { get; set; }
    public int Priority { get; set; }
}

public class SchedulingRecommendation
{
    public DateTime PlanningHorizon { get; set; }
    public List<ScheduledActivity> OptimizedSchedule { get; set; } = [];
    public double EfficiencyGain { get; set; }
    public double RiskReduction { get; set; }
    public ResourceUtilization ResourceUtilization { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ScheduledActivity
{
    public Guid MachineId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> Resources { get; set; } = [];
}

public class ResourceUtilization
{
    public double TechnicianUtilization { get; set; }
    public double EquipmentUtilization { get; set; }
    public double OverallEfficiency { get; set; }
}

public class ResourceAllocationPlan
{
    public DateTime PlanningPeriod { get; set; }
    public List<ResourceAllocation> Allocations { get; set; } = [];
    public double ResourceUtilizationRate { get; set; }
    public List<BottleneckResource> BottleneckResources { get; set; } = [];
    public double CostEfficiency { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class MaintenanceRequirement
{
    public Guid MachineId { get; set; }
    public List<string> RequiredActions { get; set; } = [];
    public TimeSpan EstimatedDuration { get; set; }
    public List<string> RequiredSkills { get; set; } = [];
    public List<string> PartsNeeded { get; set; } = [];
}

public class Resource
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double AvailableHours { get; set; }
}

public class ResourceAllocation
{
    public Guid RequirementId { get; set; }
    public List<AllocatedResource> AllocatedResources { get; set; } = [];
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
}

public class AllocatedResource
{
    public string ResourceId { get; set; } = string.Empty;
    public double Hours { get; set; }
}

public class BottleneckResource
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public double UtilizationRate { get; set; }
    public double RequiredAdditionalCapacity { get; set; }
}

public class CostOptimizationResult
{
    public BudgetConstraints Budget { get; set; } = new();
    public CostScenario OptimalStrategy { get; set; } = new();
    public List<CostScenario> AlternativeScenarios { get; set; } = [];
    public CostBenefitAnalysis CostBenefitAnalysis { get; set; } = new();
    public double RiskAdjustedROI { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class BudgetConstraints
{
    public decimal TotalBudget { get; set; } = 100000m;
    public Dictionary<string, decimal> CategoryLimits { get; set; } = [];
}

public class MaintenanceStrategy
{
    public string StrategyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> MachinePriorities { get; set; } = [];
    public decimal BudgetAllocation { get; set; }
}

public class CostScenario
{
    public string StrategyId { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public decimal TotalBenefit { get; set; }
    public decimal NetBenefit { get; set; }
    public double ROI { get; set; }
    public int AvoidedFailures { get; set; }
    public double BudgetUtilization { get; set; }
}

public class CostBenefitAnalysis
{
    public CostScenario OptimalScenario { get; set; } = new();
    public decimal OpportunityCost { get; set; }
    public double BreakEvenPoint { get; set; }
    public Dictionary<string, double> SensitivityAnalysis { get; set; } = [];
}

#endregion