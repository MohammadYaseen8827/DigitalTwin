using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class TenantSetting : BaseEntity<Guid>
{
    private string _key = string.Empty;
    private string _value = string.Empty;

    // Private constructor for EF Core
    private TenantSetting() { }

    // Factory method with validation
    public static Result<TenantSetting> Create(
        Guid tenantId,
        string key,
        string value)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(key))
            return new Result<TenantSetting>.Failure("Setting key is required");
        
        if (string.IsNullOrWhiteSpace(value))
            return new Result<TenantSetting>.Failure("Setting value is required");
        
        if (key.Length > 100)
            return new Result<TenantSetting>.Failure("Setting key cannot exceed 100 characters");
        
        if (value.Length > 1000)
            return new Result<TenantSetting>.Failure("Setting value cannot exceed 1000 characters");

        var setting = new TenantSetting
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            _key = key.Trim(),
            _value = value.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<TenantSetting>.Success(setting);
    }

    // Properties (new to explicitly hide base TenantId, CreatedAt, UpdatedAt)
    public new Guid TenantId { get; private set; }
    public string Key => _key;
    public string Value => _value;
    public new DateTime CreatedAt { get; private set; }
    public new DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    // Domain methods
    public Result UpdateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new Result.Failure("Setting value is required");
        
        if (value.Length > 1000)
            return new Result.Failure("Setting value cannot exceed 1000 characters");

        _value = value.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return new Result.Failure("Setting key is required");
        
        if (key.Length > 100)
            return new Result.Failure("Setting key cannot exceed 100 characters");

        _key = key.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }
}