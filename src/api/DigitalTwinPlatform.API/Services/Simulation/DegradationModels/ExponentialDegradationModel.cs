namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

public class ExponentialDegradationModel : IDegradationModel
{
    public double Lambda { get; set; } = 0.001; // decay rate

    public double Step(double currentState, TimeSpan delta, Random random)
    {
        var t = delta.TotalSeconds;
        var growth = Math.Exp(Lambda * t);
        // add small noise
        var noise = 0.01 * random.NextDouble();
        return currentState * growth + noise;
    }
}
