using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Maintenance.Models;
using DigitalTwinPlatform.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaintenanceController(IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpPost("plan")]
    public async Task<ActionResult<MaintenanceRecord>> Plan(PlanMaintenanceRequest request)
    {
        var record = await maintenanceService.PlanMaintenanceAsync(
            request.MachineId,
            request.Type,
            request.PlannedDate,
            request.Notes,
            request.AlertId);
        return Ok(record);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<MaintenanceRecord>> Start(Guid id)
    {
        var record = await maintenanceService.StartMaintenanceAsync(id);
        return Ok(record);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<MaintenanceRecord>> Complete(Guid id, CompleteMaintenanceRequest request)
    {
        var record = await maintenanceService.CompleteMaintenanceAsync(id, request.Technician, request.FinalNotes);
        return Ok(record);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<MaintenanceRecord>> Cancel(Guid id, [FromBody] string reason)
    {
        var record = await maintenanceService.CancelMaintenanceAsync(id, reason);
        return Ok(record);
    }

    [HttpGet("machine/{machineId:guid}")]
    public async Task<ActionResult<IEnumerable<MaintenanceRecord>>> GetHistory(Guid machineId)
    {
        var history = await maintenanceService.GetMaintenanceHistoryAsync(machineId);
        return Ok(history);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<MaintenanceRecord>>> GetActive()
    {
        var active = await maintenanceService.GetActiveMaintenanceAsync();
        return Ok(active);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MaintenanceRecord>>> Search(
        [FromQuery] string query,
        [FromQuery] string? status = null,
        [FromQuery] Guid? machineId = null,
        CancellationToken ct = default)
    {
        var results = await maintenanceService.SearchMaintenanceAsync(query, status, machineId, ct);
        return Ok(results);
    }
}
