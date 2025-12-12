using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

public sealed class ShipmentPackage : Entity<Guid>
{
    private ShipmentPackage(Guid id) : base(id) { }

    private ShipmentPackage(
        Guid id,
        Guid siteId,
        Guid shipmentId,
        Guid packageId,
        string? packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid? salesAllocationId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (shipmentId == Guid.Empty) throw new ArgumentException("ShipmentId cannot be empty", nameof(shipmentId));
        if (packageId == Guid.Empty) throw new ArgumentException("PackageId cannot be empty", nameof(packageId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("UnitOfMeasure is required", nameof(unitOfMeasure));

        SiteId = siteId;
        ShipmentId = shipmentId;
        PackageId = packageId;
        PackageLabel = packageLabel?.Trim().ToUpperInvariant();
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure.Trim();
        SalesAllocationId = salesAllocationId;
    }

    public Guid SiteId { get; private set; }
    public Guid ShipmentId { get; private set; }
    public Guid? SalesAllocationId { get; private set; }

    public Guid PackageId { get; private set; }
    public string? PackageLabel { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;

    public DateTime? PackedAt { get; private set; }
    public Guid? PackedByUserId { get; private set; }

    public static ShipmentPackage Create(
        Guid siteId,
        Guid shipmentId,
        Guid packageId,
        string? packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid? salesAllocationId)
    {
        return new ShipmentPackage(
            Guid.NewGuid(),
            siteId,
            shipmentId,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure,
            salesAllocationId);
    }

    public void MarkPacked(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        PackedAt = DateTime.UtcNow;
        PackedByUserId = userId;
    }
}

