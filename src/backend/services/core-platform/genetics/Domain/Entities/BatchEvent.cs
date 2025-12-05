using Harvestry.Genetics.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch event - immutable audit trail of batch activities
/// </summary>
public sealed class BatchEvent : Entity<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchEvent(Guid id) : base(id) { }

    private BatchEvent(
        Guid id,
        Guid siteId,
        Guid batchId,
        EventType eventType,
        Guid performedByUserId,
        DateTime performedAt,
        Dictionary<string, object>? eventData = null,
        string? notes = null) : base(id)
    {
        ValidateConstructorArgs(siteId, batchId, performedByUserId);

        SiteId = siteId;
        BatchId = batchId;
        EventType = eventType;
        EventData = eventData ?? new Dictionary<string, object>();
        PerformedByUserId = performedByUserId;
        PerformedAt = performedAt;
        Notes = notes?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid BatchId { get; private set; }
    public EventType EventType { get; private set; }
    public Dictionary<string, object> EventData { get; private set; } = new();
    public Guid PerformedByUserId { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create new batch event
    /// </summary>
    public static BatchEvent Create(
        Guid siteId,
        Guid batchId,
        EventType eventType,
        Guid performedByUserId,
        DateTime performedAt,
        Dictionary<string, object>? eventData = null,
        string? notes = null)
    {
        return new BatchEvent(
            Guid.NewGuid(),
            siteId,
            batchId,
            eventType,
            performedByUserId,
            performedAt,
            eventData,
            notes);
    }

    /// <summary>
    /// Create event for batch creation
    /// </summary>
    public static BatchEvent CreateBatchCreated(
        Guid siteId,
        Guid batchId,
        Guid strainId,
        int plantCount,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["strainId"] = strainId,
            ["plantCount"] = plantCount
        };

        return Create(siteId, batchId, EventType.Created, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Create event for stage change
    /// </summary>
    public static BatchEvent CreateStageChange(
        Guid siteId,
        Guid batchId,
        Guid? fromStageId,
        Guid toStageId,
        Guid userId,
        string? notes = null)
    {
        var eventData = new Dictionary<string, object>
        {
            ["toStageId"] = toStageId
        };

        if (fromStageId.HasValue)
            eventData["fromStageId"] = fromStageId.Value;

        return Create(siteId, batchId, EventType.StageChange, userId, DateTime.UtcNow, eventData, notes);
    }

    /// <summary>
    /// Create event for plant count change
    /// </summary>
    public static BatchEvent CreatePlantCountChange(
        Guid siteId,
        Guid batchId,
        int oldCount,
        int newCount,
        string reason,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["oldCount"] = oldCount,
            ["newCount"] = newCount,
            ["reason"] = reason
        };

        return Create(siteId, batchId, EventType.PlantCountChange, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Factory method to rehydrate batch event from persistence
    /// </summary>
    public static BatchEvent FromPersistence(
        Guid id,
        Guid siteId,
        Guid batchId,
        EventType eventType,
        Dictionary<string, object> eventData,
        Guid performedByUserId,
        DateTime performedAt,
        string? notes,
        DateTime createdAt)
    {
        var batchEvent = new BatchEvent(id)
        {
            SiteId = siteId,
            BatchId = batchId,
            EventType = eventType,
            EventData = eventData,
            PerformedByUserId = performedByUserId,
            PerformedAt = performedAt,
            Notes = notes,
            CreatedAt = createdAt
        };
        return batchEvent;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid batchId,
        Guid performedByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch ID cannot be empty", nameof(batchId));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty", nameof(performedByUserId));
    }
}

