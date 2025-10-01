using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;

namespace Harvestry.Spatial.Application.Mappers;

public static class EquipmentMapper
{
    public static EquipmentResponse ToResponse(Equipment equipment, IReadOnlyList<EquipmentChannel>? channels = null)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        return new EquipmentResponse
        {
            Id = equipment.Id,
            SiteId = equipment.SiteId,
            LocationId = equipment.LocationId,
            Code = equipment.Code,
            TypeCode = equipment.TypeCode,
            CoreType = equipment.CoreType,
            Status = equipment.Status,
            InstalledAt = equipment.InstalledAt,
            DecommissionedAt = equipment.DecommissionedAt,
            Manufacturer = equipment.Manufacturer,
            Model = equipment.Model,
            SerialNumber = equipment.SerialNumber,
            FirmwareVersion = equipment.FirmwareVersion,
            IpAddress = equipment.IpAddress,
            MacAddress = equipment.MacAddress,
            MqttTopic = equipment.MqttTopic,
            DeviceTwinJson = equipment.DeviceTwinJson,
            LastCalibrationAt = equipment.LastCalibrationAt,
            NextCalibrationDueAt = equipment.NextCalibrationDueAt,
            CalibrationIntervalDays = equipment.CalibrationIntervalDays,
            LastHeartbeatAt = equipment.LastHeartbeatAt,
            Online = equipment.IsOnline(),
            SignalStrengthDbm = equipment.SignalStrengthDbm,
            BatteryPercent = equipment.BatteryPercent,
            ErrorCount = equipment.ErrorCount,
            UptimeSeconds = equipment.UptimeSeconds,
            Notes = equipment.Notes,
            MetadataJson = equipment.MetadataJson,
            CreatedAt = equipment.CreatedAt,
            CreatedByUserId = equipment.CreatedByUserId,
            UpdatedAt = equipment.UpdatedAt,
            UpdatedByUserId = equipment.UpdatedByUserId,
            Channels = (channels ?? Array.Empty<EquipmentChannel>())
                .Select(ToChannelResponse)
                .ToArray()
        };
    }

    public static EquipmentChannelResponse ToChannelResponse(EquipmentChannel channel)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));

        return new EquipmentChannelResponse
        {
            Id = channel.Id,
            EquipmentId = channel.EquipmentId,
            ChannelCode = channel.ChannelCode,
            Role = channel.Role,
            PortMetaJson = channel.PortMetaJson,
            Enabled = channel.Enabled,
            AssignedZoneId = channel.AssignedZoneId,
            Notes = channel.Notes,
            CreatedAt = channel.CreatedAt
        };
    }
}
