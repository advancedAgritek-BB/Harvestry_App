using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

public sealed class SalesAllocation : Entity<Guid>
{
    private SalesAllocation(Guid id) : base(id) { }

    private SalesAllocation(
        Guid id,
        Guid siteId,
        Guid salesOrderId,
        Guid salesOrderLineId,
        Guid packageId,
        string? packageLabel,
        decimal allocatedQuantity,
        string unitOfMeasure,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (salesOrderId == Guid.Empty) throw new ArgumentException("SalesOrderId cannot be empty", nameof(salesOrderId));
        if (salesOrderLineId == Guid.Empty) throw new ArgumentException("SalesOrderLineId cannot be empty", nameof(salesOrderLineId));
        if (packageId == Guid.Empty) throw new ArgumentException("PackageId cannot be empty", nameof(packageId));
        if (allocatedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(allocatedQuantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("UnitOfMeasure is required", nameof(unitOfMeasure));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        SalesOrderId = salesOrderId;
        SalesOrderLineId = salesOrderLineId;
        PackageId = packageId;
        PackageLabel = packageLabel?.Trim().ToUpperInvariant();
        AllocatedQuantity = allocatedQuantity;
        UnitOfMeasure = unitOfMeasure.Trim();

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid SalesOrderLineId { get; private set; }

    public Guid PackageId { get; private set; }
    public string? PackageLabel { get; private set; }
    public decimal AllocatedQuantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancelReason { get; private set; }

    public bool IsCancelled => CancelledAt.HasValue;

    public static SalesAllocation Create(
        Guid siteId,
        Guid salesOrderId,
        Guid salesOrderLineId,
        Guid packageId,
        string? packageLabel,
        decimal allocatedQuantity,
        string unitOfMeasure,
        Guid createdByUserId)
    {
        return new SalesAllocation(
            Guid.NewGuid(),
            siteId,
            salesOrderId,
            salesOrderLineId,
            packageId,
            packageLabel,
            allocatedQuantity,
            unitOfMeasure,
            createdByUserId);
    }

    public void Cancel(string reason, Guid userId)
    {
        if (IsCancelled) return;
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancel reason is required", nameof(reason));
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));

        CancelReason = reason.Trim();
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }
}

