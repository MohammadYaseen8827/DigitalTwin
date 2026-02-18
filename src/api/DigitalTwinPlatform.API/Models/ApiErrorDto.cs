namespace DigitalTwinPlatform.API.Models;

public class ApiErrorDto
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<ValidationError>? ValidationErrors { get; set; }
}

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}

public static class ErrorCodes
{
    // General errors
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    
    // Domain errors
    public const string ValidationError = "VALIDATION_ERROR";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string InvalidOperation = "INVALID_OPERATION";
    
    // Specific domain errors
    public const string MachineNotFound = "MACHINE_NOT_FOUND";
    public const string ModelNotFound = "MODEL_NOT_FOUND";
    public const string ConfigurationError = "CONFIGURATION_ERROR";
    public const string SimulationError = "SIMULATION_ERROR";
    public const string PredictionError = "PREDICTION_ERROR";
}
