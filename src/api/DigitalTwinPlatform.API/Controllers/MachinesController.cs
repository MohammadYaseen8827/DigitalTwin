using Asp.Versioning;
using DigitalTwinPlatform.Application.Machines.Commands;
using DigitalTwinPlatform.Application.Machines.Models;
using DigitalTwinPlatform.Application.Machines.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DigitalTwinPlatform.API.Controllers;

/// <summary>
/// API controller for managing machines in the Digital Twin Platform.
/// Provides CRUD operations for machine entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ApiVersion("1.0")]
public class MachinesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves all machines registered in the system.
    /// </summary>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A list of all machines with their current status and health metrics.</returns>
    /// <response code="200">Returns the list of machines successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MachineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MachineDto>>> GetMachines(CancellationToken ct)
        => Ok(await mediator.Send(new GetMachinesQuery(), ct));

    /// <summary>
    /// Retrieves a specific machine by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the machine.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The machine details including health status and RUL predictions.</returns>
    /// <response code="200">Returns the machine details successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MachineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MachineDto>> GetMachine(Guid id, CancellationToken ct)
        => Ok(await mediator.Send(new GetMachineQuery(id), ct));

    /// <summary>
    /// Creates a new machine in the Digital Twin Platform.
    /// </summary>
    /// <param name="request">The machine creation request containing name, type, status, and properties.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The created machine with its assigned identifier.</returns>
    /// <example>
    /// {
    ///   "name": "CNC Mill 01",
    ///   "type": "CNC",
    ///   "status": "Operational",
    ///   "properties": { "manufacturer": "Haas", "model": "VF-2" }
    /// }
    /// </example>
    /// <response code="201">Machine created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MachineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MachineDto>> Create([FromBody] MachineCreateDto request, CancellationToken ct)
    {
        var created = await mediator.Send(new CreateMachineCommand(request), ct);
        return CreatedAtAction(nameof(GetMachine), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing machine's information.
    /// </summary>
    /// <param name="id">The unique identifier of the machine to update.</param>
    /// <param name="request">The machine update request containing the new values.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The updated machine details.</returns>
    /// <example>
    /// {
    ///   "name": "CNC Mill 01 - Updated",
    ///   "type": "CNC",
    ///   "status": "Maintenance",
    ///   "properties": { "manufacturer": "Haas", "model": "VF-2", "lastMaintenance": "2024-01-15" },
    ///   "remainingUsefulLifeDays": 45.5,
    ///   "failureProbability": 0.15,
    ///   "healthStatus": "Good"
    /// }
    /// </example>
    /// <response code="200">Machine updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(MachineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MachineDto>> Update(Guid id, [FromBody] MachineUpdateDto request, CancellationToken ct)
        => Ok(await mediator.Send(new UpdateMachineCommand(id, request), ct));

    /// <summary>
    /// Deletes a machine from the Digital Twin Platform.
    /// </summary>
    /// <param name="id">The unique identifier of the machine to delete.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Machine deleted successfully.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="404">Machine not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMachineCommand(id), ct);
        return NoContent();
    }
}
