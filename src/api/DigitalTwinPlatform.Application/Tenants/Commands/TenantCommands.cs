using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Domain.Common;
using MediatR;

namespace DigitalTwinPlatform.Application.Tenants.Commands;

// Tenant Commands
public record CreateTenantCommand(TenantCreateDto Tenant) : IRequest<Result<TenantDto>>;

public record UpdateTenantCommand(Guid Id, TenantUpdateDto Tenant) : IRequest<Result<TenantDto>>;

public record DeleteTenantCommand(Guid Id) : IRequest<Result>;

public record ActivateTenantCommand(Guid Id) : IRequest<Result>;

public record DeactivateTenantCommand(Guid Id) : IRequest<Result>;

// Tenant User Commands
public record AddUserToTenantCommand(Guid TenantId, TenantUserCreateDto User) : IRequest<Result<TenantUserDto>>;

public record RemoveUserFromTenantCommand(Guid TenantId, Guid UserId) : IRequest<Result>;

public record UpdateTenantUserRoleCommand(Guid TenantId, Guid UserId, UserRole Role) : IRequest<Result>;

// Tenant Setting Commands
public record CreateTenantSettingCommand(Guid TenantId, TenantSettingCreateDto Setting) : IRequest<Result<TenantSettingDto>>;

public record UpdateTenantSettingCommand(Guid TenantId, Guid SettingId, TenantSettingUpdateDto Setting) : IRequest<Result<TenantSettingDto>>;

public record DeleteTenantSettingCommand(Guid TenantId, Guid SettingId) : IRequest<Result>;