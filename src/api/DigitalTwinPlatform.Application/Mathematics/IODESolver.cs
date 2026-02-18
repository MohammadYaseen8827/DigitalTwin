using System.Threading;
using System.Threading.Tasks;

namespace DigitalTwinPlatform.Application.Mathematics;

/// <summary>
/// Interface for numerical ODE solvers.
/// </summary>
public interface IODESolver
{
    /// <summary>
    /// Solves an ODE problem numerically.
    /// </summary>
    Task<ODESolution> SolveAsync(ODEProblem problem, CancellationToken cancellationToken = default);
}
