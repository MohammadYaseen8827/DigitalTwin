namespace DigitalTwinPlatform.Domain.Common;

/// <summary>
/// Base exception for domain-level errors.
/// </summary>
public class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when domain validation fails.
/// </summary>
public class DomainValidationException(string message, string propertyName, object? attemptedValue = null)
    : DomainException(message)
{
    public string PropertyName { get; } = propertyName;
    public object? AttemptedValue { get; } = attemptedValue;
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public abstract class BusinessRuleViolationException(string message, string ruleName) : DomainException(message)
{
    public string RuleName { get; } = ruleName;
}
