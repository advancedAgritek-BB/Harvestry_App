namespace Harvestry.Integration.Growlink.Application.DTOs;

/// <summary>
/// Represents a Growlink device (controller) from the API.
/// </summary>
public sealed record GrowlinkDeviceDto(
    string DeviceId,
    string Name,
    string DeviceType,
    string? Location,
    bool IsOnline,
    DateTimeOffset? LastSeen,
    List<GrowlinkSensorDto> Sensors);

/// <summary>
/// Represents a sensor attached to a Growlink device.
/// </summary>
public sealed record GrowlinkSensorDto(
    string SensorId,
    string Name,
    string SensorType,
    string Unit,
    double? CurrentValue,
    DateTimeOffset? LastUpdated);

/// <summary>
/// Represents a sensor reading from Growlink.
/// </summary>
public sealed record GrowlinkSensorReadingDto(
    string DeviceId,
    string SensorId,
    double Value,
    string Unit,
    DateTimeOffset Timestamp);

/// <summary>
/// Batch of sensor readings from Growlink.
/// </summary>
public sealed record GrowlinkReadingsBatchDto(
    List<GrowlinkSensorReadingDto> Readings,
    DateTimeOffset FetchedAt);




