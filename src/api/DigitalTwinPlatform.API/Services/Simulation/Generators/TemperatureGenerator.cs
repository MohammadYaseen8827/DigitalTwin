namespace DigitalTwinPlatform.API.Services.Simulation.Generators;

public class TemperatureGenerator : ITelemetryGenerator
{
    public string DataType => "temperature";

    public object Generate(double degradationState, Random random)
    {
        var baseTemp = 40 + degradationState * 5;
        var noise = random.NextDouble() * 2 - 1;
        return new
        {
            value = baseTemp + noise,
            unit = "°C",
            minThreshold = 20,
            maxThreshold = 80
        };
    }
}
