using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Application.Tenants.Services;

public class MockTenantService : ITenantService
{
    private readonly List<TenantDto> _tenants = new();
    private readonly List<TenantUserDto> _tenantUsers = new();
    private readonly List<TenantSettingDto> _tenantSettings = new();

    public MockTenantService()
    {
        // Seed with sample data
        SeedSampleData();
    }

    private void SeedSampleData()
    {
        // Sample tenants
        var tenant1 = new TenantDto(
            Guid.NewGuid(),
            "Acme Manufacturing",
            "acme",
            "Leading industrial equipment manufacturer",
            "Server=tcp:acme-db.database.windows.net,1433;Initial Catalog=acme_digitaltwin;",
            true,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow
        );

        var tenant2 = new TenantDto(
            Guid.NewGuid(),
            "Global Industries",
            "global",
            "Multi-national manufacturing conglomerate",
            "Server=tcp:global-db.database.windows.net,1433;Initial Catalog=global_digitaltwin;",
            true,
            DateTime.UtcNow.AddDays(-15),
            DateTime.UtcNow
        );

        _tenants.Add(tenant1);
        _tenants.Add(tenant2);

        // Sample users
        var user1 = new TenantUserDto(
            Guid.NewGuid(),
            tenant1.Id,
            Guid.NewGuid(),
            "John Smith",
            "john.smith@acme.com",
            UserRole.Admin,
            DateTime.UtcNow.AddDays(-25),
            DateTime.UtcNow
        );

        var user2 = new TenantUserDto(
            Guid.NewGuid(),
            tenant1.Id,
            Guid.NewGuid(),
            "Jane Doe",
            "jane.doe@acme.com",
            UserRole.User,
            DateTime.UtcNow.AddDays(-20),
            DateTime.UtcNow
        );

        _tenantUsers.Add(user1);
        _tenantUsers.Add(user2);

        // Sample settings
        var setting1 = new TenantSettingDto(
            Guid.NewGuid(),
            tenant1.Id,
            "branding.theme",
            "industrial-dark",
            DateTime.UtcNow.AddDays(-25),
            DateTime.UtcNow
        );

        var setting2 = new TenantSettingDto(
            Guid.NewGuid(),
            tenant1.Id,
            "notifications.email",
            "true",
            DateTime.UtcNow.AddDays(-25),
            DateTime.UtcNow
        );

        _tenantSettings.Add(setting1);
        _tenantSettings.Add(setting2);
    }

    // Tenant Management
    public async Task<Result<TenantDto>> CreateTenantAsync(TenantCreateDto tenantDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct); // Simulate async operation
        
        var newTenant = new TenantDto(
            Guid.NewGuid(),
            tenantDto.Name,
            tenantDto.Slug,
            tenantDto.Description,
            tenantDto.ConnectionString,
            true,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _tenants.Add(newTenant);
        return new Result<TenantDto>.Success(newTenant);
    }

    public async Task<Result<TenantDto>> UpdateTenantAsync(Guid id, TenantUpdateDto tenantDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        if (tenant == null)
            return new Result<TenantDto>.Failure("Tenant not found");

        var updatedTenant = tenant with
        {
            Name = tenantDto.Name ?? tenant.Name,
            Slug = tenantDto.Slug ?? tenant.Slug,
            Description = tenantDto.Description ?? tenant.Description,
            ConnectionString = tenantDto.ConnectionString ?? tenant.ConnectionString,
            UpdatedAt = DateTime.UtcNow
        };

        _tenants.Remove(tenant);
        _tenants.Add(updatedTenant);

        return new Result<TenantDto>.Success(updatedTenant);
    }

    public async Task<Result> DeleteTenantAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        if (tenant == null)
            return new Result.Failure("Tenant not found");

        _tenants.Remove(tenant);
        return new Result.Success();
    }

    public async Task<Result<TenantDto>> GetTenantByIdAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        if (tenant == null)
            return new Result<TenantDto>.Failure("Tenant not found");

        return new Result<TenantDto>.Success(tenant);
    }

    public async Task<Result<List<TenantDto>>> GetAllTenantsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        return new Result<List<TenantDto>>.Success(_tenants.ToList());
    }

    public async Task<Result<List<TenantDto>>> GetActiveTenantsAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var activeTenants = _tenants.Where(t => t.IsActive).ToList();
        return new Result<List<TenantDto>>.Success(activeTenants);
    }

    public async Task<Result<TenantDto>> GetTenantBySlugAsync(string slug, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        if (tenant == null)
            return new Result<TenantDto>.Failure("Tenant not found");

        return new Result<TenantDto>.Success(tenant);
    }

    public async Task<Result> ActivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        if (tenant == null)
            return new Result.Failure("Tenant not found");

        var updatedTenant = tenant with { IsActive = true, UpdatedAt = DateTime.UtcNow };
        _tenants.Remove(tenant);
        _tenants.Add(updatedTenant);

        return new Result.Success();
    }

    public async Task<Result> DeactivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == id);
        if (tenant == null)
            return new Result.Failure("Tenant not found");

        var updatedTenant = tenant with { IsActive = false, UpdatedAt = DateTime.UtcNow };
        _tenants.Remove(tenant);
        _tenants.Add(updatedTenant);

        return new Result.Success();
    }

    // Tenant User Management
    public async Task<Result<TenantUserDto>> AddUserToTenantAsync(Guid tenantId, TenantUserCreateDto userDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
        if (tenant == null)
            return new Result<TenantUserDto>.Failure("Tenant not found");

        // In a real implementation, you'd validate the user exists
        var newUser = new TenantUserDto(
            Guid.NewGuid(),
            tenantId,
            userDto.UserId,
            $"User {userDto.UserId.ToString()[..8]}",
            $"user{userDto.UserId.ToString()[..8]}@{tenant.Slug}.com",
            userDto.Role,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _tenantUsers.Add(newUser);
        return new Result<TenantUserDto>.Success(newUser);
    }

    public async Task<Result> RemoveUserFromTenantAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenantUser = _tenantUsers.FirstOrDefault(tu => tu.TenantId == tenantId && tu.UserId == userId);
        if (tenantUser == null)
            return new Result.Failure("User not found in tenant");

        _tenantUsers.Remove(tenantUser);
        return new Result.Success();
    }

    public async Task<Result> UpdateTenantUserRoleAsync(Guid tenantId, Guid userId, UserRole role, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenantUser = _tenantUsers.FirstOrDefault(tu => tu.TenantId == tenantId && tu.UserId == userId);
        if (tenantUser == null)
            return new Result.Failure("User not found in tenant");

        var updatedUser = tenantUser with { Role = role, UpdatedAt = DateTime.UtcNow };
        _tenantUsers.Remove(tenantUser);
        _tenantUsers.Add(updatedUser);

        return new Result.Success();
    }

    public async Task<Result<List<TenantUserDto>>> GetTenantUsersAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var users = _tenantUsers.Where(tu => tu.TenantId == tenantId).ToList();
        return new Result<List<TenantUserDto>>.Success(users);
    }

    public async Task<Result<List<TenantDto>>> GetUserTenantsAsync(Guid userId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var tenantIds = _tenantUsers.Where(tu => tu.UserId == userId).Select(tu => tu.TenantId).ToList();
        var tenants = _tenants.Where(t => tenantIds.Contains(t.Id)).ToList();
        return new Result<List<TenantDto>>.Success(tenants);
    }

    public async Task<Result<TenantUserDto>> GetTenantUserAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenantUser = _tenantUsers.FirstOrDefault(tu => tu.TenantId == tenantId && tu.UserId == userId);
        if (tenantUser == null)
            return new Result<TenantUserDto>.Failure("User not found in tenant");

        return new Result<TenantUserDto>.Success(tenantUser);
    }

    // Tenant Setting Management
    public async Task<Result<TenantSettingDto>> CreateTenantSettingAsync(Guid tenantId, TenantSettingCreateDto settingDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
        if (tenant == null)
            return new Result<TenantSettingDto>.Failure("Tenant not found");

        var newSetting = new TenantSettingDto(
            Guid.NewGuid(),
            tenantId,
            settingDto.Key,
            settingDto.Value,
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _tenantSettings.Add(newSetting);
        return new Result<TenantSettingDto>.Success(newSetting);
    }

    public async Task<Result<TenantSettingDto>> UpdateTenantSettingAsync(Guid tenantId, Guid settingId, TenantSettingUpdateDto settingDto, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var setting = _tenantSettings.FirstOrDefault(s => s.Id == settingId && s.TenantId == tenantId);
        if (setting == null)
            return new Result<TenantSettingDto>.Failure("Setting not found");

        var updatedSetting = setting with
        {
            Key = settingDto.Key ?? setting.Key,
            Value = settingDto.Value ?? setting.Value,
            UpdatedAt = DateTime.UtcNow
        };

        _tenantSettings.Remove(setting);
        _tenantSettings.Add(updatedSetting);

        return new Result<TenantSettingDto>.Success(updatedSetting);
    }

    public async Task<Result> DeleteTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var setting = _tenantSettings.FirstOrDefault(s => s.Id == settingId && s.TenantId == tenantId);
        if (setting == null)
            return new Result.Failure("Setting not found");

        _tenantSettings.Remove(setting);
        return new Result.Success();
    }

    public async Task<Result<List<TenantSettingDto>>> GetTenantSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        var settings = _tenantSettings.Where(s => s.TenantId == tenantId).ToList();
        return new Result<List<TenantSettingDto>>.Success(settings);
    }

    public async Task<Result<TenantSettingDto>> GetTenantSettingAsync(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var setting = _tenantSettings.FirstOrDefault(s => s.Id == settingId && s.TenantId == tenantId);
        if (setting == null)
            return new Result<TenantSettingDto>.Failure("Setting not found");

        return new Result<TenantSettingDto>.Success(setting);
    }

    public async Task<Result<TenantSettingDto>> GetTenantSettingByKeyAsync(Guid tenantId, string key, CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
        
        var setting = _tenantSettings.FirstOrDefault(s => s.TenantId == tenantId && s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (setting == null)
            return new Result<TenantSettingDto>.Failure("Setting not found");

        return new Result<TenantSettingDto>.Success(setting);
    }
}