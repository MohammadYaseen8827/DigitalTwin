using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalTwinPlatform.Application.Mathematics;

/// <summary>
/// Runge-Kutta 4th order numerical integration method for solving ODEs.
/// Provides accurate and stable integration for deterministic degradation models.
/// </summary>
public class RungeKutta : INumericalODESolver
{
    private readonly ILogger<RungeKutta> _logger;

    public RungeKutta(ILogger<RungeKutta> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Solves an ODE using the 4th order Runge-Kutta method.
    /// </summary>
    public async Task<ODESolution> SolveAsync(
        ODEProblem problem,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving ODE '{ProblemName}' with Runge-Kutta 4th order", problem.ProblemName);

        return await Task.Run(() =>
        {
            var solution = new ODESolution
            {
                TimePoints = new List<double>(problem.TimePoints),
                Values = new double[problem.InitialConditions.Length][],
                SolutionMetadata = new Dictionary<string, object>
                {
                    { "Method", "Runge-Kutta 4" },
                    { "ProblemName", problem.ProblemName },
                    { "InitialConditions", problem.InitialConditions },
                    { "Parameters", problem.Parameters }
                }
            };

            // Initialize solution arrays
            for (int i = 0; i < problem.InitialConditions.Length; i++)
            {
                solution.Values[i] = new double[problem.TimePoints.Count];
                solution.Values[i][0] = problem.InitialConditions[i];
            }

            // Perform RK4 integration
            for (int n = 0; n < problem.TimePoints.Count - 1; n++)
            {
                var t = problem.TimePoints[n];
                var h = problem.TimePoints[n + 1] - t;
                var y = new double[problem.InitialConditions.Length];

                // Extract current values
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    y[i] = solution.Values[i][n];
                }

                // RK4 coefficients
                var k1 = problem.DerivativeFunction(t, y);
                
                var y2 = new double[problem.InitialConditions.Length];
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    y2[i] = y[i] + 0.5 * h * k1[i];
                }
                var k2 = problem.DerivativeFunction(t + 0.5 * h, y2);
                
                var y3 = new double[problem.InitialConditions.Length];
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    y3[i] = y[i] + 0.5 * h * k2[i];
                }
                var k3 = problem.DerivativeFunction(t + 0.5 * h, y3);
                
                var y4 = new double[problem.InitialConditions.Length];
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    y4[i] = y[i] + h * k3[i];
                }
                var k4 = problem.DerivativeFunction(t + h, y4);

                // Update solution
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    solution.Values[i][n + 1] = y[i] + (h / 6.0) * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Runge-Kutta integration cancelled at time step {TimeStep}", n);
                    break;
                }
            }

            _logger.LogInformation("Runge-Kutta integration completed successfully");
            return solution;
        }, cancellationToken);
    }

    /// <summary>
    /// Adaptive step size Runge-Kutta method for stiff problems.
    /// </summary>
    public async Task<ODESolution> SolveAdaptiveAsync(
        ODEProblem problem,
        double tolerance = 1e-6,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving ODE '{ProblemName}' with adaptive Runge-Kutta", problem.ProblemName);

        return await Task.Run(() =>
        {
            var solution = new ODESolution
            {
                TimePoints = new List<double>(),
                Values = new double[problem.InitialConditions.Length][],
                SolutionMetadata = new Dictionary<string, object>
                {
                    { "Method", "Adaptive Runge-Kutta" },
                    { "Tolerance", tolerance },
                    { "ProblemName", problem.ProblemName }
                }
            };

            // Initialize solution arrays
            for (int i = 0; i < problem.InitialConditions.Length; i++)
            {
                solution.Values[i] = new List<double>().ToArray();
            }

            var timePoints = new List<double> { problem.TimePoints.First() };
            var currentValues = (double[])problem.InitialConditions.Clone();
            var currentTime = problem.TimePoints.First();
            var maxTime = problem.TimePoints.Last();
            var initialStep = (maxTime - currentTime) / 100.0; // Initial guess
            var minStep = 1e-10;
            var maxStep = (maxTime - currentTime) / 10.0;

            while (currentTime < maxTime && !cancellationToken.IsCancellationRequested)
            {
                var step = Math.Min(initialStep, maxTime - currentTime);
                
                // Take two half-steps and one full step for error estimation
                var (yFull, error) = ComputeStepWithEstimate(problem, currentTime, currentValues, step);
                
                // Adjust step size based on error
                var newStep = AdjustStepSize(step, error, tolerance, minStep, maxStep);
                
                if (error < tolerance)
                {
                    // Accept the step
                    currentTime += step;
                    currentValues = yFull;
                    
                    timePoints.Add(currentTime);
                    for (int i = 0; i < currentValues.Length; i++)
                    {
                        var list = solution.Values[i].ToList();
                        list.Add(currentValues[i]);
                        solution.Values[i] = list.ToArray();
                    }
                }
                
                initialStep = newStep;
            }

            solution.TimePoints = timePoints;

            _logger.LogInformation("Adaptive Runge-Kutta integration completed with {TimePoints} points", 
                solution.TimePoints.Count);
            return solution;
        }, cancellationToken);
    }

    private (double[] yFull, double error) ComputeStepWithEstimate(
        ODEProblem problem, 
        double t, 
        double[] y, 
        double h)
    {
        // Two half-steps
        var yHalf1 = StepRK4(problem, t, y, h / 2);
        var yHalf2 = StepRK4(problem, t + h / 2, yHalf1, h / 2);
        
        // One full step
        var yFull = StepRK4(problem, t, y, h);
        
        // Error estimate (difference between methods)
        var error = 0.0;
        for (int i = 0; i < y.Length; i++)
        {
            var localError = Math.Abs(yHalf2[i] - yFull[i]);
            error = Math.Max(error, localError);
        }
        
        return (yFull, error);
    }

    private double[] StepRK4(ODEProblem problem, double t, double[] y, double h)
    {
        var k1 = problem.DerivativeFunction(t, y);
        
        var y2 = new double[y.Length];
        for (int i = 0; i < y.Length; i++)
        {
            y2[i] = y[i] + 0.5 * h * k1[i];
        }
        var k2 = problem.DerivativeFunction(t + 0.5 * h, y2);
        
        var y3 = new double[y.Length];
        for (int i = 0; i < y.Length; i++)
        {
            y3[i] = y[i] + 0.5 * h * k2[i];
        }
        var k3 = problem.DerivativeFunction(t + 0.5 * h, y3);
        
        var y4 = new double[y.Length];
        for (int i = 0; i < y.Length; i++)
        {
            y4[i] = y[i] + h * k3[i];
        }
        var k4 = problem.DerivativeFunction(t + h, y4);

        var result = new double[y.Length];
        for (int i = 0; i < y.Length; i++)
        {
            result[i] = y[i] + (h / 6.0) * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
        }

        return result;
    }

    private double AdjustStepSize(double currentStep, double error, double tolerance, double minStep, double maxStep)
    {
        if (error == 0) return Math.Min(currentStep * 2.0, maxStep);
        
        var safetyFactor = 0.9;
        var newStep = currentStep * safetyFactor * Math.Pow(tolerance / error, 0.2);
        
        return Math.Max(minStep, Math.Min(newStep, maxStep));
    }
}
