using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class TenantUser : BaseEntity<Guid>
{
    private UserRole _role = UserRole.User;

    // Private constructor for EF Core
    private TenantUser() { }

    // Factory method with validation
    public static Result<TenantUser> Create(
        Guid tenantId,
        Guid userId,
        UserRole role = UserRole.User)
    {
        var tenantUser = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            _role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<TenantUser>.Success(tenantUser);
    }

    // Properties (new to explicitly hide base TenantId, CreatedAt, UpdatedAt)
    public new Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public UserRole Role => _role;
    public new DateTime CreatedAt { get; private set; }
    public new DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // Domain methods
    public Result UpdateRole(UserRole newRole)
    {
        if (newRole == _role)
            return new Result.Failure($"User already has role {newRole}");

        _role = newRole;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result AssignAdminRole()
    {
        return UpdateRole(UserRole.Admin);
    }

    public Result AssignUserRole()
    {
        return UpdateRole(UserRole.User);
    }
}

public enum UserRole
{
    User = 0,
    Admin = 1
}