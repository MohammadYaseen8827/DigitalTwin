using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Application.ExternalSystems.Services;
using DigitalTwinPlatform.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExternalSystemStatus = DigitalTwinPlatform.Application.ExternalSystems.Dtos.ExternalSystemStatus;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExternalSystemsController(IExternalSystemService externalSystemService)
    : ControllerBase
{
    // GET: api/externalsystems
[HttpGet]
public async Task<ActionResult<List<ExternalSystemDto>>> GetAllExternalSystems(CancellationToken ct = default)
{
    var result = await externalSystemService.GetAllExternalSystemsAsync(ct);
    return result switch
    {
        Result<List<ExternalSystemDto>>.Success s => Ok(s.Value),
        Result<List<ExternalSystemDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/connected
[HttpGet("connected")]
public async Task<ActionResult<List<ExternalSystemDto>>> GetConnectedExternalSystems(CancellationToken ct = default)
{
    var result = await externalSystemService.GetConnectedExternalSystemsAsync(ct);
    return result switch
    {
        Result<List<ExternalSystemDto>>.Success s => Ok(s.Value),
        Result<List<ExternalSystemDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/type/{systemType}
[HttpGet("type/{systemType}")]
public async Task<ActionResult<List<ExternalSystemDto>>> GetExternalSystemsByType(string systemType, CancellationToken ct = default)
{
    var result = await externalSystemService.GetExternalSystemsByTypeAsync(systemType, ct);
    return result switch
    {
        Result<List<ExternalSystemDto>>.Success s => Ok(s.Value),
        Result<List<ExternalSystemDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/{id}
[HttpGet("{id:guid}")]
public async Task<ActionResult<ExternalSystemDto>> GetExternalSystemById(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.GetExternalSystemByIdAsync(id, ct);
    return result switch
    {
        Result<ExternalSystemDto>.Success s => Ok(s.Value),
        Result<ExternalSystemDto>.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/{id}/status
[HttpGet("{id:guid}/status")]
public async Task<ActionResult<ExternalSystemStatus>> GetExternalSystemStatus(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.GetExternalSystemStatusAsync(id, ct);
    return result switch
    {
        Result<ExternalSystemStatus>.Success s => Ok(s.Value),
        Result<ExternalSystemStatus>.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems
[HttpPost]
public async Task<ActionResult<ExternalSystemDto>> CreateExternalSystem([FromBody] ExternalSystemCreateDto systemDto, CancellationToken ct = default)
{
    var result = await externalSystemService.CreateExternalSystemAsync(systemDto, ct);
    return result switch
    {
        Result<ExternalSystemDto>.Success s => CreatedAtAction(nameof(GetExternalSystemById), new { id = s.Value.Id }, s.Value),
        Result<ExternalSystemDto>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// PUT: api/externalsystems/{id}
[HttpPut("{id:guid}")]
public async Task<ActionResult<ExternalSystemDto>> UpdateExternalSystem(Guid id, [FromBody] ExternalSystemUpdateDto systemDto, CancellationToken ct = default)
{
    var result = await externalSystemService.UpdateExternalSystemAsync(id, systemDto, ct);
    return result switch
    {
        Result<ExternalSystemDto>.Success s => Ok(s.Value),
        Result<ExternalSystemDto>.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// DELETE: api/externalsystems/{id}
[HttpDelete("{id:guid}")]
public async Task<ActionResult> DeleteExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/{id}/connect
[HttpPost("{id:guid}/connect")]
public async Task<ActionResult> ConnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.ConnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/{id}/disconnect
[HttpPost("{id:guid}/disconnect")]
public async Task<ActionResult> DisconnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisconnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/{id}/test
[HttpPost("{id:guid}/test")]
public async Task<ActionResult<bool>> TestExternalSystemConnection(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.TestExternalSystemConnectionAsync(id, ct);
    return result switch
    {
        Result<bool>.Success s => Ok(s.Value),
        Result<bool>.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// System Integration endpoints
// GET: api/externalsystems/integrations
[HttpGet("integrations")]
public async Task<ActionResult<List<SystemIntegrationDto>>> GetSystemIntegrations(CancellationToken ct = default)
{
    var result = await externalSystemService.GetSystemIntegrationsAsync(ct);
    return result switch
    {
        Result<List<SystemIntegrationDto>>.Success s => Ok(s.Value),
        Result<List<SystemIntegrationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/{systemId}/integrations
[HttpGet("{systemId:guid}/integrations")]
public async Task<ActionResult<List<SystemIntegrationDto>>> GetSystemIntegrationsBySystem(Guid systemId, CancellationToken ct = default)
{
    var result = await externalSystemService.GetSystemIntegrationsBySystemAsync(systemId, ct);
    return result switch
    {
        Result<List<SystemIntegrationDto>>.Success s => Ok(s.Value),
        Result<List<SystemIntegrationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/integrations/active
[HttpGet("integrations/active")]
public async Task<ActionResult<List<SystemIntegrationDto>>> GetActiveSystemIntegrations(CancellationToken ct = default)
{
    var result = await externalSystemService.GetActiveSystemIntegrationsAsync(ct);
    return result switch
    {
        Result<List<SystemIntegrationDto>>.Success s => Ok(s.Value),
        Result<List<SystemIntegrationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/integrations
[HttpPost("integrations")]
public async Task<ActionResult<SystemIntegrationDto>> CreateSystemIntegration([FromBody] SystemIntegrationCreateDto integrationDto, CancellationToken ct = default)
{
    var result = await externalSystemService.CreateSystemIntegrationAsync(integrationDto, ct);
    return result switch
    {
        Result<SystemIntegrationDto>.Success => Created(),
        Result<SystemIntegrationDto>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// PUT: api/externalsystems/integrations/{id}
[HttpPut("integrations/{id:guid}")]
public async Task<ActionResult<SystemIntegrationDto>> UpdateSystemIntegration(Guid id, [FromBody] SystemIntegrationUpdateDto integrationDto, CancellationToken ct = default)
{
    var result = await externalSystemService.UpdateSystemIntegrationAsync(id, integrationDto, ct);
    return result switch
    {
        Result<SystemIntegrationDto>.Success s => Ok(s.Value),
        Result<SystemIntegrationDto>.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// DELETE: api/externalsystems/integrations/{id}
[HttpDelete("integrations/{id:guid}")]
public async Task<ActionResult> DeleteSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/integrations/{id}/enable
[HttpPost("integrations/{id:guid}/enable")]
public async Task<ActionResult> EnableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.EnableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/integrations/{id}/disable
[HttpPost("integrations/{id:guid}/disable")]
public async Task<ActionResult> DisableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// Data Synchronization endpoints
// GET: api/externalsystems/synchronizations
[HttpGet("synchronizations")]
public async Task<ActionResult<List<DataSynchronizationDto>>> GetDataSynchronizations(CancellationToken ct = default)
{
    var result = await externalSystemService.GetDataSynchronizationsAsync(ct);
    return result switch
    {
        Result<List<DataSynchronizationDto>>.Success s => Ok(s.Value),
        Result<List<DataSynchronizationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/synchronizations/pending
[HttpGet("synchronizations/pending")]
public async Task<ActionResult<List<DataSynchronizationDto>>> GetPendingSynchronizations(CancellationToken ct = default)
{
    var result = await externalSystemService.GetPendingSynchronizationsAsync(ct);
    return result switch
    {
        Result<List<DataSynchronizationDto>>.Success s => Ok(s.Value),
        Result<List<DataSynchronizationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/synchronizations/failed
[HttpGet("synchronizations/failed")]
public async Task<ActionResult<List<DataSynchronizationDto>>> GetFailedSynchronizations(CancellationToken ct = default)
{
    var result = await externalSystemService.GetFailedSynchronizationsAsync(ct);
    return result switch
    {
        Result<List<DataSynchronizationDto>>.Success s => Ok(s.Value),
        Result<List<DataSynchronizationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// GET: api/externalsystems/synchronizations/recent
[HttpGet("synchronizations/recent")]
public async Task<ActionResult<List<DataSynchronizationDto>>> GetRecentSynchronizations([FromQuery] int limit = 50, CancellationToken ct = default)
{
    var result = await externalSystemService.GetRecentSynchronizationsAsync(limit, ct);
    return result switch
    {
        Result<List<DataSynchronizationDto>>.Success s => Ok(s.Value),
        Result<List<DataSynchronizationDto>>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/synchronizations
[HttpPost("synchronizations")]
public async Task<ActionResult<DataSynchronizationDto>> CreateDataSynchronization([FromBody] DataSynchronizationCreateDto syncDto, CancellationToken ct = default)
{
    var result = await externalSystemService.CreateDataSynchronizationAsync(syncDto, ct);
    return result switch
    {
        Result<DataSynchronizationDto>.Success => Created(),
        Result<DataSynchronizationDto>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}

// POST: api/externalsystems/synchronizations/process
[HttpPost("synchronizations/process")]
public async Task<ActionResult<int>> ProcessPendingSynchronizations(CancellationToken ct = default)
{
    var result = await externalSystemService.ProcessPendingSynchronizationsAsync(ct);
    return result switch
    {
        Result<int>.Success s => Ok(s.Value),
        Result<int>.Failure f => BadRequest(f.Error),
        _ => throw new InvalidOperationException()
    };
}
}
