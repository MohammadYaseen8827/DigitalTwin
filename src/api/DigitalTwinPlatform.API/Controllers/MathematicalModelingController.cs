using DigitalTwinPlatform.API.Services.MathematicalModeling;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MathematicalModelingController : ControllerBase
{
    private readonly IDifferentialEquationSolver _odeSolver;
    private readonly IOptimizationService _optimizationService;
    private readonly ILogger<MathematicalModelingController> _logger;

    public MathematicalModelingController(
        IDifferentialEquationSolver odeSolver,
        IOptimizationService optimizationService,
        ILogger<MathematicalModelingController> logger)
    {
        _odeSolver = odeSolver;
        _optimizationService = optimizationService;
        _logger = logger;
    }

    /// <summary>
    /// Solve ordinary differential equations for system dynamics
    /// </summary>
    [HttpPost("ode/solve")]
    public async Task<ActionResult<OdeSolutionDto>> SolveOde(
        [FromBody] OdeSolveRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Solving ODE system: {SystemName}", request.SystemName);

            var derivatives = CreateDerivativesFunction(request.Equations);
            var solution = await _odeSolver.SolveOdeAsync(
                derivatives,
                request.InitialConditions,
                request.StartTime,
                request.EndTime,
                request.StepSize,
                ct);

            var response = new OdeSolutionDto
            {
                SystemName = request.SystemName,
                TimePoints = solution.TimePoints,
                Solutions = solution.Solutions,
                Steps = solution.Steps,
                FinalTime = solution.FinalTime,
                FinalState = solution.FinalState
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error solving ODE system: {SystemName}", request.SystemName);
            return StatusCode(500, new { Error = "Failed to solve ODE system", Details = ex.Message });
        }
    }

    /// <summary>
    /// Solve complex system dynamics with derived quantities
    /// </summary>
    [HttpPost("system-dynamics/solve")]
    public async Task<ActionResult<SystemDynamicsSolutionDto>> SolveSystemDynamics(
        [FromBody] SystemDynamicsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Solving system dynamics: {SystemName}", request.SystemName);

            var model = new SystemDynamicsModel
            {
                SystemName = request.SystemName,
                Derivatives = CreateDerivativesFunction(request.Equations),
                InitialConditions = request.InitialConditions,
                StepSize = request.StepSize,
                DerivedQuantityCalculators = CreateDerivedQuantityCalculators(request.DerivedQuantities)
            };

            var solution = await _odeSolver.SolveSystemDynamicsAsync(model, request.SimulationTime, ct);

            var response = new SystemDynamicsSolutionDto
            {
                SystemName = solution.SystemName,
                TimePoints = solution.OdeSolution.TimePoints,
                StateVariables = solution.OdeSolution.Solutions,
                DerivedQuantities = solution.DerivedQuantities,
                Stability = new StabilityAnalysisDto
                {
                    IsStable = solution.StabilityAnalysis.IsStable,
                    MaxChange = solution.StabilityAnalysis.MaxChange,
                    ConvergenceRate = solution.StabilityAnalysis.ConvergenceRate
                },
                EnergyBalance = new EnergyBalanceDto
                {
                    KineticEnergy = solution.EnergyBalance.KineticEnergy,
                    PotentialEnergy = solution.EnergyBalance.PotentialEnergy,
                    TotalEnergy = solution.EnergyBalance.TotalEnergy,
                    EnergyConserved = solution.EnergyBalance.EnergyConservation
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error solving system dynamics: {SystemName}", request.SystemName);
            return StatusCode(500, new { Error = "Failed to solve system dynamics", Details = ex.Message });
        }
    }

    /// <summary>
    /// Perform gradient-based parameter optimization
    /// </summary>
    [HttpPost("optimization/gradient")]
    public async Task<ActionResult<OptimizationResultDto>> GradientOptimization(
        [FromBody] GradientOptimizationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing gradient optimization for {Parameters} parameters", 
                request.InitialGuess.Length);

            var objectiveFunction = CreateObjectiveFunction(request.ObjectiveExpression);
            var options = new OptimizationOptions
            {
                LearningRate = request.LearningRate,
                MaxIterations = request.MaxIterations,
                Tolerance = request.Tolerance,
                Epsilon = request.Epsilon,
                Bounds = request.ParameterBounds?.Select(b => new ParameterBounds 
                { 
                    Min = b.Min, 
                    Max = b.Max 
                }).ToArray()
            };

            var result = await _optimizationService.OptimizeParametersAsync(
                objectiveFunction,
                request.InitialGuess,
                options,
                ct);

            var response = new OptimizationResultDto
            {
                OptimalParameters = result.OptimalParameters,
                OptimalValue = result.OptimalValue,
                Iterations = result.Iterations,
                Converged = result.Converged,
                Method = result.Method
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing gradient optimization");
            return StatusCode(500, new { Error = "Failed to perform gradient optimization", Details = ex.Message });
        }
    }

    /// <summary>
    /// Perform genetic algorithm optimization
    /// </summary>
    [HttpPost("optimization/genetic")]
    public async Task<ActionResult<OptimizationResultDto>> GeneticOptimization(
        [FromBody] GeneticOptimizationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing genetic optimization with population size {PopulationSize}", 
                request.PopulationSize);

            var fitnessFunction = CreateObjectiveFunction(request.FitnessExpression);
            var options = new GeneticAlgorithmOptions
            {
                PopulationSize = request.PopulationSize,
                MaxGenerations = request.MaxGenerations,
                MutationRate = request.MutationRate,
                TournamentSize = request.TournamentSize,
                ParameterBounds = request.ParameterBounds.Select(b => new ParameterBounds 
                { 
                    Min = b.Min, 
                    Max = b.Max 
                }).ToArray()
            };

            var result = await _optimizationService.GeneticOptimizationAsync(
                fitnessFunction,
                request.ParameterCount,
                options,
                ct);

            var response = new OptimizationResultDto
            {
                OptimalParameters = result.OptimalParameters,
                OptimalValue = result.OptimalValue,
                Iterations = result.Iterations,
                Converged = result.Converged,
                Method = result.Method
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing genetic optimization");
            return StatusCode(500, new { Error = "Failed to perform genetic optimization", Details = ex.Message });
        }
    }

    /// <summary>
    /// Perform multi-objective optimization using NSGA-II
    /// </summary>
    [HttpPost("optimization/multi-objective")]
    public async Task<ActionResult<MultiObjectiveResultDto>> MultiObjectiveOptimization(
        [FromBody] MultiObjectiveOptimizationRequest request,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing multi-objective optimization");

            var objectiveFunctions = CreateMultiObjectiveFunction(request.ObjectiveExpressions);
            var options = new MultiObjectiveOptions
            {
                PopulationSize = request.PopulationSize,
                MaxGenerations = request.MaxGenerations,
                MutationRate = request.MutationRate,
                ParameterBounds = request.ParameterBounds.Select(b => new ParameterBounds 
                { 
                    Min = b.Min, 
                    Max = b.Max 
                }).ToArray()
            };

            var result = await _optimizationService.MultiObjectiveOptimizationAsync(
                objectiveFunctions,
                request.ParameterCount,
                options,
                ct);

            var response = new MultiObjectiveResultDto
            {
                ParetoOptimalSolutions = result.ParetoOptimalSolutions,
                ObjectiveValues = result.ObjectiveValues,
                Generations = result.Generations,
                PopulationSize = result.PopulationSize,
                Method = result.Method,
                ParetoFrontSize = result.ParetoOptimalSolutions.Length
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing multi-objective optimization");
            return StatusCode(500, new { Error = "Failed to perform multi-objective optimization", Details = ex.Message });
        }
    }

    /// <summary>
    /// Get predefined system models
    /// </summary>
    [HttpGet("models")]
    public ActionResult<SystemModelCatalogDto> GetSystemModels()
    {
        var models = new SystemModelCatalogDto
        {
            Models = new[]
            {
                new SystemModelDto
                {
                    Name = "Mass-Spring-Damper",
                    Description = "Second-order mechanical system",
                    Equations = new[] { "dv/dt = (-c*v - k*x)/m", "dx/dt = v" },
                    Parameters = new[] { "mass (m)", "damping (c)", "stiffness (k)" },
                    Variables = new[] { "velocity (v)", "position (x)" }
                },
                new SystemModelDto
                {
                    Name = "RC Circuit",
                    Description = "First-order electrical circuit",
                    Equations = new[] { "dv/dt = (V_source - v)/(R*C)" },
                    Parameters = new[] { "resistance (R)", "capacitance (C)", "source_voltage" },
                    Variables = new[] { "voltage (v)" }
                },
                new SystemModelDto
                {
                    Name = "Chemical Reaction",
                    Description = "First-order chemical kinetics",
                    Equations = new[] { "dC/dt = -k*C" },
                    Parameters = new[] { "rate_constant (k)" },
                    Variables = new[] { "concentration (C)" }
                }
            }
        };

        return Ok(models);
    }

    #region Private Helper Methods

    private Func<double, double[], double[]> CreateDerivativesFunction(string[] equations)
    {
        // This is a simplified implementation
        // In practice, you would parse and compile the equations
        return (t, state) =>
        {
            // Example: Simple harmonic oscillator
            if (equations.Length >= 2 && 
                equations[0].Contains("dv/dt") && 
                equations[1].Contains("dx/dt"))
            {
                // dv/dt = -k*x/m - c*v/m
                // dx/dt = v
                var v = state[0]; // velocity
                var x = state[1]; // position
                var k = 1.0; // stiffness
                var m = 1.0; // mass
                var c = 0.1; // damping
                
                return new double[] { -(k * x + c * v) / m, v };
            }
            
            // Default: return zero derivatives
            return new double[state.Length];
        };
    }

    private List<Func<double, double[], double>> CreateDerivedQuantityCalculators(
        Dictionary<string, string> derivedQuantities)
    {
        var calculators = new List<Func<double, double[], double>>();

        foreach (var quantity in derivedQuantities)
        {
            // Parse and create calculator functions
            // This is simplified - in practice you'd use expression trees or scripting
            calculators.Add((t, state) =>
            {
                // Example implementation
                if (quantity.Key == "energy")
                {
                    var v = state.Length > 0 ? state[0] : 0;
                    var x = state.Length > 1 ? state[1] : 0;
                    return 0.5 * (v * v + x * x); // Kinetic + Potential
                }
                return 0.0;
            });
        }

        return calculators;
    }

    private Func<double[], double> CreateObjectiveFunction(string expression)
    {
        // Simplified implementation
        return parameters =>
        {
            // Example: Minimize sum of squares
            return parameters.Sum(p => p * p);
        };
    }

    private Func<double[], double[]> CreateMultiObjectiveFunction(string[] expressions)
    {
        return parameters =>
        {
            // Example: Two objectives - minimize sum and maximize negative sum
            var sum = parameters.Sum();
            return new double[] { sum * sum, -sum * sum }; // Minimize sum², maximize -sum²
        };
    }

    #endregion
}

#region DTOs

public class OdeSolveRequest
{
    public string SystemName { get; set; } = string.Empty;
    public string[] Equations { get; set; } = [];
    public double[] InitialConditions { get; set; } = [];
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public double StepSize { get; set; } = 0.01;
}

public class SystemDynamicsRequest
{
    public string SystemName { get; set; } = string.Empty;
    public string[] Equations { get; set; } = [];
    public double[] InitialConditions { get; set; } = [];
    public double StepSize { get; set; } = 0.01;
    public double SimulationTime { get; set; } = 10.0;
    public Dictionary<string, string> DerivedQuantities { get; set; } = [];
}

public class GradientOptimizationRequest
{
    public string ObjectiveExpression { get; set; } = string.Empty;
    public double[] InitialGuess { get; set; } = [];
    public double LearningRate { get; set; } = 0.01;
    public int MaxIterations { get; set; } = 1000;
    public double Tolerance { get; set; } = 1e-6;
    public double Epsilon { get; set; } = 1e-8;
    public ParameterBoundsDto[]? ParameterBounds { get; set; }
}

public class GeneticOptimizationRequest
{
    public string FitnessExpression { get; set; } = string.Empty;
    public int ParameterCount { get; set; }
    public int PopulationSize { get; set; } = 100;
    public int MaxGenerations { get; set; } = 100;
    public double MutationRate { get; set; } = 0.1;
    public int TournamentSize { get; set; } = 3;
    public ParameterBoundsDto[] ParameterBounds { get; set; } = [];
}

public class MultiObjectiveOptimizationRequest
{
    public string[] ObjectiveExpressions { get; set; } = [];
    public int ParameterCount { get; set; }
    public int PopulationSize { get; set; } = 100;
    public int MaxGenerations { get; set; } = 100;
    public double MutationRate { get; set; } = 0.1;
    public ParameterBoundsDto[] ParameterBounds { get; set; } = [];
}

public class OdeSolutionDto
{
    public string SystemName { get; set; } = string.Empty;
    public double[] TimePoints { get; set; } = [];
    public double[][] Solutions { get; set; } = [];
    public int Steps { get; set; }
    public double FinalTime { get; set; }
    public double[] FinalState { get; set; } = [];
}

public class SystemDynamicsSolutionDto
{
    public string SystemName { get; set; } = string.Empty;
    public double[] TimePoints { get; set; } = [];
    public double[][] StateVariables { get; set; } = [];
    public Dictionary<string, double[]> DerivedQuantities { get; set; } = [];
    public StabilityAnalysisDto Stability { get; set; } = new();
    public EnergyBalanceDto EnergyBalance { get; set; } = new();
}

public class StabilityAnalysisDto
{
    public bool IsStable { get; set; }
    public double MaxChange { get; set; }
    public double ConvergenceRate { get; set; }
}

public class EnergyBalanceDto
{
    public double KineticEnergy { get; set; }
    public double PotentialEnergy { get; set; }
    public double TotalEnergy { get; set; }
    public bool EnergyConserved { get; set; }
}

public class OptimizationResultDto
{
    public double[] OptimalParameters { get; set; } = [];
    public double OptimalValue { get; set; }
    public int Iterations { get; set; }
    public bool Converged { get; set; }
    public string Method { get; set; } = string.Empty;
}

public class MultiObjectiveResultDto
{
    public double[][] ParetoOptimalSolutions { get; set; } = [];
    public double[][] ObjectiveValues { get; set; } = [];
    public int Generations { get; set; }
    public int PopulationSize { get; set; }
    public string Method { get; set; } = string.Empty;
    public int ParetoFrontSize { get; set; }
}

public class ParameterBoundsDto
{
    public double Min { get; set; }
    public double Max { get; set; }
}

public class SystemModelCatalogDto
{
    public SystemModelDto[] Models { get; set; } = [];
}

public class SystemModelDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Equations { get; set; } = [];
    public string[] Parameters { get; set; } = [];
    public string[] Variables { get; set; } = [];
}

#endregion