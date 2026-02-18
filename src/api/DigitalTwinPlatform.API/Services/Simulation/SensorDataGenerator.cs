using System.Text.Json;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Domain.Models;

namespace DigitalTwinPlatform.API.Services.Simulation;

public interface ISensorDataGenerator
{
    double GenerateReading(
        SensorMappingConfiguration sensorMapping,
        double degradationState,
        Random random);
    
    TelemetryData CreateTelemetryData(
        Guid machineId,
        SensorMappingConfiguration sensorMapping,
        double reading,
        DateTime timestamp);
}

public class SensorDataGenerator : ISensorDataGenerator
{
    private readonly ITransferFunctionEvaluator _transferFunctionEvaluator;

    public SensorDataGenerator(ITransferFunctionEvaluator transferFunctionEvaluator)
    {
        _transferFunctionEvaluator = transferFunctionEvaluator;
    }

    public double GenerateReading(
        SensorMappingConfiguration sensorMapping,
        double degradationState,
        Random random)
    {
        if (sensorMapping == null)
        {
            throw new ArgumentNullException(nameof(sensorMapping));
        }

        // Combine transfer function parameters with noise level for evaluation
        var parameters = new Dictionary<string, double>(sensorMapping.TransferFunctionParameters)
        {
            ["noiseLevel"] = sensorMapping.NoiseLevel
        };

        return _transferFunctionEvaluator.Evaluate(
            sensorMapping.TransferFunction,
            parameters,
            degradationState,
            random
        );
    }

    public TelemetryData CreateTelemetryData(
        Guid machineId,
        SensorMappingConfiguration sensorMapping,
        double reading,
        DateTime timestamp)
    {
        var dataJson = JsonSerializer.Serialize(new
        {
            value = reading,
            unit = sensorMapping.Unit
        });

        return new TelemetryData
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            DataType = sensorMapping.SensorType,
            Data = JsonDocument.Parse(dataJson),
            Timestamp = timestamp
        };
    }
}
