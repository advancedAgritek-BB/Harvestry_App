using Harvestry.Genetics.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch relationship - tracks parent-child relationships (splits, merges, propagation)
/// </summary>
public sealed class BatchRelationship : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchRelationship(Guid id) : base(id) { }

    private BatchRelationship(
        Guid id,
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        RelationshipType relationshipType,
        int? plantCountTransferred,
        DateOnly transferDate,
        Guid createdByUserId,
        string? notes = null) : base(id)
    {
        ValidateConstructorArgs(siteId, parentBatchId, childBatchId, createdByUserId);

        SiteId = siteId;
        ParentBatchId = parentBatchId;
        ChildBatchId = childBatchId;
        RelationshipType = relationshipType;
        PlantCountTransferred = plantCountTransferred;
        TransferDate = transferDate;
        Notes = notes?.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid ParentBatchId { get; private set; }
    public Guid ChildBatchId { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public int? PlantCountTransferred { get; private set; }
    public DateOnly TransferDate { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new relationship
    /// </summary>
    public static BatchRelationship Create(
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        RelationshipType relationshipType,
        int? plantCountTransferred,
        DateOnly transferDate,
        Guid createdByUserId,
        string? notes = null)
    {
        return new BatchRelationship(
            Guid.NewGuid(),
            siteId,
            parentBatchId,
            childBatchId,
            relationshipType,
            plantCountTransferred,
            transferDate,
            createdByUserId,
            notes);
    }

    /// <summary>
    /// Create relationship for batch split
    /// </summary>
    public static BatchRelationship CreateSplit(
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        int plantCountTransferred,
        Guid userId,
        string? notes = null)
    {
        return Create(
            siteId,
            parentBatchId,
            childBatchId,
            RelationshipType.Split,
            plantCountTransferred,
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId,
            notes);
    }

    /// <summary>
    /// Create relationship for batch merge
    /// </summary>
    public static BatchRelationship CreateMerge(
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        int plantCountTransferred,
        Guid userId,
        string? notes = null)
    {
        return Create(
            siteId,
            parentBatchId,
            childBatchId,
            RelationshipType.Merge,
            plantCountTransferred,
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId,
            notes);
    }

    /// <summary>
    /// Create relationship for propagation (cloning)
    /// </summary>
    public static BatchRelationship CreatePropagation(
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        int cloneCount,
        Guid userId,
        string? notes = null)
    {
        return Create(
            siteId,
            parentBatchId,
            childBatchId,
            RelationshipType.Propagation,
            cloneCount,
            DateOnly.FromDateTime(DateTime.UtcNow),
            userId,
            notes);
    }

    /// <summary>
    /// Factory method to rehydrate batch relationship from persistence
    /// </summary>
    public static BatchRelationship FromPersistence(
        Guid id,
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        RelationshipType relationshipType,
        int? plantCountTransferred,
        DateOnly transferDate,
        string? notes,
        DateTime createdAt,
        Guid createdByUserId)
    {
        var relationship = new BatchRelationship(id)
        {
            SiteId = siteId,
            ParentBatchId = parentBatchId,
            ChildBatchId = childBatchId,
            RelationshipType = relationshipType,
            PlantCountTransferred = plantCountTransferred,
            TransferDate = transferDate,
            Notes = notes,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId
        };
        return relationship;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid parentBatchId,
        Guid childBatchId,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (parentBatchId == Guid.Empty)
            throw new ArgumentException("Parent batch ID cannot be empty", nameof(parentBatchId));

        if (childBatchId == Guid.Empty)
            throw new ArgumentException("Child batch ID cannot be empty", nameof(childBatchId));

        if (parentBatchId == childBatchId)
            throw new ArgumentException("Parent and child batch cannot be the same", nameof(childBatchId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

