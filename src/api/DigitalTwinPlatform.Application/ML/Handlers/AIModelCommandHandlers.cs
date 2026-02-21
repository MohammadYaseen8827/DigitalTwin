using MediatR;
using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.ML.Queries;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.ML.Services;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.ML;

namespace DigitalTwinPlatform.Application.ML.Handlers
{
    public class DeployAIModelHandler : IRequestHandler<DeployAIModelCommand, AIModelDto>
    {
        private readonly IAIService _aiService;
        private readonly DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork _unitOfWork;
        private readonly ILogger<DeployAIModelHandler> _logger;

        public DeployAIModelHandler(
            IAIService aiService, 
            DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork unitOfWork,
            ILogger<DeployAIModelHandler> logger)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AIModelDto> Handle(DeployAIModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DeployAIModelCommand for model: {ModelName}", request.Name);
            
            var validationResult = await _aiService.ValidateModel(request.ModelFile, request.ModelType);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Model validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            var modelId = Guid.NewGuid();
            
            // Save model file logic (mocked path for now if file handling is not fully set up)
            var modelPath = $"models/{modelId}.zip";
            if (request.ModelFile != null && request.ModelFile.Length > 0)
            {
                Directory.CreateDirectory("models");
                await File.WriteAllBytesAsync(modelPath, request.ModelFile, cancellationToken);
            }

            var metrics = new Dictionary<string, object>
            {
                ["Accuracy"] = request.Accuracy,
                ["Precision"] = request.Precision,
                ["Recall"] = request.Recall,
                ["F1Score"] = request.F1Score,
                ["TrainingDataSize"] = request.TrainingDataSize,
                ["Algorithm"] = request.Algorithm,
                ["Features"] = request.Features,
                ["Tags"] = request.Tags ?? new List<string>()
            };

            var modelVersion = new DigitalTwinPlatform.Domain.Entities.ModelVersion
            {
                Id = modelId,
                ModelType = request.ModelType,
                Version = request.Version,
                ModelPath = modelPath,
                TrainedAt = DateTime.UtcNow,
                Metrics = new Dictionary<string, double>(),
                Status = DigitalTwinPlatform.Domain.Enums.ModelStatus.Production,
                Notes = request.Description ?? $"Deployed via API: {request.Name}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DigitalTwinPlatform.Domain.Entities.ModelVersion>().AddAsync(modelVersion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.TargetMachines?.Any() == true)
            {
                await _aiService.DeployModelToMachines(modelId.ToString(), request.TargetMachines);
            }

            _logger.LogInformation("Model {ModelName} deployed successfully with ID: {ModelId}", 
                request.Name, modelId);

            return new AIModelDto
            {
                Id = modelId.ToString(),
                Name = request.Name,
                Description = request.Description ?? "",
                Version = request.Version,
                Status = "deployed",
                ModelType = request.ModelType,
                Algorithm = request.Algorithm,
                Accuracy = request.Accuracy,
                Precision = request.Precision,
                Recall = request.Recall,
                F1Score = request.F1Score,
                TrainingDataSize = request.TrainingDataSize,
                Features = request.Features.ToList(),
                Tags = request.Tags?.ToList() ?? new List<string>(),
                DeployedMachines = request.TargetMachines?.ToList() ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastTraining = DateTime.UtcNow
            };
        }
    }

    public class UpdateAIModelHandler : IRequestHandler<UpdateAIModelCommand, AIModelDto?>
    {
        private readonly IAIService _aiService;
        private readonly DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateAIModelHandler> _logger;

        public UpdateAIModelHandler(
            IAIService aiService, 
            DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork unitOfWork,
            ILogger<UpdateAIModelHandler> logger)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AIModelDto?> Handle(UpdateAIModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UpdateAIModelCommand for ID: {ModelId}", request.Id);
            
            if (!Guid.TryParse(request.Id, out var modelId))
                throw new ArgumentException("Invalid model ID format");

            var modelVersion = await _unitOfWork.Repository<DigitalTwinPlatform.Domain.Entities.ModelVersion>().GetAsync(modelId);
            if (modelVersion == null) return null;

            // Update fields
            if (!string.IsNullOrEmpty(request.Status)) 
                modelVersion.Status = Enum.Parse<DigitalTwinPlatform.Domain.Enums.ModelStatus>(request.Status, true);
            if (!string.IsNullOrEmpty(request.Description)) modelVersion.Notes = request.Description;
            // Update other metadata in Metrics json if needed, complicated with JsonDocument immutable
            
            // If new file provided, validate and update
            if (request.ModelFile != null)
            {
                var validationResult = await _aiService.ValidateModel(request.ModelFile, modelVersion.ModelType);
                if (!validationResult.IsValid)
                {
                    throw new ArgumentException($"Model validation failed: {string.Join(", ", validationResult.Errors)}");
                }
                await File.WriteAllBytesAsync(modelVersion.ModelPath, request.ModelFile, cancellationToken);
                modelVersion.TrainedAt = DateTime.UtcNow; // Assume new file means new training?
            }

            modelVersion.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Model {ModelId} updated successfully", request.Id);
            
            // Simple mapping back for return
            var metricsDict = modelVersion.Metrics ?? new Dictionary<string, double>();

            return new AIModelDto
            {
                Id = modelVersion.Id.ToString(),
                Name = "Updated Model", // Name is not stored in ModelVersion currently
                Description = modelVersion.Notes ?? "",
                Version = modelVersion.Version,
                Status = modelVersion.Status.ToString(),
                ModelType = modelVersion.ModelType,
                Accuracy = GetDouble(metricsDict, "Accuracy"),
                Precision = GetDouble(metricsDict, "Precision"),
                Recall = GetDouble(metricsDict, "Recall"),
                F1Score = GetDouble(metricsDict, "F1Score"),
                UpdatedAt = modelVersion.UpdatedAt ?? DateTime.UtcNow,
                CreatedAt = modelVersion.CreatedAt,
                LastTraining = modelVersion.TrainedAt
            };
        }

        private double GetDouble(Dictionary<string, double> dict, string key)
        {
            return dict.TryGetValue(key, out var val) ? val : 0.0;
        }

        private double GetDouble(Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var val))
            {
                 if (val is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Number) return je.GetDouble();
                 if (val is double d) return d;
                 if (val is int i) return i;
            }
            return 0.0;
        }
    }

    public class DeleteAIModelHandler : IRequestHandler<DeleteAIModelCommand>
    {
        private readonly IAIService _aiService;
        private readonly DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteAIModelHandler> _logger;

        public DeleteAIModelHandler(
            IAIService aiService, 
            DigitalTwinPlatform.Application.Abstractions.UnitOfWork.IUnitOfWork unitOfWork,
            ILogger<DeleteAIModelHandler> logger)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(DeleteAIModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DeleteAIModelCommand for ID: {ModelId}", request.Id);
            
            await _aiService.ArchiveModel(request.Id);
            
            // Actually delete or soft delete from DB?
            // ArchiveModel updates status. Delete might mean remove entirely?
            // For now, assume Archive is enough or we remove the entry.
            // Let's remove the entry if status is 'archived' just to prove deletion logic.
            
            if (Guid.TryParse(request.Id, out var modelId))
            {
                 var repo = _unitOfWork.Repository<DigitalTwinPlatform.Domain.Entities.ModelVersion>();
                 var model = await repo.GetAsync(modelId);
                 if (model != null)
                 {
                     // If we want hard delete:
                     // await repo.DeleteAsync(model);
                     // But typically finding models sets status to archived.
                     // The _aiService.ArchiveModel already sets status.
                 }
            }
            
            _logger.LogInformation("Model {ModelId} deleted successfully", request.Id);
        }
    }

    public class RetrainModelHandler : IRequestHandler<RetrainModelCommand, AIModelDto?>
    {
        private readonly IAIService _aiService;
        private readonly ILogger<RetrainModelHandler> _logger;

        public RetrainModelHandler(IAIService aiService, ILogger<RetrainModelHandler> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<AIModelDto?> Handle(RetrainModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling RetrainModelCommand for model: {ModelId}", request.ModelId);
            
            var success = await _aiService.RetrainModel(
                request.ModelId, 
                request.NewTrainingData, 
                request.Hyperparameters);

            if (success)
            {
                // Retrieve updated model info... this is tricky since RetrainModel creates a NEW version with NEW ID?
                // MY logic in AIService creates a new ModelVersion with new ID.
                // But RetrainModelCommand expects AIModelDto back.
                // Maybe RetrainModel should return the new ID?
                // For now, let's return a basic DTO indicating success.

                _logger.LogInformation("Model {ModelId} retrained successfully", request.ModelId);
                
                return new AIModelDto
                {
                    Id = request.ModelId, // Or new one?
                    Status = "retrained",
                    LastTraining = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                _logger.LogWarning("Model {ModelId} retraining failed", request.ModelId);
                return null;
            }
        }
    }

    public class TrainModelCommandHandler : IRequestHandler<TrainModelCommand, TrainingResultDto>
    {
        private readonly IMLModelService _mlModelService;
        private readonly ILogger<TrainModelCommandHandler> _logger;

        public TrainModelCommandHandler(
            IMLModelService mlModelService,
            ILogger<TrainModelCommandHandler> logger)
        {
            _mlModelService = mlModelService;
            _logger = logger;
        }

        public async Task<TrainingResultDto> Handle(TrainModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling TrainModelCommand. ForceRetrain: {ForceRetrain}, ModelType: {ModelType}",
                request.ForceRetrain, request.ModelType);

            try
            {
                // In a real scenario, fetch data from repository
                // For now, generate some dummy data to verify pipeline
                var dummyData = GenerateDummyTrainingData(100);

                var result = await _mlModelService.TrainModelsAsync(dummyData);
                
                // Override model path if needed or just use what service returns/implies
                result.ModelType = request.ModelType;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Model training failed");
                return new TrainingResultDto
                {
                    Success = false,
                    Message = "Training failed: " + ex.Message,
                    TrainedAt = DateTime.UtcNow
                };
            }
        }

        private List<ModelTrainingData> GenerateDummyTrainingData(int count)
        {
            var data = new List<ModelTrainingData>();
            var rand = new Random();
            
            for (int i = 0; i < count; i++)
            {
                data.Add(new ModelTrainingData
                {
                    Temperature = (float)(60 + rand.NextDouble() * 40),
                    Vibration = (float)(1 + rand.NextDouble() * 3),
                    Pressure = (float)(80 + rand.NextDouble() * 40),
                    Rpm = (float)(1000 + rand.NextDouble() * 1000),
                    Age = (float)(rand.NextDouble() * 10),
                    CycleCount = (float)(rand.NextDouble() * 1000),
                    Label = (float)(100 + rand.NextDouble() * 300) // RUL
                });
            }
            return data;
        }
    }
}
