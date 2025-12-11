using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Strain aggregate root - named combination of genetics + phenotype with cultivation details
/// </summary>
public sealed class Strain : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private Strain(Guid id) : base(id) { }

    private Strain(
        Guid id,
        Guid siteId,
        Guid geneticsId,
        Guid? phenotypeId,
        string name,
        string description,
        Guid createdByUserId,
        string? breeder = null,
        string? seedBank = null,
        string? cultivationNotes = null,
        int? expectedHarvestWindowDays = null,
        TargetEnvironment? targetEnvironment = null,
        ComplianceRequirements? complianceRequirements = null) : base(id)
    {
        ValidateConstructorArgs(siteId, geneticsId, name, description, expectedHarvestWindowDays, createdByUserId);

        SiteId = siteId;
        GeneticsId = geneticsId;
        PhenotypeId = phenotypeId;
        Name = name.Trim();
        Breeder = breeder?.Trim();
        SeedBank = seedBank?.Trim();
        Description = description.Trim();
        CultivationNotes = cultivationNotes?.Trim();
        ExpectedHarvestWindowDays = expectedHarvestWindowDays;
        TargetEnvironment = targetEnvironment ?? ValueObjects.TargetEnvironment.Empty;
        ComplianceRequirements = complianceRequirements ?? ValueObjects.ComplianceRequirements.Empty;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid GeneticsId { get; private set; }
    public Guid? PhenotypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Breeder { get; private set; }
    public string? SeedBank { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? CultivationNotes { get; private set; }
    public int? ExpectedHarvestWindowDays { get; private set; }
    public TargetEnvironment TargetEnvironment { get; private set; }
    public ComplianceRequirements ComplianceRequirements { get; private set; }

    // Crop Steering
    /// <summary>
    /// Optional reference to a strain-specific crop steering profile.
    /// If null, site default steering profile will be used.
    /// </summary>
    public Guid? CropSteeringProfileId { get; private set; }

    // METRC Compliance Fields
    /// <summary>
    /// Genetic classification (Indica, Sativa, Hybrid)
    /// </summary>
    public GeneticClassification GeneticClassification { get; private set; }

    /// <summary>
    /// Testing status for the strain
    /// </summary>
    public StrainTestingStatus TestingStatus { get; private set; }

    /// <summary>
    /// Nominal/expected THC percentage for this strain
    /// </summary>
    public decimal? NominalThcPercent { get; private set; }

    /// <summary>
    /// Nominal/expected CBD percentage for this strain
    /// </summary>
    public decimal? NominalCbdPercent { get; private set; }

    /// <summary>
    /// METRC's internal strain identifier
    /// </summary>
    public long? MetrcStrainId { get; private set; }

    /// <summary>
    /// Last successful sync with METRC
    /// </summary>
    public DateTime? MetrcLastSyncAt { get; private set; }

    /// <summary>
    /// METRC sync status message
    /// </summary>
    public string? MetrcSyncStatus { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new strain
    /// </summary>
    public static Strain Create(
        Guid siteId,
        Guid geneticsId,
        Guid? phenotypeId,
        string name,
        string description,
        Guid createdByUserId,
        string? breeder = null,
        string? seedBank = null,
        string? cultivationNotes = null,
        int? expectedHarvestWindowDays = null,
        TargetEnvironment? targetEnvironment = null,
        ComplianceRequirements? complianceRequirements = null)
    {
        return new Strain(
            Guid.NewGuid(),
            siteId,
            geneticsId,
            phenotypeId,
            name,
            description,
            createdByUserId,
            breeder,
            seedBank,
            cultivationNotes,
            expectedHarvestWindowDays,
            targetEnvironment,
            complianceRequirements);
    }

    /// <summary>
    /// Factory method to rehydrate strain from persistence
    /// </summary>
    public static Strain FromPersistence(
        Guid id,
        Guid siteId,
        Guid geneticsId,
        Guid? phenotypeId,
        string name,
        string description,
        string? breeder,
        string? seedBank,
        string? cultivationNotes,
        int? expectedHarvestWindowDays,
        TargetEnvironment targetEnvironment,
        ComplianceRequirements complianceRequirements,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId,
        // METRC fields
        GeneticClassification geneticClassification = GeneticClassification.Unspecified,
        StrainTestingStatus testingStatus = StrainTestingStatus.None,
        decimal? nominalThcPercent = null,
        decimal? nominalCbdPercent = null,
        long? metrcStrainId = null,
        DateTime? metrcLastSyncAt = null,
        string? metrcSyncStatus = null,
        // Crop steering
        Guid? cropSteeringProfileId = null)
    {
        var strain = new Strain(id)
        {
            SiteId = siteId,
            GeneticsId = geneticsId,
            PhenotypeId = phenotypeId,
            Name = name,
            Description = description,
            Breeder = breeder,
            SeedBank = seedBank,
            CultivationNotes = cultivationNotes,
            ExpectedHarvestWindowDays = expectedHarvestWindowDays,
            TargetEnvironment = targetEnvironment,
            ComplianceRequirements = complianceRequirements,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId,
            // METRC fields
            GeneticClassification = geneticClassification,
            TestingStatus = testingStatus,
            NominalThcPercent = nominalThcPercent,
            NominalCbdPercent = nominalCbdPercent,
            MetrcStrainId = metrcStrainId,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus,
            // Crop steering
            CropSteeringProfileId = cropSteeringProfileId
        };

        return strain;
    }

    /// <summary>
    /// Update strain details
    /// </summary>
    public void Update(
        string description,
        string? breeder,
        string? seedBank,
        string? cultivationNotes,
        int? expectedHarvestWindowDays,
        TargetEnvironment targetEnvironment,
        ComplianceRequirements complianceRequirements,
        Guid updatedByUserId)
    {
        var trimmedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedDescription))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        // Validate description length (matching constructor validation)
        if (trimmedDescription.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));

        if (expectedHarvestWindowDays.HasValue && (expectedHarvestWindowDays.Value < 1 || expectedHarvestWindowDays.Value > 365))
            throw new ArgumentException("Expected harvest window must be between 1 and 365 days", nameof(expectedHarvestWindowDays));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Description = trimmedDescription;
        Breeder = breeder?.Trim();
        SeedBank = seedBank?.Trim();
        CultivationNotes = cultivationNotes?.Trim();
        ExpectedHarvestWindowDays = expectedHarvestWindowDays;
        TargetEnvironment = targetEnvironment;
        ComplianceRequirements = complianceRequirements;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update cultivation notes
    /// </summary>
    public void UpdateCultivationNotes(string? cultivationNotes, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        CultivationNotes = cultivationNotes?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set genetic classification for METRC
    /// </summary>
    public void SetGeneticClassification(GeneticClassification classification, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        GeneticClassification = classification;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set nominal potency values
    /// </summary>
    public void SetNominalPotency(decimal? thcPercent, decimal? cbdPercent, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        if (thcPercent.HasValue && (thcPercent.Value < 0 || thcPercent.Value > 100))
            throw new ArgumentException("THC percent must be between 0 and 100", nameof(thcPercent));

        if (cbdPercent.HasValue && (cbdPercent.Value < 0 || cbdPercent.Value > 100))
            throw new ArgumentException("CBD percent must be between 0 and 100", nameof(cbdPercent));

        NominalThcPercent = thcPercent;
        NominalCbdPercent = cbdPercent;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update testing status
    /// </summary>
    public void UpdateTestingStatus(StrainTestingStatus status, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        TestingStatus = status;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcStrainId, string? syncStatus = null)
    {
        MetrcStrainId = metrcStrainId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Configure METRC-specific fields
    /// </summary>
    public void ConfigureMetrc(
        GeneticClassification classification,
        decimal? nominalThcPercent,
        decimal? nominalCbdPercent,
        Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        GeneticClassification = classification;
        NominalThcPercent = nominalThcPercent;
        NominalCbdPercent = nominalCbdPercent;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set or clear the strain-specific crop steering profile.
    /// Pass null to use site default profile instead.
    /// </summary>
    public void SetCropSteeringProfile(Guid? cropSteeringProfileId, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        CropSteeringProfileId = cropSteeringProfileId;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if this strain has a custom steering profile assigned.
    /// </summary>
    public bool HasCustomSteeringProfile => CropSteeringProfileId.HasValue;

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid geneticsId,
        string name,
        string description,
        int? expectedHarvestWindowDays,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (geneticsId == Guid.Empty)
            throw new ArgumentException("Genetics ID cannot be empty", nameof(geneticsId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        if (description.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));

        if (expectedHarvestWindowDays.HasValue && (expectedHarvestWindowDays.Value < 1 || expectedHarvestWindowDays.Value > 365))
            throw new ArgumentException("Expected harvest window must be between 1 and 365 days", nameof(expectedHarvestWindowDays));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

