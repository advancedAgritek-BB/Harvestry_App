using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Genetics aggregate root - represents the core genetic profile of cannabis varieties
/// </summary>
public sealed class Genetics : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private Genetics(Guid id) : base(id) { }

    private Genetics(
        Guid id,
        Guid siteId,
        string name,
        string description,
        GeneticType geneticType,
        (decimal Min, decimal Max) thcRange,
        (decimal Min, decimal Max) cbdRange,
        int? floweringTimeDays,
        YieldPotential yieldPotential,
        GeneticProfile growthCharacteristics,
        TerpeneProfile terpeneProfile,
        Guid createdByUserId,
        string? breedingNotes = null) : base(id)
    {
        ValidateConstructorArgs(siteId, name, description, thcRange, cbdRange, floweringTimeDays, createdByUserId);

        SiteId = siteId;
        Name = name.Trim();
        Description = description.Trim();
        GeneticType = geneticType;
        ThcMinPercentage = thcRange.Min;
        ThcMaxPercentage = thcRange.Max;
        CbdMinPercentage = cbdRange.Min;
        CbdMaxPercentage = cbdRange.Max;
        FloweringTimeDays = floweringTimeDays;
        YieldPotential = yieldPotential;
        GrowthCharacteristics = growthCharacteristics;
        TerpeneProfile = terpeneProfile;
        BreedingNotes = breedingNotes?.Trim();
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public GeneticType GeneticType { get; private set; }
    public decimal ThcMinPercentage { get; private set; }
    public decimal ThcMaxPercentage { get; private set; }
    public decimal CbdMinPercentage { get; private set; }
    public decimal CbdMaxPercentage { get; private set; }
    public int? FloweringTimeDays { get; private set; }
    public YieldPotential YieldPotential { get; private set; }
    public GeneticProfile GrowthCharacteristics { get; private set; }
    public TerpeneProfile TerpeneProfile { get; private set; }
    public string? BreedingNotes { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new genetics
    /// </summary>
    public static Genetics Create(
        Guid siteId,
        string name,
        string description,
        GeneticType geneticType,
        (decimal Min, decimal Max) thcRange,
        (decimal Min, decimal Max) cbdRange,
        int? floweringTimeDays,
        YieldPotential yieldPotential,
        GeneticProfile growthCharacteristics,
        TerpeneProfile terpeneProfile,
        Guid createdByUserId,
        string? breedingNotes = null)
    {
        return new Genetics(
            Guid.NewGuid(),
            siteId,
            name,
            description,
            geneticType,
            thcRange,
            cbdRange,
            floweringTimeDays,
            yieldPotential,
            growthCharacteristics,
            terpeneProfile,
            createdByUserId,
            breedingNotes);
    }

    /// <summary>
    /// Factory method to rehydrate genetics from persistence
    /// </summary>
    public static Genetics FromPersistence(
        Guid id,
        Guid siteId,
        string name,
        string description,
        GeneticType geneticType,
        (decimal Min, decimal Max) thcRange,
        (decimal Min, decimal Max) cbdRange,
        int? floweringTimeDays,
        YieldPotential yieldPotential,
        GeneticProfile growthCharacteristics,
        TerpeneProfile terpeneProfile,
        Guid createdByUserId,
        string? breedingNotes,
        DateTime createdAt,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var genetics = new Genetics(id)
        {
            SiteId = siteId,
            Name = name,
            Description = description,
            GeneticType = geneticType,
            ThcMinPercentage = thcRange.Min,
            ThcMaxPercentage = thcRange.Max,
            CbdMinPercentage = cbdRange.Min,
            CbdMaxPercentage = cbdRange.Max,
            FloweringTimeDays = floweringTimeDays,
            YieldPotential = yieldPotential,
            GrowthCharacteristics = growthCharacteristics,
            TerpeneProfile = terpeneProfile,
            BreedingNotes = breedingNotes,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return genetics;
    }

    /// <summary>
    /// Update genetic profile information
    /// </summary>
    public void UpdateProfile(
        string description,
        GeneticProfile growthCharacteristics,
        TerpeneProfile terpeneProfile,
        Guid updatedByUserId,
        string? breedingNotes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Description = description.Trim();
        GrowthCharacteristics = growthCharacteristics;
        TerpeneProfile = terpeneProfile;
        BreedingNotes = breedingNotes?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update cannabinoid ranges (THC/CBD)
    /// </summary>
    public void UpdateCannabinoidRanges(
        (decimal Min, decimal Max) thcRange,
        (decimal Min, decimal Max) cbdRange,
        Guid updatedByUserId)
    {
        ValidateCannabinoidRange(thcRange.Min, thcRange.Max, "THC");
        ValidateCannabinoidRange(cbdRange.Min, cbdRange.Max, "CBD");

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        ThcMinPercentage = thcRange.Min;
        ThcMaxPercentage = thcRange.Max;
        CbdMinPercentage = cbdRange.Min;
        CbdMaxPercentage = cbdRange.Max;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update flowering time
    /// </summary>
    public void UpdateFloweringTime(int? floweringTimeDays, Guid updatedByUserId)
    {
        if (floweringTimeDays.HasValue && (floweringTimeDays.Value < 1 || floweringTimeDays.Value > 365))
            throw new ArgumentException("Flowering time must be between 1 and 365 days", nameof(floweringTimeDays));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        FloweringTimeDays = floweringTimeDays;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update yield potential
    /// </summary>
    public void UpdateYieldPotential(YieldPotential yieldPotential, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        YieldPotential = yieldPotential;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        string name,
        string description,
        (decimal Min, decimal Max) thcRange,
        (decimal Min, decimal Max) cbdRange,
        int? floweringTimeDays,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        // Trim inputs before validation
        var trimmedName = name?.Trim();
        var trimmedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (trimmedName.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(trimmedDescription))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (trimmedDescription.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));

        ValidateCannabinoidRange(thcRange.Min, thcRange.Max, "THC");
        ValidateCannabinoidRange(cbdRange.Min, cbdRange.Max, "CBD");

        if (floweringTimeDays.HasValue && (floweringTimeDays.Value < 1 || floweringTimeDays.Value > 365))
            throw new ArgumentException("Flowering time must be between 1 and 365 days", nameof(floweringTimeDays));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }

    private static void ValidateCannabinoidRange(decimal min, decimal max, string cannabinoidName)
    {
        if (min < 0 || min > 100)
            throw new ArgumentException($"{cannabinoidName} minimum must be between 0 and 100", nameof(min));

        if (max < 0 || max > 100)
            throw new ArgumentException($"{cannabinoidName} maximum must be between 0 and 100", nameof(max));

        if (max < min)
            throw new ArgumentException($"{cannabinoidName} maximum must be greater than or equal to minimum", nameof(max));
    }
}

