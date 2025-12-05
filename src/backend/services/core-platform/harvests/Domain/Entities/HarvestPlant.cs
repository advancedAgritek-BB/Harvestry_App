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
    }

    public Guid HarvestId { get; private set; }
    public Guid PlantId { get; private set; }
    public string PlantTag { get; private set; } = string.Empty;
    public decimal WetWeight { get; private set; }
    public string UnitOfWeight { get; private set; } = "Grams";
    public DateTime HarvestedAt { get; private set; }

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
            HarvestedAt = harvestedAt
        };
    }
}



