using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.Domain.Common;

/// <summary>
/// Base entity class providing common properties for all domain entities
/// </summary>
/// <typeparam name="TId">Type of the entity ID</typeparam>
public abstract class BaseEntity<TId> where TId : struct
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public TId Id { get; protected set; } = default!;
    
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }
    
    /// <summary>
    /// ID of the user who created the entity
    /// </summary>
    public Guid? CreatedById { get; protected set; }
    
    /// <summary>
    /// ID of the user who last modified the entity
    /// </summary>
    public Guid? UpdatedById { get; protected set; }
    
    /// <summary>
    /// Tenant ID for multi-tenancy support
    /// </summary>
    public Guid? TenantId { get; protected set; }
    
    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; protected set; }
    
    /// <summary>
    /// Timestamp when entity was deleted
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }
    
    /// <summary>
    /// ID of the user who deleted the entity
    /// </summary>
    public Guid? DeletedById { get; protected set; }
    
    /// <summary>
    /// Row version for concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; protected set; }
    
    /// <summary>
    /// Protected constructor for EF Core
    /// </summary>
    protected BaseEntity() { }
    
    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    protected BaseEntity(TId id)
    {
        Id = id;
    }
    
    /// <summary>
    /// Update the entity's modification timestamp
    /// </summary>
    /// <param name="userId">ID of the user making the update</param>
    private void UpdateTimestamp(Guid? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = userId;
    }
    
    /// <summary>
    /// Mark entity as deleted
    /// </summary>
    /// <param name="userId">ID of the user performing the deletion</param>
    public void Delete(Guid? userId = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = userId;
        UpdateTimestamp(userId);
    }
    
    /// <summary>
    /// Restore a deleted entity
    /// </summary>
    /// <param name="userId">ID of the user performing the restoration</param>
    public void Restore(Guid? userId = null)
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
        UpdateTimestamp(userId);
    }
}