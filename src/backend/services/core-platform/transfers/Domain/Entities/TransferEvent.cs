using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class TransferEvent : Entity<Guid>
{
    private TransferEvent(Guid id) : base(id) { }

    private TransferEvent(
        Guid id,
        Guid siteId,
        Guid outboundTransferId,
        string eventType,
        string? eventReason,
        Dictionary<string, object>? metadata,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (outboundTransferId == Guid.Empty) throw new ArgumentException("OutboundTransferId cannot be empty", nameof(outboundTransferId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType is required", nameof(eventType));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        OutboundTransferId = outboundTransferId;
        EventType = eventType.Trim();
        EventReason = eventReason?.Trim();
        Metadata = metadata ?? new Dictionary<string, object>();
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid OutboundTransferId { get; private set; }

    public string EventType { get; private set; } = string.Empty;
    public string? EventReason { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public static TransferEvent Create(
        Guid siteId,
        Guid outboundTransferId,
        string eventType,
        string? eventReason,
        Dictionary<string, object>? metadata,
        Guid createdByUserId)
    {
        return new TransferEvent(
            Guid.NewGuid(),
            siteId,
            outboundTransferId,
            eventType,
            eventReason,
            metadata,
            createdByUserId);
    }
}

