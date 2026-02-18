namespace DigitalTwinPlatform.Application.Tenants.Dtos;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ConnectionString,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TenantCreateDto(
    string Name,
    string Slug,
    string? Description = null,
    string? ConnectionString = null);

public record TenantUpdateDto(
    string? Name = null,
    string? Slug = null,
    string? Description = null,
    string? ConnectionString = null);

public record TenantUserDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string UserName,
    string UserEmail,
    UserRole Role,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TenantUserCreateDto(
    Guid UserId,
    UserRole Role = UserRole.User);

public record TenantSettingDto(
    Guid Id,
    Guid TenantId,
    string Key,
    string Value,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TenantSettingCreateDto(
    string Key,
    string Value);

public record TenantSettingUpdateDto(
    string? Key = null,
    string? Value = null);

public enum UserRole
{
    User = 0,
    Admin = 1
}