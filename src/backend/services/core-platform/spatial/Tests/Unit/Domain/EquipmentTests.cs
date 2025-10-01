using System;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Domain;

public sealed class EquipmentTests
{
    private static readonly Guid SiteId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void RecordHeartbeat_UpdatesPropertiesAndRecoversFromFaulty()
    {
        var equipment = Equipment.FromPersistence(
            Guid.NewGuid(),
            SiteId,
            code: "EQ-100",
            typeCode: "controller",
            coreType: CoreEquipmentType.Controller,
            status: EquipmentStatus.Faulty,
            locationId: Guid.NewGuid(),
            installedAt: null,
            decommissionedAt: null,
            manufacturer: "HydroCore",
            model: "HC-200",
            serialNumber: "SN001",
            firmwareVersion: "1.0",
            ipAddress: null,
            macAddress: null,
            mqttTopic: null,
            deviceTwinJson: null,
            lastCalibrationAt: null,
            nextCalibrationDueAt: null,
            calibrationIntervalDays: null,
            lastHeartbeatAt: null,
            signalStrengthDbm: null,
            batteryPercent: null,
            errorCount: 0,
            uptimeSeconds: null,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-2),
            createdByUserId: UserId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: UserId);

        var heartbeatAt = DateTime.UtcNow;
        equipment.RecordHeartbeat(heartbeatAt, signalStrengthDbm: -60, batteryPercent: 80, uptimeSeconds: 1000);

        Assert.Equal(heartbeatAt, equipment.LastHeartbeatAt);
        Assert.Equal(-60, equipment.SignalStrengthDbm);
        Assert.Equal(80, equipment.BatteryPercent);
        Assert.Equal(EquipmentStatus.Active, equipment.Status); // should recover from Faulty when online
    }

    [Fact]
    public void RecordHeartbeat_InvalidBattery_Throws()
    {
        var equipment = new Equipment(SiteId, "EQ-200", "Monitor", CoreEquipmentType.Sensor, UserId);

        Assert.Throws<ArgumentException>(() => equipment.RecordHeartbeat(DateTime.UtcNow, batteryPercent: 150));
    }
}
