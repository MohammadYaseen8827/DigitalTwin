using DigitalTwinPlatform.Domain.Common;

namespace DigitalTwinPlatform.Domain.ValueObjects;

public sealed class MachineType(string value) : IEquatable<MachineType>
{
    public string Value { get; } = value;

    private static readonly string[] ValidTypes = 
    { 
        "CNC", "Lathe", "Press", "Welder", "Robot", 
        "InjectionMolder", "Conveyor", "3D Printer", "Laser Cutter",
        "Assembly Station", "Inspection", "SMT", "Oven", "Soldering",
        "Extruder", "Granulator", "Dryer"
    };

    public static Result<MachineType> Create(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return new Result<MachineType>.Failure("Machine type cannot be empty");

        if (!ValidTypes.Contains(type))
            return new Result<MachineType>.Failure($"Invalid machine type: {type}. Valid types: {string.Join(", ", ValidTypes)}");

        return new Result<MachineType>.Success(new MachineType(type));
    }

    public override bool Equals(object? obj) => obj is MachineType other && Equals(other);
    public bool Equals(MachineType? other) => other?.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
