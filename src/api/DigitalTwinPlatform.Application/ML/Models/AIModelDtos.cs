using MediatR;

namespace DigitalTwinPlatform.Application.ML.Models
{
    public class AIModelDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // draft, training, deployed, archived
        public string ModelType { get; set; } = string.Empty; // degradation, failure, performance, quality
        public string Algorithm { get; set; } = string.Empty; // random_forest, neural_network, svm, xgboost, lstm
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public int TrainingDataSize { get; set; }
        public IEnumerable<string> Features { get; set; } = new List<string>();
        public IEnumerable<string> DeployedMachines { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastTraining { get; set; }
    }

    public class ModelPredictionDto
    {
        public double RemainingUsefulLife { get; set; }
        public double Confidence { get; set; }
        public string RiskLevel { get; set; } = string.Empty; // low, medium, high
        public Dictionary<string, object> FeatureImportance { get; set; } = new();
        public Dictionary<string, double> ShapValues { get; set; } = new(); // Local explanations
        public DateTime PredictionTime { get; set; }
    }

    public class ModelMetricsDto
    {
        public double Accuracy { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1Score { get; set; }
        public double MeanAbsoluteError { get; set; }
        public double RootMeanSquareError { get; set; }
        public Dictionary<string, double> ClassMetrics { get; set; } = new();
        public DateTime LastEvaluated { get; set; }
    }

    public class ModelInputDto
    {
        public Dictionary<string, double> Features { get; set; } = new();
    }

    public class ModelValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public ModelCompatibilityDto Compatibility { get; set; } = new();
    }

    public class ModelCompatibilityDto
    {
        public bool IsCompatible { get; set; }
        public IEnumerable<string> CompatibleMachineTypes { get; set; } = new List<string>();
        public IEnumerable<string> RequiredFeatures { get; set; } = new List<string>();
        public string Framework { get; set; } = string.Empty;
    }

    public class DeploymentStatusDto
    {
        public string Status { get; set; } = string.Empty; // pending, deploying, deployed, failed
        public int TotalMachines { get; set; }
        public int SuccessfulDeployments { get; set; }
        public int FailedDeployments { get; set; }
        public IEnumerable<MachineDeploymentStatus> MachineStatuses { get; set; } = new List<MachineDeploymentStatus>();
        public DateTime LastUpdated { get; set; }
    }

    public class MachineDeploymentStatus
    {
        public string MachineId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // success, failed, pending
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime DeployedAt { get; set; }
    }

    /// <summary>
    /// Result DTO for model training.
    /// </summary>
    public class TrainingResultDto
    {
        public bool Success { get; set; }
        public int SamplesUsed { get; set; }
        public double R2Score { get; set; }
        public double Mape { get; set; }
        public double Accuracy { get; set; }
        public string ModelPath { get; set; } = string.Empty;
        public string ModelType { get; set; } = string.Empty;
        public DateTime TrainedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, double> Metrics { get; set; } = new();
    }
}
