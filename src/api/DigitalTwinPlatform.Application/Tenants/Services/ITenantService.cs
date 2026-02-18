using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Application.Tenants.Services;

public interface ITenantService
{
    // Tenant Management
    Task<Result<TenantDto>> CreateTenantAsync(TenantCreateDto tenantDto, CancellationToken ct = default);
    Task<Result<TenantDto>> UpdateTenantAsync(Guid id, TenantUpdateDto tenantDto, CancellationToken ct = default);
    Task<Result> DeleteTenantAsync(Guid id, CancellationToken ct = default);
    Task<Result<TenantDto>> GetTenantByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<TenantDto>>> GetAllTenantsAsync(CancellationToken ct = default);
    Task<Result<List<TenantDto>>> GetActiveTenantsAsync(CancellationToken ct = default);
    Task<Result<TenantDto>> GetTenantBySlugAsync(string slug, CancellationToken ct = default);
    Task<Result> ActivateTenantAsync(Guid id, CancellationToken ct = default);
    Task<Result> DeactivateTenantAsync(Guid id, CancellationToken ct = default);

    // Tenant User Management
    Task<Result<TenantUserDto>> AddUserToTenantAsync(Guid tenantId, TenantUserCreateDto userDto, CancellationToken ct = default);
    Task<Result> RemoveUserFromTenantAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
    Task<Result> UpdateTenantUserRoleAsync(Guid tenantId, Guid userId, UserRole role, CancellationToken ct = default);
    Task<Result<List<TenantUserDto>>> GetTenantUsersAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<List<TenantDto>>> GetUserTenantsAsync(Guid userId, CancellationToken ct = default);
    Task<Result<TenantUserDto>> GetTenantUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    // Tenant Setting Management
    Task<Result<TenantSettingDto>> CreateTenantSettingAsync(Guid tenantId, TenantSettingCreateDto settingDto, CancellationToken ct = default);
    Task<Result<TenantSettingDto>> UpdateTenantSettingAsync(Guid tenantId, Guid settingId, TenantSettingUpdateDto settingDto, CancellationToken ct = default);
    Task<Result> DeleteTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default);
    Task<Result<List<TenantSettingDto>>> GetTenantSettingsAsync(Guid tenantId, CancellationToken ct = default);
    Task<Result<TenantSettingDto>> GetTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default);
    Task<Result<TenantSettingDto>> GetTenantSettingByKeyAsync(Guid tenantId, string key, CancellationToken ct = default);
}