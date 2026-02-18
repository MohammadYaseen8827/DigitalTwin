namespace DigitalTwinPlatform.API.Services.Simulation.Generators;

public class VibrationGenerator : ITelemetryGenerator
{
    public string DataType => "vibration";

    public object Generate(double degradationState, Random random)
    {
        var baseRms = 1.5 + degradationState * 0.5;
        var noise = random.NextDouble() * 0.2 - 0.1;
        return new
        {
            value = baseRms + noise,
            unit = "mm/s",
            frequency = 50,
            direction = "vertical"
        };
    }
}
