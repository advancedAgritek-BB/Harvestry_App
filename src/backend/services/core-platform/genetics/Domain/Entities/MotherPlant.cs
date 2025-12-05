using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Mother plant aggregate root - source plants for cloning/propagation
/// </summary>
public sealed class MotherPlant : AggregateRoot<Guid>
{
    private readonly List<MotherHealthLog> _healthLogs = new();

    // Private constructor for EF Core/rehydration
    private MotherPlant(Guid id) : base(id) { }

    private MotherPlant(
        Guid id,
        Guid siteId,
        Guid batchId,
        PlantId plantId,
        Guid strainId,
        DateOnly dateEstablished,
        Guid createdByUserId,
        Guid? locationId = null,
        Guid? roomId = null,
        int? maxPropagationCount = null) : base(id)
    {
        ValidateConstructorArgs(siteId, batchId, plantId, strainId, createdByUserId);

        SiteId = siteId;
        BatchId = batchId;
        PlantId = plantId;
        StrainId = strainId;
        LocationId = locationId;
        RoomId = roomId;
        Status = MotherPlantStatus.Active;
        DateEstablished = dateEstablished;
        PropagationCount = 0;
        MaxPropagationCount = maxPropagationCount;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid BatchId { get; private set; }
    public PlantId PlantId { get; private set; } = null!;
    public Guid StrainId { get; private set; }
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public MotherPlantStatus Status { get; private set; }
    public DateOnly DateEstablished { get; private set; }
    public DateOnly? LastPropagationDate { get; private set; }
    public int PropagationCount { get; private set; }
    public int? MaxPropagationCount { get; private set; }
    public string? Notes { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public IReadOnlyCollection<MotherHealthLog> HealthLogs => _healthLogs.AsReadOnly();

    /// <summary>
    /// Factory method to create new mother plant
    /// </summary>
    public static MotherPlant Create(
        Guid siteId,
        Guid batchId,
        PlantId plantId,
        Guid strainId,
        DateOnly dateEstablished,
        Guid createdByUserId,
        Guid? locationId = null,
        Guid? roomId = null,
        int? maxPropagationCount = null)
    {
        return new MotherPlant(
            Guid.NewGuid(),
            siteId,
            batchId,
            plantId,
            strainId,
            dateEstablished,
            createdByUserId,
            locationId,
            roomId,
            maxPropagationCount);
    }

    public static MotherPlant FromPersistence(
        Guid id,
        Guid siteId,
        Guid batchId,
        string plantTag,
        Guid strainId,
        Guid? locationId,
        Guid? roomId,
        MotherPlantStatus status,
        DateOnly dateEstablished,
        DateOnly? lastPropagationDate,
        int propagationCount,
        int? maxPropagationCount,
        string? notes,
        Dictionary<string, object> metadata,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        if (metadata is null)
        {
            metadata = new Dictionary<string, object>();
        }

        var aggregate = new MotherPlant(id)
        {
            SiteId = siteId,
            BatchId = batchId,
            PlantId = PlantId.Create(plantTag),
            StrainId = strainId,
            LocationId = locationId,
            RoomId = roomId,
            Status = status,
            DateEstablished = dateEstablished,
            LastPropagationDate = lastPropagationDate,
            PropagationCount = propagationCount,
            MaxPropagationCount = maxPropagationCount,
            Notes = notes,
            Metadata = metadata,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return aggregate;
    }

    /// <summary>
    /// Record health assessment
    /// </summary>
    public void RecordHealthLog(
        DateOnly logDate,
        HealthAssessment assessment,
        Guid userId)
    {
        if (assessment == null)
            throw new ArgumentNullException(nameof(assessment));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var healthLog = MotherHealthLog.Create(SiteId, Id, logDate, assessment, userId);
        _healthLogs.Add(healthLog);

        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Register propagation event (clones taken)
    /// </summary>
    public void RegisterPropagation(
        int cloneCount,
        Guid userId)
    {
        if (cloneCount < 1)
            throw new ArgumentException("Clone count must be at least 1", nameof(cloneCount));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != MotherPlantStatus.Active)
            throw new InvalidOperationException($"Cannot propagate from mother plant with status: {Status}");

        PropagationCount += cloneCount;
        LastPropagationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Retire the mother plant
    /// </summary>
    public void Retire(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for retirement", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = MotherPlantStatus.Retired;
        
        // Preserve existing notes by appending retirement note
        var trimmedReason = reason.Trim();
        var timestamp = DateTime.UtcNow;
        var retirementNote = $"Retired: {trimmedReason}";
        
        if (string.IsNullOrWhiteSpace(Notes))
        {
            Notes = retirementNote;
        }
        else
        {
            Notes = $"{Notes}\n\n--- {timestamp:yyyy-MM-dd HH:mm:ss} ---\n{retirementNote}";
        }
        
        UpdatedByUserId = userId;
        UpdatedAt = timestamp;
    }

    /// <summary>
    /// Reactivate retired mother plant
    /// </summary>
    public void Reactivate(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != MotherPlantStatus.Retired)
            throw new InvalidOperationException("Can only reactivate retired mother plants");

        Status = MotherPlantStatus.Active;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Quarantine the mother plant
    /// </summary>
    public void Quarantine(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for quarantine", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = MotherPlantStatus.Quarantine;
        Notes = reason.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release from quarantine
    /// </summary>
    public void ReleaseFromQuarantine(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (Status != MotherPlantStatus.Quarantine)
            throw new InvalidOperationException("Mother plant is not in quarantine");

        Status = MotherPlantStatus.Active;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Destroy the mother plant
    /// </summary>
    public void Destroy(string reason, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for destruction", nameof(reason));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Status = MotherPlantStatus.Destroyed;
        
        // Preserve existing notes by appending destruction note
        var trimmedReason = reason.Trim();
        var timestamp = DateTime.UtcNow;
        var destructionNote = $"Destroyed: {trimmedReason}";
        
        if (string.IsNullOrWhiteSpace(Notes))
        {
            Notes = destructionNote;
        }
        else
        {
            Notes = $"{Notes}\n\n--- {timestamp:yyyy-MM-dd HH:mm:ss} ---\n{destructionNote}";
        }
        
        UpdatedByUserId = userId;
        UpdatedAt = timestamp;
    }

    /// <summary>
    /// Update location
    /// </summary>
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        LocationId = locationId;
        RoomId = roomId;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(Dictionary<string, object>? metadata, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Metadata = metadata ?? new Dictionary<string, object>();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePropagationLimit(int? maxPropagationCount, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (maxPropagationCount.HasValue && maxPropagationCount.Value < 1)
            throw new ArgumentException("Max propagation count must be at least 1 when specified", nameof(maxPropagationCount));

        MaxPropagationCount = maxPropagationCount;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetHealthLogs(IEnumerable<MotherHealthLog> logs)
    {
        _healthLogs.Clear();

        if (logs is null)
        {
            return;
        }

        _healthLogs.AddRange(logs);
    }

    /// <summary>
    /// Check if mother can propagate (within limits)
    /// </summary>
    public bool CanPropagate(PropagationSettings settings, int requestedCloneCount)
    {
        if (Status != MotherPlantStatus.Active)
            return false;

        if (requestedCloneCount < 1)
            return false;

        // Check per-mother limit
        if (MaxPropagationCount.HasValue && (PropagationCount + requestedCloneCount) > MaxPropagationCount.Value)
            return false;

        if (settings.MotherPropagationLimit.HasValue 
            && (PropagationCount + requestedCloneCount) > settings.MotherPropagationLimit.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Check if health check is overdue
    /// </summary>
    public bool IsOverdueForHealthCheck(TimeSpan reminderFrequency)
    {
        if (_healthLogs.Count == 0)
            return true; // Never logged

        var latestLog = _healthLogs.MaxBy(l => l.LogDate);
        if (latestLog == null)
            return true;

        var daysSinceLastCheck = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - latestLog.LogDate.DayNumber;
        return daysSinceLastCheck > reminderFrequency.TotalDays;
    }

    /// <summary>
    /// Get latest health assessment
    /// </summary>
    public HealthAssessment? GetLatestHealthAssessment()
    {
        var latestLog = _healthLogs.MaxBy(l => l.LogDate);
        return latestLog?.GetHealthAssessment();
    }

    /// <summary>
    /// Get age of mother plant
    /// </summary>
    public TimeSpan GetAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysSinceEstablished = today.DayNumber - DateEstablished.DayNumber;
        return TimeSpan.FromDays(daysSinceEstablished);
    }

    /// <summary>
    /// Get next health check due date
    /// </summary>
    public DateOnly? GetNextHealthCheckDue(TimeSpan reminderFrequency)
    {
        if (_healthLogs.Count == 0)
            return DateEstablished.AddDays((int)reminderFrequency.TotalDays);

        var latestLog = _healthLogs.MaxBy(l => l.LogDate);
        return latestLog?.LogDate.AddDays((int)reminderFrequency.TotalDays);
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid batchId,
        PlantId plantId,
        Guid strainId,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (batchId == Guid.Empty)
            throw new ArgumentException("Batch ID cannot be empty", nameof(batchId));

        if (plantId == null)
            throw new ArgumentNullException(nameof(plantId));

        if (strainId == Guid.Empty)
            throw new ArgumentException("Strain ID cannot be empty", nameof(strainId));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

