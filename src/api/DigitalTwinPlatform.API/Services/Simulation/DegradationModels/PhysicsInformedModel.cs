namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

public class PhysicsInformedModel : IDegradationModel
{
    public double BaselineDrift { get; set; } = 0.0005;
    public double LoadFactor { get; set; } = 1.0;

    public double Step(double currentState, TimeSpan delta, Random random)
    {
        var t = delta.TotalSeconds;
        var loadImpact = LoadFactor * 0.001 * t;
        var noise = 0.005 * random.NextDouble();
        return currentState + BaselineDrift * t + loadImpact + noise;
    }
}
