using Harvestry.Harvests.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Harvest aggregate root - represents a harvest event in METRC
/// </summary>
public sealed partial class Harvest : AggregateRoot<Guid>
{
    private readonly List<HarvestPlant> _harvestPlants = new();
    private readonly List<HarvestWaste> _wasteRecords = new();

    // Private constructor for EF Core
    private Harvest(Guid id) : base(id) { }

    private Harvest(
        Guid id,
        Guid siteId,
        string harvestName,
        HarvestType harvestType,
        Guid strainId,
        string strainName,
        Guid createdByUserId,
        Guid? locationId = null,
        string? sublocationName = null) : base(id)
    {
        ValidateConstructorArgs(siteId, harvestName, strainId, strainName, createdByUserId);

        SiteId = siteId;
        HarvestName = harvestName.Trim();
        HarvestType = harvestType;
        StrainId = strainId;
        StrainName = strainName.Trim();
        LocationId = locationId;
        SublocationName = sublocationName?.Trim();
        Status = HarvestStatus.Active;
        HarvestStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        TotalWetWeight = 0;
        TotalDryWeight = 0;
        CurrentWeight = 0;
        TotalWasteWeight = 0;
        UnitOfWeight = "Grams";
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Core identification
    public Guid SiteId { get; private set; }
    public string HarvestName { get; private set; } = string.Empty;
    public HarvestType HarvestType { get; private set; }
    public Guid StrainId { get; private set; }
    public string StrainName { get; private set; } = string.Empty;

    // Location
    public Guid? LocationId { get; private set; }
    public string? LocationName { get; private set; }
    public string? SublocationName { get; private set; }

    // Dates
    public DateOnly HarvestStartDate { get; private set; }
    public DateOnly? HarvestEndDate { get; private set; }
    public DateOnly? DryingDate { get; private set; }

    // Weights
    public decimal TotalWetWeight { get; private set; }
    public decimal TotalDryWeight { get; private set; }
    public decimal CurrentWeight { get; private set; }
    public decimal TotalWasteWeight { get; private set; }
    public string UnitOfWeight { get; private set; } = "Grams";

    // Status
    public HarvestStatus Status { get; private set; }
    public string? Notes { get; private set; }

    // METRC sync tracking
    public long? MetrcHarvestId { get; private set; }
    public DateTime? MetrcLastSyncAt { get; private set; }
    public string? MetrcSyncStatus { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    // Navigation collections
    public IReadOnlyCollection<HarvestPlant> HarvestPlants => _harvestPlants.AsReadOnly();
    public IReadOnlyCollection<HarvestWaste> WasteRecords => _wasteRecords.AsReadOnly();

    /// <summary>
    /// Factory method to create a new harvest
    /// </summary>
    public static Harvest Create(
        Guid siteId,
        string harvestName,
        HarvestType harvestType,
        Guid strainId,
        string strainName,
        Guid createdByUserId,
        Guid? locationId = null,
        string? sublocationName = null)
    {
        return new Harvest(
            Guid.NewGuid(),
            siteId,
            harvestName,
            harvestType,
            strainId,
            strainName,
            createdByUserId,
            locationId,
            sublocationName);
    }

    /// <summary>
    /// Add a plant to this harvest
    /// </summary>
    public void AddPlant(Guid plantId, string plantTag, decimal wetWeight, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (string.IsNullOrWhiteSpace(plantTag))
            throw new ArgumentException("Plant tag is required", nameof(plantTag));

        if (wetWeight <= 0)
            throw new ArgumentException("Wet weight must be greater than 0", nameof(wetWeight));

        if (_harvestPlants.Any(hp => hp.PlantId == plantId))
            throw new InvalidOperationException($"Plant {plantTag} is already in this harvest");

        var harvestPlant = HarvestPlant.Create(Id, plantId, plantTag, wetWeight, UnitOfWeight);
        _harvestPlants.Add(harvestPlant);

        TotalWetWeight += wetWeight;
        CurrentWeight += wetWeight;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record wet weight for the harvest
    /// </summary>
    public void RecordWetWeight(decimal wetWeight, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (wetWeight < 0)
            throw new ArgumentException("Wet weight cannot be negative", nameof(wetWeight));

        TotalWetWeight = wetWeight;
        CurrentWeight = wetWeight;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record dry weight for the harvest
    /// </summary>
    public void RecordDryWeight(decimal dryWeight, DateOnly dryingDate, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (dryWeight < 0)
            throw new ArgumentException("Dry weight cannot be negative", nameof(dryWeight));

        if (dryWeight > TotalWetWeight)
            throw new ArgumentException("Dry weight cannot exceed wet weight", nameof(dryWeight));

        TotalDryWeight = dryWeight;
        CurrentWeight = dryWeight;
        DryingDate = dryingDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record waste from harvest
    /// </summary>
    public void RecordWaste(
        HarvestWasteType wasteType,
        decimal wasteWeight,
        WasteMethod wasteMethod,
        DateOnly actualDate,
        Guid userId,
        string? notes = null)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (wasteWeight <= 0)
            throw new ArgumentException("Waste weight must be greater than 0", nameof(wasteWeight));

        var waste = HarvestWaste.Create(Id, wasteType, wasteWeight, UnitOfWeight, wasteMethod, actualDate, userId, notes);
        _wasteRecords.Add(waste);

        TotalWasteWeight += wasteWeight;
        CurrentWeight -= wasteWeight;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deduct weight when creating packages from this harvest
    /// </summary>
    public void DeductWeightForPackage(decimal weight, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        if (weight <= 0)
            throw new ArgumentException("Weight must be greater than 0", nameof(weight));

        if (weight > CurrentWeight)
            throw new InvalidOperationException($"Cannot deduct {weight} {UnitOfWeight} - only {CurrentWeight} {UnitOfWeight} available");

        CurrentWeight -= weight;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update harvest location
    /// </summary>
    public void UpdateLocation(Guid? locationId, string? locationName, string? sublocationName, Guid userId)
    {
        ValidateUserId(userId);
        ValidateActiveStatus();

        LocationId = locationId;
        LocationName = locationName?.Trim();
        SublocationName = sublocationName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rename the harvest
    /// </summary>
    public void Rename(string newName, Guid userId)
    {
        ValidateUserId(userId);
        
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Harvest name cannot be empty", nameof(newName));

        if (newName.Length > 200)
            throw new ArgumentException("Harvest name cannot exceed 200 characters", nameof(newName));

        HarvestName = newName.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Place harvest on hold
    /// </summary>
    public void PlaceOnHold(string reason, Guid userId)
    {
        ValidateUserId(userId);
        
        if (Status != HarvestStatus.Active)
            throw new InvalidOperationException($"Cannot place harvest on hold with status {Status}");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Hold reason is required", nameof(reason));

        Status = HarvestStatus.OnHold;
        AddNote($"Placed on hold: {reason}", userId);
    }

    /// <summary>
    /// Release harvest from hold
    /// </summary>
    public void ReleaseFromHold(Guid userId)
    {
        ValidateUserId(userId);
        
        if (Status != HarvestStatus.OnHold)
            throw new InvalidOperationException("Harvest is not on hold");

        Status = HarvestStatus.Active;
        AddNote("Released from hold", userId);
    }

    /// <summary>
    /// Finish/complete the harvest
    /// </summary>
    public void Finish(DateOnly finishDate, Guid userId)
    {
        ValidateUserId(userId);

        if (Status == HarvestStatus.Finished)
            throw new InvalidOperationException("Harvest is already finished");

        Status = HarvestStatus.Finished;
        HarvestEndDate = finishDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unfinish/reopen the harvest
    /// </summary>
    public void Unfinish(Guid userId)
    {
        ValidateUserId(userId);

        if (Status != HarvestStatus.Finished)
            throw new InvalidOperationException("Harvest is not finished");

        Status = HarvestStatus.Active;
        HarvestEndDate = null;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcHarvestId, string? syncStatus = null)
    {
        MetrcHarvestId = metrcHarvestId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add notes to the harvest
    /// </summary>
    public void AddNote(string note, Guid userId)
    {
        ValidateUserId(userId);
        
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("Note cannot be empty", nameof(note));

        Notes = string.IsNullOrWhiteSpace(Notes)
            ? note.Trim()
            : $"{Notes}\n\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] {note.Trim()}";
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set harvest plants from persistence
    /// </summary>
    public void SetHarvestPlants(IEnumerable<HarvestPlant> plants)
    {
        _harvestPlants.Clear();
        if (plants != null)
            _harvestPlants.AddRange(plants);
    }

    /// <summary>
    /// Set waste records from persistence
    /// </summary>
    public void SetWasteRecords(IEnumerable<HarvestWaste> wasteRecords)
    {
        _wasteRecords.Clear();
        if (wasteRecords != null)
            _wasteRecords.AddRange(wasteRecords);
    }

    private void ValidateActiveStatus()
    {
        if (Status == HarvestStatus.Finished)
            throw new InvalidOperationException("Cannot modify a finished harvest");
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        string harvestName,
        Guid strainId,
        string strainName,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(harvestName))
            throw new ArgumentException("Harvest name cannot be empty", nameof(harvestName));

        if (harvestName.Length > 200)
            throw new ArgumentException("Harvest name cannot exceed 200 characters", nameof(harvestName));

        if (strainId == Guid.Empty)
            throw new ArgumentException("Strain ID cannot be empty", nameof(strainId));

        if (string.IsNullOrWhiteSpace(strainName))
            throw new ArgumentException("Strain name cannot be empty", nameof(strainName));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}









