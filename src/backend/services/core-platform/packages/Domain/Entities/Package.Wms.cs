using Harvestry.Packages.Domain.Enums;

namespace Harvestry.Packages.Domain.Entities;

/// <summary>
/// Package entity WMS extensions - costing, reservations, holds, vendor info, quality
/// </summary>
public sealed partial class Package
{
    // =========================================================================
    // COSTING PROPERTIES
    // =========================================================================

    /// <summary>
    /// Cost per unit of measure
    /// </summary>
    public decimal? UnitCost { get; private set; }

    /// <summary>
    /// Raw material cost component
    /// </summary>
    public decimal MaterialCost { get; private set; }

    /// <summary>
    /// Labor cost component
    /// </summary>
    public decimal LaborCost { get; private set; }

    /// <summary>
    /// Overhead cost component
    /// </summary>
    public decimal OverheadCost { get; private set; }

    /// <summary>
    /// Total cost (material + labor + overhead)
    /// </summary>
    public decimal TotalCost => MaterialCost + LaborCost + OverheadCost;

    /// <summary>
    /// Total value of this package (quantity * unit cost)
    /// </summary>
    public decimal TotalValue => Quantity * (UnitCost ?? 0);

    // =========================================================================
    // RESERVATION PROPERTIES
    // =========================================================================

    /// <summary>
    /// Quantity reserved for orders but not yet consumed
    /// </summary>
    public decimal ReservedQuantity { get; private set; }

    /// <summary>
    /// Quantity available for new orders (quantity - reserved)
    /// </summary>
    public decimal AvailableQuantity => Quantity - ReservedQuantity;

    // =========================================================================
    // CLASSIFICATION PROPERTIES
    // =========================================================================

    /// <summary>
    /// Inventory classification for financial reporting
    /// </summary>
    public InventoryCategory InventoryCategory { get; private set; } = InventoryCategory.FinishedGood;

    // =========================================================================
    // EXTENDED HOLD MANAGEMENT
    // =========================================================================

    /// <summary>
    /// Reason code for the hold
    /// </summary>
    public HoldReasonCode? HoldReasonCode { get; private set; }

    /// <summary>
    /// When the hold was placed
    /// </summary>
    public DateTime? HoldPlacedAt { get; private set; }

    /// <summary>
    /// User who placed the hold
    /// </summary>
    public Guid? HoldPlacedByUserId { get; private set; }

    /// <summary>
    /// When the hold was released
    /// </summary>
    public DateTime? HoldReleasedAt { get; private set; }

    /// <summary>
    /// User who released the hold
    /// </summary>
    public Guid? HoldReleasedByUserId { get; private set; }

    /// <summary>
    /// Whether release requires two-person approval
    /// </summary>
    public bool RequiresTwoPersonRelease { get; private set; }

    /// <summary>
    /// First approver for two-person release
    /// </summary>
    public Guid? HoldFirstApproverId { get; private set; }

    /// <summary>
    /// When first approval was given
    /// </summary>
    public DateTime? HoldFirstApprovedAt { get; private set; }

    // =========================================================================
    // VENDOR/RECEIVING PROPERTIES
    // =========================================================================

    /// <summary>
    /// Reference to vendor for purchased inventory
    /// </summary>
    public Guid? VendorId { get; private set; }

    /// <summary>
    /// Vendor name for display
    /// </summary>
    public string? VendorName { get; private set; }

    /// <summary>
    /// Vendor's batch/lot number
    /// </summary>
    public string? VendorLotNumber { get; private set; }

    /// <summary>
    /// Reference to purchase order
    /// </summary>
    public Guid? PurchaseOrderId { get; private set; }

    /// <summary>
    /// PO number for display
    /// </summary>
    public string? PurchaseOrderNumber { get; private set; }

    /// <summary>
    /// Date inventory was received
    /// </summary>
    public DateOnly? ReceivedDate { get; private set; }

    // =========================================================================
    // QUALITY PROPERTIES
    // =========================================================================

    /// <summary>
    /// Quality grade classification
    /// </summary>
    public QualityGrade? Grade { get; private set; }

    /// <summary>
    /// Numeric quality score (0-100)
    /// </summary>
    public decimal? QualityScore { get; private set; }

    /// <summary>
    /// Notes about quality assessment
    /// </summary>
    public string? QualityNotes { get; private set; }

    // =========================================================================
    // LINEAGE PROPERTIES
    // =========================================================================

    /// <summary>
    /// Number of transformations from origin (0 = original)
    /// </summary>
    public int GenerationDepth { get; private set; }

    /// <summary>
    /// ID of the original ancestor package
    /// </summary>
    public Guid? RootAncestorId { get; private set; }

    /// <summary>
    /// Materialized path of ancestor IDs for fast queries
    /// </summary>
    public string? AncestryPath { get; private set; }

    // =========================================================================
    // COSTING METHODS
    // =========================================================================

    /// <summary>
    /// Set the unit cost
    /// </summary>
    public void SetUnitCost(decimal unitCost, Guid userId)
    {
        ValidateUserId(userId);

        if (unitCost < 0)
            throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

        UnitCost = unitCost;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set cost components
    /// </summary>
    public void SetCostComponents(
        decimal materialCost,
        decimal laborCost,
        decimal overheadCost,
        Guid userId)
    {
        ValidateUserId(userId);

        if (materialCost < 0)
            throw new ArgumentException("Material cost cannot be negative", nameof(materialCost));
        if (laborCost < 0)
            throw new ArgumentException("Labor cost cannot be negative", nameof(laborCost));
        if (overheadCost < 0)
            throw new ArgumentException("Overhead cost cannot be negative", nameof(overheadCost));

        MaterialCost = materialCost;
        LaborCost = laborCost;
        OverheadCost = overheadCost;

        // Calculate unit cost from components
        if (Quantity > 0)
        {
            UnitCost = TotalCost / Quantity;
        }

        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // RESERVATION METHODS
    // =========================================================================

    /// <summary>
    /// Reserve quantity for an order
    /// </summary>
    public void Reserve(decimal quantity, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (quantity <= 0)
            throw new ArgumentException("Reserve quantity must be positive", nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                $"Cannot reserve {quantity}. Only {AvailableQuantity} available.");

        ReservedQuantity += quantity;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release reserved quantity
    /// </summary>
    public void Unreserve(decimal quantity, Guid userId)
    {
        ValidateUserId(userId);

        if (quantity <= 0)
            throw new ArgumentException("Unreserve quantity must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                $"Cannot unreserve {quantity}. Only {ReservedQuantity} reserved.");

        ReservedQuantity -= quantity;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Consume reserved quantity (actually remove from inventory)
    /// </summary>
    public void ConsumeReserved(decimal quantity, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (quantity <= 0)
            throw new ArgumentException("Consume quantity must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                $"Cannot consume {quantity}. Only {ReservedQuantity} reserved.");

        ReservedQuantity -= quantity;
        Quantity -= quantity;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // CLASSIFICATION METHODS
    // =========================================================================

    /// <summary>
    /// Set inventory category
    /// </summary>
    public void SetInventoryCategory(InventoryCategory category, Guid userId)
    {
        ValidateUserId(userId);

        InventoryCategory = category;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // ENHANCED HOLD METHODS
    // =========================================================================

    /// <summary>
    /// Place package on hold with reason code
    /// </summary>
    public void PlaceOnHoldWithReason(
        HoldReasonCode reasonCode,
        Guid userId,
        bool requiresTwoPersonRelease = false,
        string? notes = null)
    {
        ValidateUserId(userId);

        if (Status != PackageStatus.Active)
            throw new InvalidOperationException($"Cannot place package on hold with status {Status}");

        Status = PackageStatus.OnHold;
        HoldReasonCode = reasonCode;
        HoldPlacedAt = DateTime.UtcNow;
        HoldPlacedByUserId = userId;
        RequiresTwoPersonRelease = requiresTwoPersonRelease;
        HoldReleasedAt = null;
        HoldReleasedByUserId = null;
        HoldFirstApproverId = null;
        HoldFirstApprovedAt = null;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            AddNote($"Placed on hold ({reasonCode}): {notes}", userId);
        }
        else
        {
            AddNote($"Placed on hold: {reasonCode}", userId);
        }
    }

    /// <summary>
    /// First approval for two-person release
    /// </summary>
    public void ApproveHoldRelease(Guid approverId)
    {
        ValidateUserId(approverId);

        if (Status != PackageStatus.OnHold)
            throw new InvalidOperationException("Package is not on hold");

        if (!RequiresTwoPersonRelease)
            throw new InvalidOperationException("This hold does not require two-person approval");

        if (approverId == HoldPlacedByUserId)
            throw new InvalidOperationException("First approver cannot be the same person who placed the hold");

        HoldFirstApproverId = approverId;
        HoldFirstApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release from hold with full tracking
    /// </summary>
    public void ReleaseFromHoldWithTracking(Guid userId, string? notes = null)
    {
        ValidateUserId(userId);

        if (Status != PackageStatus.OnHold)
            throw new InvalidOperationException("Package is not on hold");

        if (RequiresTwoPersonRelease && HoldFirstApproverId == null)
            throw new InvalidOperationException("Two-person release requires first approval");

        if (RequiresTwoPersonRelease && userId == HoldFirstApproverId)
            throw new InvalidOperationException("Second approver must be different from first approver");

        Status = PackageStatus.Active;
        HoldReleasedAt = DateTime.UtcNow;
        HoldReleasedByUserId = userId;

        var releaseNote = RequiresTwoPersonRelease
            ? $"Released from hold (two-person approval)"
            : "Released from hold";

        if (!string.IsNullOrWhiteSpace(notes))
        {
            releaseNote += $": {notes}";
        }

        AddNote(releaseNote, userId);
    }

    // =========================================================================
    // VENDOR METHODS
    // =========================================================================

    /// <summary>
    /// Set vendor information for purchased inventory
    /// </summary>
    public void SetVendorInfo(
        Guid vendorId,
        string vendorName,
        string? vendorLotNumber,
        Guid? purchaseOrderId,
        string? purchaseOrderNumber,
        DateOnly receivedDate,
        Guid userId)
    {
        ValidateUserId(userId);

        if (vendorId == Guid.Empty)
            throw new ArgumentException("Vendor ID cannot be empty", nameof(vendorId));

        if (string.IsNullOrWhiteSpace(vendorName))
            throw new ArgumentException("Vendor name cannot be empty", nameof(vendorName));

        VendorId = vendorId;
        VendorName = vendorName.Trim();
        VendorLotNumber = vendorLotNumber?.Trim();
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderNumber = purchaseOrderNumber?.Trim();
        ReceivedDate = receivedDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // QUALITY METHODS
    // =========================================================================

    /// <summary>
    /// Set quality grade
    /// </summary>
    public void SetGrade(QualityGrade grade, decimal? score, string? notes, Guid userId)
    {
        ValidateUserId(userId);

        if (score.HasValue && (score.Value < 0 || score.Value > 100))
            throw new ArgumentException("Quality score must be between 0 and 100", nameof(score));

        Grade = grade;
        QualityScore = score;
        QualityNotes = notes?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    // =========================================================================
    // LINEAGE METHODS
    // =========================================================================

    /// <summary>
    /// Set lineage information
    /// </summary>
    public void SetLineage(Guid? rootAncestorId, int generationDepth, string? ancestryPath)
    {
        RootAncestorId = rootAncestorId;
        GenerationDepth = generationDepth;
        AncestryPath = ancestryPath;
        UpdatedAt = DateTime.UtcNow;
    }
}




