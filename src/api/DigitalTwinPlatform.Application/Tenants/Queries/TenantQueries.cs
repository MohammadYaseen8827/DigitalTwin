using DigitalTwinPlatform.Application.Tenants.Dtos;
using DigitalTwinPlatform.Domain.Common;
using MediatR;

namespace DigitalTwinPlatform.Application.Tenants.Queries;

// Tenant Queries
public record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantDto>>;

public record GetAllTenantsQuery : IRequest<Result<List<TenantDto>>>;

public record GetActiveTenantsQuery : IRequest<Result<List<TenantDto>>>;

public record GetTenantBySlugQuery(string Slug) : IRequest<Result<TenantDto>>;

// Tenant User Queries
public record GetTenantUsersQuery(Guid TenantId) : IRequest<Result<List<TenantUserDto>>>;

public record GetUserTenantsQuery(Guid UserId) : IRequest<Result<List<TenantDto>>>;

public record GetTenantUserQuery(Guid TenantId, Guid UserId) : IRequest<Result<TenantUserDto>>;

// Tenant Setting Queries
public record GetTenantSettingsQuery(Guid TenantId) : IRequest<Result<List<TenantSettingDto>>>;

public record GetTenantSettingQuery(Guid TenantId, Guid SettingId) : IRequest<Result<TenantSettingDto>>;

public record GetTenantSettingByKeyQuery(Guid TenantId, string Key) : IRequest<Result<TenantSettingDto>>;