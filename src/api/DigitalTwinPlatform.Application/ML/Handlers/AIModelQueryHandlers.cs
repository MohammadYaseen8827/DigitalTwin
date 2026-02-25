using MediatR;
using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Application.ML.Models;
using DigitalTwinPlatform.Application.ML.Queries;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.ML.Services;

namespace DigitalTwinPlatform.Application.ML.Handlers
{
    public class GetAllAIModelsHandler : IRequestHandler<GetAllAIModelsQuery, IEnumerable<AIModelDto>>
    {
        private readonly IAIService _aiService;
        private readonly ILogger<GetAllAIModelsHandler> _logger;

        public GetAllAIModelsHandler(IAIService aiService, ILogger<GetAllAIModelsHandler> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<IEnumerable<AIModelDto>> Handle(GetAllAIModelsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllAIModelsQuery");
            
            var models = new List<AIModelDto>
            {
                new AIModelDto
                {
                    Id = "mdl_cnc_degradation_v2",
                    Name = "CNC Degradation Predictor",
                    Description = "Predicts remaining useful life for CNC machines based on vibration and temperature data",
                    Version = "2.1.0",
                    Status = "deployed",
                    ModelType = "degradation",
                    Algorithm = "random_forest",
                    Accuracy = 0.92,
                    Precision = 0.89,
                    Recall = 0.94,
                    F1Score = 0.91,
                    TrainingDataSize = 15000,
                    Features = new List<string> { "temperature", "vibration_x", "vibration_y", "vibration_z", "current", "rpm" },
                    DeployedMachines = new List<string> { "cnc_001", "cnc_002", "cnc_003" },
                    Tags = new List<string> { "cnc", "degradation", "predictive" },
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    LastTraining = DateTime.UtcNow.AddDays(-7)
                }
            };

            return await Task.FromResult(models);
        }
    }

    public class GetAIModelByIdHandler : IRequestHandler<GetAIModelByIdQuery, AIModelDto?>
    {
        private readonly IAIService _aiService;
        private readonly ILogger<GetAIModelByIdHandler> _logger;

        public GetAIModelByIdHandler(IAIService aiService, ILogger<GetAIModelByIdHandler> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<AIModelDto?> Handle(GetAIModelByIdQuery request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            _logger.LogInformation("Handling GetAIModelByIdQuery for ID: {ModelId}", request.Id);
            
            return new AIModelDto
            {
                Id = request.Id,
                Name = "Requested Model",
                Description = "Mock model returned by ID",
                Version = "1.0.0",
                Status = "deployed",
                ModelType = "degradation",
                Algorithm = "random_forest",
                Accuracy = 0.90,
                Precision = 0.85,
                Recall = 0.88,
                F1Score = 0.86,
                TrainingDataSize = 10000,
                Features = new List<string> { "temperature", "vibration" },
                DeployedMachines = new List<string> { "machine_001" },
                Tags = new List<string> { "mock" },
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                LastTraining = DateTime.UtcNow.AddDays(-7)
            };
        }
    }
}
