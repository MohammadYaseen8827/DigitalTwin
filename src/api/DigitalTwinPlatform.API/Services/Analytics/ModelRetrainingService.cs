using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Predictions.Models;
using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.API.Services.Simulation;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Analytics;

public interface IModelRetrainingService
{
    Task<bool> StartRetrainingAsync(Guid machineId);
    Task<bool> CheckDriftAndRetrainAsync(Guid machineId, IEnumerable<double> recentData, string benchmarkDataset);
}

public class ModelRetrainingService : IModelRetrainingService
{
    private readonly IRepository<Prediction> _predictionRepository;
    private readonly IDataValidationService _validationService;
    private readonly ILogger<ModelRetrainingService> _logger;

    public ModelRetrainingService(
        IRepository<Prediction> predictionRepository,
        IDataValidationService validationService,
        ILogger<ModelRetrainingService> logger)
    {
        _predictionRepository = predictionRepository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<bool> CheckDriftAndRetrainAsync(Guid machineId, IEnumerable<double> recentData, string benchmarkDataset)
    {
        _logger.LogInformation("Checking for distribution drift for machine {MachineId}...", machineId);
        
        var validationResult = _validationService.ValidateAgainstBenchmark(recentData, benchmarkDataset);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Significant drift detected for {MachineId} (MMD: {Mmd:F4}, KL: {Kl:F4}). Triggering auto-retraining...", 
                machineId, validationResult.MmdStatistic, validationResult.KlDivergence);
            
            return await StartRetrainingAsync(machineId);
        }

        _logger.LogInformation("No significant drift detected for machine {MachineId}.", machineId);
        return false;
    }

    public async Task<bool> StartRetrainingAsync(Guid machineId)
    {
        _logger.LogInformation("Initiating model retraining for machine {MachineId}...", machineId);
        
        // 1. Data Fetching
        await Task.Delay(100);
        _logger.LogInformation("Fetched historical data for {MachineId}. Training new pipeline with LightGBM...", machineId);

        // 2. Training Simulation
        await Task.Delay(300);
        var accuracy = 0.94 + (new Random().NextDouble() * 0.04); // Slightly improved expectation for LightGBM
        _logger.LogInformation("Model training completed. Validation R-squared: {Accuracy:F4}", accuracy);

        // 3. Deployment
        var newVersion = $"v2.{DateTime.UtcNow:yyyyMMdd}.{new Random().Next(100, 999)}";
        _logger.LogInformation("Deployed new model version {Version} for machine {MachineId}.", newVersion, machineId);
        
        return true;
    }
}
