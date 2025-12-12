namespace Harvestry.Integration.Growlink.Domain.Entities;

/// <summary>
/// Maps a Growlink sensor to a Harvestry SensorStream.
/// Enables data flow from Growlink devices into Harvestry's telemetry system.
/// </summary>
public sealed class GrowlinkStreamMapping : AggregateRoot<Guid>
{
    public Guid SiteId { get; private set; }
    public string GrowlinkDeviceId { get; private set; } = string.Empty;
    public string GrowlinkSensorId { get; private set; } = string.Empty;
    public string GrowlinkSensorName { get; private set; } = string.Empty;
    public string GrowlinkSensorType { get; private set; } = string.Empty;
    public Guid HarvestryStreamId { get; private set; }
    public bool IsActive { get; private set; }
    public bool AutoCreated { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private GrowlinkStreamMapping() { }

    private GrowlinkStreamMapping(
        Guid id,
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId,
        bool autoCreated)
        : base(id)
    {
        SiteId = siteId;
        GrowlinkDeviceId = growlinkDeviceId;
        GrowlinkSensorId = growlinkSensorId;
        GrowlinkSensorName = growlinkSensorName;
        GrowlinkSensorType = growlinkSensorType;
        HarvestryStreamId = harvestryStreamId;
        IsActive = true;
        AutoCreated = autoCreated;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new manual stream mapping.
    /// </summary>
    public static GrowlinkStreamMapping CreateManual(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId)
    {
        ValidateInputs(siteId, growlinkDeviceId, growlinkSensorId, harvestryStreamId);

        return new GrowlinkStreamMapping(
            Guid.NewGuid(),
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            growlinkSensorName,
            growlinkSensorType,
            harvestryStreamId,
            autoCreated: false);
    }

    /// <summary>
    /// Creates an auto-generated stream mapping.
    /// </summary>
    public static GrowlinkStreamMapping CreateAuto(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId)
    {
        ValidateInputs(siteId, growlinkDeviceId, growlinkSensorId, harvestryStreamId);

        return new GrowlinkStreamMapping(
            Guid.NewGuid(),
            siteId,
            growlinkDeviceId,
            growlinkSensorId,
            growlinkSensorName,
            growlinkSensorType,
            harvestryStreamId,
            autoCreated: true);
    }

    private static void ValidateInputs(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        Guid harvestryStreamId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));

        if (string.IsNullOrWhiteSpace(growlinkDeviceId))
            throw new ArgumentException("Growlink device ID is required", nameof(growlinkDeviceId));

        if (string.IsNullOrWhiteSpace(growlinkSensorId))
            throw new ArgumentException("Growlink sensor ID is required", nameof(growlinkSensorId));

        if (harvestryStreamId == Guid.Empty)
            throw new ArgumentException("Harvestry stream ID is required", nameof(harvestryStreamId));
    }

    /// <summary>
    /// Updates the target Harvestry stream.
    /// </summary>
    public void UpdateHarvestryStream(Guid harvestryStreamId)
    {
        if (harvestryStreamId == Guid.Empty)
            throw new ArgumentException("Harvestry stream ID is required", nameof(harvestryStreamId));

        HarvestryStreamId = harvestryStreamId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Activates the mapping.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Deactivates the mapping.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Returns a unique key for this Growlink sensor.
    /// </summary>
    public string GetGrowlinkSensorKey() => $"{GrowlinkDeviceId}:{GrowlinkSensorId}";

    /// <summary>
    /// Rehydrates from persistence.
    /// </summary>
    public static GrowlinkStreamMapping FromPersistence(
        Guid id,
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId,
        bool isActive,
        bool autoCreated,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new GrowlinkStreamMapping
        {
            Id = id,
            SiteId = siteId,
            GrowlinkDeviceId = growlinkDeviceId,
            GrowlinkSensorId = growlinkSensorId,
            GrowlinkSensorName = growlinkSensorName,
            GrowlinkSensorType = growlinkSensorType,
            HarvestryStreamId = harvestryStreamId,
            IsActive = isActive,
            AutoCreated = autoCreated,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}





