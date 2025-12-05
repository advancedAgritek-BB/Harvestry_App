using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.Interfaces;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents a sensor data stream (channel) that produces time-series readings.
/// Aggregate root for sensor stream configuration and metadata.
/// </summary>
public class SensorStream : AggregateRoot<Guid>
{
    private static ITimeProvider _timeProvider = new DefaultTimeProvider();

    // For testing purposes
    public static void SetTimeProvider(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    // Reset to system time provider
    public static void ResetTimeProvider()
    {
        _timeProvider = new DefaultTimeProvider();
    }

    private class DefaultTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
    public Guid SiteId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public Guid? EquipmentChannelId { get; private set; }
    public StreamType StreamType { get; private set; }
    public Unit Unit { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public Guid? LocationId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? ZoneId { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    // For EF Core
    private SensorStream() { }
    
    private SensorStream(
        Guid id,
        Guid siteId,
        Guid equipmentId,
        StreamType streamType,
        Unit unit,
        string displayName)
    {
        Id = id;
        SiteId = siteId;
        EquipmentId = equipmentId;
        StreamType = streamType;
        Unit = unit;
        DisplayName = displayName;
        IsActive = true;
        CreatedAt = _timeProvider.UtcNow;
        UpdatedAt = _timeProvider.UtcNow;
    }
    
    /// <summary>
    /// Creates a new sensor stream.
    /// </summary>
    public static SensorStream Create(
        Guid siteId,
        Guid equipmentId,
        StreamType streamType,
        Unit unit,
        string displayName,
        Guid? equipmentChannelId = null,
        Guid? locationId = null,
        Guid? roomId = null,
        Guid? zoneId = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
            
        var stream = new SensorStream(Guid.NewGuid(), siteId, equipmentId, streamType, unit, displayName)
        {
            EquipmentChannelId = equipmentChannelId,
            LocationId = locationId,
            RoomId = roomId,
            ZoneId = zoneId
        };
        
        return stream;
    }
    
    /// <summary>
    /// Rehydrates sensor stream from persistence layer.
    /// </summary>
    public static SensorStream FromPersistence(
        Guid id,
        Guid siteId,
        Guid equipmentId,
        Guid? equipmentChannelId,
        StreamType streamType,
        Unit unit,
        string displayName,
        Guid? locationId,
        Guid? roomId,
        Guid? zoneId,
        bool isActive,
        Dictionary<string, object>? metadata,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        return new SensorStream
        {
            Id = id,
            SiteId = siteId,
            EquipmentId = equipmentId,
            EquipmentChannelId = equipmentChannelId,
            StreamType = streamType,
            Unit = unit,
            DisplayName = displayName,
            LocationId = locationId,
            RoomId = roomId,
            ZoneId = zoneId,
            IsActive = isActive,
            Metadata = metadata,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    
    public void UpdateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name is required", nameof(displayName));
            
        DisplayName = displayName;
        UpdatedAt = _timeProvider.UtcNow;
    }
    
    public void UpdateLocation(Guid? locationId, Guid? roomId, Guid? zoneId)
    {
        LocationId = locationId;
        RoomId = roomId;
        ZoneId = zoneId;
        UpdatedAt = _timeProvider.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = _timeProvider.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = _timeProvider.UtcNow;
    }

    public void SetMetadata(Dictionary<string, object> metadata)
    {
        Metadata = metadata;
        UpdatedAt = _timeProvider.UtcNow;
    }
}

