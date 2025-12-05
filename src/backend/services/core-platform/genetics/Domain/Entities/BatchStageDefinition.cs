using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch stage definition - site-configurable lifecycle stages
/// </summary>
public sealed class BatchStageDefinition : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchStageDefinition(Guid id) : base(id) { }

    private BatchStageDefinition(
        Guid id,
        Guid siteId,
        StageKey stageKey,
        string displayName,
        int sequenceOrder,
        Guid createdByUserId,
        string? description = null,
        bool isTerminal = false,
        bool requiresHarvestMetrics = false) : base(id)
    {
        ValidateConstructorArgs(siteId, stageKey, displayName, sequenceOrder, createdByUserId);

        SiteId = siteId;
        StageKey = stageKey;
        DisplayName = displayName.Trim();
        Description = description?.Trim();
        SequenceOrder = sequenceOrder;
        IsTerminal = isTerminal;
        RequiresHarvestMetrics = requiresHarvestMetrics;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public StageKey StageKey { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int SequenceOrder { get; private set; }
    public bool IsTerminal { get; private set; }
    public bool RequiresHarvestMetrics { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new stage definition
    /// </summary>
    public static BatchStageDefinition Create(
        Guid siteId,
        StageKey stageKey,
        string displayName,
        int sequenceOrder,
        Guid createdByUserId,
        string? description = null,
        bool isTerminal = false,
        bool requiresHarvestMetrics = false)
    {
        return new BatchStageDefinition(
            Guid.NewGuid(),
            siteId,
            stageKey,
            displayName,
            sequenceOrder,
            createdByUserId,
            description,
            isTerminal,
            requiresHarvestMetrics);
    }

    /// <summary>
    /// Update stage definition
    /// </summary>
    public void Update(
        string displayName,
        string? description,
        int sequenceOrder,
        bool isTerminal,
        bool requiresHarvestMetrics,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters", nameof(displayName));

        if (sequenceOrder < 0)
            throw new ArgumentException("Sequence order cannot be negative", nameof(sequenceOrder));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        DisplayName = displayName.Trim();
        Description = description?.Trim();
        SequenceOrder = sequenceOrder;
        IsTerminal = isTerminal;
        RequiresHarvestMetrics = requiresHarvestMetrics;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update sequence order (for reordering)
    /// </summary>
    public void UpdateSequenceOrder(int newOrder, Guid updatedByUserId)
    {
        if (newOrder < 0)
            throw new ArgumentException("Sequence order cannot be negative", nameof(newOrder));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        SequenceOrder = newOrder;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to rehydrate batch stage definition from persistence
    /// </summary>
    public static BatchStageDefinition FromPersistence(
        Guid id,
        Guid siteId,
        StageKey stageKey,
        string displayName,
        string? description,
        int sequenceOrder,
        bool isTerminal,
        bool requiresHarvestMetrics,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var stage = new BatchStageDefinition(id)
        {
            SiteId = siteId,
            StageKey = stageKey,
            DisplayName = displayName,
            Description = description,
            SequenceOrder = sequenceOrder,
            IsTerminal = isTerminal,
            RequiresHarvestMetrics = requiresHarvestMetrics,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };
        return stage;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        StageKey stageKey,
        string displayName,
        int sequenceOrder,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (stageKey == null)
            throw new ArgumentNullException(nameof(stageKey));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty", nameof(displayName));

        if (displayName.Length > 100)
            throw new ArgumentException("Display name cannot exceed 100 characters", nameof(displayName));

        if (sequenceOrder < 0)
            throw new ArgumentException("Sequence order cannot be negative", nameof(sequenceOrder));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

