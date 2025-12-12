using Harvestry.Shared.Kernel.Domain;
using Harvestry.Transfers.Domain.Enums;

namespace Harvestry.Transfers.Domain.Entities;

public sealed class OutboundTransfer : AggregateRoot<Guid>
{
    private readonly List<OutboundTransferPackage> _packages = new();

    private OutboundTransfer(Guid id) : base(id) { }

    private OutboundTransfer(
        Guid id,
        Guid siteId,
        Guid? shipmentId,
        Guid? salesOrderId,
        string destinationLicenseNumber,
        string? destinationFacilityName,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (string.IsNullOrWhiteSpace(destinationLicenseNumber))
            throw new ArgumentException("Destination license number is required", nameof(destinationLicenseNumber));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        ShipmentId = shipmentId;
        SalesOrderId = salesOrderId;
        DestinationLicenseNumber = destinationLicenseNumber.Trim();
        DestinationFacilityName = destinationFacilityName?.Trim();
        Status = OutboundTransferStatus.Draft;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }
    public Guid? ShipmentId { get; private set; }
    public Guid? SalesOrderId { get; private set; }

    public string DestinationLicenseNumber { get; private set; } = string.Empty;
    public string? DestinationFacilityName { get; private set; }

    public DateTime? PlannedDepartureAt { get; private set; }
    public DateTime? PlannedArrivalAt { get; private set; }

    public OutboundTransferStatus Status { get; private set; }
    public string? StatusReason { get; private set; }

    public long? MetrcTransferTemplateId { get; private set; }
    public string? MetrcTransferNumber { get; private set; }
    public DateTime? MetrcLastSubmittedAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }
    public string? MetrcSyncError { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyList<OutboundTransferPackage> Packages => _packages.AsReadOnly();

    public static OutboundTransfer CreateDraft(
        Guid siteId,
        Guid? shipmentId,
        Guid? salesOrderId,
        string destinationLicenseNumber,
        string? destinationFacilityName,
        Guid createdByUserId)
    {
        return new OutboundTransfer(
            Guid.NewGuid(),
            siteId,
            shipmentId,
            salesOrderId,
            destinationLicenseNumber,
            destinationFacilityName,
            createdByUserId);
    }

    public OutboundTransferPackage AddPackage(Guid packageId, string? packageLabel, decimal quantity, string unitOfMeasure, Guid userId)
    {
        EnsureMutable();
        if (packageId == Guid.Empty) throw new ArgumentException("PackageId cannot be empty", nameof(packageId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (string.IsNullOrWhiteSpace(unitOfMeasure)) throw new ArgumentException("UnitOfMeasure is required", nameof(unitOfMeasure));

        var p = OutboundTransferPackage.Create(SiteId, Id, packageId, packageLabel, quantity, unitOfMeasure);
        _packages.Add(p);
        Touch(userId);
        return p;
    }

    public void SetPlannedWindow(DateTime? plannedDepartureAt, DateTime? plannedArrivalAt, Guid userId)
    {
        EnsureMutable();
        PlannedDepartureAt = plannedDepartureAt;
        PlannedArrivalAt = plannedArrivalAt;
        Touch(userId);
    }

    public void MarkReady(Guid userId)
    {
        EnsureMutable();
        if (_packages.Count == 0) throw new InvalidOperationException("Cannot mark transfer ready without packages.");
        Status = OutboundTransferStatus.Ready;
        Touch(userId);
    }

    public void MarkSubmittedToMetrc(long templateId, string? transferNumber, Guid userId)
    {
        if (Status is OutboundTransferStatus.Cancelled or OutboundTransferStatus.Voided)
            throw new InvalidOperationException($"Cannot submit transfer in status {Status}.");

        if (templateId <= 0) throw new ArgumentOutOfRangeException(nameof(templateId));
        MetrcTransferTemplateId = templateId;
        MetrcTransferNumber = transferNumber?.Trim();
        MetrcLastSubmittedAt = DateTime.UtcNow;
        MetrcSyncStatus = "submitted";
        MetrcSyncError = null;
        Status = OutboundTransferStatus.SubmittedToMetrc;
        Touch(userId);
    }

    public void MarkVoided(string reason, Guid userId)
    {
        if (Status == OutboundTransferStatus.Voided) return;
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Void reason is required", nameof(reason));
        Status = OutboundTransferStatus.Voided;
        StatusReason = reason.Trim();
        Touch(userId);
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status == OutboundTransferStatus.Cancelled) return;
        if (Status == OutboundTransferStatus.Accepted) throw new InvalidOperationException("Cannot cancel an accepted transfer.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancel reason is required", nameof(reason));
        Status = OutboundTransferStatus.Cancelled;
        StatusReason = reason.Trim();
        Touch(userId);
    }

    public void UpdateMetrcSync(string status, string? error, Guid userId)
    {
        MetrcSyncStatus = status?.Trim();
        MetrcSyncError = error;
        Touch(userId);
    }

    private void EnsureMutable()
    {
        if (Status is OutboundTransferStatus.Cancelled or OutboundTransferStatus.Voided or OutboundTransferStatus.Accepted)
            throw new InvalidOperationException($"Transfer is not mutable in status {Status}.");
    }

    private void Touch(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}

