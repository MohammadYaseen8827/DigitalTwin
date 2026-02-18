using DigitalTwinPlatform.Application.Mathematics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalTwinPlatform.Application.Services;

/// <summary>
/// Service for solving ordinary differential equations (ODEs) for degradation modeling.
/// Provides numerical integration methods for various degradation models including
/// exponential decay, power-law degradation, and complex multi-variable systems.
/// </summary>
public class ODESolverService : IODESolver
{
    private readonly ILogger<ODESolverService> _logger;
    private readonly RungeKutta _rungeKutta;
    private readonly EulerMaruyama _eulerMaruyama;

    public ODESolverService(
        ILogger<ODESolverService> logger,
        RungeKutta rungeKutta,
        EulerMaruyama eulerMaruyama)
    {
        _logger = logger;
        _rungeKutta = rungeKutta;
        _eulerMaruyama = eulerMaruyama;
    }

    /// <summary>
    /// Solves a single ODE using the specified numerical method.
    /// </summary>
    public async Task<ODESolution> SolveODEAsync(
        ODEProblem problem,
        ODESolverMethod method = ODESolverMethod.RungeKutta4,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving ODE using {Method} with {TimePoints} time points", 
            method, problem.TimePoints.Count);

        try
        {
            var solution = method switch
            {
                ODESolverMethod.RungeKutta4 => await _rungeKutta.SolveAsync(problem, cancellationToken),
                ODESolverMethod.EulerMaruyama => await _eulerMaruyama.SolveAsync(problem, cancellationToken),
                _ => throw new ArgumentException($"Unsupported solver method: {method}")
            };

            _logger.LogInformation("Successfully solved ODE with {SolutionPoints} solution points", 
                solution.TimePoints.Count);

            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to solve ODE using method {Method}", method);
            throw;
        }
    }

    /// <summary>
    /// Solves a system of coupled ODEs.
    /// </summary>
    public async Task<ODESolution[]> SolveSystemAsync(
        ODEProblem[] problems,
        ODESolverMethod method = ODESolverMethod.RungeKutta4,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving {ProblemCount} coupled ODEs using {Method}", 
            problems.Length, method);

        try
        {
            var tasks = problems.Select(p => SolveODEAsync(p, method, cancellationToken));
            var solutions = await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully solved {ProblemCount} coupled ODEs", problems.Length);
            return solutions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to solve coupled ODE system");
            throw;
        }
    }

    /// <summary>
    /// Solves a stochastic differential equation (SDE).
    /// </summary>
    public async Task<ODESolution> SolveSDEAsync(
        ODEProblem problem,
        double diffusionCoefficient,
        int numberOfSimulations = 100,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving SDE with {Simulations} simulations and diffusion coefficient {Diffusion}", 
            numberOfSimulations, diffusionCoefficient);

        try
        {
            var solutions = new List<ODESolution>();
            for (int i = 0; i < numberOfSimulations; i++)
            {
                var solution = await _eulerMaruyama.SolveAsync(problem, diffusionCoefficient, cancellationToken);
                solutions.Add(solution);
            }

            return AggregateSolutions(solutions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to solve SDE with {Simulations} simulations", numberOfSimulations);
            throw;
        }
    }

    public ODEProblem CreateExponentialDegradationProblem(double initialValue, double degradationRate, double timeSpan, int timePoints = 100)
    {
        var problem = new ODEProblem
        {
            ProblemName = "Exponential Degradation",
            InitialConditions = new[] { initialValue },
            TimePoints = Enumerable.Range(0, timePoints).Select(i => i * timeSpan / (timePoints - 1)).ToList(),
            Parameters = new Dictionary<string, double> { { "DegradationRate", degradationRate } },
            DerivativeFunction = (t, y) => new[] { -degradationRate * y[0] }
        };
        return problem;
    }

    public ODEProblem CreatePowerLawDegradationProblem(double initialValue, double degradationRate, double power, double timeSpan, int timePoints = 100)
    {
        var problem = new ODEProblem
        {
            ProblemName = "Power-Law Degradation",
            InitialConditions = new[] { initialValue },
            TimePoints = Enumerable.Range(0, timePoints).Select(i => i * timeSpan / (timePoints - 1)).ToList(),
            Parameters = new Dictionary<string, double> { { "DegradationRate", degradationRate }, { "Power", power } },
            DerivativeFunction = (t, y) => new[] { -degradationRate * Math.Pow(t, power - 1) * y[0] }
        };
        return problem;
    }

    public ODEProblem CreateMultiVariableDegradationProblem(double[] initialValues, Dictionary<string, double> parameters, double timeSpan, int timePoints = 100)
    {
        var problem = new ODEProblem
        {
            ProblemName = "Multi-Variable Degradation",
            InitialConditions = initialValues,
            TimePoints = Enumerable.Range(0, timePoints).Select(i => i * timeSpan / (timePoints - 1)).ToList(),
            Parameters = parameters,
            DerivativeFunction = (t, y) =>
            {
                var derivatives = new double[y.Length];
                for (int i = 0; i < y.Length; i++)
                {
                    derivatives[i] = -parameters.GetValueOrDefault($"Rate_{i}", 0.01) * y[i];
                }
                return derivatives;
            }
        };
        return problem;
    }

    public ODEValidationResult ValidateSolution(ODESolution solution, ODEProblem problem)
    {
        var result = new ODEValidationResult { IsValid = true };
        
        if (solution.Values.Any(v => v.Any(double.IsNaN)))
        {
            result.IsValid = false;
            result.ValidationMessages.Add("Solution contains NaN values.");
        }

        if (solution.Values.Any(v => v.Any(double.IsInfinity)))
        {
            result.IsValid = false;
            result.ValidationMessages.Add("Solution contains infinity.");
        }

        return result;
    }

    private ODESolution AggregateSolutions(List<ODESolution> solutions)
    {
        if (solutions.Count == 0) throw new ArgumentException("No solutions to aggregate");
        
        var referenceSolution = solutions[0];
        var aggregatedValues = new double[referenceSolution.Values.Length][];

        for (int i = 0; i < referenceSolution.Values.Length; i++)
        {
            aggregatedValues[i] = new double[referenceSolution.TimePoints.Count];
            for (int j = 0; j < referenceSolution.TimePoints.Count; j++)
            {
                var valuesAtTime = solutions.Select(s => s.Values[i][j]);
                aggregatedValues[i][j] = valuesAtTime.Average(); // Mean aggregation
            }
        }

        return new ODESolution
        {
            TimePoints = referenceSolution.TimePoints,
            Values = aggregatedValues,
            SolutionMetadata = new Dictionary<string, object>
            {
                { "SimulationCount", solutions.Count },
                { "AggregationMethod", "Mean" },
                { "OriginalSolutions", solutions.Count }
            }
        };
    }
}

public interface IODESolver
{
    Task<ODESolution> SolveODEAsync(ODEProblem problem, ODESolverMethod method = ODESolverMethod.RungeKutta4, CancellationToken cancellationToken = default);
    Task<ODESolution[]> SolveSystemAsync(ODEProblem[] problems, ODESolverMethod method = ODESolverMethod.RungeKutta4, CancellationToken cancellationToken = default);
    Task<ODESolution> SolveSDEAsync(ODEProblem problem, double diffusionCoefficient, int numberOfSimulations = 100, CancellationToken cancellationToken = default);
    ODEProblem CreateExponentialDegradationProblem(double initialValue, double degradationRate, double timeSpan, int timePoints = 100);
    ODEProblem CreatePowerLawDegradationProblem(double initialValue, double degradationRate, double power, double timeSpan, int timePoints = 100);
    ODEProblem CreateMultiVariableDegradationProblem(double[] initialValues, Dictionary<string, double> parameters, double timeSpan, int timePoints = 100);
    ODEValidationResult ValidateSolution(ODESolution solution, ODEProblem problem);
}
