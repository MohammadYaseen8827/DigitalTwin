using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.ValueObjects;

public sealed class MachineName(string value) : IEquatable<MachineName>
{
    public string Value { get; } = value;

    public static Result<MachineName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new Result<MachineName>.Failure("Machine name cannot be empty");

        if (name.Length > 100)
            return new Result<MachineName>.Failure("Machine name cannot exceed 100 characters");

        return new Result<MachineName>.Success(new MachineName(name.Trim()));
    }

    public override bool Equals(object? obj) => obj is MachineName other && Equals(other);
    public bool Equals(MachineName? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
