namespace DigitalTwinPlatform.Domain.Entities;

public class SavedSearch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<string> EntityTypes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsed { get; set; }
}
