using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

/// <summary>
/// Append-only audit log for compliance-related events.
/// </summary>
public sealed class ComplianceEvent : Entity<Guid>
{
    private ComplianceEvent(Guid id) : base(id) { }

    private ComplianceEvent(
        Guid id,
        Guid siteId,
        string entityType,
        Guid entityId,
        string eventType,
        string? payloadJson,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("EntityType is required", nameof(entityType));
        if (entityId == Guid.Empty) throw new ArgumentException("EntityId cannot be empty", nameof(entityId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType is required", nameof(eventType));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        EntityType = entityType;
        EntityId = entityId;
        EventType = eventType;
        PayloadJson = payloadJson;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? PayloadJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public static ComplianceEvent Create(
        Guid siteId,
        string entityType,
        Guid entityId,
        string eventType,
        string? payloadJson,
        Guid createdByUserId)
    {
        return new ComplianceEvent(
            Guid.NewGuid(),
            siteId,
            entityType,
            entityId,
            eventType,
            payloadJson,
            createdByUserId);
    }
}

/// <summary>
/// Known event types for compliance tracking.
/// </summary>
public static class ComplianceEventTypes
{
    // Customer events
    public const string CustomerCreated = "customer.created";
    public const string CustomerUpdated = "customer.updated";
    public const string LicenseVerified = "license.verified";
    public const string LicenseVerificationFailed = "license.verification_failed";

    // Order events
    public const string OrderCreated = "order.created";
    public const string OrderSubmitted = "order.submitted";
    public const string OrderAllocated = "order.allocated";
    public const string OrderCancelled = "order.cancelled";

    // Shipment events
    public const string ShipmentCreated = "shipment.created";
    public const string ShipmentPacked = "shipment.packed";
    public const string ShipmentShipped = "shipment.shipped";

    // Transfer events
    public const string TransferCreated = "transfer.created";
    public const string MetrcSubmitted = "metrc.submitted";
    public const string MetrcFailed = "metrc.failed";
    public const string TransferVoided = "transfer.voided";
}
