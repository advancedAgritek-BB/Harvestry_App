using Harvestry.Packages.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Packages.Domain.Entities;

/// <summary>
/// Inventory movement entity - tracks all inventory transactions for audit trail
/// </summary>
public sealed class InventoryMovement : AggregateRoot<Guid>
{
    private readonly List<string> _evidenceUrls = new();
    private readonly List<string> _photoUrls = new();

    // Private constructor for EF Core
    private InventoryMovement(Guid id) : base(id) { }

    private InventoryMovement(
        Guid id,
        Guid siteId,
        MovementType movementType,
        Guid packageId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        if (packageId == Guid.Empty)
            throw new ArgumentException("Package ID cannot be empty", nameof(packageId));
        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label cannot be empty", nameof(packageLabel));
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be empty", nameof(unitOfMeasure));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        MovementType = movementType;
        Status = MovementStatus.Completed;
        PackageId = packageId;
        PackageLabel = packageLabel.Trim().ToUpperInvariant();
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure.Trim();
        SyncStatus = "pending";
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // CORE PROPERTIES
    // =========================================================================

    public Guid SiteId { get; private set; }
    public MovementType MovementType { get; private set; }
    public MovementStatus Status { get; private set; }

    // =========================================================================
    // PACKAGE REFERENCE
    // =========================================================================

    public Guid PackageId { get; private set; }
    public string PackageLabel { get; private set; } = string.Empty;
    public Guid? ItemId { get; private set; }
    public string? ItemName { get; private set; }

    // =========================================================================
    // LOCATIONS
    // =========================================================================

    public Guid? FromLocationId { get; private set; }
    public string? FromLocationPath { get; private set; }
    public Guid? ToLocationId { get; private set; }
    public string? ToLocationPath { get; private set; }

    // =========================================================================
    // QUANTITY TRACKING
    // =========================================================================

    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal? QuantityBefore { get; private set; }
    public decimal? QuantityAfter { get; private set; }

    // =========================================================================
    // COST TRACKING
    // =========================================================================

    public decimal? UnitCost { get; private set; }
    public decimal? TotalCost { get; private set; }

    // =========================================================================
    // ADJUSTMENT DETAILS
    // =========================================================================

    public string? ReasonCode { get; private set; }
    public string? ReasonNotes { get; private set; }

    // =========================================================================
    // SPLIT/MERGE REFERENCES
    // =========================================================================

    public List<Guid> SourcePackageIds { get; private set; } = new();
    public List<Guid> TargetPackageIds { get; private set; } = new();

    // =========================================================================
    // PROCESSING REFERENCE
    // =========================================================================

    public Guid? ProcessingJobId { get; private set; }
    public string? ProcessingJobNumber { get; private set; }

    // =========================================================================
    // ORDER REFERENCE
    // =========================================================================

    public Guid? SalesOrderId { get; private set; }
    public string? SalesOrderNumber { get; private set; }
    public Guid? TransferId { get; private set; }

    // =========================================================================
    // COMPLIANCE
    // =========================================================================

    public string? MetrcManifestId { get; private set; }
    public string? BiotrackTransferId { get; private set; }
    public string SyncStatus { get; private set; } = "pending";
    public string? SyncError { get; private set; }
    public DateTime? SyncedAt { get; private set; }

    // =========================================================================
    // VERIFICATION
    // =========================================================================

    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? ScanData { get; private set; }
    public string? BarcodeScanned { get; private set; }

    // =========================================================================
    // EVIDENCE
    // =========================================================================

    public string? Notes { get; private set; }
    public IReadOnlyList<string> EvidenceUrls => _evidenceUrls.AsReadOnly();
    public IReadOnlyList<string> PhotoUrls => _photoUrls.AsReadOnly();

    // =========================================================================
    // TWO-PERSON APPROVAL
    // =========================================================================

    public bool RequiresApproval { get; private set; }
    public Guid? FirstApproverId { get; private set; }
    public DateTime? FirstApprovedAt { get; private set; }
    public Guid? SecondApproverId { get; private set; }
    public DateTime? SecondApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // =========================================================================
    // BATCH REFERENCE
    // =========================================================================

    public Guid? BatchMovementId { get; private set; }
    public int? BatchSequence { get; private set; }

    // =========================================================================
    // AUDIT
    // =========================================================================

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // =========================================================================
    // METADATA
    // =========================================================================

    public Dictionary<string, object> Metadata { get; private set; } = new();

    // =========================================================================
    // FACTORY METHODS
    // =========================================================================

    /// <summary>
    /// Create a transfer movement
    /// </summary>
    public static InventoryMovement CreateTransfer(
        Guid siteId,
        Guid packageId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid fromLocationId,
        string fromLocationPath,
        Guid toLocationId,
        string toLocationPath,
        Guid userId,
        string? notes = null)
    {
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            siteId,
            MovementType.Transfer,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure,
            userId);

        movement.FromLocationId = fromLocationId;
        movement.FromLocationPath = fromLocationPath;
        movement.ToLocationId = toLocationId;
        movement.ToLocationPath = toLocationPath;
        movement.Notes = notes?.Trim();
        movement.CompletedAt = DateTime.UtcNow;
        movement.CompletedByUserId = userId;

        return movement;
    }

    /// <summary>
    /// Create an adjustment movement
    /// </summary>
    public static InventoryMovement CreateAdjustment(
        Guid siteId,
        Guid packageId,
        string packageLabel,
        decimal adjustmentQuantity,
        string unitOfMeasure,
        decimal quantityBefore,
        decimal quantityAfter,
        string reasonCode,
        Guid userId,
        string? reasonNotes = null,
        decimal? unitCost = null)
    {
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            siteId,
            MovementType.Adjustment,
            packageId,
            packageLabel,
            adjustmentQuantity,
            unitOfMeasure,
            userId);

        movement.QuantityBefore = quantityBefore;
        movement.QuantityAfter = quantityAfter;
        movement.ReasonCode = reasonCode;
        movement.ReasonNotes = reasonNotes?.Trim();
        movement.UnitCost = unitCost;
        movement.TotalCost = unitCost.HasValue ? adjustmentQuantity * unitCost.Value : null;
        movement.CompletedAt = DateTime.UtcNow;
        movement.CompletedByUserId = userId;

        return movement;
    }

    /// <summary>
    /// Create a receive movement
    /// </summary>
    public static InventoryMovement CreateReceive(
        Guid siteId,
        Guid packageId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid toLocationId,
        string toLocationPath,
        Guid userId,
        decimal? unitCost = null,
        string? notes = null)
    {
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            siteId,
            MovementType.Receive,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure,
            userId);

        movement.ToLocationId = toLocationId;
        movement.ToLocationPath = toLocationPath;
        movement.UnitCost = unitCost;
        movement.TotalCost = unitCost.HasValue ? quantity * unitCost.Value : null;
        movement.Notes = notes?.Trim();
        movement.CompletedAt = DateTime.UtcNow;
        movement.CompletedByUserId = userId;

        return movement;
    }

    /// <summary>
    /// Create a ship movement
    /// </summary>
    public static InventoryMovement CreateShip(
        Guid siteId,
        Guid packageId,
        string packageLabel,
        decimal quantity,
        string unitOfMeasure,
        Guid fromLocationId,
        string fromLocationPath,
        Guid userId,
        Guid? salesOrderId = null,
        string? salesOrderNumber = null,
        decimal? unitCost = null)
    {
        var movement = new InventoryMovement(
            Guid.NewGuid(),
            siteId,
            MovementType.Ship,
            packageId,
            packageLabel,
            quantity,
            unitOfMeasure,
            userId);

        movement.FromLocationId = fromLocationId;
        movement.FromLocationPath = fromLocationPath;
        movement.SalesOrderId = salesOrderId;
        movement.SalesOrderNumber = salesOrderNumber;
        movement.UnitCost = unitCost;
        movement.TotalCost = unitCost.HasValue ? quantity * unitCost.Value : null;
        movement.CompletedAt = DateTime.UtcNow;
        movement.CompletedByUserId = userId;

        return movement;
    }

    // =========================================================================
    // METHODS
    // =========================================================================

    /// <summary>
    /// Set item information
    /// </summary>
    public void SetItemInfo(Guid itemId, string itemName)
    {
        ItemId = itemId;
        ItemName = itemName?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add verification
    /// </summary>
    public void Verify(Guid userId, string? scanData = null, string? barcodeScanned = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        VerifiedByUserId = userId;
        VerifiedAt = DateTime.UtcNow;
        ScanData = scanData;
        BarcodeScanned = barcodeScanned?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add evidence URL
    /// </summary>
    public void AddEvidenceUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            _evidenceUrls.Add(url.Trim());
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Add photo URL
    /// </summary>
    public void AddPhotoUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            _photoUrls.Add(url.Trim());
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Update sync status
    /// </summary>
    public void UpdateSyncStatus(string status, string? error = null)
    {
        SyncStatus = status;
        SyncError = error;
        if (status == "synced")
        {
            SyncedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set METRC manifest ID
    /// </summary>
    public void SetMetrcManifest(string manifestId)
    {
        MetrcManifestId = manifestId?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set batch movement reference
    /// </summary>
    public void SetBatchMovement(Guid batchMovementId, int sequence)
    {
        BatchMovementId = batchMovementId;
        BatchSequence = sequence;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as completed
    /// </summary>
    public void Complete(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = MovementStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark as failed
    /// </summary>
    public void Fail(string reason)
    {
        Status = MovementStatus.Failed;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Failed: {reason}"
            : $"{Notes}\n\nFailed: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancel the movement
    /// </summary>
    public void Cancel(Guid userId, string? reason = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status == MovementStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed movement");

        Status = MovementStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? $"Cancelled: {reason}"
                : $"{Notes}\n\nCancelled: {reason}";
        }
        UpdatedAt = DateTime.UtcNow;
    }
}



