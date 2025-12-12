using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

public sealed class SalesOrderLine : Entity<Guid>
{
    private SalesOrderLine(Guid id) : base(id) { }

    private SalesOrderLine(
        Guid id,
        Guid siteId,
        Guid salesOrderId,
        int lineNumber,
        Guid itemId,
        string itemName,
        decimal requestedQuantity,
        string unitOfMeasure,
        decimal? unitPrice,
        string currencyCode,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (salesOrderId == Guid.Empty) throw new ArgumentException("SalesOrderId cannot be empty", nameof(salesOrderId));
        if (lineNumber <= 0) throw new ArgumentOutOfRangeException(nameof(lineNumber));
        if (itemId == Guid.Empty) throw new ArgumentException("ItemId cannot be empty", nameof(itemId));
        if (string.IsNullOrWhiteSpace(itemName)) throw new ArgumentException("Item name is required", nameof(itemName));
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("Unit of measure is required", nameof(unitOfMeasure));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        SalesOrderId = salesOrderId;
        LineNumber = lineNumber;
        ItemId = itemId;
        ItemName = itemName.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
        RequestedQuantity = requestedQuantity;
        AllocatedQuantity = 0;
        ShippedQuantity = 0;
        UnitPrice = unitPrice;
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "USD" : currencyCode.Trim().ToUpperInvariant();

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public int LineNumber { get; private set; }

    public Guid ItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;

    public decimal RequestedQuantity { get; private set; }
    public decimal AllocatedQuantity { get; private set; }
    public decimal ShippedQuantity { get; private set; }

    public decimal? UnitPrice { get; private set; }
    public string CurrencyCode { get; private set; } = "USD";

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    public static SalesOrderLine Create(
        Guid siteId,
        Guid salesOrderId,
        int lineNumber,
        Guid itemId,
        string itemName,
        decimal requestedQuantity,
        string unitOfMeasure,
        decimal? unitPrice,
        string currencyCode,
        Guid createdByUserId)
    {
        return new SalesOrderLine(
            Guid.NewGuid(),
            siteId,
            salesOrderId,
            lineNumber,
            itemId,
            itemName,
            requestedQuantity,
            unitOfMeasure,
            unitPrice,
            currencyCode,
            createdByUserId);
    }

    public void AddAllocation(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (AllocatedQuantity + quantity > RequestedQuantity)
            throw new InvalidOperationException("Allocation exceeds requested quantity.");

        AllocatedQuantity += quantity;
    }

    public void RemoveAllocation(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        AllocatedQuantity = Math.Max(0, AllocatedQuantity - quantity);
    }

    public void AddShipment(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        ShippedQuantity += quantity;
    }
}

