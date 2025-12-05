using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch aggregate root - represents a group of plants with shared genetics and lifecycle
/// </summary>
public sealed class Batch : AggregateRoot<Guid>
{
    private readonly List<BatchEvent> _events = new();

    // Private constructor for EF Core/rehydration
    private Batch(Guid id) : base(id) { }

    private Batch(
        Guid id,
        Guid siteId,
        Guid strainId,
        BatchCode batchCode,
        string batchName,
        BatchType batchType,
        BatchSourceType sourceType,
        int plantCount,
        Guid currentStageId,
        Guid createdByUserId,
        Guid? parentBatchId = null,
        int generation = 1,
        int? targetPlantCount = null) : base(id)
    {
        ValidateConstructorArgs(siteId, strainId, batchCode, batchName, plantCount, currentStageId, createdByUserId);

        SiteId = siteId;
        StrainId = strainId;
        BatchCode = batchCode;
        BatchName = batchName.Trim();
        BatchType = batchType;
        SourceType = sourceType;
        ParentBatchId = parentBatchId;
        Generation = generation;
        PlantCount = plantCount;
        TargetPlantCount = targetPlantCount ?? plantCount;
        CurrentStageId = currentStageId;
        StageStartedAt = DateTime.UtcNow;
        Status = BatchStatus.Active;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Add creation event
        _events.Add(BatchEvent.CreateBatchCreated(siteId, id, strainId, plantCount, createdByUserId));
    }

    public Guid SiteId { get; private set; }
    public Guid StrainId { get; private set; }
    public BatchCode BatchCode { get; private set; } = null!;
    public string BatchName { get; private set; } = string.Empty;
    public BatchType BatchType { get; private set; }
    public BatchSourceType SourceType { get; private set; }
    public Guid? ParentBatchId { get; private set; }
    public int Generation { get; private set; }
    public int PlantCount { get; private set; }
    public int TargetPlantCount { get; private set; }
    public Guid CurrentStageId { get; private set; }
    public DateTime StageStartedAt { get; private set; }
    public DateOnly? ExpectedHarvestDate { get; private set; }
    public DateOnly? ActualHarvestDate { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public BatchStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    // METRC Compliance Fields
    /// <summary>
    /// METRC's internal plant batch identifier (returned from API)
    /// </summary>
    public long? MetrcPlantBatchId { get; private set; }

    /// <summary>
    /// Propagation type for METRC (Clone, Seed, PlantMaterial)
    /// Maps to METRC's PlantBatch Type field
    /// </summary>
    public PropagationType PropagationType { get; private set; }

    /// <summary>
    /// Patient license number for medical batches (required in medical states)
    /// </summary>
    public string? PatientLicenseNumber { get; private set; }

    /// <summary>
    /// Sub-location within the primary location (METRC sublocation)
    /// </summary>
    public string? SublocationName { get; private set; }

    /// <summary>
    /// Source package label when batch is created from a package (METRC plantings from package)
    /// </summary>
    public string? SourcePackageLabel { get; private set; }

    /// <summary>
    /// Quantity used from source package when creating this batch
    /// </summary>
    public decimal? SourcePackageAdjustmentAmount { get; private set; }

    /// <summary>
    /// Unit of measure for source package adjustment (e.g., "Each", "Grams")
    /// </summary>
    public string? SourcePackageAdjustmentUom { get; private set; }

    /// <summary>
    /// Actual date for METRC reporting (may differ from created date)
    /// </summary>
    public DateOnly? ActualDate { get; private set; }

    /// <summary>
    /// Strain name cached for METRC reporting
    /// </summary>
    public string? StrainName { get; private set; }

    /// <summary>
    /// Location name cached for METRC reporting
    /// </summary>
    public string? LocationName { get; private set; }

    /// <summary>
    /// Last successful sync with METRC
    /// </summary>
    public DateTime? MetrcLastSyncAt { get; private set; }

    /// <summary>
    /// METRC sync status message (for troubleshooting)
    /// </summary>
    public string? MetrcSyncStatus { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyCollection<BatchEvent> Events => _events.AsReadOnly();

    public void ClearEvents() => _events.Clear();

    /// <summary>
    /// Factory method to create new batch
    /// </summary>
    public static Batch Create(
        Guid siteId,
        Guid strainId,
        BatchCode batchCode,
        string batchName,
        BatchType batchType,
        BatchSourceType sourceType,
        int plantCount,
        Guid currentStageId,
        Guid createdByUserId,
        Guid? parentBatchId = null,
        int generation = 1,
        int? targetPlantCount = null)
    {
        return new Batch(
            Guid.NewGuid(),
            siteId,
            strainId,
            batchCode,
            batchName,
            batchType,
            sourceType,
            plantCount,
            currentStageId,
            createdByUserId,
            parentBatchId,
            generation,
            targetPlantCount);
    }

    /// <summary>
    /// Factory method to rehydrate batch from persistence
    /// </summary>
    public static Batch FromPersistence(
        Guid id,
        Guid siteId,
        Guid strainId,
        BatchCode batchCode,
        string batchName,
        BatchType batchType,
        BatchSourceType sourceType,
        Guid? parentBatchId,
        int generation,
        int plantCount,
        int targetPlantCount,
        Guid currentStageId,
        DateTime stageStartedAt,
        DateOnly? expectedHarvestDate,
        DateOnly? actualHarvestDate,
        Guid? locationId,
        Guid? roomId,
        Guid? zoneId,
        BatchStatus status,
        string? notes,
        Dictionary<string, object> metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId,
        // METRC fields
        long? metrcPlantBatchId = null,
        PropagationType propagationType = PropagationType.Clone,
        string? patientLicenseNumber = null,
        string? sublocationName = null,
        string? sourcePackageLabel = null,
        decimal? sourcePackageAdjustmentAmount = null,
        string? sourcePackageAdjustmentUom = null,
        DateOnly? actualDate = null,
        string? strainName = null,
        string? locationName = null,
        DateTime? metrcLastSyncAt = null,
        string? metrcSyncStatus = null)
    {
        var batch = new Batch(id)
        {
            SiteId = siteId,
            StrainId = strainId,
            BatchCode = batchCode,
            BatchName = batchName,
            BatchType = batchType,
            SourceType = sourceType,
            ParentBatchId = parentBatchId,
            Generation = generation,
            PlantCount = plantCount,
            TargetPlantCount = targetPlantCount,
            CurrentStageId = currentStageId,
            StageStartedAt = stageStartedAt,
            ExpectedHarvestDate = expectedHarvestDate,
            ActualHarvestDate = actualHarvestDate,
            LocationId = locationId,
            RoomId = roomId,
            ZoneId = zoneId,
            Status = status,
            Notes = notes,
            Metadata = metadata,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId,
            // METRC fields
            MetrcPlantBatchId = metrcPlantBatchId,
            PropagationType = propagationType,
            PatientLicenseNumber = patientLicenseNumber,
            SublocationName = sublocationName,
            SourcePackageLabel = sourcePackageLabel,
            SourcePackageAdjustmentAmount = sourcePackageAdjustmentAmount,
            SourcePackageAdjustmentUom = sourcePackageAdjustmentUom,
            ActualDate = actualDate,
            StrainName = strainName,
            LocationName = locationName,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus
        };
        return batch;
    }

    /// <summary>
    /// Change batch to a new stage
    /// </summary>
    public void ChangeStage(
        Guid newStageId,
        Guid userId,
        string? notes = null)
    {
        if (newStageId == Guid.Empty)
            throw new ArgumentException("New stage ID cannot be empty", nameof(newStageId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != BatchStatus.Active)
            throw new InvalidOperationException($"Cannot change stage for batch with status: {Status}");

        var oldStageId = CurrentStageId;
        CurrentStageId = newStageId;
        StageStartedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Add stage change event
        _events.Add(BatchEvent.CreateStageChange(SiteId, Id, oldStageId, newStageId, userId, notes));
    }

    /// <summary>
    /// Update batch location
    /// </summary>
    public void UpdateLocation(
        Guid? locationId,
        Guid? roomId,
        Guid? zoneId,
        Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        LocationId = locationId;
        RoomId = roomId;
        ZoneId = zoneId;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Add location change event
        var eventData = new Dictionary<string, object>();
        if (locationId.HasValue) eventData["locationId"] = locationId.Value;
        if (roomId.HasValue) eventData["roomId"] = roomId.Value;
        if (zoneId.HasValue) eventData["zoneId"] = zoneId.Value;

        _events.Add(BatchEvent.Create(SiteId, Id, EventType.LocationChange, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Update plant count (mortality, culling, etc.)
    /// </summary>
    public void UpdatePlantCount(
        int newCount,
        string reason,
        Guid userId)
    {
        if (newCount < 0)
            throw new ArgumentException("Plant count cannot be negative", nameof(newCount));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for plant count changes", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var oldCount = PlantCount;
        PlantCount = newCount;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Add plant count change event
        _events.Add(BatchEvent.CreatePlantCountChange(SiteId, Id, oldCount, newCount, reason, userId));
    }

    /// <summary>
    /// Quarantine the batch
    /// </summary>
    public void Quarantine(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for quarantine", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = BatchStatus.Quarantine;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        var eventData = new Dictionary<string, object> { ["reason"] = reason };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.Quarantine, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Release batch from quarantine
    /// </summary>
    public void ReleaseFromQuarantine(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != BatchStatus.Quarantine)
            throw new InvalidOperationException("Batch is not in quarantine");

        var previousStatus = Status;
        Status = BatchStatus.Active;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event for audit trail
        var eventData = new Dictionary<string, object>
        {
            ["previousStatus"] = previousStatus.ToString(),
            ["newStatus"] = Status.ToString(),
            ["batchCode"] = BatchCode
        };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.ReleaseFromQuarantine, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Place batch on hold
    /// </summary>
    public void Hold(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for hold", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = BatchStatus.Hold;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        var eventData = new Dictionary<string, object> { ["reason"] = reason };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.Hold, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Harvest the batch
    /// </summary>
    public void Harvest(DateOnly harvestDate, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != BatchStatus.Active)
            throw new InvalidOperationException($"Cannot harvest batch with status: {Status}");

        ActualHarvestDate = harvestDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        var eventData = new Dictionary<string, object> { ["harvestDate"] = harvestDate };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.Harvest, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Complete the batch (transferred to inventory)
    /// </summary>
    public void Complete(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = BatchStatus.Completed;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Destroy the batch
    /// </summary>
    public void Destroy(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for destruction", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = BatchStatus.Destroyed;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        var eventData = new Dictionary<string, object> { ["reason"] = reason };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.Destroy, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Add notes to the batch (appends to existing notes)
    /// </summary>
    public void AddNotes(string notes, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var trimmedNotes = notes.Trim();
        var previousNotes = Notes;
        
        // Preserve existing notes by appending new notes
        if (string.IsNullOrWhiteSpace(Notes))
        {
            Notes = trimmedNotes;
        }
        else
        {
            Notes = $"{Notes}\n\n--- {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} ---\n{trimmedNotes}";
        }
        
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;

        var eventData = new Dictionary<string, object> 
        { 
            ["notes"] = trimmedNotes,
            ["previousNotes"] = previousNotes ?? string.Empty
        };
        _events.Add(BatchEvent.Create(SiteId, Id, EventType.NoteAdded, userId, DateTime.UtcNow, eventData));
    }

    /// <summary>
    /// Update expected harvest date
    /// </summary>
    public void UpdateExpectedHarvestDate(DateOnly? expectedDate, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        ExpectedHarvestDate = expectedDate;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update batch name
    /// </summary>
    public void UpdateName(string newName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Batch name cannot be empty", nameof(newName));

        if (newName.Length > 200)
            throw new ArgumentException("Batch name cannot exceed 200 characters", nameof(newName));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        BatchName = newName.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update target plant count
    /// </summary>
    public void UpdateTargetPlantCount(int targetCount, Guid userId)
    {
        if (targetCount < 1)
            throw new ArgumentException("Target plant count must be at least 1", nameof(targetCount));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        TargetPlantCount = targetCount;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update metadata
    /// </summary>
    public void UpdateMetadata(Dictionary<string, object> metadata, Guid userId)
    {
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Metadata = new Dictionary<string, object>(metadata);
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if batch can be split
    /// </summary>
    public bool CanSplit(int plantCountToSplit)
    {
        if (Status != BatchStatus.Active)
            return false;

        if (plantCountToSplit < 1)
            return false;

        if (plantCountToSplit >= PlantCount)
            return false; // Must leave at least 1 plant in original batch

        return true;
    }

    /// <summary>
    /// Get time spent in current stage
    /// </summary>
    public TimeSpan GetStageDuration()
    {
        return DateTime.UtcNow - StageStartedAt;
    }

    /// <summary>
    /// Configure METRC-specific fields for the batch
    /// </summary>
    public void ConfigureMetrc(
        PropagationType propagationType,
        string? strainName,
        string? locationName,
        string? sublocationName = null,
        string? patientLicenseNumber = null,
        DateOnly? actualDate = null)
    {
        PropagationType = propagationType;
        StrainName = strainName?.Trim();
        LocationName = locationName?.Trim();
        SublocationName = sublocationName?.Trim();
        PatientLicenseNumber = patientLicenseNumber?.Trim();
        ActualDate = actualDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set source package information when batch is created from a package
    /// </summary>
    public void SetSourcePackage(
        string packageLabel,
        decimal adjustmentAmount,
        string adjustmentUom,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(packageLabel))
            throw new ArgumentException("Package label is required", nameof(packageLabel));

        if (adjustmentAmount <= 0)
            throw new ArgumentException("Adjustment amount must be positive", nameof(adjustmentAmount));

        if (string.IsNullOrWhiteSpace(adjustmentUom))
            throw new ArgumentException("Unit of measure is required", nameof(adjustmentUom));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        SourcePackageLabel = packageLabel.Trim();
        SourcePackageAdjustmentAmount = adjustmentAmount;
        SourcePackageAdjustmentUom = adjustmentUom.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcPlantBatchId, string? syncStatus = null)
    {
        MetrcPlantBatchId = metrcPlantBatchId;
        MetrcLastSyncAt = DateTime.UtcNow;
        MetrcSyncStatus = syncStatus ?? "Synced";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync status (for error reporting)
    /// </summary>
    public void UpdateMetrcSyncStatus(bool success, string? statusMessage = null)
    {
        if (success)
        {
            MetrcLastSyncAt = DateTime.UtcNow;
            MetrcSyncStatus = statusMessage ?? "Sync successful";
        }
        else
        {
            MetrcSyncStatus = statusMessage ?? "Sync failed";
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update patient license number (for medical batches)
    /// </summary>
    public void UpdatePatientLicenseNumber(string? patientLicenseNumber, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        PatientLicenseNumber = patientLicenseNumber?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update sublocation name
    /// </summary>
    public void UpdateSublocation(string? sublocationName, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        SublocationName = sublocationName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update cached location and strain names for METRC reporting
    /// </summary>
    public void UpdateMetrcCachedNames(string? strainName, string? locationName)
    {
        StrainName = strainName?.Trim();
        LocationName = locationName?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if batch is ready for METRC sync
    /// </summary>
    public bool IsReadyForMetrcSync => 
        !string.IsNullOrWhiteSpace(StrainName) && 
        !string.IsNullOrWhiteSpace(LocationName);

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid strainId,
        BatchCode batchCode,
        string batchName,
        int plantCount,
        Guid currentStageId,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (strainId == Guid.Empty)
            throw new ArgumentException("Strain ID cannot be empty", nameof(strainId));

        if (batchCode == null)
            throw new ArgumentNullException(nameof(batchCode));

        if (string.IsNullOrWhiteSpace(batchName))
            throw new ArgumentException("Batch name cannot be empty", nameof(batchName));

        if (batchName.Length > 200)
            throw new ArgumentException("Batch name cannot exceed 200 characters", nameof(batchName));

        if (plantCount < 1)
            throw new ArgumentException("Plant count must be at least 1", nameof(plantCount));

        if (currentStageId == Guid.Empty)
            throw new ArgumentException("Current stage ID cannot be empty", nameof(currentStageId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}
