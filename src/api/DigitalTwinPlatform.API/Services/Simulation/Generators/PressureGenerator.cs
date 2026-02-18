namespace DigitalTwinPlatform.API.Services.Simulation.Generators;

public class PressureGenerator : ITelemetryGenerator
{
    public string DataType => "pressure";

    public object Generate(double degradationState, Random random)
    {
        var basePressure = 3.0 + degradationState * 0.3;
        var noise = random.NextDouble() * 0.1 - 0.05;
        return new
        {
            value = basePressure + noise,
            unit = "bar"
        };
    }
}
