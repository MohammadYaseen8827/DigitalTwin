using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalTwinPlatform.Application.Mathematics;

/// <summary>
/// Euler-Maruyama method for solving stochastic differential equations (SDEs).
/// Provides numerical integration for degradation models with random noise and uncertainty.
/// </summary>
public class EulerMaruyama : INumericalODESolver
{
    private readonly ILogger<EulerMaruyama> _logger;
    private readonly Random _random;

    public EulerMaruyama(ILogger<EulerMaruyama> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Solves a deterministic ODE using Euler-Maruyama method (equivalent to Euler method).
    /// </summary>
    public async Task<ODESolution> SolveAsync(
        ODEProblem problem,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving ODE '{ProblemName}' with Euler-Maruyama method", problem.ProblemName);

        return await Task.Run(() =>
        {
            var solution = new ODESolution
            {
                TimePoints = new List<double>(problem.TimePoints),
                Values = new double[problem.InitialConditions.Length][],
                SolutionMetadata = new Dictionary<string, object>
                {
                    { "Method", "Euler-Maruyama" },
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

            // Perform Euler-Maruyama integration
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

                // Euler-Maruyama step
                var derivatives = problem.DerivativeFunction(t, y);

                // Update solution
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    solution.Values[i][n + 1] = y[i] + h * derivatives[i];
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Euler-Maruyama integration cancelled at time step {TimeStep}", n);
                    break;
                }
            }

            _logger.LogInformation("Euler-Maruyama integration completed successfully");
            return solution;
        }, cancellationToken);
    }

    /// <summary>
    /// Solves a stochastic differential equation with the specified diffusion coefficient.
    /// </summary>
    public async Task<ODESolution> SolveAsync(
        ODEProblem problem,
        double diffusionCoefficient,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving SDE '{ProblemName}' with diffusion coefficient {Diffusion}", 
            problem.ProblemName, diffusionCoefficient);

        return await Task.Run(() =>
        {
            var solution = new ODESolution
            {
                TimePoints = new List<double>(problem.TimePoints),
                Values = new double[problem.InitialConditions.Length][],
                SolutionMetadata = new Dictionary<string, object>
                {
                    { "Method", "Euler-Maruyama SDE" },
                    { "ProblemName", problem.ProblemName },
                    { "DiffusionCoefficient", diffusionCoefficient },
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

            // Perform Euler-Maruyama integration for SDE
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

                // Euler-Maruyama step for SDE: dy = f(t,y)dt + g(t,y)dW
                var drift = problem.DerivativeFunction(t, y);
                var sqrtH = Math.Sqrt(h);

                // Update solution with drift and diffusion
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    // Generate Brownian motion increment
                    var dW = NormalRandom(0, sqrtH);
                    
                    // Euler-Maruyama update: y_{n+1} = y_n + h*f(t_n,y_n) + sigma*dW
                    solution.Values[i][n + 1] = y[i] + h * drift[i] + diffusionCoefficient * dW;
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Euler-Maruyama SDE integration cancelled at time step {TimeStep}", n);
                    break;
                }
            }

            _logger.LogInformation("Euler-Maruyama SDE integration completed successfully");
            return solution;
        }, cancellationToken);
    }

    /// <summary>
    /// Solves a system of coupled SDEs with correlation matrix.
    /// </summary>
    public async Task<ODESolution> SolveCoupledSDEAsync(
        ODEProblem problem,
        double[,] correlationMatrix,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Solving coupled SDE '{ProblemName}' with correlation matrix", problem.ProblemName);

        return await Task.Run(() =>
        {
            var solution = new ODESolution
            {
                TimePoints = new List<double>(problem.TimePoints),
                Values = new double[problem.InitialConditions.Length][],
                SolutionMetadata = new Dictionary<string, object>
                {
                    { "Method", "Coupled Euler-Maruyama SDE" },
                    { "ProblemName", problem.ProblemName },
                    { "CorrelationMatrix", correlationMatrix },
                    { "InitialConditions", problem.InitialConditions }
                }
            };

            // Initialize solution arrays
            for (int i = 0; i < problem.InitialConditions.Length; i++)
            {
                solution.Values[i] = new double[problem.TimePoints.Count];
                solution.Values[i][0] = problem.InitialConditions[i];
            }

            // Generate correlated Brownian motion increments
            var brownianIncrements = GenerateCorrelatedBrownianMotion(
                problem.TimePoints.Count - 1, 
                problem.InitialConditions.Length, 
                correlationMatrix,
                problem);

            // Perform Euler-Maruyama integration for coupled SDEs
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

                // Euler-Maruyama step for coupled SDEs
                var drift = problem.DerivativeFunction(t, y);

                // Update solution with drift and correlated diffusion
                for (int i = 0; i < problem.InitialConditions.Length; i++)
                {
                    // Coupled Euler-Maruyama update with correlation
                    var diffusionTerm = 0.0;
                    for (int j = 0; j < problem.InitialConditions.Length; j++)
                    {
                        diffusionTerm += correlationMatrix[i, j] * brownianIncrements[n, j];
                    }
                    
                    solution.Values[i][n + 1] = y[i] + h * drift[i] + diffusionTerm;
                }

                // Check for cancellation
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Coupled Euler-Maruyama SDE integration cancelled at time step {TimeStep}", n);
                    break;
                }
            }

            _logger.LogInformation("Coupled Euler-Maruyama SDE integration completed successfully");
            return solution;
        }, cancellationToken);
    }

    /// <summary>
    /// Generates correlated Brownian motion increments.
    /// </summary>
    private double[,] GenerateCorrelatedBrownianMotion(int timeSteps, int dimensions, double[,] correlationMatrix, ODEProblem problem)
    {
        var increments = new double[timeSteps, dimensions];
        var sqrtH = Math.Sqrt((problem.TimePoints[1] - problem.TimePoints[0])); // Assuming uniform time steps

        // Perform Cholesky decomposition of correlation matrix
        var cholesky = CholeskyDecomposition(correlationMatrix);

        for (int n = 0; n < timeSteps; n++)
        {
            // Generate independent normal random variables
            var independentNormals = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                independentNormals[i] = NormalRandom(0, sqrtH);
            }

            // Transform to correlated normals using Cholesky decomposition
            for (int i = 0; i < dimensions; i++)
            {
                increments[n, i] = 0.0;
                for (int j = 0; j <= i; j++)
                {
                    increments[n, i] += cholesky[i, j] * independentNormals[j];
                }
            }
        }

        return increments;
    }

    /// <summary>
    /// Performs Cholesky decomposition of a symmetric positive definite matrix.
    /// </summary>
    private double[,] CholeskyDecomposition(double[,] matrix)
    {
        var n = matrix.GetLength(0);
        var L = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = 0;
                
                if (j == i)
                {
                    for (int k = 0; k < j; k++)
                    {
                        sum += L[j, k] * L[j, k];
                    }
                    L[j, j] = Math.Sqrt(Math.Max(0, matrix[j, j] - sum));
                }
                else
                {
                    for (int k = 0; k < j; k++)
                    {
                        sum += L[i, k] * L[j, k];
                    }
                    L[i, j] = (matrix[i, j] - sum) / L[j, j];
                }
            }
        }

        return L;
    }

    /// <summary>
    /// Generates a normally distributed random number using Box-Muller transform.
    /// </summary>
    private double NormalRandom(double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = _random.NextDouble();
        var u2 = _random.NextDouble();
        var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        
        return mean + stdDev * z0;
    }

    /// <summary>
    /// Validates numerical stability of SDE solution.
    /// </summary>
    public SDEValidationResult ValidateSDESolution(ODESolution solution, double tolerance = 1e-6)
    {
        var result = new SDEValidationResult
        {
            IsStable = true,
            ValidationMessages = new List<string>(),
            MaximumStepSize = 0,
            AverageStepSize = 0
        };

        // Check for numerical instabilities
        for (int i = 0; i < solution.Values.Length; i++)
        {
            var values = solution.Values[i];
            
            // Check for NaN or infinite values
            if (values.Any(v => double.IsNaN(v) || double.IsInfinity(v)))
            {
                result.IsStable = false;
                result.ValidationMessages.Add($"Numerical instability detected in variable {i}");
                continue;
            }

            // Check for extreme values
            var maxAbsValue = values.Max(Math.Abs);
            if (maxAbsValue > 1e6)
            {
                result.ValidationMessages.Add($"Extreme values detected in variable {i}: max = {maxAbsValue}");
            }
        }

        // Calculate step sizes
        if (solution.TimePoints.Count > 1)
        {
            var stepSizes = new List<double>();
            for (int i = 1; i < solution.TimePoints.Count; i++)
            {
                stepSizes.Add(solution.TimePoints[i] - solution.TimePoints[i - 1]);
            }
            
            result.MaximumStepSize = stepSizes.Max();
            result.AverageStepSize = stepSizes.Average();
        }

        return result;
    }
}

/// <summary>
/// Validation results specific to SDE solutions.
/// </summary>
public class SDEValidationResult
{
    public bool IsStable { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
    public double MaximumStepSize { get; set; }
    public double AverageStepSize { get; set; }
}
