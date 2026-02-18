using DigitalTwinPlatform.Domain.Models;

namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

public interface IDegradationModelFactory
{
    IDegradationModel Create(DegradationModelConfiguration config);
}

public class DegradationModelFactory : IDegradationModelFactory
{
    public IDegradationModel Create(DegradationModelConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return config.ModelType.ToLower() switch
        {
            "wiener" => CreateWienerModel(config),
            "exponential" => CreateExponentialModel(config),
            "markov" => CreateMarkovModel(config),
            "physicsinformed" => CreatePhysicsInformedModel(config),
            _ => throw new NotSupportedException($"Degradation model type '{config.ModelType}' is not supported")
        };
    }

    private IDegradationModel CreateWienerModel(DegradationModelConfiguration config)
    {
        var drift = config.Parameters.GetValueOrDefault("drift", 0.001);
        var volatility = config.Parameters.GetValueOrDefault("volatility", 0.05);

        return new WienerProcessModel
        {
            Drift = drift,
            Volatility = volatility
        };
    }

    private IDegradationModel CreateExponentialModel(DegradationModelConfiguration config)
    {
        var lambda = config.Parameters.GetValueOrDefault("lambda", 0.001);

        return new ExponentialDegradationModel
        {
            Lambda = lambda
        };
    }

    private IDegradationModel CreateMarkovModel(DegradationModelConfiguration config)
    {
        var timeScale = config.Parameters.GetValueOrDefault("timeScale", 1.0);
        
        // Extract transition matrix from model-specific settings if available
        Dictionary<string, object>? parameters = null;
        if (config.ModelSpecificSettings != null && config.ModelSpecificSettings.Count > 0)
        {
            parameters = new Dictionary<string, object>(config.ModelSpecificSettings)
            {
                ["timeScale"] = timeScale
            };
        }
        else
        {
            parameters = new Dictionary<string, object> { ["timeScale"] = timeScale };
        }

        return new MarkovChainDegradationModel(parameters);
    }

    private IDegradationModel CreatePhysicsInformedModel(DegradationModelConfiguration config)
    {
        var baselineDrift = config.Parameters.GetValueOrDefault("baselineDrift", 0.0005);
        var loadFactor = config.Parameters.GetValueOrDefault("loadFactor", 1.0);

        return new PhysicsInformedModel
        {
            BaselineDrift = baselineDrift,
            LoadFactor = loadFactor
        };
    }
}
