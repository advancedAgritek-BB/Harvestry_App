using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Phenotype entity - specific expression/variation of genetics
/// </summary>
public sealed class Phenotype : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private Phenotype(Guid id) : base(id) { }

    private Phenotype(
        Guid id,
        Guid siteId,
        Guid geneticsId,
        string name,
        string description,
        Guid createdByUserId,
        string? expressionNotes = null,
        Dictionary<string, object>? visualCharacteristics = null,
        Dictionary<string, object>? aromaProfile = null,
        Dictionary<string, object>? growthPattern = null) : base(id)
    {
        ValidateConstructorArgs(siteId, geneticsId, name, description, createdByUserId);

        SiteId = siteId;
        GeneticsId = geneticsId;
        Name = name.Trim();
        Description = description.Trim();
        ExpressionNotes = expressionNotes?.Trim();
        VisualCharacteristics = visualCharacteristics ?? new Dictionary<string, object>();
        AromaProfile = aromaProfile ?? new Dictionary<string, object>();
        GrowthPattern = growthPattern ?? new Dictionary<string, object>();
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid GeneticsId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? ExpressionNotes { get; private set; }
    public Dictionary<string, object> VisualCharacteristics { get; private set; } = new();
    public Dictionary<string, object> AromaProfile { get; private set; } = new();
    public Dictionary<string, object> GrowthPattern { get; private set; } = new();
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new phenotype
    /// </summary>
    public static Phenotype Create(
        Guid siteId,
        Guid geneticsId,
        string name,
        string description,
        Guid createdByUserId,
        string? expressionNotes = null,
        Dictionary<string, object>? visualCharacteristics = null,
        Dictionary<string, object>? aromaProfile = null,
        Dictionary<string, object>? growthPattern = null)
    {
        return new Phenotype(
            Guid.NewGuid(),
            siteId,
            geneticsId,
            name,
            description,
            createdByUserId,
            expressionNotes,
            visualCharacteristics,
            aromaProfile,
            growthPattern);
    }

    /// <summary>
    /// Factory method to rehydrate phenotype from persistence
    /// </summary>
    public static Phenotype FromPersistence(
        Guid id,
        Guid siteId,
        Guid geneticsId,
        string name,
        string description,
        string? expressionNotes,
        Dictionary<string, object> visualCharacteristics,
        Dictionary<string, object> aromaProfile,
        Dictionary<string, object> growthPattern,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var phenotype = new Phenotype(id)
        {
            SiteId = siteId,
            GeneticsId = geneticsId,
            Name = name,
            Description = description,
            ExpressionNotes = expressionNotes,
            VisualCharacteristics = visualCharacteristics,
            AromaProfile = aromaProfile,
            GrowthPattern = growthPattern,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return phenotype;
    }

    /// <summary>
    /// Update phenotype details
    /// </summary>
    public void Update(
        string description,
        string? expressionNotes,
        Dictionary<string, object>? visualCharacteristics,
        Dictionary<string, object>? aromaProfile,
        Dictionary<string, object>? growthPattern,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Description = description.Trim();
        ExpressionNotes = expressionNotes?.Trim();
        VisualCharacteristics = visualCharacteristics ?? new Dictionary<string, object>();
        AromaProfile = aromaProfile ?? new Dictionary<string, object>();
        GrowthPattern = growthPattern ?? new Dictionary<string, object>();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid geneticsId,
        string name,
        string description,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (geneticsId == Guid.Empty)
            throw new ArgumentException("Genetics ID cannot be empty", nameof(geneticsId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (description.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

