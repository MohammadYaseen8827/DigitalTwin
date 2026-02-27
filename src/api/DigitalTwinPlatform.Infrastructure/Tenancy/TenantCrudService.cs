using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Application.Tenants.Services;
using DigitalTwinPlatform.Domain.Common;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Infrastructure.Tenancy;

/// <summary>
/// Production implementation of the Application-layer ITenantService.
/// Backed by Entity Framework Core / PostgreSQL.
/// </summary>
public class TenantCrudService(
    DigitalTwinDbContext dbContext,
    UserManager<Domain.Entities.Auth.ApplicationUser> userManager,
    ILogger<TenantCrudService> logger) : ITenantService
{
    // ─── Tenant Management ──────────────────────────────────────────────────

    public async Task<Result<TenantDto>> CreateTenantAsync(TenantCreateDto dto, CancellationToken ct = default)
    {
        var result = Tenant.Create(dto.Name, dto.Slug, dto.Description, dto.ConnectionString);
        if (result is not Result<Tenant>.Success success)
            return new Result<TenantDto>.Failure(((Result<Tenant>.Failure)result).Error);

        var tenant = success.Value;

        // Check slug uniqueness
        if (await dbContext.Tenants.AnyAsync(t => t.Slug == tenant.Slug, ct))
            return new Result<TenantDto>.Failure($"A tenant with slug '{dto.Slug}' already exists.");

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Created tenant {TenantId} ({Slug})", tenant.Id, tenant.Slug);
        return new Result<TenantDto>.Success(ToDto(tenant));
    }

    public async Task<Result<TenantDto>> UpdateTenantAsync(Guid id, TenantUpdateDto dto, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.FindAsync([id], ct);
        if (tenant is null)
            return new Result<TenantDto>.Failure("Tenant not found.");

        if (dto.Name is not null) tenant.UpdateName(dto.Name);
        if (dto.Slug is not null) tenant.UpdateSlug(dto.Slug);
        if (dto.Description is not null) tenant.UpdateDescription(dto.Description);
        if (dto.ConnectionString is not null) tenant.UpdateConnectionString(dto.ConnectionString);

        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Updated tenant {TenantId}", id);
        return new Result<TenantDto>.Success(ToDto(tenant));
    }

    public async Task<Result> DeleteTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.FindAsync([id], ct);
        if (tenant is null)
            return new Result.Failure("Tenant not found.");

        dbContext.Tenants.Remove(tenant);
        await dbContext.SaveChangesAsync(ct);
        logger.LogInformation("Deleted tenant {TenantId}", id);
        return new Result.Success();
    }

    public async Task<Result<TenantDto>> GetTenantByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
        return tenant is null
            ? new Result<TenantDto>.Failure("Tenant not found.")
            : new Result<TenantDto>.Success(ToDto(tenant));
    }

    public async Task<Result<List<TenantDto>>> GetAllTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await dbContext.Tenants.AsNoTracking().ToListAsync(ct);
        return new Result<List<TenantDto>>.Success(tenants.Select(ToDto).ToList());
    }

    public async Task<Result<List<TenantDto>>> GetActiveTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await dbContext.Tenants.AsNoTracking().Where(t => t.IsActive).ToListAsync(ct);
        return new Result<List<TenantDto>>.Success(tenants.Select(ToDto).ToList());
    }

    public async Task<Result<TenantDto>> GetTenantBySlugAsync(string slug, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug.ToLowerInvariant(), ct);
        return tenant is null
            ? new Result<TenantDto>.Failure("Tenant not found.")
            : new Result<TenantDto>.Success(ToDto(tenant));
    }

    public async Task<Result> ActivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.FindAsync([id], ct);
        if (tenant is null) return new Result.Failure("Tenant not found.");

        var result = tenant.Activate();
        if (result is Result.Failure f) return f;

        await dbContext.SaveChangesAsync(ct);
        return new Result.Success();
    }

    public async Task<Result> DeactivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await dbContext.Tenants.FindAsync([id], ct);
        if (tenant is null) return new Result.Failure("Tenant not found.");

        var result = tenant.Deactivate();
        if (result is Result.Failure f) return f;

        await dbContext.SaveChangesAsync(ct);
        return new Result.Success();
    }

    // ─── Tenant User Management ─────────────────────────────────────────────

    public async Task<Result<TenantUserDto>> AddUserToTenantAsync(Guid tenantId, TenantUserCreateDto dto, CancellationToken ct = default)
    {
        if (!await dbContext.Tenants.AnyAsync(t => t.Id == tenantId, ct))
            return new Result<TenantUserDto>.Failure("Tenant not found.");

        if (await dbContext.TenantUsers.AnyAsync(tu => tu.TenantId == tenantId && tu.UserId == dto.UserId, ct))
            return new Result<TenantUserDto>.Failure("User is already a member of this tenant.");

        var domainRole = (Domain.Entities.UserRole)(int)dto.Role;
        var result = TenantUser.Create(tenantId, dto.UserId, domainRole);
        if (result is not Result<TenantUser>.Success success)
            return new Result<TenantUserDto>.Failure(((Result<TenantUser>.Failure)result).Error);

        dbContext.TenantUsers.Add(success.Value);
        await dbContext.SaveChangesAsync(ct);

        var appUser = await userManager.FindByIdAsync(dto.UserId.ToString());
        return new Result<TenantUserDto>.Success(ToUserDto(success.Value, appUser?.UserName ?? "Unknown", appUser?.Email ?? ""));
    }

    public async Task<Result> RemoveUserFromTenantAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var tu = await dbContext.TenantUsers
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
        if (tu is null) return new Result.Failure("User not found in tenant.");

        dbContext.TenantUsers.Remove(tu);
        await dbContext.SaveChangesAsync(ct);
        return new Result.Success();
    }

    public async Task<Result> UpdateTenantUserRoleAsync(Guid tenantId, Guid userId, Application.Tenants.Dtos.UserRole role, CancellationToken ct = default)
    {
        var tu = await dbContext.TenantUsers
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
        if (tu is null) return new Result.Failure("User not found in tenant.");

        tu.UpdateRole((Domain.Entities.UserRole)(int)role);
        await dbContext.SaveChangesAsync(ct);
        return new Result.Success();
    }

    public async Task<Result<List<TenantUserDto>>> GetTenantUsersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var users = await dbContext.TenantUsers.AsNoTracking()
            .Where(tu => tu.TenantId == tenantId)
            .ToListAsync(ct);

        var dtos = new List<TenantUserDto>();
        foreach (var u in users)
        {
            var appUser = await userManager.FindByIdAsync(u.UserId.ToString());
            dtos.Add(ToUserDto(u, appUser?.UserName ?? "Unknown", appUser?.Email ?? ""));
        }
        return new Result<List<TenantUserDto>>.Success(dtos);
    }

    public async Task<Result<List<TenantDto>>> GetUserTenantsAsync(Guid userId, CancellationToken ct = default)
    {
        var tenantIds = await dbContext.TenantUsers.AsNoTracking()
            .Where(tu => tu.UserId == userId)
            .Select(tu => tu.TenantId)
            .ToListAsync(ct);

        var tenants = await dbContext.Tenants.AsNoTracking()
            .Where(t => tenantIds.Contains(t.Id))
            .ToListAsync(ct);

        return new Result<List<TenantDto>>.Success(tenants.Select(ToDto).ToList());
    }

    public async Task<Result<TenantUserDto>> GetTenantUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var tu = await dbContext.TenantUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, ct);
        if (tu is null) return new Result<TenantUserDto>.Failure("User not found in tenant.");

        var appUser = await userManager.FindByIdAsync(userId.ToString());
        return new Result<TenantUserDto>.Success(ToUserDto(tu, appUser?.UserName ?? "Unknown", appUser?.Email ?? ""));
    }

    // ─── Tenant Setting Management ───────────────────────────────────────────

    public async Task<Result<TenantSettingDto>> CreateTenantSettingAsync(Guid tenantId, TenantSettingCreateDto dto, CancellationToken ct = default)
    {
        if (!await dbContext.Tenants.AnyAsync(t => t.Id == tenantId, ct))
            return new Result<TenantSettingDto>.Failure("Tenant not found.");

        var result = TenantSetting.Create(tenantId, dto.Key, dto.Value);
        if (result is not Result<TenantSetting>.Success success)
            return new Result<TenantSettingDto>.Failure(((Result<TenantSetting>.Failure)result).Error);

        dbContext.TenantSettings.Add(success.Value);
        await dbContext.SaveChangesAsync(ct);
        return new Result<TenantSettingDto>.Success(ToSettingDto(success.Value));
    }

    public async Task<Result<TenantSettingDto>> UpdateTenantSettingAsync(Guid tenantId, Guid settingId, TenantSettingUpdateDto dto, CancellationToken ct = default)
    {
        var setting = await dbContext.TenantSettings
            .FirstOrDefaultAsync(s => s.Id == settingId && s.TenantId == tenantId, ct);
        if (setting is null) return new Result<TenantSettingDto>.Failure("Setting not found.");

        if (dto.Key is not null) setting.UpdateKey(dto.Key);
        if (dto.Value is not null) setting.UpdateValue(dto.Value);

        await dbContext.SaveChangesAsync(ct);
        return new Result<TenantSettingDto>.Success(ToSettingDto(setting));
    }

    public async Task<Result> DeleteTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        var setting = await dbContext.TenantSettings
            .FirstOrDefaultAsync(s => s.Id == settingId && s.TenantId == tenantId, ct);
        if (setting is null) return new Result.Failure("Setting not found.");

        dbContext.TenantSettings.Remove(setting);
        await dbContext.SaveChangesAsync(ct);
        return new Result.Success();
    }

    public async Task<Result<List<TenantSettingDto>>> GetTenantSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var settings = await dbContext.TenantSettings.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);
        return new Result<List<TenantSettingDto>>.Success(settings.Select(ToSettingDto).ToList());
    }

    public async Task<Result<TenantSettingDto>> GetTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        var setting = await dbContext.TenantSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == settingId && s.TenantId == tenantId, ct);
        return setting is null
            ? new Result<TenantSettingDto>.Failure("Setting not found.")
            : new Result<TenantSettingDto>.Success(ToSettingDto(setting));
    }

    public async Task<Result<TenantSettingDto>> GetTenantSettingByKeyAsync(Guid tenantId, string key, CancellationToken ct = default)
    {
        var setting = await dbContext.TenantSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Key == key.ToLowerInvariant(), ct);
        return setting is null
            ? new Result<TenantSettingDto>.Failure("Setting not found.")
            : new Result<TenantSettingDto>.Success(ToSettingDto(setting));
    }

    // ─── Mapping helpers ───────────────────────────────────────────────────

    private static TenantDto ToDto(Tenant t) => new(
        t.Id, t.Name, t.Slug, t.Description, t.ConnectionString, t.IsActive, t.CreatedAt, t.UpdatedAt);

    private static TenantUserDto ToUserDto(TenantUser tu, string userName, string email) => new(
        tu.Id, tu.TenantId, tu.UserId, userName, email,
        (Application.Tenants.Dtos.UserRole)(int)tu.Role, tu.CreatedAt, tu.UpdatedAt);

    private static TenantSettingDto ToSettingDto(TenantSetting s) => new(
        s.Id, s.TenantId, s.Key, s.Value, s.CreatedAt, s.UpdatedAt);
}
