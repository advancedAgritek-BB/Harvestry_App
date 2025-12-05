using Harvestry.Genetics.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Propagation override request - approval workflow for exceeding propagation limits
/// </summary>
public sealed class PropagationOverrideRequest : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private PropagationOverrideRequest(Guid id) : base(id) { }

    private PropagationOverrideRequest(
        Guid id,
        Guid siteId,
        Guid requestedByUserId,
        int requestedQuantity,
        string reason,
        Guid? motherPlantId = null,
        Guid? batchId = null) : base(id)
    {
        ValidateConstructorArgs(siteId, requestedByUserId, requestedQuantity, reason);

        SiteId = siteId;
        RequestedByUserId = requestedByUserId;
        MotherPlantId = motherPlantId;
        BatchId = batchId;
        RequestedQuantity = requestedQuantity;
        Reason = reason.Trim();
        Status = PropagationOverrideStatus.Pending;
        RequestedOn = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? MotherPlantId { get; private set; }
    public Guid? BatchId { get; private set; }
    public int RequestedQuantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public PropagationOverrideStatus Status { get; private set; }
    public DateTime RequestedOn { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ResolvedOn { get; private set; }
    public string? DecisionNotes { get; private set; }

    /// <summary>
    /// Factory method to create new override request
    /// </summary>
    public static PropagationOverrideRequest Create(
        Guid siteId,
        Guid requestedByUserId,
        int requestedQuantity,
        string reason,
        Guid? motherPlantId = null,
        Guid? batchId = null)
    {
        return new PropagationOverrideRequest(
            Guid.NewGuid(),
            siteId,
            requestedByUserId,
            requestedQuantity,
            reason,
            motherPlantId,
            batchId);
    }

    public static PropagationOverrideRequest FromPersistence(
        Guid id,
        Guid siteId,
        Guid requestedByUserId,
        int requestedQuantity,
        string reason,
        Guid? motherPlantId,
        Guid? batchId,
        PropagationOverrideStatus status,
        DateTime requestedOn,
        Guid? approvedByUserId,
        DateTime? resolvedOn,
        string? decisionNotes)
    {
        var entity = new PropagationOverrideRequest(id)
        {
            SiteId = siteId,
            RequestedByUserId = requestedByUserId,
            MotherPlantId = motherPlantId,
            BatchId = batchId,
            RequestedQuantity = requestedQuantity,
            Reason = reason,
            Status = status,
            RequestedOn = requestedOn,
            ApprovedByUserId = approvedByUserId,
            ResolvedOn = resolvedOn,
            DecisionNotes = decisionNotes
        };

        return entity;
    }

    /// <summary>
    /// Approve the override request
    /// </summary>
    public void Approve(Guid approvedByUserId, string? notes = null)
    {
        if (approvedByUserId == Guid.Empty)
            throw new ArgumentException("Approved by user ID cannot be empty", nameof(approvedByUserId));

        if (approvedByUserId == RequestedByUserId)
            throw new InvalidOperationException("User cannot approve their own override request");

        if (Status != PropagationOverrideStatus.Pending)
            throw new InvalidOperationException($"Cannot approve request with status: {Status}");

        Status = PropagationOverrideStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        DecisionNotes = notes?.Trim();
        ResolvedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Reject the override request
    /// </summary>
    public void Reject(Guid rejectedByUserId, string? notes = null)
    {
        if (rejectedByUserId == Guid.Empty)
            throw new ArgumentException("Rejected by user ID cannot be empty", nameof(rejectedByUserId));

        if (Status != PropagationOverrideStatus.Pending)
            throw new InvalidOperationException($"Cannot reject request with status: {Status}");

        Status = PropagationOverrideStatus.Rejected;
        ApprovedByUserId = rejectedByUserId;
        DecisionNotes = notes?.Trim();
        ResolvedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Expire the override request (approval timeout)
    /// </summary>
    public void Expire()
    {
        if (Status != PropagationOverrideStatus.Pending)
            throw new InvalidOperationException($"Cannot expire request with status: {Status}");

        Status = PropagationOverrideStatus.Expired;
        ResolvedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if request has expired (24 hours default timeout)
    /// </summary>
    public bool IsExpired(TimeSpan timeout)
    {
        return Status == PropagationOverrideStatus.Pending
            && DateTime.UtcNow - RequestedOn > timeout;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid requestedByUserId,
        int requestedQuantity,
        string reason)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (requestedByUserId == Guid.Empty)
            throw new ArgumentException("Requested by user ID cannot be empty", nameof(requestedByUserId));

        if (requestedQuantity < 1)
            throw new ArgumentException("Requested quantity must be at least 1", nameof(requestedQuantity));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        if (reason.Length > 1000)
            throw new ArgumentException("Reason cannot exceed 1000 characters", nameof(reason));
    }
}

