using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class InboundTransferReceiptLine : Entity<Guid>
{
    private InboundTransferReceiptLine(Guid id) : base(id) { }

    private InboundTransferReceiptLine(
        Guid id,
        Guid siteId,
        Guid inboundReceiptId,
        string packageLabel,
        decimal receivedQuantity,
        string unitOfMeasure,
        bool accepted,
        string? rejectionReason) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (inboundReceiptId == Guid.Empty) throw new ArgumentException("InboundReceiptId cannot be empty", nameof(inboundReceiptId));
        if (string.IsNullOrWhiteSpace(packageLabel)) throw new ArgumentException("Package label is required", nameof(packageLabel));
        if (receivedQuantity < 0) throw new ArgumentOutOfRangeException(nameof(receivedQuantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("Unit of measure is required", nameof(unitOfMeasure));

        if (!accepted && string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new InvalidOperationException("Rejection reason is required when accepted=false.");
        }

        SiteId = siteId;
        InboundReceiptId = inboundReceiptId;
        PackageLabel = packageLabel.Trim().ToUpperInvariant();
        ReceivedQuantity = receivedQuantity;
        UnitOfMeasure = unitOfMeasure.Trim();
        Accepted = accepted;
        RejectionReason = rejectionReason?.Trim();
    }

    public Guid SiteId { get; private set; }
    public Guid InboundReceiptId { get; private set; }

    public string PackageLabel { get; private set; } = string.Empty;
    public decimal ReceivedQuantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;

    public bool Accepted { get; private set; }
    public string? RejectionReason { get; private set; }

    public static InboundTransferReceiptLine Create(
        Guid siteId,
        Guid inboundReceiptId,
        string packageLabel,
        decimal receivedQuantity,
        string unitOfMeasure,
        bool accepted,
        string? rejectionReason)
    {
        return new InboundTransferReceiptLine(
            Guid.NewGuid(),
            siteId,
            inboundReceiptId,
            packageLabel,
            receivedQuantity,
            unitOfMeasure,
            accepted,
            rejectionReason);
    }
}

