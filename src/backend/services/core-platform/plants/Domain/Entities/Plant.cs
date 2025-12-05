using Harvestry.Plants.Domain.Enums;
using Harvestry.Plants.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Plants.Domain.Entities;

/// <summary>
/// Individual tagged plant aggregate root - represents a single cannabis plant tracked in METRC
/// </summary>
public sealed partial class Plant : AggregateRoot<Guid>
{
    // Private constructor for EF Core
    private Plant(Guid id) : base(id) { }

    private Plant(
        Guid id,
        Guid siteId,
        PlantTag plantTag,
        Guid batchId,
        Guid strainId,
        string strainName,
        PlantGrowthPhase growthPhase,
        Guid createdByUserId,
        DateOnly plantedDate,
        Guid? locationId = null,
        string? sublocationName = null,
        string? patientLicenseNumber = null) : base(id)
    {
        ValidateConstructorArgs(siteId, plantTag, batchId, strainId, strainName, createdByUserId);

        SiteId = siteId;
        PlantTag = plantTag;
        BatchId = batchId;
        StrainId = strainId;
        StrainName = strainName.Trim();
        GrowthPhase = growthPhase;
        Status = PlantStatus.Active;
        LocationId = locationId;
        SublocationName = sublocationName?.Trim();
        PlantedDate = plantedDate;
        PatientLicenseNumber = patientLicenseNumber?.Trim();
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public PlantTag PlantTag { get; private set; } = null!;
    public Guid BatchId { get; private set; }
    public Guid StrainId { get; private set; }
    public string StrainName { get; private set; } = string.Empty;

    // Growth tracking
    public PlantGrowthPhase GrowthPhase { get; private set; }
    public PlantStatus Status { get; private set; }
    public DateOnly PlantedDate { get; private set; }
    public DateOnly? VegetativeDate { get; private set; }
    public DateOnly? FloweringDate { get; private set; }

    // Location
    public Guid? LocationId { get; private set; }
    public string? SublocationName { get; private set; }

    // Medical tracking (for medical states)
    public string? PatientLicenseNumber { get; private set; }

    // Harvest tracking
    public Guid? HarvestId { get; private set; }
    public DateOnly? HarvestDate { get; private set; }
    public decimal? HarvestWetWeight { get; private set; }
    public string? HarvestWeightUnit { get; private set; }

    // Destruction tracking
    public DateOnly? DestroyedDate { get; private set; }
    public PlantDestroyReason? DestroyReason { get; private set; }
    public string? DestroyReasonNote { get; private set; }
    public decimal? WasteWeight { get; private set; }
    public string? WasteWeightUnit { get; private set; }
    public WasteMethod? WasteMethod { get; private set; }
    public Guid? DestroyedByUserId { get; private set; }
    public Guid? DestroyWitnessUserId { get; private set; }

    // METRC sync tracking
    public long? MetrcPlantId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Notes and metadata
    public string? Notes { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new plant from a batch
    /// </summary>
    public static Plant Create(
        Guid siteId,
        PlantTag plantTag,
        Guid batchId,
        Guid strainId,
        string strainName,
        PlantGrowthPhase growthPhase,
        Guid createdByUserId,
        DateOnly plantedDate,
        Guid? locationId = null,
        string? sublocationName = null,
        string? patientLicenseNumber = null)
    {
        return new Plant(
            Guid.NewGuid(),
            siteId,
            plantTag,
            batchId,
            strainId,
            strainName,
            growthPhase,
            createdByUserId,
            plantedDate,
            locationId,
            sublocationName,
            patientLicenseNumber);
    }

    /// <summary>
    /// Transition plant to vegetative phase
    /// </summary>
    public void TransitionToVegetative(DateOnly vegetativeDate, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (GrowthPhase != PlantGrowthPhase.Immature)
            throw new InvalidOperationException($"Cannot transition to vegetative from {GrowthPhase}");

        GrowthPhase = PlantGrowthPhase.Vegetative;
        VegetativeDate = vegetativeDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transition plant to flowering phase
    /// </summary>
    public void TransitionToFlowering(DateOnly floweringDate, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (GrowthPhase != PlantGrowthPhase.Vegetative && GrowthPhase != PlantGrowthPhase.Mother)
            throw new InvalidOperationException($"Cannot transition to flowering from {GrowthPhase}");

        GrowthPhase = PlantGrowthPhase.Flowering;
        FloweringDate = floweringDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Designate plant as a mother plant
    /// </summary>
    public void DesignateAsMother(Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (GrowthPhase != PlantGrowthPhase.Vegetative)
            throw new InvalidOperationException("Only vegetative plants can be designated as mothers");

        GrowthPhase = PlantGrowthPhase.Mother;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update plant location
    /// </summary>
    public void UpdateLocation(Guid? locationId, string? sublocationName, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        LocationId = locationId;
        SublocationName = sublocationName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Harvest the plant
    /// </summary>
    public void Harvest(
        Guid harvestId,
        DateOnly harvestDate,
        decimal wetWeight,
        string weightUnit,
        Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (GrowthPhase != PlantGrowthPhase.Flowering && GrowthPhase != PlantGrowthPhase.Vegetative)
            throw new InvalidOperationException($"Cannot harvest plant in {GrowthPhase} phase");

        if (wetWeight <= 0)
            throw new ArgumentException("Wet weight must be greater than 0", nameof(wetWeight));

        HarvestId = harvestId;
        HarvestDate = harvestDate;
        HarvestWetWeight = wetWeight;
        HarvestWeightUnit = weightUnit?.Trim() ?? "Grams";
        GrowthPhase = PlantGrowthPhase.Harvested;
        Status = PlantStatus.Harvested;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Destroy/dispose of the plant
    /// </summary>
    public void Destroy(
        DateOnly destroyedDate,
        PlantDestroyReason reason,
        decimal wasteWeight,
        string wasteWeightUnit,
        WasteMethod wasteMethod,
        Guid destroyedByUserId,
        Guid? witnessUserId = null,
        string? reasonNote = null)
    {
        ValidateUserId(destroyedByUserId);
        ValidateActiveStatus();

        if (wasteWeight < 0)
            throw new ArgumentException("Waste weight cannot be negative", nameof(wasteWeight));

        DestroyedDate = destroyedDate;
        DestroyReason = reason;
        DestroyReasonNote = reasonNote?.Trim();
        WasteWeight = wasteWeight;
        WasteWeightUnit = wasteWeightUnit?.Trim() ?? "Grams";
        WasteMethod = wasteMethod;
        DestroyedByUserId = destroyedByUserId;
        DestroyWitnessUserId = witnessUserId;
        GrowthPhase = PlantGrowthPhase.Destroyed;
        Status = PlantStatus.Destroyed;
        UpdatedByUserId = destroyedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Place plant on regulatory hold
    /// </summary>
    public void PlaceOnHold(string reason, Guid userId)
    {
        ValidateUserId(userId);
        
        if (Status != PlantStatus.Active)
            throw new InvalidOperationException($"Cannot place plant on hold with status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Hold reason is required", nameof(reason));

        Status = PlantStatus.OnHold;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Hold: {reason.Trim()}"
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Hold: {reason.Trim()}";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release plant from hold
    /// </summary>
    public void ReleaseFromHold(Guid userId)
    {
        ValidateUserId(userId);
        
        if (Status != PlantStatus.OnHold)
            throw new InvalidOperationException("Plant is not on hold");

        Status = PlantStatus.Active;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"Released from hold"
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] Released from hold";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcPlantId, string? syncStatus = null)
    {
        MetrcPlantId = metrcPlantId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add notes to the plant
    /// </summary>
    public void AddNotes(string notes, Guid userId)
    {
        ValidateUserId(userId);
        
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? notes.Trim()
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {notes.Trim()}";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateActiveStatus()
    {
        if (Status != PlantStatus.Active && Status != PlantStatus.OnHold)
            throw new InvalidOperationException($"Cannot modify plant with status {Status}");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        PlantTag plantTag,
        Guid batchId,
        Guid strainId,
        string strainName,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (plantTag == null)
            throw new ArgumentNullException(nameof(plantTag));

        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch ID cannot be empty", nameof(batchId));

        if (strainId == Guid.Empty)
            throw new ArgumentException("Strain ID cannot be empty", nameof(strainId));

        if (string.IsNullOrWhiteSpace(strainName))
            throw new ArgumentException("Strain name cannot be empty", nameof(strainName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}




