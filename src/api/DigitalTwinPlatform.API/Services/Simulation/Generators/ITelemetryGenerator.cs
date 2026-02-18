namespace DigitalTwinPlatform.API.Services.Simulation.Generators;

public interface ITelemetryGenerator
{
    string DataType { get; }
    object Generate(double degradationState, Random random);
}
