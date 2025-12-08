using Harvestry.Integration.Growlink.Domain.Enums;

namespace Harvestry.Integration.Growlink.Application.DTOs;

/// <summary>
/// Result of a Growlink sync operation.
/// </summary>
public sealed record GrowlinkSyncResultDto(
    Guid SiteId,
    GrowlinkSyncStatus Status,
    int ReadingsReceived,
    int ReadingsIngested,
    int ReadingsRejected,
    int ReadingsDuplicate,
    long ProcessingTimeMs,
    string? ErrorMessage = null);

/// <summary>
/// Connection status response for the API.
/// </summary>
public sealed record GrowlinkConnectionStatusDto(
    Guid SiteId,
    bool IsConnected,
    string Status,
    string? GrowlinkAccountId,
    DateTimeOffset? LastSyncAt,
    string? LastSyncError,
    int DeviceCount,
    int MappedSensorCount);

/// <summary>
/// Request to create a stream mapping.
/// </summary>
public sealed record CreateStreamMappingRequest(
    string GrowlinkDeviceId,
    string GrowlinkSensorId,
    Guid? HarvestryStreamId,
    bool AutoCreateStream);

/// <summary>
/// Stream mapping response.
/// </summary>
public sealed record StreamMappingDto(
    Guid Id,
    string GrowlinkDeviceId,
    string GrowlinkSensorId,
    string GrowlinkSensorName,
    string GrowlinkSensorType,
    Guid HarvestryStreamId,
    bool IsActive,
    bool AutoCreated);
