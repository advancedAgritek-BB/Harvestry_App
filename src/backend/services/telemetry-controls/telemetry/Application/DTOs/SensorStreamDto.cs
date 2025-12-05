using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// DTO for sensor stream configuration.
/// </summary>
public record SensorStreamDto(
    Guid Id,
    Guid SiteId,
    Guid EquipmentId,
    Guid? EquipmentChannelId,
    StreamType StreamType,
    Unit Unit,
    string DisplayName,
    Guid? LocationId,
    Guid? RoomId,
    Guid? ZoneId,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// Request to create a new sensor stream.
/// </summary>
public record CreateSensorStreamRequestDto(
    Guid EquipmentId,
    Guid? EquipmentChannelId,
    StreamType StreamType,
    Unit Unit,
    string DisplayName,
    Guid? LocationId = null,
    Guid? RoomId = null,
    Guid? ZoneId = null
);

/// <summary>
/// Request to update sensor stream.
/// </summary>
public record UpdateSensorStreamRequestDto(
    string? DisplayName = null,
    Guid? LocationId = null,
    Guid? RoomId = null,
    Guid? ZoneId = null
);

