using System.Numerics;

namespace DigitalTwinPlatform.API.Services.MathematicalModeling;

public interface IDifferentialEquationSolver
{
    /// <summary>
    /// Solves ordinary differential equations using Runge-Kutta method
    /// </summary>
    Task<OdeSolution> SolveOdeAsync(
        Func<double, double[], double[]> derivatives,
        double[] initialConditions,
        double startTime,
        double endTime,
        double stepSize,
        CancellationToken ct = default);

    /// <summary>
    /// Solves system of differential equations for dynamic system modeling
    /// </summary>
    Task<SystemDynamicsSolution> SolveSystemDynamicsAsync(
        SystemDynamicsModel model,
        double simulationTime,
        CancellationToken ct = default);
}

public interface IOptimizationService
{
    /// <summary>
    /// Performs gradient-based optimization for parameter tuning
    /// </summary>
    Task<OptimizationResult> OptimizeParametersAsync(
        Func<double[], double> objectiveFunction,
        double[] initialGuess,
        OptimizationOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Genetic algorithm optimization for complex parameter spaces
    /// </summary>
    Task<OptimizationResult> GeneticOptimizationAsync(
        Func<double[], double> fitnessFunction,
        int parameterCount,
        GeneticAlgorithmOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Multi-objective optimization using NSGA-II algorithm
    /// </summary>
    Task<MultiObjectiveResult> MultiObjectiveOptimizationAsync(
        Func<double[], double[]> objectives,
        int parameterCount,
        MultiObjectiveOptions options,
        CancellationToken ct = default);
}

public class DifferentialEquationSolver : IDifferentialEquationSolver
{
    private readonly ILogger<DifferentialEquationSolver> _logger;

    public DifferentialEquationSolver(ILogger<DifferentialEquationSolver> logger)
    {
        _logger = logger;
    }

    public async Task<OdeSolution> SolveOdeAsync(
        Func<double, double[], double[]> derivatives,
        double[] initialConditions,
        double startTime,
        double endTime,
        double stepSize,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        _logger.LogInformation("Solving ODE from {StartTime} to {EndTime} with step size {StepSize}", 
            startTime, endTime, stepSize);

        var steps = (int)((endTime - startTime) / stepSize) + 1;
        var timePoints = new double[steps];
        var solutions = new double[steps][];

        // Initialize
        timePoints[0] = startTime;
        solutions[0] = (double[])initialConditions.Clone();

        // Fourth-order Runge-Kutta method
        for (int i = 1; i < steps; i++)
        {
            ct.ThrowIfCancellationRequested();

            var currentTime = startTime + (i - 1) * stepSize;
            var currentSolution = solutions[i - 1];

            // RK4 stages
            var k1 = derivatives(currentTime, currentSolution);
            var k2 = derivatives(currentTime + stepSize / 2, 
                VectorAdd(currentSolution, VectorMultiply(k1, stepSize / 2)));
            var k3 = derivatives(currentTime + stepSize / 2, 
                VectorAdd(currentSolution, VectorMultiply(k2, stepSize / 2)));
            var k4 = derivatives(currentTime + stepSize, 
                VectorAdd(currentSolution, VectorMultiply(k3, stepSize)));

            // Combine stages
            var delta = new double[currentSolution.Length];
            for (int j = 0; j < delta.Length; j++)
            {
                delta[j] = (k1[j] + 2 * k2[j] + 2 * k3[j] + k4[j]) * stepSize / 6;
            }

            timePoints[i] = currentTime + stepSize;
            solutions[i] = VectorAdd(currentSolution, delta);
        }

        _logger.LogInformation("ODE solved successfully with {Steps} steps", steps);

        return new OdeSolution
        {
            TimePoints = timePoints,
            Solutions = solutions,
            Steps = steps,
            FinalTime = timePoints[^1],
            FinalState = solutions[^1]
        };
    }

    public async Task<SystemDynamicsSolution> SolveSystemDynamicsAsync(
        SystemDynamicsModel model,
        double simulationTime,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Solving system dynamics for {SystemName}", model.SystemName);

        var result = await SolveOdeAsync(
            model.Derivatives,
            model.InitialConditions,
            0.0,
            simulationTime,
            model.StepSize,
            ct);

        // Calculate derived quantities
        var derivedQuantities = new Dictionary<string, double[]>();
        foreach (var calculator in model.DerivedQuantityCalculators)
        {
            var values = new double[result.Steps];
            for (int i = 0; i < result.Steps; i++)
            {
                values[i] = calculator(result.TimePoints[i], result.Solutions[i]);
            }
            derivedQuantities[calculator.Method.Name] = values;
        }

        // Perform stability analysis
        var stability = AnalyzeStability(result.Solutions);

        return new SystemDynamicsSolution
        {
            OdeSolution = result,
            SystemName = model.SystemName,
            DerivedQuantities = derivedQuantities,
            StabilityAnalysis = stability,
            EnergyBalance = CalculateEnergyBalance(result)
        };
    }

    private StabilityAnalysis AnalyzeStability(double[][] solutions)
    {
        if (solutions.Length < 2) return new StabilityAnalysis { IsStable = true };

        var lastState = solutions[^1];
        var previousState = solutions[^2];
        
        var maxChange = 0.0;
        for (int i = 0; i < lastState.Length; i++)
        {
            var change = Math.Abs(lastState[i] - previousState[i]);
            maxChange = Math.Max(maxChange, change);
        }

        return new StabilityAnalysis
        {
            IsStable = maxChange < 1e-6,
            MaxChange = maxChange,
            ConvergenceRate = maxChange == 0 ? 1.0 : Math.Min(1.0, 1.0 / maxChange)
        };
    }

    private EnergyBalance CalculateEnergyBalance(OdeSolution solution)
    {
        // Simplified energy balance calculation
        var kineticEnergy = 0.0;
        var potentialEnergy = 0.0;

        for (int i = 0; i < solution.Steps; i++)
        {
            // Assuming first variable is velocity, second is position
            if (solution.Solutions[i].Length >= 2)
            {
                var velocity = solution.Solutions[i][0];
                var position = solution.Solutions[i][1];
                
                kineticEnergy += 0.5 * Math.Pow(velocity, 2);
                potentialEnergy += 0.5 * Math.Pow(position, 2);
            }
        }

        return new EnergyBalance
        {
            KineticEnergy = kineticEnergy / solution.Steps,
            PotentialEnergy = potentialEnergy / solution.Steps,
            TotalEnergy = (kineticEnergy + potentialEnergy) / solution.Steps,
            EnergyConservation = Math.Abs(kineticEnergy + potentialEnergy) < 1e-3
        };
    }

    #region Vector Operations

    private double[] VectorAdd(double[] a, double[] b)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i];
        }
        return result;
    }

    private double[] VectorMultiply(double[] vector, double scalar)
    {
        var result = new double[vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            result[i] = vector[i] * scalar;
        }
        return result;
    }

    #endregion
}

public class OptimizationService : IOptimizationService
{
    private readonly ILogger<OptimizationService> _logger;

    public OptimizationService(ILogger<OptimizationService> logger)
    {
        _logger = logger;
    }

    public async Task<OptimizationResult> OptimizeParametersAsync(
        Func<double[], double> objectiveFunction,
        double[] initialGuess,
        OptimizationOptions options,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        _logger.LogInformation("Starting gradient-based optimization");

        var currentSolution = (double[])initialGuess.Clone();
        var bestSolution = (double[])currentSolution.Clone();
        var bestValue = objectiveFunction(currentSolution);
        var iterations = 0;

        while (iterations < options.MaxIterations && !ct.IsCancellationRequested)
        {
            // Calculate gradient using finite differences
            var gradient = CalculateGradient(objectiveFunction, currentSolution, options.Epsilon);

            // Update solution using gradient descent
            for (int i = 0; i < currentSolution.Length; i++)
            {
                currentSolution[i] -= options.LearningRate * gradient[i];
                
                // Apply bounds
                if (options.Bounds != null && options.Bounds.Length > i)
                {
                    currentSolution[i] = Math.Clamp(currentSolution[i], 
                        options.Bounds[i].Min, options.Bounds[i].Max);
                }
            }

            var currentValue = objectiveFunction(currentSolution);
            
            if (currentValue < bestValue)
            {
                bestValue = currentValue;
                Array.Copy(currentSolution, bestSolution, currentSolution.Length);
            }

            iterations++;

            // Check convergence
            if (Math.Abs(currentValue - bestValue) < options.Tolerance)
                break;
        }

        _logger.LogInformation("Optimization completed in {Iterations} iterations", iterations);

        return new OptimizationResult
        {
            OptimalParameters = bestSolution,
            OptimalValue = bestValue,
            Iterations = iterations,
            Converged = iterations < options.MaxIterations,
            Method = "Gradient Descent"
        };
    }

    public async Task<OptimizationResult> GeneticOptimizationAsync(
        Func<double[], double> fitnessFunction,
        int parameterCount,
        GeneticAlgorithmOptions options,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        _logger.LogInformation("Starting genetic algorithm optimization");

        // Initialize population
        var population = InitializePopulation(parameterCount, options.PopulationSize, options.ParameterBounds);
        var fitnessValues = new double[options.PopulationSize];
        
        for (int i = 0; i < options.PopulationSize; i++)
        {
            fitnessValues[i] = fitnessFunction(population[i]);
        }

        var bestIndividual = population[0];
        var bestFitness = fitnessValues[0];
        var generation = 0;

        while (generation < options.MaxGenerations && !ct.IsCancellationRequested)
        {
            // Selection
            var selectedParents = TournamentSelection(population, fitnessValues, options.TournamentSize);

            // Crossover
            var offspring = new List<double[]>();
            for (int i = 0; i < selectedParents.Count; i += 2)
            {
                if (i + 1 < selectedParents.Count)
                {
                    var children = UniformCrossover(selectedParents[i], selectedParents[i + 1]);
                    offspring.AddRange(children);
                }
            }

            // Mutation
            foreach (var individual in offspring)
            {
                Mutate(individual, options.MutationRate, options.ParameterBounds);
            }

            // Evaluate offspring
            var offspringFitness = new double[offspring.Count];
            for (int i = 0; i < offspring.Count; i++)
            {
                offspringFitness[i] = fitnessFunction(offspring[i]);
            }

            // Replacement (steady state)
            ReplaceWorstIndividuals(population, fitnessValues, offspring.ToArray(), offspringFitness);

            // Update best solution
            for (int i = 0; i < population.Length; i++)
            {
                if (fitnessValues[i] < bestFitness)
                {
                    bestFitness = fitnessValues[i];
                    bestIndividual = (double[])population[i].Clone();
                }
            }

            generation++;
        }

        _logger.LogInformation("Genetic optimization completed in {Generations} generations", generation);

        return new OptimizationResult
        {
            OptimalParameters = bestIndividual,
            OptimalValue = bestFitness,
            Iterations = generation,
            Converged = generation < options.MaxGenerations,
            Method = "Genetic Algorithm"
        };
    }

    public async Task<MultiObjectiveResult> MultiObjectiveOptimizationAsync(
        Func<double[], double[]> objectives,
        int parameterCount,
        MultiObjectiveOptions options,
        CancellationToken ct = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        _logger.LogInformation("Starting multi-objective optimization (NSGA-II)");

        // Initialize population
        var population = InitializePopulation(parameterCount, options.PopulationSize, options.ParameterBounds);
        var objectiveValues = new double[options.PopulationSize][];
        
        for (int i = 0; i < options.PopulationSize; i++)
        {
            objectiveValues[i] = objectives(population[i]);
        }

        var paretoFront = new List<int>();
        var generation = 0;

        while (generation < options.MaxGenerations && !ct.IsCancellationRequested)
        {
            // Fast non-dominated sorting
            var fronts = FastNonDominatedSort(objectiveValues);
            
            // Select parent population
            var parents = SelectParents(population, fronts, options.PopulationSize / 2);

            // Create offspring through crossover and mutation
            var offspring = CreateOffspring(parents, options);
            
            // Evaluate offspring
            var offspringObjectives = new double[offspring.Count][];
            for (int i = 0; i < offspring.Count; i++)
            {
                offspringObjectives[i] = objectives(offspring[i]);
            }

            // Combine parent and offspring populations
            var combinedPopulation = population.Concat(offspring).ToArray();
            var combinedObjectives = objectiveValues.Concat(offspringObjectives).ToArray();

            // Environmental selection
            var nextGenerationIndices = EnvironmentalSelection(combinedObjectives, options.PopulationSize);
            population = nextGenerationIndices.Select(i => combinedPopulation[i]).ToArray();
            objectiveValues = nextGenerationIndices.Select(i => combinedObjectives[i]).ToArray();

            generation++;
        }

        // Final Pareto front
        var finalFronts = FastNonDominatedSort(objectiveValues);
        paretoFront = finalFronts.FirstOrDefault() ?? [];

        _logger.LogInformation("Multi-objective optimization completed with {Solutions} Pareto optimal solutions", 
            paretoFront.Count);

        return new MultiObjectiveResult
        {
            ParetoOptimalSolutions = paretoFront.Select(i => population[i]).ToArray(),
            ObjectiveValues = paretoFront.Select(i => objectiveValues[i]).ToArray(),
            Generations = generation,
            PopulationSize = options.PopulationSize,
            Method = "NSGA-II"
        };
    }

    #region Private Helper Methods

    private double[] CalculateGradient(Func<double[], double> function, double[] point, double epsilon)
    {
        var gradient = new double[point.Length];
        var originalValue = function(point);

        for (int i = 0; i < point.Length; i++)
        {
            var perturbedPoint = (double[])point.Clone();
            perturbedPoint[i] += epsilon;
            var perturbedValue = function(perturbedPoint);
            gradient[i] = (perturbedValue - originalValue) / epsilon;
        }

        return gradient;
    }

    private double[][] InitializePopulation(int parameterCount, int populationSize, ParameterBounds[] bounds)
    {
        var random = new Random();
        var population = new double[populationSize][];

        for (int i = 0; i < populationSize; i++)
        {
            population[i] = new double[parameterCount];
            for (int j = 0; j < parameterCount; j++)
            {
                var min = bounds?.Length > j ? bounds[j].Min : -10.0;
                var max = bounds?.Length > j ? bounds[j].Max : 10.0;
                population[i][j] = min + random.NextDouble() * (max - min);
            }
        }

        return population;
    }

    private List<double[]> TournamentSelection(double[][] population, double[] fitness, int tournamentSize)
    {
        var random = new Random();
        var selected = new List<double[]>();

        for (int i = 0; i < population.Length; i++)
        {
            var tournamentIndices = Enumerable.Range(0, population.Length)
                .OrderBy(_ => random.Next())
                .Take(tournamentSize)
                .ToArray();

            var winnerIndex = tournamentIndices.OrderBy(idx => fitness[idx]).First();
            selected.Add((double[])population[winnerIndex].Clone());
        }

        return selected;
    }

    private List<double[]> UniformCrossover(double[] parent1, double[] parent2)
    {
        var random = new Random();
        var child1 = (double[])parent1.Clone();
        var child2 = (double[])parent2.Clone();

        for (int i = 0; i < parent1.Length; i++)
        {
            if (random.NextDouble() < 0.5)
            {
                child1[i] = parent2[i];
                child2[i] = parent1[i];
            }
        }

        return [child1, child2];
    }

    private void Mutate(double[] individual, double mutationRate, ParameterBounds[] bounds)
    {
        var random = new Random();
        for (int i = 0; i < individual.Length; i++)
        {
            if (random.NextDouble() < mutationRate)
            {
                var min = bounds?.Length > i ? bounds[i].Min : -10.0;
                var max = bounds?.Length > i ? bounds[i].Max : 10.0;
                individual[i] = min + random.NextDouble() * (max - min);
            }
        }
    }

    private void ReplaceWorstIndividuals(double[][] population, double[] fitness, 
        double[][] offspring, double[] offspringFitness)
    {
        var worstIndices = fitness.Select((f, i) => new { Fitness = f, Index = i })
            .OrderByDescending(x => x.Fitness)
            .Take(offspring.Length)
            .Select(x => x.Index)
            .ToArray();

        for (int i = 0; i < offspring.Length; i++)
        {
            Array.Copy(offspring[i], population[worstIndices[i]], offspring[i].Length);
            fitness[worstIndices[i]] = offspringFitness[i];
        }
    }

    private List<List<int>> FastNonDominatedSort(double[][] objectives)
    {
        var fronts = new List<List<int>>();
        var dominatedBy = new Dictionary<int, HashSet<int>>();
        var dominatesCount = new Dictionary<int, int>();

        for (int i = 0; i < objectives.Length; i++)
        {
            dominatedBy[i] = [];
            dominatesCount[i] = 0;

            for (int j = 0; j < objectives.Length; j++)
            {
                if (i != j)
                {
                    if (Dominates(objectives[i], objectives[j]))
                    {
                        dominatedBy[i].Add(j);
                    }
                    else if (Dominates(objectives[j], objectives[i]))
                    {
                        dominatesCount[i]++;
                    }
                }
            }
        }

        var currentFront = new List<int>();
        for (int i = 0; i < objectives.Length; i++)
        {
            if (dominatesCount[i] == 0)
            {
                currentFront.Add(i);
            }
        }

        while (currentFront.Any())
        {
            fronts.Add(new List<int>(currentFront));
            var nextFront = new List<int>();

            foreach (var individual in currentFront)
            {
                foreach (var dominated in dominatedBy[individual])
                {
                    dominatesCount[dominated]--;
                    if (dominatesCount[dominated] == 0)
                    {
                        nextFront.Add(dominated);
                    }
                }
            }

            currentFront = nextFront;
        }

        return fronts;
    }

    private bool Dominates(double[] solution1, double[] solution2)
    {
        bool betterInAny = false;
        for (int i = 0; i < solution1.Length; i++)
        {
            if (solution1[i] > solution2[i]) return false;
            if (solution1[i] < solution2[i]) betterInAny = true;
        }
        return betterInAny;
    }

    private List<double[]> SelectParents(double[][] population, List<List<int>> fronts, int count)
    {
        var selected = new List<double[]>();
        foreach (var front in fronts)
        {
            if (selected.Count + front.Count <= count)
            {
                selected.AddRange(front.Select(i => (double[])population[i].Clone()));
            }
            else
            {
                // Partially fill the last front
                var remaining = count - selected.Count;
                var indices = front.Take(remaining).ToArray();
                selected.AddRange(indices.Select(i => (double[])population[i].Clone()));
                break;
            }
        }
        return selected;
    }

    private List<double[]> CreateOffspring(List<double[]> parents, MultiObjectiveOptions options)
    {
        var offspring = new List<double[]>();
        var random = new Random();

        while (offspring.Count < parents.Count)
        {
            var parent1 = parents[random.Next(parents.Count)];
            var parent2 = parents[random.Next(parents.Count)];

            var children = UniformCrossover(parent1, parent2);
            
            foreach (var child in children.Take(2)) // Take at most 2 children
            {
                Mutate(child, options.MutationRate, options.ParameterBounds);
                offspring.Add(child);
                
                if (offspring.Count >= parents.Count) break;
            }
        }

        return offspring.Take(parents.Count).ToList();
    }

    private int[] EnvironmentalSelection(double[][] objectives, int populationSize)
    {
        var fronts = FastNonDominatedSort(objectives);
        var selected = new List<int>();

        foreach (var front in fronts)
        {
            if (selected.Count + front.Count <= populationSize)
            {
                selected.AddRange(front);
            }
            else
            {
                // Crowding distance selection for partial front
                var remaining = populationSize - selected.Count;
                var crowdedIndices = CrowdingDistanceSelection(objectives, front, remaining);
                selected.AddRange(crowdedIndices);
                break;
            }
        }

        return selected.ToArray();
    }

    private List<int> CrowdingDistanceSelection(double[][] objectives, List<int> front, int count)
    {
        if (front.Count <= count) return front;

        var distances = new double[front.Count];
        
        for (int obj = 0; obj < objectives[0].Length; obj++)
        {
            var sortedIndices = front.Select((idx, pos) => new { Index = idx, Position = pos })
                .OrderBy(x => objectives[x.Index][obj])
                .ToArray();

            distances[sortedIndices[0].Position] = double.PositiveInfinity;
            distances[sortedIndices[^1].Position] = double.PositiveInfinity;

            for (int i = 1; i < sortedIndices.Length - 1; i++)
            {
                var prevObj = objectives[sortedIndices[i - 1].Index][obj];
                var nextObj = objectives[sortedIndices[i + 1].Index][obj];
                distances[sortedIndices[i].Position] += nextObj - prevObj;
            }
        }

        return front.Select((idx, pos) => new { Index = idx, Distance = distances[pos], OriginalPos = pos })
            .OrderByDescending(x => x.Distance)
            .Take(count)
            .Select(x => x.Index)
            .ToList();
    }

    #endregion
}

#region Data Models

public class OdeSolution
{
    public double[] TimePoints { get; set; } = [];
    public double[][] Solutions { get; set; } = [];
    public int Steps { get; set; }
    public double FinalTime { get; set; }
    public double[] FinalState { get; set; } = [];
}

public class SystemDynamicsSolution
{
    public OdeSolution OdeSolution { get; set; } = new();
    public string SystemName { get; set; } = string.Empty;
    public Dictionary<string, double[]> DerivedQuantities { get; set; } = [];
    public StabilityAnalysis StabilityAnalysis { get; set; } = new();
    public EnergyBalance EnergyBalance { get; set; } = new();
}

public class SystemDynamicsModel
{
    public string SystemName { get; set; } = string.Empty;
    public Func<double, double[], double[]> Derivatives { get; set; } = (_, _) => [];
    public double[] InitialConditions { get; set; } = [];
    public double StepSize { get; set; } = 0.01;
    public List<Func<double, double[], double>> DerivedQuantityCalculators { get; set; } = [];
}

public class StabilityAnalysis
{
    public bool IsStable { get; set; }
    public double MaxChange { get; set; }
    public double ConvergenceRate { get; set; }
}

public class EnergyBalance
{
    public double KineticEnergy { get; set; }
    public double PotentialEnergy { get; set; }
    public double TotalEnergy { get; set; }
    public bool EnergyConservation { get; set; }
}

public class OptimizationResult
{
    public double[] OptimalParameters { get; set; } = [];
    public double OptimalValue { get; set; }
    public int Iterations { get; set; }
    public bool Converged { get; set; }
    public string Method { get; set; } = string.Empty;
}

public class MultiObjectiveResult
{
    public double[][] ParetoOptimalSolutions { get; set; } = [];
    public double[][] ObjectiveValues { get; set; } = [];
    public int Generations { get; set; }
    public int PopulationSize { get; set; }
    public string Method { get; set; } = string.Empty;
}

public class OptimizationOptions
{
    public double LearningRate { get; set; } = 0.01;
    public int MaxIterations { get; set; } = 1000;
    public double Tolerance { get; set; } = 1e-6;
    public double Epsilon { get; set; } = 1e-8;
    public ParameterBounds[]? Bounds { get; set; }
}

public class GeneticAlgorithmOptions
{
    public int PopulationSize { get; set; } = 100;
    public int MaxGenerations { get; set; } = 100;
    public double MutationRate { get; set; } = 0.1;
    public int TournamentSize { get; set; } = 3;
    public ParameterBounds[] ParameterBounds { get; set; } = [];
}

public class MultiObjectiveOptions
{
    public int PopulationSize { get; set; } = 100;
    public int MaxGenerations { get; set; } = 100;
    public double MutationRate { get; set; } = 0.1;
    public ParameterBounds[] ParameterBounds { get; set; } = [];
}

public class ParameterBounds
{
    public double Min { get; set; }
    public double Max { get; set; }
}

#endregion