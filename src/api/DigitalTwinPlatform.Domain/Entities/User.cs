using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class User : BaseEntity<Guid>
{
    private string _email = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private bool _isActive = true;

    // Private constructor for EF Core
    private User() { }

    // Factory method with validation
    public static Result<User> Create(
        string email,
        string firstName,
        string lastName,
        string? phoneNumber = null)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
            return new Result<User>.Failure("Email is required");
        
        if (!IsValidEmail(email))
            return new Result<User>.Failure("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(firstName))
            return new Result<User>.Failure("First name is required");
        
        if (string.IsNullOrWhiteSpace(lastName))
            return new Result<User>.Failure("Last name is required");

        var user = new User
        {
            Id = Guid.NewGuid(),
            _email = email.ToLowerInvariant().Trim(),
            _firstName = firstName.Trim(),
            _lastName = lastName.Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<User>.Success(user);
    }

    // Properties
    public string Email => _email;
    public string FirstName => _firstName;
    public string LastName => _lastName;
    public string? PhoneNumber { get; private set; }
    public bool IsActive => _isActive;

    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; private set; } = [];

    // Domain methods
    public Result UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new Result.Failure("Email is required");
        
        if (!IsValidEmail(email))
            return new Result.Failure("Invalid email format");

        _email = email.ToLowerInvariant().Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return new Result.Failure("First name is required");

        _firstName = firstName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            return new Result.Failure("Last name is required");

        _lastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Activate()
    {
        if (_isActive)
            return new Result.Failure("User is already active");

        _isActive = true;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Deactivate()
    {
        if (!_isActive)
            return new Result.Failure("User is already inactive");

        _isActive = false;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    // Helper method for email validation
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}