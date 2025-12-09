using Harvestry.Harvests.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Records adjustments made to locked weights with full audit trail
/// </summary>
public sealed class WeightAdjustment : Entity<Guid>
{
    // Private constructor for EF Core
    private WeightAdjustment(Guid id) : base(id) { }

    private WeightAdjustment(
        Guid id,
        Guid harvestId,
        Guid? harvestPlantId,
        WeightType weightType,
        decimal previousWeight,
        decimal newWeight,
        string reasonCode,
        string? notes,
        Guid adjustedByUserId,
        bool pinOverrideUsed) : base(id)
    {
        HarvestId = harvestId;
        HarvestPlantId = harvestPlantId;
        WeightType = weightType;
        PreviousWeight = previousWeight;
        NewWeight = newWeight;
        AdjustmentAmount = newWeight - previousWeight;
        ReasonCode = reasonCode;
        Notes = notes;
        AdjustedByUserId = adjustedByUserId;
        PinOverrideUsed = pinOverrideUsed;
        AdjustedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// The harvest this adjustment belongs to
    /// </summary>
    public Guid HarvestId { get; private set; }

    /// <summary>
    /// Optional: specific plant if adjusting individual plant weight
    /// </summary>
    public Guid? HarvestPlantId { get; private set; }

    /// <summary>
    /// Type of weight being adjusted
    /// </summary>
    public WeightType WeightType { get; private set; }

    /// <summary>
    /// Weight before adjustment
    /// </summary>
    public decimal PreviousWeight { get; private set; }

    /// <summary>
    /// Weight after adjustment
    /// </summary>
    public decimal NewWeight { get; private set; }

    /// <summary>
    /// Difference between new and previous weight
    /// </summary>
    public decimal AdjustmentAmount { get; private set; }

    /// <summary>
    /// Reason code for the adjustment (e.g., SCALE_ERROR, RECOUNTED, DATA_ENTRY_ERROR)
    /// </summary>
    public string ReasonCode { get; private set; } = string.Empty;

    /// <summary>
    /// Optional notes explaining the adjustment
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// User who made the adjustment
    /// </summary>
    public Guid AdjustedByUserId { get; private set; }

    /// <summary>
    /// Whether a PIN override was used to make this adjustment
    /// </summary>
    public bool PinOverrideUsed { get; private set; }

    /// <summary>
    /// When the adjustment was made
    /// </summary>
    public DateTime AdjustedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new weight adjustment for a harvest
    /// </summary>
    public static WeightAdjustment Create(
        Guid harvestId,
        WeightType weightType,
        decimal previousWeight,
        decimal newWeight,
        string reasonCode,
        string? notes,
        Guid adjustedByUserId,
        bool pinOverrideUsed)
    {
        if (harvestId == Guid.Empty)
            throw new ArgumentException("Harvest ID cannot be empty", nameof(harvestId));

        if (adjustedByUserId == Guid.Empty)
            throw new ArgumentException("Adjusted by user ID cannot be empty", nameof(adjustedByUserId));

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new ArgumentException("Reason code is required", nameof(reasonCode));

        return new WeightAdjustment(
            Guid.NewGuid(),
            harvestId,
            null,
            weightType,
            previousWeight,
            newWeight,
            reasonCode.Trim(),
            notes?.Trim(),
            adjustedByUserId,
            pinOverrideUsed);
    }

    /// <summary>
    /// Factory method to create a new weight adjustment for a specific plant
    /// </summary>
    public static WeightAdjustment CreateForPlant(
        Guid harvestId,
        Guid harvestPlantId,
        WeightType weightType,
        decimal previousWeight,
        decimal newWeight,
        string reasonCode,
        string? notes,
        Guid adjustedByUserId,
        bool pinOverrideUsed)
    {
        if (harvestId == Guid.Empty)
            throw new ArgumentException("Harvest ID cannot be empty", nameof(harvestId));

        if (harvestPlantId == Guid.Empty)
            throw new ArgumentException("Harvest plant ID cannot be empty", nameof(harvestPlantId));

        if (adjustedByUserId == Guid.Empty)
            throw new ArgumentException("Adjusted by user ID cannot be empty", nameof(adjustedByUserId));

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new ArgumentException("Reason code is required", nameof(reasonCode));

        return new WeightAdjustment(
            Guid.NewGuid(),
            harvestId,
            harvestPlantId,
            weightType,
            previousWeight,
            newWeight,
            reasonCode.Trim(),
            notes?.Trim(),
            adjustedByUserId,
            pinOverrideUsed);
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static WeightAdjustment Restore(
        Guid id,
        Guid harvestId,
        Guid? harvestPlantId,
        WeightType weightType,
        decimal previousWeight,
        decimal newWeight,
        decimal adjustmentAmount,
        string reasonCode,
        string? notes,
        Guid adjustedByUserId,
        bool pinOverrideUsed,
        DateTime adjustedAt)
    {
        return new WeightAdjustment(id)
        {
            HarvestId = harvestId,
            HarvestPlantId = harvestPlantId,
            WeightType = weightType,
            PreviousWeight = previousWeight,
            NewWeight = newWeight,
            AdjustmentAmount = adjustmentAmount,
            ReasonCode = reasonCode,
            Notes = notes,
            AdjustedByUserId = adjustedByUserId,
            PinOverrideUsed = pinOverrideUsed,
            AdjustedAt = adjustedAt
        };
    }
}
