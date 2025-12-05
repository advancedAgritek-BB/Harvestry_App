using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch stage transition - defines allowed stage-to-stage flows
/// </summary>
public sealed class BatchStageTransition : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchStageTransition(Guid id) : base(id) { }

    private BatchStageTransition(
        Guid id,
        Guid siteId,
        Guid fromStageId,
        Guid toStageId,
        Guid createdByUserId,
        bool autoAdvance = false,
        bool requiresApproval = false,
        string? approvalRole = null) : base(id)
    {
        ValidateConstructorArgs(siteId, fromStageId, toStageId, createdByUserId);

        SiteId = siteId;
        FromStageId = fromStageId;
        ToStageId = toStageId;
        AutoAdvance = autoAdvance;
        RequiresApproval = requiresApproval;
        ApprovalRole = approvalRole?.Trim();
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public bool AutoAdvance { get; private set; }
    public bool RequiresApproval { get; private set; }
    public string? ApprovalRole { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new transition
    /// </summary>
    public static BatchStageTransition Create(
        Guid siteId,
        Guid fromStageId,
        Guid toStageId,
        Guid createdByUserId,
        bool autoAdvance = false,
        bool requiresApproval = false,
        string? approvalRole = null)
    {
        return new BatchStageTransition(
            Guid.NewGuid(),
            siteId,
            fromStageId,
            toStageId,
            createdByUserId,
            autoAdvance,
            requiresApproval,
            approvalRole);
    }

    /// <summary>
    /// Update transition rules
    /// </summary>
    public void Update(
        bool autoAdvance,
        bool requiresApproval,
        string? approvalRole,
        Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        AutoAdvance = autoAdvance;
        RequiresApproval = requiresApproval;
        ApprovalRole = approvalRole?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to rehydrate batch stage transition from persistence
    /// </summary>
    public static BatchStageTransition FromPersistence(
        Guid id,
        Guid siteId,
        Guid fromStageId,
        Guid toStageId,
        bool autoAdvance,
        bool requiresApproval,
        string? approvalRole,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var transition = new BatchStageTransition(id)
        {
            SiteId = siteId,
            FromStageId = fromStageId,
            ToStageId = toStageId,
            AutoAdvance = autoAdvance,
            RequiresApproval = requiresApproval,
            ApprovalRole = approvalRole,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };
        return transition;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid fromStageId,
        Guid toStageId,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (fromStageId == Guid.Empty)
            throw new ArgumentException("From stage ID cannot be empty", nameof(fromStageId));

        if (toStageId == Guid.Empty)
            throw new ArgumentException("To stage ID cannot be empty", nameof(toStageId));

        if (fromStageId == toStageId)
            throw new ArgumentException("From stage and to stage cannot be the same", nameof(toStageId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

