using DigitalTwinPlatform.API.Services.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RunToFailureController : ControllerBase
{
    private readonly IRunToFailureOrchestrator _orchestrator;
    private readonly ILogger<RunToFailureController> _logger;

    public RunToFailureController(
        IRunToFailureOrchestrator orchestrator,
        ILogger<RunToFailureController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Run a single run-to-failure simulation for a machine
    /// </summary>
    [HttpPost("{machineId:guid}")]
    [ProducesResponseType(typeof(RunToFailureResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RunToFailureResult>> RunSimulation(
        Guid machineId,
        [FromBody] RunToFailureOptions? options = null,
        CancellationToken ct = default)
    {
        try
        {
            options ??= new RunToFailureOptions();
            
            var result = await _orchestrator.RunToFailureAsync(machineId, options, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run simulation for machine {MachineId}", machineId);
            return BadRequest(new { message = $"Failed to run simulation: {ex.Message}" });
        }
    }

    /// <summary>
    /// Generate multiple degradation trajectories for a machine type
    /// </summary>
    [HttpPost("generate-trajectories")]
    [ProducesResponseType(typeof(List<DegradationTrajectory>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DegradationTrajectory>>> GenerateTrajectories(
        [FromBody] GenerateTrajectoriesRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (request.Count <= 0 || request.Count > 1000)
            {
                return BadRequest(new { message = "Count must be between 1 and 1000" });
            }

            var result = await _orchestrator.GenerateTrajectoriesAsync(
                request.MachineType,
                request.Count,
                request.Options ?? new RunToFailureOptions(),
                ct
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate trajectories for machine type {MachineType}", 
                request.MachineType);
            return BadRequest(new { message = $"Failed to generate trajectories: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get run-to-failure results for a machine
    /// </summary>
    [HttpGet("{machineId:guid}/results")]
    [ProducesResponseType(typeof(List<RunToFailureResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RunToFailureResult>>> GetResults(Guid machineId)
    {
        try
        {
            var results = await _orchestrator.GetStoredResultsAsync(machineId);
            if (!results.Any())
            {
                return NotFound(new { message = $"No results found for machine {machineId}" });
            }
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve results for machine {MachineId}", machineId);
            return BadRequest(new { message = $"Failed to retrieve results: {ex.Message}" });
        }
    }
}

public class GenerateTrajectoriesRequest
{
    public string MachineType { get; set; } = string.Empty;
    public int Count { get; set; }
    public RunToFailureOptions? Options { get; set; }
}
