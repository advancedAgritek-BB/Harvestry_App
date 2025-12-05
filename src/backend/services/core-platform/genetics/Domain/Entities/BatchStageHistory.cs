using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch stage history - audit trail for stage changes
/// Immutable record of all stage transitions
/// </summary>
public sealed class BatchStageHistory : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchStageHistory(Guid id) : base(id) { }

    private BatchStageHistory(
        Guid id,
        Guid batchId,
        Guid? fromStageId,
        Guid toStageId,
        Guid changedByUserId,
        DateTime changedAt,
        string? notes = null) : base(id)
    {
        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch ID cannot be empty", nameof(batchId));

        if (toStageId == Guid.Empty)
            throw new ArgumentException("To stage ID cannot be empty", nameof(toStageId));

        if (changedByUserId == Guid.Empty)
            throw new ArgumentException("Changed by user ID cannot be empty", nameof(changedByUserId));

        BatchId = batchId;
        FromStageId = fromStageId;
        ToStageId = toStageId;
        ChangedByUserId = changedByUserId;
        ChangedAt = changedAt;
        Notes = notes?.Trim();
    }

    public Guid BatchId { get; private set; }
    public Guid? FromStageId { get; private set; }  // Null for initial stage
    public Guid ToStageId { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>
    /// Factory method to create new history entry
    /// </summary>
    public static BatchStageHistory Create(
        Guid batchId,
        Guid? fromStageId,
        Guid toStageId,
        Guid changedByUserId,
        DateTime changedAt,
        string? notes = null)
    {
        return new BatchStageHistory(
            Guid.NewGuid(),
            batchId,
            fromStageId,
            toStageId,
            changedByUserId,
            changedAt,
            notes);
    }

    /// <summary>
    /// Factory method to rehydrate batch stage history from persistence
    /// </summary>
    public static BatchStageHistory FromPersistence(
        Guid id,
        Guid batchId,
        Guid? fromStageId,
        Guid toStageId,
        Guid changedByUserId,
        DateTime changedAt,
        string? notes)
    {
        var history = new BatchStageHistory(id)
        {
            BatchId = batchId,
            FromStageId = fromStageId,
            ToStageId = toStageId,
            ChangedByUserId = changedByUserId,
            ChangedAt = changedAt,
            Notes = notes
        };
        return history;
    }
}

