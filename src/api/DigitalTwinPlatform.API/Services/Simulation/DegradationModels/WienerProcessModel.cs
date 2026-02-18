namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

public class WienerProcessModel : IDegradationModel
{
    public double Drift { get; set; } = 0.001; // μ
    public double Volatility { get; set; } = 0.05; // σ

    public double Step(double currentState, TimeSpan delta, Random random)
    {
        var t = delta.TotalSeconds;
        var noise = Volatility * Math.Sqrt(t) * SampleStandardNormal(random);
        return currentState + Drift * t + noise;
    }

    private static double SampleStandardNormal(Random random)
    {
        // Box–Muller transform
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
