using DigitalTwinPlatform.Application.Tenants.Commands;
using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Application.Tenants.Queries;
using DigitalTwinPlatform.Application.Tenants.Services;
using DigitalTwinPlatform.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserRole = DigitalTwinPlatform.Application.Tenants.Dtos.UserRole;

namespace DigitalTwinPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;

    public TenantsController(IMediator mediator, ITenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    // GET: api/tenants
    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> GetAllTenants(CancellationToken ct = default)
    {
        var result = await _tenantService.GetAllTenantsAsync(ct);
        return result switch
        {
            Result<List<TenantDto>>.Success s => Ok(s.Value),
            Result<List<TenantDto>>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // GET: api/tenants/active
    [HttpGet("active")]
    public async Task<ActionResult<List<TenantDto>>> GetActiveTenants(CancellationToken ct = default)
    {
        var result = await _tenantService.GetActiveTenantsAsync(ct);
        return result switch
        {
            Result<List<TenantDto>>.Success s => Ok(s.Value),
            Result<List<TenantDto>>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // GET: api/tenants/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetTenantById(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.GetTenantByIdAsync(id, ct);
        return result switch
        {
            Result<TenantDto>.Success s => Ok(s.Value),
            Result<TenantDto>.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // GET: api/tenants/slug/{slug}
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<TenantDto>> GetTenantBySlug(string slug, CancellationToken ct = default)
    {
        var result = await _tenantService.GetTenantBySlugAsync(slug, ct);
        return result switch
        {
            Result<TenantDto>.Success s => Ok(s.Value),
            Result<TenantDto>.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // POST: api/tenants
    [HttpPost]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] TenantCreateDto tenantDto, CancellationToken ct = default)
    {
        var result = await _tenantService.CreateTenantAsync(tenantDto, ct);
        return result switch
        {
            Result<TenantDto>.Success s => CreatedAtAction(nameof(GetTenantById), new { id = s.Value.Id }, s.Value),
            Result<TenantDto>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // PUT: api/tenants/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] TenantUpdateDto tenantDto, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateTenantAsync(id, tenantDto, ct);
        return result switch
        {
            Result<TenantDto>.Success s => Ok(s.Value),
            Result<TenantDto>.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // DELETE: api/tenants/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantAsync(id, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // POST: api/tenants/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> ActivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.ActivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // POST: api/tenants/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeactivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // Tenant Users endpoints
    // GET: api/tenants/{tenantId}/users
    [HttpGet("{tenantId:guid}/users")]
    public async Task<ActionResult<List<TenantUserDto>>> GetTenantUsers(Guid tenantId, CancellationToken ct = default)
    {
        var result = await _tenantService.GetTenantUsersAsync(tenantId, ct);
        return result switch
        {
            Result<List<TenantUserDto>>.Success s => Ok(s.Value),
            Result<List<TenantUserDto>>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // POST: api/tenants/{tenantId}/users
    [HttpPost("{tenantId:guid}/users")]
    public async Task<ActionResult<TenantUserDto>> AddUserToTenant(Guid tenantId, [FromBody] TenantUserCreateDto userDto, CancellationToken ct = default)
    {
        var result = await _tenantService.AddUserToTenantAsync(tenantId, userDto, ct);
        return result switch
        {
            Result<TenantUserDto>.Success => Created(),
            Result<TenantUserDto>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // DELETE: api/tenants/{tenantId}/users/{userId}
    [HttpDelete("{tenantId:guid}/users/{userId:guid}")]
    public async Task<ActionResult> RemoveUserFromTenant(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var result = await _tenantService.RemoveUserFromTenantAsync(tenantId, userId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // PUT: api/tenants/{tenantId}/users/{userId}/role
    [HttpPut("{tenantId:guid}/users/{userId:guid}/role")]
    public async Task<ActionResult> UpdateTenantUserRole(Guid tenantId, Guid userId, [FromBody] UserRole role, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateTenantUserRoleAsync(tenantId, userId, role, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // Tenant Settings endpoints
    // GET: api/tenants/{tenantId}/settings
    [HttpGet("{tenantId:guid}/settings")]
    public async Task<ActionResult<List<TenantSettingDto>>> GetTenantSettings(Guid tenantId, CancellationToken ct = default)
    {
        var result = await _tenantService.GetTenantSettingsAsync(tenantId, ct);
        return result switch
        {
            Result<List<TenantSettingDto>>.Success s => Ok(s.Value),
            Result<List<TenantSettingDto>>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // POST: api/tenants/{tenantId}/settings
    [HttpPost("{tenantId:guid}/settings")]
    public async Task<ActionResult<TenantSettingDto>> CreateTenantSetting(Guid tenantId, [FromBody] TenantSettingCreateDto settingDto, CancellationToken ct = default)
    {
        var result = await _tenantService.CreateTenantSettingAsync(tenantId, settingDto, ct);
        return result switch
        {
            Result<TenantSettingDto>.Success => Created(),
            Result<TenantSettingDto>.Failure f => BadRequest(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // PUT: api/tenants/{tenantId}/settings/{settingId}
    [HttpPut("{tenantId:guid}/settings/{settingId:guid}")]
    public async Task<ActionResult<TenantSettingDto>> UpdateTenantSetting(Guid tenantId, Guid settingId, [FromBody] TenantSettingUpdateDto settingDto, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateTenantSettingAsync(tenantId, settingId, settingDto, ct);
        return result switch
        {
            Result<TenantSettingDto>.Success s => Ok(s.Value),
            Result<TenantSettingDto>.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // DELETE: api/tenants/{tenantId}/settings/{settingId}
    [HttpDelete("{tenantId:guid}/settings/{settingId:guid}")]
    public async Task<ActionResult> DeleteTenantSetting(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantSettingAsync(tenantId, settingId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    // GET: api/tenants/{tenantId}/settings/key/{key}
    [HttpGet("{tenantId:guid}/settings/key/{key}")]
    public async Task<ActionResult<TenantSettingDto>> GetTenantSettingByKey(Guid tenantId, string key, CancellationToken ct = default)
    {
        var result = await _tenantService.GetTenantSettingByKeyAsync(tenantId, key, ct);
        return result switch
        {
            Result<TenantSettingDto>.Success s => Ok(s.Value),
            Result<TenantSettingDto>.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }
}