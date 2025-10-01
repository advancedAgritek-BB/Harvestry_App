using System;
using System.Collections.Generic;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.ViewModels;

public sealed record EquipmentResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public Guid? LocationId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string TypeCode { get; init; } = string.Empty;
    public CoreEquipmentType CoreType { get; init; }
    public EquipmentStatus Status { get; init; }
    public DateTime? InstalledAt { get; init; }
    public DateTime? DecommissionedAt { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public string? FirmwareVersion { get; init; }
    public string? IpAddress { get; init; }
    public string? MacAddress { get; init; }
    public string? MqttTopic { get; init; }
    public string? DeviceTwinJson { get; init; }
    public DateTime? LastCalibrationAt { get; init; }
    public DateTime? NextCalibrationDueAt { get; init; }
    public int? CalibrationIntervalDays { get; init; }
    public DateTime? LastHeartbeatAt { get; init; }
    public bool Online { get; init; }
    public int? SignalStrengthDbm { get; init; }
    public int? BatteryPercent { get; init; }
    public int ErrorCount { get; init; }
    public long? UptimeSeconds { get; init; }
    public string? Notes { get; init; }
    public string? MetadataJson { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid UpdatedByUserId { get; init; }
    public IReadOnlyList<EquipmentChannelResponse> Channels { get; init; } = Array.Empty<EquipmentChannelResponse>();
}
