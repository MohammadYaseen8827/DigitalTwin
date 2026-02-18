using MediatR;
using DigitalTwinPlatform.Application.ML.Models;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.Application.ML.Queries
{
    public class GetAllAIModelsQuery : IRequest<IEnumerable<AIModelDto>>
    {
    }

    public class GetAIModelByIdQuery : IRequest<AIModelDto?>
    {
        public string Id { get; set; }

        public GetAIModelByIdQuery(string id)
        {
            Id = id;
        }
    }
}

namespace DigitalTwinPlatform.Application.ML.Commands
{
    public class DeployAIModelCommand : IRequest<AIModelDto>
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Version { get; set; } = string.Empty;
        
        [Required]
        public string ModelType { get; set; } = string.Empty;
        
        [Required]
        public string Algorithm { get; set; } = string.Empty;
        
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public int TrainingDataSize { get; set; }
        
        [Required]
        public IEnumerable<string> Features { get; set; } = new List<string>();
        
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        
        [Required]
        public byte[] ModelFile { get; set; } = [];
        
        public IEnumerable<string> TargetMachines { get; set; } = new List<string>();
    }

    public class UpdateAIModelCommand : IRequest<AIModelDto?>
    {
        public string Id { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public string? Status { get; set; }
        public double? Accuracy { get; set; }
        public double? Precision { get; set; }
        public double? Recall { get; set; }
        public double? F1Score { get; set; }
        public int? TrainingDataSize { get; set; }
        public IEnumerable<string>? Features { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public byte[]? ModelFile { get; set; }
    }

    public class DeleteAIModelCommand : IRequest
    {
        public string Id { get; set; }

        public DeleteAIModelCommand(string id)
        {
            Id = id;
        }
    }

    public class RetrainModelCommand : IRequest<AIModelDto?>
    {
        public string ModelId { get; set; } = string.Empty;
        public IEnumerable<string> NewTrainingData { get; set; } = new List<string>();
        public Dictionary<string, object>? Hyperparameters { get; set; }
    }

    public class TrainModelCommand : IRequest<TrainingResultDto>
    {
        public bool ForceRetrain { get; set; }
        public string ModelType { get; set; } = "all";
    }
}
