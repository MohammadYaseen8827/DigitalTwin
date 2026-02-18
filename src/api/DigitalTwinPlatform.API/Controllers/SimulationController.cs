using DigitalTwinPlatform.Application.Simulations.Commands;
using DigitalTwinPlatform.Application.Simulations.Models;
using DigitalTwinPlatform.Application.Simulations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SimulationController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SimulationStateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSimulation([FromBody] Dictionary<string, object> parameters, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var state = await mediator.Send(new CreateSimulationCommand(machineId, parameters), ct);
            return CreatedAtAction(nameof(GetSimulation), new { id = state.Id }, state);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SimulationStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSimulation(Guid id, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var state = await mediator.Send(new GetSimulationStateQuery(id, machineId), ct);
            return Ok(state);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/run")]
    [ProducesResponseType(typeof(SimulationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunSimulation(Guid id, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var result = await mediator.Send(new RunSimulationCommand(id, machineId), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/pause")]
    [ProducesResponseType(typeof(SimulationStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PauseSimulation(Guid id, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var state = await mediator.Send(new PauseSimulationCommand(id, machineId), ct);
            return Ok(state);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/resume")]
    [ProducesResponseType(typeof(SimulationStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeSimulation(Guid id, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var state = await mediator.Send(new ResumeSimulationCommand(id, machineId), ct);
            return Ok(state);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(SimulationStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSimulation(Guid id, [FromQuery] Guid machineId, CancellationToken ct = default)
    {
        try
        {
            var state = await mediator.Send(new CancelSimulationCommand(id, machineId), ct);
            return Ok(state);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SimulationStateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSimulations([FromQuery] Guid machineId, CancellationToken ct = default)
    {
        var simulations = await mediator.Send(new ListSimulationsQuery(machineId), ct);
        return Ok(simulations);
    }

    [HttpGet("status/{machineId:guid}")]
    [ProducesResponseType(typeof(SimulationStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSimulationStatus(Guid machineId, CancellationToken ct = default)
    {
        var status = await mediator.Send(new GetSimulationStatusQuery(machineId), ct);
        return Ok(status);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<Guid, SimulationStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSimulationStatuses(CancellationToken ct = default)
    {
        var statuses = await mediator.Send(new GetAllSimulationStatusesQuery(), ct);
        return Ok(statuses);
    }
}