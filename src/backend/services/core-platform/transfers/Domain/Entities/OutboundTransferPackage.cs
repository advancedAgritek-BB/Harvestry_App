using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class OutboundTransferPackage : Entity<Guid>
{
    private OutboundTransferPackage(Guid id) : base(id) { }

    private OutboundTransferPackage(
        Guid id,
        Guid siteId,
        Guid outboundTransferId,
        Guid packageId,
        string? packageLabel,
        decimal quantity,
        string unitOfMeasure) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (outboundTransferId == Guid.Empty) throw new ArgumentException("OutboundTransferId cannot be empty", nameof(outboundTransferId));
        if (packageId == Guid.Empty) throw new ArgumentException("PackageId cannot be empty", nameof(packageId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("UnitOfMeasure is required", nameof(unitOfMeasure));

        SiteId = siteId;
        OutboundTransferId = outboundTransferId;
        PackageId = packageId;
        PackageLabel = packageLabel?.Trim().ToUpperInvariant();
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure.Trim();
    }

    public Guid SiteId { get; private set; }
    public Guid OutboundTransferId { get; private set; }

    public Guid PackageId { get; private set; }
    public string? PackageLabel { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;

    public static OutboundTransferPackage Create(
        Guid siteId,
        Guid outboundTransferId,
        Guid packageId,
        string? packageLabel,
        decimal quantity,
        string unitOfMeasure)
    {
        return new OutboundTransferPackage(
            Guid.NewGuid(),
            siteId,
            outboundTransferId,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure);
    }
}

