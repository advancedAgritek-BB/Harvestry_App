using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Analytics.Domain.Entities;

public sealed class Share : Entity<Guid>
{
    private Share(Guid id) : base(id) { }
    
    public Share(Guid id, string resourceType, Guid resourceId, Guid sharedWithId, string sharedWithType, string permissionLevel, Guid createdBy) : base(id)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
        SharedWithId = sharedWithId;
        SharedWithType = sharedWithType;
        PermissionLevel = permissionLevel;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public string ResourceType { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid SharedWithId { get; private set; }
    public string SharedWithType { get; private set; }
    public string PermissionLevel { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    public static Share Create(string resourceType, Guid resourceId, Guid sharedWithId, string sharedWithType, string permissionLevel, Guid createdBy)
    {
        return new Share(Guid.NewGuid(), resourceType, resourceId, sharedWithId, sharedWithType, permissionLevel, createdBy);
    }
    
    public static Share FromPersistence(Guid id, string resourceType, Guid resourceId, Guid sharedWithId, string sharedWithType, string permissionLevel, Guid createdBy, DateTime createdAt)
    {
        var share = new Share(id)
        {
            ResourceType = resourceType,
            ResourceId = resourceId,
            SharedWithId = sharedWithId,
            SharedWithType = sharedWithType,
            PermissionLevel = permissionLevel,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
        return share;
    }
}





