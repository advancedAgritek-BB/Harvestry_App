using Harvestry.Packages.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Packages.Domain.Entities;

/// <summary>
/// Package quantity adjustment record (METRC package adjust)
/// </summary>
public sealed class PackageAdjustment : Entity<Guid>
{
    // Private constructor for EF Core
    private PackageAdjustment(Guid id) : base(id) { }

    private PackageAdjustment(
        Guid id,
        Guid packageId,
        decimal quantity,
        string unitOfMeasure,
        AdjustmentReason reason,
        DateOnly adjustmentDate,
        Guid performedByUserId,
        string? reasonNote = null) : base(id)
    {
        PackageId = packageId;
        Quantity = quantity;
        UnitOfMeasure = unitOfMeasure;
        Reason = reason;
        AdjustmentDate = adjustmentDate;
        PerformedByUserId = performedByUserId;
        ReasonNote = reasonNote;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PackageId { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public AdjustmentReason Reason { get; private set; }
    public string? ReasonNote { get; private set; }
    public DateOnly AdjustmentDate { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // METRC sync
    public long? MetrcAdjustmentId { get; private set; }

    /// <summary>
    /// Factory method to create a new adjustment
    /// </summary>
    public static PackageAdjustment Create(
        Guid packageId,
        decimal quantity,
        string unitOfMeasure,
        AdjustmentReason reason,
        DateOnly adjustmentDate,
        Guid performedByUserId,
        string? reasonNote = null)
    {
        if (packageId == Guid.Empty)
            throw new ArgumentException("Package ID cannot be empty", nameof(packageId));

        if (quantity == 0)
            throw new ArgumentException("Adjustment quantity cannot be zero", nameof(quantity));

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("Unit of measure cannot be empty", nameof(unitOfMeasure));

        if (performedByUserId == Guid.Empty)
            throw new ArgumentException("Performed by user ID cannot be empty", nameof(performedByUserId));

        // Require note for 'Other' reason
        if (reason == AdjustmentReason.Other && string.IsNullOrWhiteSpace(reasonNote))
            throw new ArgumentException("Reason note is required for 'Other' adjustments", nameof(reasonNote));

        return new PackageAdjustment(
            Guid.NewGuid(),
            packageId,
            quantity,
            unitOfMeasure.Trim(),
            reason,
            adjustmentDate,
            performedByUserId,
            reasonNote?.Trim());
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcAdjustmentId)
    {
        MetrcAdjustmentId = metrcAdjustmentId;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static PackageAdjustment Restore(
        Guid id,
        Guid packageId,
        decimal quantity,
        string unitOfMeasure,
        AdjustmentReason reason,
        string? reasonNote,
        DateOnly adjustmentDate,
        Guid performedByUserId,
        DateTime createdAt,
        long? metrcAdjustmentId)
    {
        return new PackageAdjustment(id)
        {
            PackageId = packageId,
            Quantity = quantity,
            UnitOfMeasure = unitOfMeasure,
            Reason = reason,
            ReasonNote = reasonNote,
            AdjustmentDate = adjustmentDate,
            PerformedByUserId = performedByUserId,
            CreatedAt = createdAt,
            MetrcAdjustmentId = metrcAdjustmentId
        };
    }
}









