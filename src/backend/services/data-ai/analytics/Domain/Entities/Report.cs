using Harvestry.Analytics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Analytics.Domain.Entities;

public sealed class Report : AggregateRoot<Guid>
{
    private Report(Guid id) : base(id) { }

    private Report(
        Guid id,
        string name,
        string? description,
        ReportConfig config,
        string? visualizationConfigJson,
        bool isPublic,
        Guid ownerId
    ) : base(id)
    {
        Name = name;
        Description = description;
        Config = config;
        VisualizationConfigJson = visualizationConfigJson ?? "{}";
        IsPublic = isPublic;
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedByUserId = ownerId;
        UpdatedByUserId = ownerId;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ReportConfig Config { get; private set; } = null!;
    public string VisualizationConfigJson { get; private set; } = "{}";
    public bool IsPublic { get; private set; }
    public Guid OwnerId { get; private set; }
    
    // Audit
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public static Report Create(
        string name,
        string? description,
        ReportConfig config,
        Guid ownerId,
        bool isPublic = false,
        string? visualizationConfigJson = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        return new Report(
            Guid.NewGuid(),
            name,
            description,
            config,
            visualizationConfigJson,
            isPublic,
            ownerId
        );
    }
    
    public void Update(string name, string? description, ReportConfig config, string? visualizationConfigJson, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
            
        Name = name;
        Description = description;
        Config = config;
        if (visualizationConfigJson != null) 
            VisualizationConfigJson = visualizationConfigJson;
            
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic, Guid userId)
    {
        IsPublic = isPublic;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Report FromPersistence(
        Guid id,
        string name,
        string? description,
        ReportConfig config,
        string? visualizationConfigJson,
        bool isPublic,
        Guid ownerId)
    {
        return new Report(id, name, description, config, visualizationConfigJson, isPublic, ownerId);
    }
}
