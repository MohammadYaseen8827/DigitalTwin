using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class Tenant : BaseEntity<Guid>
{
    private string _name = string.Empty;
    private string _slug = string.Empty;
    private bool _isActive = true;

    // Private constructor for EF Core
    private Tenant() { }

    // Factory method with validation
    public static Result<Tenant> Create(
        string name,
        string slug,
        string? description = null,
        string? connectionString = null)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
            return new Result<Tenant>.Failure("Tenant name is required");
        
        if (string.IsNullOrWhiteSpace(slug))
            return new Result<Tenant>.Failure("Tenant slug is required");
        
        if (name.Length > 100)
            return new Result<Tenant>.Failure("Tenant name cannot exceed 100 characters");
        
        if (slug.Length > 50)
            return new Result<Tenant>.Failure("Tenant slug cannot exceed 50 characters");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            _name = name.Trim(),
            _slug = slug.ToLowerInvariant().Trim(),
            Description = description?.Trim(),
            ConnectionString = connectionString?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<Tenant>.Success(tenant);
    }

    // Properties
    public string Name => _name;
    public string Slug => _slug;
    public string? Description { get; private set; }
    public string? ConnectionString { get; private set; }
    public bool IsActive => _isActive;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<TenantUser> Users { get; private set; } = [];
    public ICollection<TenantSetting> Settings { get; private set; } = [];

    // Domain methods
    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new Result.Failure("Tenant name is required");
        
        if (name.Length > 100)
            return new Result.Failure("Tenant name cannot exceed 100 characters");

        _name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return new Result.Failure("Tenant slug is required");
        
        if (slug.Length > 50)
            return new Result.Failure("Tenant slug cannot exceed 50 characters");

        _slug = slug.ToLowerInvariant().Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateConnectionString(string? connectionString)
    {
        ConnectionString = connectionString?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Activate()
    {
        if (_isActive)
            return new Result.Failure("Tenant is already active");

        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Deactivate()
    {
        if (!_isActive)
            return new Result.Failure("Tenant is already inactive");

        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }
}