using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Links a plant to a harvest with the weight contributed
/// </summary>
public sealed class HarvestPlant : Entity<Guid>
{
    // Private constructor for EF Core
    private HarvestPlant(Guid id) : base(id) { }

    private HarvestPlant(
        Guid id,
        Guid harvestId,
        Guid plantId,
        string plantTag,
        decimal wetWeight,
        string unitOfWeight) : base(id)
    {
        HarvestId = harvestId;
        PlantId = plantId;
        PlantTag = plantTag;
        WetWeight = wetWeight;
        UnitOfWeight = unitOfWeight;
        HarvestedAt = DateTime.UtcNow;
        IsWeightLocked = false;
    }

    public Guid HarvestId { get; private set; }
    public Guid PlantId { get; private set; }
    public string PlantTag { get; private set; } = string.Empty;
    public decimal WetWeight { get; private set; }
    public string UnitOfWeight { get; private set; } = "Grams";
    public DateTime HarvestedAt { get; private set; }

    // ===== SCALE READING LINK =====
    /// <summary>
    /// Scale reading that captured this weight (if from scale)
    /// </summary>
    public Guid? ScaleReadingId { get; private set; }

    /// <summary>
    /// Source of the weight: 'scale' or 'manual'
    /// </summary>
    public string WeightSource { get; private set; } = "manual";

    // ===== WEIGHT LOCK =====
    /// <summary>
    /// Whether the weight is locked (requires PIN override to change)
    /// </summary>
    public bool IsWeightLocked { get; private set; }

    /// <summary>
    /// When the weight was locked
    /// </summary>
    public DateTime? WeightLockedAt { get; private set; }

    /// <summary>
    /// User who locked the weight
    /// </summary>
    public Guid? WeightLockedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new harvest plant record
    /// </summary>
    public static HarvestPlant Create(
        Guid harvestId,
        Guid plantId,
        string plantTag,
        decimal wetWeight,
        string unitOfWeight)
    {
        if (harvestId == Guid.Empty)
            throw new ArgumentException("Harvest ID cannot be empty", nameof(harvestId));

        if (plantId == Guid.Empty)
            throw new ArgumentException("Plant ID cannot be empty", nameof(plantId));

        if (string.IsNullOrWhiteSpace(plantTag))
            throw new ArgumentException("Plant tag cannot be empty", nameof(plantTag));

        if (wetWeight <= 0)
            throw new ArgumentException("Wet weight must be greater than 0", nameof(wetWeight));

        return new HarvestPlant(
            Guid.NewGuid(),
            harvestId,
            plantId,
            plantTag.Trim(),
            wetWeight,
            unitOfWeight?.Trim() ?? "Grams");
    }

    /// <summary>
    /// Factory method to create a harvest plant record with scale reading
    /// </summary>
    public static HarvestPlant CreateFromScaleReading(
        Guid harvestId,
        Guid plantId,
        string plantTag,
        decimal wetWeight,
        string unitOfWeight,
        Guid scaleReadingId)
    {
        var plant = Create(harvestId, plantId, plantTag, wetWeight, unitOfWeight);
        plant.ScaleReadingId = scaleReadingId;
        plant.WeightSource = "scale";
        return plant;
    }

    /// <summary>
    /// Update the wet weight (only if not locked)
    /// </summary>
    public void UpdateWetWeight(decimal newWeight, Guid? scaleReadingId, string weightSource)
    {
        if (IsWeightLocked)
            throw new InvalidOperationException("Cannot update weight - weight is locked. Use PIN override to adjust.");

        if (newWeight <= 0)
            throw new ArgumentException("Wet weight must be greater than 0", nameof(newWeight));

        WetWeight = newWeight;
        ScaleReadingId = scaleReadingId;
        WeightSource = weightSource?.Trim() ?? "manual";
    }

    /// <summary>
    /// Lock the weight to prevent changes without PIN override
    /// </summary>
    public void LockWeight(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (IsWeightLocked)
            throw new InvalidOperationException("Weight is already locked");

        IsWeightLocked = true;
        WeightLockedAt = DateTime.UtcNow;
        WeightLockedByUserId = userId;
    }

    /// <summary>
    /// Unlock the weight (with PIN override) - typically done through adjustment
    /// </summary>
    public void UnlockWeight()
    {
        IsWeightLocked = false;
        WeightLockedAt = null;
        WeightLockedByUserId = null;
    }

    /// <summary>
    /// Adjust weight with PIN override (bypasses lock)
    /// </summary>
    public void AdjustWeightWithOverride(decimal newWeight, Guid? scaleReadingId, string weightSource)
    {
        if (newWeight <= 0)
            throw new ArgumentException("Wet weight must be greater than 0", nameof(newWeight));

        WetWeight = newWeight;
        ScaleReadingId = scaleReadingId;
        WeightSource = weightSource?.Trim() ?? "manual";
        // Keep locked status - adjustment doesn't unlock
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static HarvestPlant Restore(
        Guid id,
        Guid harvestId,
        Guid plantId,
        string plantTag,
        decimal wetWeight,
        string unitOfWeight,
        DateTime harvestedAt)
    {
        return new HarvestPlant(id)
        {
            HarvestId = harvestId,
            PlantId = plantId,
            PlantTag = plantTag,
            WetWeight = wetWeight,
            UnitOfWeight = unitOfWeight,
            HarvestedAt = harvestedAt,
            WeightSource = "manual",
            IsWeightLocked = false
        };
    }

    /// <summary>
    /// Restore from persistence with all fields
    /// </summary>
    public static HarvestPlant RestoreFull(
        Guid id,
        Guid harvestId,
        Guid plantId,
        string plantTag,
        decimal wetWeight,
        string unitOfWeight,
        DateTime harvestedAt,
        Guid? scaleReadingId,
        string weightSource,
        bool isWeightLocked,
        DateTime? weightLockedAt,
        Guid? weightLockedByUserId)
    {
        return new HarvestPlant(id)
        {
            HarvestId = harvestId,
            PlantId = plantId,
            PlantTag = plantTag,
            WetWeight = wetWeight,
            UnitOfWeight = unitOfWeight,
            HarvestedAt = harvestedAt,
            ScaleReadingId = scaleReadingId,
            WeightSource = weightSource ?? "manual",
            IsWeightLocked = isWeightLocked,
            WeightLockedAt = weightLockedAt,
            WeightLockedByUserId = weightLockedByUserId
        };
    }
}




