using Harvestry.Analytics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Analytics.Domain.Entities;

public sealed class Dashboard : AggregateRoot<Guid>
{
    private Dashboard(Guid id) : base(id) { }

    private Dashboard(
        Guid id,
        string name,
        string? description,
        List<DashboardWidget> layoutConfig,
        bool isPublic,
        Guid ownerId
    ) : base(id)
    {
        Name = name;
        Description = description;
        LayoutConfig = layoutConfig ?? new();
        IsPublic = isPublic;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = ownerId;
        UpdatedByUserId = ownerId;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public List<DashboardWidget> LayoutConfig { get; private set; } = new();
    public bool IsPublic { get; private set; }
    public Guid OwnerId { get; private set; }
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public static Dashboard Create(
        string name,
        string? description,
        List<DashboardWidget> layoutConfig,
        Guid ownerId,
        bool isPublic = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new Dashboard(
            Guid.NewGuid(),
            name,
            description,
            layoutConfig,
            isPublic,
            ownerId
        );
    }
    
    public void Update(string name, string? description, List<DashboardWidget> layoutConfig, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
            
        Name = name;
        Description = description;
        LayoutConfig = layoutConfig ?? new();
            
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic, Guid userId)
    {
        IsPublic = isPublic;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Dashboard FromPersistence(
        Guid id,
        string name,
        string? description,
        List<DashboardWidget> layoutConfig,
        bool isPublic,
        Guid ownerId)
    {
        return new Dashboard(id, name, description, layoutConfig, isPublic, ownerId);
    }
}




