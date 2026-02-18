using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.Entities;

public class ExternalSystem : BaseEntity<Guid>
{
    private string _name = string.Empty;
    private string _systemType = string.Empty;
    private string _connectionUrl = string.Empty;
    private ExternalSystemStatus _status = ExternalSystemStatus.Disconnected;
    private DateTime _lastConnected = DateTime.MinValue;

    // Private constructor for EF Core
    private ExternalSystem() { }

    // Factory method with validation
    public static Result<ExternalSystem> Create(
        string name,
        string systemType,
        string connectionUrl,
        string? apiKey = null,
        string? username = null,
        string? password = null)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(name))
            return new Result<ExternalSystem>.Failure("System name is required");
        
        if (string.IsNullOrWhiteSpace(systemType))
            return new Result<ExternalSystem>.Failure("System type is required");
        
        if (string.IsNullOrWhiteSpace(connectionUrl))
            return new Result<ExternalSystem>.Failure("Connection URL is required");
        
        if (name.Length > 100)
            return new Result<ExternalSystem>.Failure("System name cannot exceed 100 characters");
        
        if (systemType.Length > 50)
            return new Result<ExternalSystem>.Failure("System type cannot exceed 50 characters");

        var system = new ExternalSystem
        {
            Id = Guid.NewGuid(),
            _name = name.Trim(),
            _systemType = systemType.Trim(),
            _connectionUrl = connectionUrl.Trim(),
            ApiKey = apiKey?.Trim(),
            Username = username?.Trim(),
            Password = password,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new Result<ExternalSystem>.Success(system);
    }

    // Properties
    public string Name => _name;
    public string SystemType => _systemType;
    public string ConnectionUrl => _connectionUrl;
    public string? ApiKey { get; private set; }
    public string? Username { get; private set; }
    public string? Password { get; private set; }
    public ExternalSystemStatus Status => _status;
    public DateTime LastConnected => _lastConnected;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public ICollection<SystemIntegration> Integrations { get; private set; } = [];
    public ICollection<DataSynchronization> Synchronizations { get; private set; } = [];

    // Domain methods
    public Result UpdateConnectionDetails(
        string? connectionUrl = null,
        string? apiKey = null,
        string? username = null,
        string? password = null)
    {
        if (connectionUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(connectionUrl))
                return new Result.Failure("Connection URL cannot be empty");
            _connectionUrl = connectionUrl.Trim();
        }

        ApiKey = apiKey?.Trim();
        Username = username?.Trim();
        Password = password;
        
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new Result.Failure("System name is required");
        
        if (name.Length > 100)
            return new Result.Failure("System name cannot exceed 100 characters");

        _name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result UpdateSystemType(string systemType)
    {
        if (string.IsNullOrWhiteSpace(systemType))
            return new Result.Failure("System type is required");
        
        if (systemType.Length > 50)
            return new Result.Failure("System type cannot exceed 50 characters");

        _systemType = systemType.Trim();
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Connect()
    {
        if (_status == ExternalSystemStatus.Connected)
            return new Result.Failure("System is already connected");

        _status = ExternalSystemStatus.Connected;
        _lastConnected = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result Disconnect()
    {
        if (_status == ExternalSystemStatus.Disconnected)
            return new Result.Failure("System is already disconnected");

        _status = ExternalSystemStatus.Disconnected;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public Result MarkAsError()
    {
        _status = ExternalSystemStatus.Error;
        UpdatedAt = DateTime.UtcNow;
        return new Result.Success();
    }

    public bool IsConnected() => _status == ExternalSystemStatus.Connected;
    public bool IsDisconnected() => _status == ExternalSystemStatus.Disconnected;
    public bool HasError() => _status == ExternalSystemStatus.Error;
}

public enum ExternalSystemStatus
{
    Disconnected = 0,
    Connected = 1,
    Error = 2
}