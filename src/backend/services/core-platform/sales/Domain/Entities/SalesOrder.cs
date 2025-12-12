using Harvestry.Sales.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

public sealed class SalesOrder : AggregateRoot<Guid>
{
    private readonly List<SalesOrderLine> _lines = new();

    private SalesOrder(Guid id) : base(id) { }

    private SalesOrder(
        Guid id,
        Guid siteId,
        string orderNumber,
        string customerName,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (string.IsNullOrWhiteSpace(orderNumber)) throw new ArgumentException("Order number is required", nameof(orderNumber));
        if (string.IsNullOrWhiteSpace(customerName)) throw new ArgumentException("Customer name is required", nameof(customerName));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        OrderNumber = orderNumber.Trim();
        CustomerName = customerName.Trim();
        Status = SalesOrderStatus.Draft;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid? CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? DestinationLicenseNumber { get; private set; }
    public string? DestinationFacilityName { get; private set; }

    public DateOnly? RequestedShipDate { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public SalesOrderStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyList<SalesOrderLine> Lines => _lines.AsReadOnly();

    public static SalesOrder CreateDraft(
        Guid siteId,
        string orderNumber,
        string customerName,
        Guid createdByUserId)
    {
        return new SalesOrder(
            Guid.NewGuid(),
            siteId,
            orderNumber,
            customerName,
            createdByUserId);
    }

    public SalesOrderLine AddLine(
        int lineNumber,
        Guid itemId,
        string itemName,
        decimal requestedQuantity,
        string unitOfMeasure,
        decimal? unitPrice,
        string currencyCode,
        Guid userId)
    {
        EnsureNotCancelled();
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException("Lines can only be added in Draft status.");

        if (lineNumber <= 0) throw new ArgumentOutOfRangeException(nameof(lineNumber));
        if (itemId == Guid.Empty) throw new ArgumentException("ItemId cannot be empty", nameof(itemId));
        if (string.IsNullOrWhiteSpace(itemName)) throw new ArgumentException("Item name is required", nameof(itemName));
        if (requestedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(requestedQuantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("Unit of measure is required", nameof(unitOfMeasure));
        if (string.IsNullOrWhiteSpace(currencyCode)) currencyCode = "USD";

        if (_lines.Any(l => l.LineNumber == lineNumber))
        {
            throw new InvalidOperationException($"Line number {lineNumber} already exists.");
        }

        var line = SalesOrderLine.Create(
            SiteId,
            Id,
            lineNumber,
            itemId,
            itemName,
            requestedQuantity,
            unitOfMeasure,
            unitPrice,
            currencyCode,
            userId);

        _lines.Add(line);
        Touch(userId);
        return line;
    }

    public void SetDestination(string licenseNumber, string? facilityName, Guid userId)
    {
        EnsureNotCancelled();
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException("Destination can only be set in Draft status.");
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new ArgumentException("Destination license number is required", nameof(licenseNumber));

        DestinationLicenseNumber = licenseNumber.Trim();
        DestinationFacilityName = facilityName?.Trim();
        Touch(userId);
    }

    public void SetRequestedShipDate(DateOnly? date, Guid userId)
    {
        EnsureNotCancelled();
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException("Requested ship date can only be set in Draft status.");
        RequestedShipDate = date;
        Touch(userId);
    }

    public void SetNotes(string? notes, Guid userId)
    {
        EnsureNotCancelled();
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException("Notes can only be set in Draft status.");
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch(userId);
    }

    public void Submit(Guid userId)
    {
        EnsureNotCancelled();
        if (Status != SalesOrderStatus.Draft) throw new InvalidOperationException($"Cannot submit order in status {Status}.");
        if (_lines.Count == 0) throw new InvalidOperationException("Cannot submit an order without lines.");
        if (string.IsNullOrWhiteSpace(DestinationLicenseNumber))
            throw new InvalidOperationException("Destination license number must be set before submitting.");

        Status = SalesOrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        Touch(userId);
    }

    public void MarkAllocated(Guid userId)
    {
        EnsureNotCancelled();
        if (Status is not (SalesOrderStatus.Submitted or SalesOrderStatus.Allocated))
        {
            throw new InvalidOperationException($"Cannot mark allocated in status {Status}.");
        }

        Status = SalesOrderStatus.Allocated;
        Touch(userId);
    }

    public void MarkShipped(bool isPartial, Guid userId)
    {
        EnsureNotCancelled();
        Status = isPartial ? SalesOrderStatus.PartiallyShipped : SalesOrderStatus.Shipped;
        Touch(userId);
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status == SalesOrderStatus.Cancelled) return;
        if (Status == SalesOrderStatus.Shipped) throw new InvalidOperationException("Cannot cancel a shipped order.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancellation reason is required", nameof(reason));

        Status = SalesOrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes}\n\nCancelled: {reason}";
        Touch(userId);
    }

    private void EnsureNotCancelled()
    {
        if (Status == SalesOrderStatus.Cancelled) throw new InvalidOperationException("Sales order is cancelled.");
    }

    private void Touch(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}

