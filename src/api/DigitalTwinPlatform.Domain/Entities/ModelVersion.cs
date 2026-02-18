using DigitalTwinPlatform.Domain.Enums;
using System.Text.Json;

namespace DigitalTwinPlatform.Domain.Entities;

public class ModelVersion
{
    public Guid Id { get; set; }
    public string ModelType { get; set; } = string.Empty; // "RUL", "Health"
    public string Version { get; set; } = string.Empty;
    public string ModelPath { get; set; } = string.Empty;
    public DateTime TrainedAt { get; set; }
    public Dictionary<string, double> Metrics { get; set; } = new();
    public string? TrainingDatasetHash { get; set; }
    public ModelStatus Status { get; set; } = ModelStatus.Draft;
    public DateTime? PromotedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties for performance tracking
    public List<Prediction> Predictions { get; set; } = new();
}
