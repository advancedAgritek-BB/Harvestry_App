using System;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Domain.Entities;

public partial class Equipment
{
    public static Equipment FromPersistence(
        Guid id,
        Guid siteId,
        string code,
        string typeCode,
        CoreEquipmentType coreType,
        EquipmentStatus status,
        Guid? locationId,
        DateTime? installedAt,
        DateTime? decommissionedAt,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? firmwareVersion,
        string? ipAddress,
        string? macAddress,
        string? mqttTopic,
        string? deviceTwinJson,
        DateTime? lastCalibrationAt,
        DateTime? nextCalibrationDueAt,
        int? calibrationIntervalDays,
        DateTime? lastHeartbeatAt,
        int? signalStrengthDbm,
        int? batteryPercent,
        int errorCount,
        long? uptimeSeconds,
        string? notes,
        string? metadataJson,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var equipment = new Equipment(id)
        {
            SiteId = siteId,
            Code = code,
            TypeCode = typeCode,
            CoreType = coreType,
            Status = status,
            LocationId = locationId,
            InstalledAt = installedAt,
            DecommissionedAt = decommissionedAt,
            Manufacturer = manufacturer,
            Model = model,
            SerialNumber = serialNumber,
            FirmwareVersion = firmwareVersion,
            IpAddress = ipAddress,
            MacAddress = macAddress,
            MqttTopic = mqttTopic,
            DeviceTwinJson = deviceTwinJson,
            LastCalibrationAt = lastCalibrationAt,
            NextCalibrationDueAt = nextCalibrationDueAt,
            CalibrationIntervalDays = calibrationIntervalDays,
            LastHeartbeatAt = lastHeartbeatAt,
            SignalStrengthDbm = signalStrengthDbm,
            BatteryPercent = batteryPercent,
            ErrorCount = errorCount,
            UptimeSeconds = uptimeSeconds,
            Notes = notes,
            MetadataJson = metadataJson,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return equipment;
    }
}
