using System;
using System.Collections.Generic;

namespace DigitalTwinPlatform.Application.Mathematics;

/// <summary>
/// Represents an ordinary differential equation problem to be solved.
/// </summary>
public class ODEProblem
{
    public double[] InitialConditions { get; set; } = Array.Empty<double>();
    public List<double> TimePoints { get; set; } = new();
    public Func<double, double[], double[]> DerivativeFunction { get; set; } = null!;
    public string ProblemName { get; set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; set; } = new();
}

/// <summary>
/// Represents the solution to an ODE problem.
/// </summary>
public class ODESolution
{
    public List<double> TimePoints { get; set; } = new();
    public double[][] Values { get; set; } = Array.Empty<double[]>();
    public Dictionary<string, object> SolutionMetadata { get; set; } = new();
}

/// <summary>
/// Represents validation results for an ODE solution.
/// </summary>
public class ODEValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidationMessages { get; set; } = new();
}

/// <summary>
/// Available ODE solver methods.
/// </summary>
public enum ODESolverMethod
{
    RungeKutta4,
    EulerMaruyama,
    AdaptiveRungeKutta,
    ImplicitEuler
}

/// <summary>
/// Represents a stochastic differential equation problem.
/// </summary>
public class SDEProblem : ODEProblem
{
    public Func<double, double[], double[]> DiffusionFunction { get; set; } = null!;
    public double DiffusionCoefficient { get; set; }
}

/// <summary>
/// Represents parameter estimation results for degradation models.
/// </summary>
public class ParameterEstimationResult
{
    public Dictionary<string, double> EstimatedParameters { get; set; } = new();
    public Dictionary<string, double> ParameterUncertainties { get; set; } = new();
    public double LogLikelihood { get; set; }
    public double AIC { get; set; } // Akaike Information Criterion
    public double BIC { get; set; } // Bayesian Information Criterion
    public int NumberOfObservations { get; set; }
    public Dictionary<string, object> EstimationMetadata { get; set; } = new();
}

/// <summary>
/// Represents confidence intervals for model predictions.
/// </summary>
public class ConfidenceInterval
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public double Mean { get; set; }
    public double ConfidenceLevel { get; set; }
    public List<double> SampleValues { get; set; } = new();
}

/// <summary>
/// Represents a degradation model configuration.
/// </summary>
public class DegradationModelConfig
{
    public string ModelType { get; set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; set; } = new();
    public Dictionary<string, double> ParameterBounds { get; set; } = new();
    public string SolverMethod { get; set; } = "RungeKutta4";
    public double TimeHorizon { get; set; }
    public int NumberOfTimePoints { get; set; } = 100;
    public bool IsStochastic { get; set; }
    public double NoiseLevel { get; set; }
    public Dictionary<string, object> ModelMetadata { get; set; } = new();
}
