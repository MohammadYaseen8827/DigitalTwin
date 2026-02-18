namespace DigitalTwinPlatform.Application.Abstractions.Analytics;

public interface IMlExperimentLogger
{
    Task LogExperimentAsync(string modelType, string version, double rSquared, double mape, string metadata = "");
}
