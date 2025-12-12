using Harvestry.Sales.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

public sealed class Shipment : AggregateRoot<Guid>
{
    private readonly List<ShipmentPackage> _packages = new();

    private Shipment(Guid id) : base(id) { }

    private Shipment(
        Guid id,
        Guid siteId,
        string shipmentNumber,
        Guid salesOrderId,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (string.IsNullOrWhiteSpace(shipmentNumber)) throw new ArgumentException("Shipment number is required", nameof(shipmentNumber));
        if (salesOrderId == Guid.Empty) throw new ArgumentException("SalesOrderId cannot be empty", nameof(salesOrderId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        ShipmentNumber = shipmentNumber.Trim();
        SalesOrderId = salesOrderId;
        Status = ShipmentStatus.Draft;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public string ShipmentNumber { get; private set; } = string.Empty;
    public Guid SalesOrderId { get; private set; }

    public ShipmentStatus Status { get; private set; }

    public DateTime? PickingStartedAt { get; private set; }
    public DateTime? PackedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public string? CarrierName { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyList<ShipmentPackage> Packages => _packages.AsReadOnly();

    public static Shipment CreateDraft(
        Guid siteId,
        string shipmentNumber,
        Guid salesOrderId,
        Guid createdByUserId)
    {
        return new Shipment(Guid.NewGuid(), siteId, shipmentNumber, salesOrderId, createdByUserId);
    }

    public ShipmentPackage AddPackage(
        Guid packageId,
        string? packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid? salesAllocationId,
        Guid userId)
    {
        EnsureNotTerminal();
        if (Status != ShipmentStatus.Draft) throw new InvalidOperationException("Can only add packages in Draft status.");

        var shipmentPackage = ShipmentPackage.Create(
            SiteId,
            Id,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure,
            salesAllocationId);

        _packages.Add(shipmentPackage);
        Touch(userId);
        return shipmentPackage;
    }

    public void SetNotes(string? notes, Guid userId)
    {
        EnsureNotTerminal();
        if (Status != ShipmentStatus.Draft) throw new InvalidOperationException("Notes can only be set in Draft status.");
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Touch(userId);
    }

    public void StartPicking(Guid userId)
    {
        EnsureNotTerminal();
        if (Status != ShipmentStatus.Draft) throw new InvalidOperationException($"Cannot start picking in status {Status}.");

        Status = ShipmentStatus.Picking;
        PickingStartedAt = DateTime.UtcNow;
        Touch(userId);
    }

    public void MarkPacked(Guid userId)
    {
        EnsureNotTerminal();
        if (Status != ShipmentStatus.Picking) throw new InvalidOperationException("Shipment must be in Picking status to pack.");
        if (_packages.Count == 0) throw new InvalidOperationException("Cannot pack an empty shipment.");

        Status = ShipmentStatus.Packed;
        PackedAt = DateTime.UtcNow;
        Touch(userId);
    }

    public void MarkShipped(string? carrierName, string? trackingNumber, Guid userId)
    {
        EnsureNotTerminal();
        if (Status != ShipmentStatus.Packed) throw new InvalidOperationException("Shipment must be Packed before shipping.");

        Status = ShipmentStatus.Shipped;
        CarrierName = carrierName?.Trim();
        TrackingNumber = trackingNumber?.Trim();
        ShippedAt = DateTime.UtcNow;
        Touch(userId);
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status == ShipmentStatus.Cancelled) return;
        if (Status == ShipmentStatus.Shipped) throw new InvalidOperationException("Cannot cancel a shipped shipment.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancellation reason is required", nameof(reason));

        Status = ShipmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Notes = string.IsNullOrWhiteSpace(Notes) ? $"Cancelled: {reason}" : $"{Notes}\n\nCancelled: {reason}";
        Touch(userId);
    }

    private void EnsureNotTerminal()
    {
        if (Status is ShipmentStatus.Cancelled or ShipmentStatus.Shipped)
        {
            throw new InvalidOperationException($"Shipment is terminal: {Status}.");
        }
    }

    private void Touch(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}

