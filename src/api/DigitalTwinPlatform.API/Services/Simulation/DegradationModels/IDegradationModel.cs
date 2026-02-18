namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

public interface IDegradationModel
{
    double Step(double currentState, TimeSpan delta, Random random);
}
