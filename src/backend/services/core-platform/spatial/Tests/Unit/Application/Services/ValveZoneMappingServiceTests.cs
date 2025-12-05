using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Application.Services;

public sealed class ValveZoneMappingServiceTests
{
    private readonly Mock<IValveZoneMappingRepository> _mappingRepository = new();
    private readonly Mock<IEquipmentRepository> _equipmentRepository = new();
    private readonly Mock<IEquipmentChannelRepository> _channelRepository = new();
    private readonly Mock<IInventoryLocationRepository> _locationRepository = new();
    private readonly ValveZoneMappingService _service;

    public ValveZoneMappingServiceTests()
    {
        var logger = Mock.Of<ILogger<ValveZoneMappingService>>();
        _service = new ValveZoneMappingService(
            _mappingRepository.Object,
            _equipmentRepository.Object,
            _channelRepository.Object,
            _locationRepository.Object,
            logger);
    }

    [Fact]
    public async Task CreateAsync_WhenDataValid_PersistsMapping()
    {
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "VALVE-1",
            typeCode: "valve",
            coreType: CoreEquipmentType.Valve,
            status: EquipmentStatus.Active,
            locationId: zoneId,
            installedAt: null,
            decommissionedAt: null,
            manufacturer: null,
            model: null,
            serialNumber: null,
            firmwareVersion: null,
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
            createdAt: DateTime.UtcNow.AddDays(-5),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        var location = InventoryLocation.FromPersistence(
            zoneId,
            siteId,
            roomId: Guid.NewGuid(),
            parentId: Guid.NewGuid(),
            locationType: LocationType.Zone,
            code: "ZONE-1",
            name: "Zone 1",
            barcode: null,
            path: "Room>Zone 1",
            depth: 1,
            status: LocationStatus.Active,
            lengthFt: null,
            widthFt: null,
            heightFt: null,
            plantCapacity: null,
            currentPlantCount: 0,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0m,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-5),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        _equipmentRepository.Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);
        _locationRepository.Setup(r => r.GetByIdAsync(zoneId, It.IsAny<CancellationToken>())).ReturnsAsync(location);
        _channelRepository.Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<EquipmentChannel>());

        ValveZoneMapping? inserted = null;
        _mappingRepository
            .Setup(r => r.InsertAsync(It.IsAny<ValveZoneMapping>(), It.IsAny<CancellationToken>()))
            .Callback<ValveZoneMapping, CancellationToken>((mapping, _) => inserted = mapping)
            .ReturnsAsync(Guid.NewGuid());

        var response = await _service.CreateAsync(siteId, equipmentId, new CreateValveZoneMappingRequest
        {
            SiteId = siteId,
            EquipmentId = equipmentId,
            ZoneLocationId = zoneId,
            RequestedByUserId = userId,
            Priority = 3,
            NormallyOpen = true,
            InterlockGroup = "GROUP-A",
            Notes = "Primary zone"
        });

        Assert.NotNull(inserted);
        Assert.Equal(response.Id, inserted!.Id);
        Assert.Equal(3, inserted.Priority);
        Assert.True(inserted.NormallyOpen);

        _mappingRepository.Verify(r => r.InsertAsync(inserted, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithMismatchedZoneSite_ThrowsTenantMismatch()
    {
        var siteId = Guid.NewGuid();
        var otherSiteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "VALVE-2",
            typeCode: "valve",
            coreType: CoreEquipmentType.Valve,
            status: EquipmentStatus.Active,
            locationId: null,
            installedAt: null,
            decommissionedAt: null,
            manufacturer: null,
            model: null,
            serialNumber: null,
            firmwareVersion: null,
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
            createdAt: DateTime.UtcNow.AddDays(-10),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        var otherLocation = InventoryLocation.FromPersistence(
            zoneId,
            otherSiteId,
            roomId: Guid.NewGuid(),
            parentId: Guid.NewGuid(),
            locationType: LocationType.Zone,
            code: "ZONE-X",
            name: "Zone X",
            barcode: null,
            path: "Room>Zone X",
            depth: 1,
            status: LocationStatus.Active,
            lengthFt: null,
            widthFt: null,
            heightFt: null,
            plantCapacity: null,
            currentPlantCount: 0,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0m,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-5),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        _equipmentRepository.Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);
        _locationRepository.Setup(r => r.GetByIdAsync(zoneId, It.IsAny<CancellationToken>())).ReturnsAsync(otherLocation);

        await Assert.ThrowsAsync<TenantMismatchException>(() =>
            _service.CreateAsync(siteId, equipmentId, new CreateValveZoneMappingRequest
            {
                SiteId = siteId,
                EquipmentId = equipmentId,
                ZoneLocationId = zoneId,
                RequestedByUserId = userId
            }));
    }

    [Fact]
    public async Task UpdateAsync_ReassignsZoneAndChannel()
    {
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var existingZoneId = Guid.NewGuid();
        var newZoneId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mapping = ValveZoneMapping.FromPersistence(
            Guid.NewGuid(),
            siteId,
            equipmentId,
            valveChannelCode: "CH-1",
            zoneLocationId: existingZoneId,
            priority: 1,
            normallyOpen: false,
            interlockGroup: null,
            enabled: true,
            notes: null,
            createdAt: DateTime.UtcNow.AddDays(-5),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-2),
            updatedByUserId: userId);

        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "VALVE-3",
            typeCode: "valve",
            coreType: CoreEquipmentType.Valve,
            status: EquipmentStatus.Active,
            locationId: null,
            installedAt: null,
            decommissionedAt: null,
            manufacturer: null,
            model: null,
            serialNumber: null,
            firmwareVersion: null,
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
            createdAt: DateTime.UtcNow.AddDays(-10),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        var newLocation = InventoryLocation.FromPersistence(
            newZoneId,
            siteId,
            roomId: Guid.NewGuid(),
            parentId: Guid.NewGuid(),
            locationType: LocationType.Zone,
            code: "ZONE-NEW",
            name: "Zone New",
            barcode: null,
            path: "Room>Zone New",
            depth: 1,
            status: LocationStatus.Active,
            lengthFt: null,
            widthFt: null,
            heightFt: null,
            plantCapacity: null,
            currentPlantCount: 0,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0m,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-5),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        _mappingRepository.Setup(r => r.GetByIdAsync(mapping.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mapping);
        _equipmentRepository.Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);
        _locationRepository.Setup(r => r.GetByIdAsync(newZoneId, It.IsAny<CancellationToken>())).ReturnsAsync(newLocation);
        _channelRepository.Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentChannel>
            {
                EquipmentChannel.FromPersistence(Guid.NewGuid(), equipmentId, "CH-2", "flow", null, true, null, null, DateTime.UtcNow.AddDays(-3))
            });

        _mappingRepository.Setup(r => r.UpdateAsync(mapping, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var response = await _service.UpdateAsync(siteId, mapping.Id, new UpdateValveZoneMappingRequest
        {
            RequestedByUserId = userId,
            ZoneLocationId = newZoneId,
            ValveChannelCode = "CH-2",
            Priority = 4,
            NormallyOpen = true,
            InterlockGroup = "GROUP-B",
            Notes = "Updated",
            Enabled = true
        });

        Assert.Equal(newZoneId, response.ZoneLocationId);
        Assert.Equal("CH-2", response.ValveChannelCode);
        Assert.Equal(4, response.Priority);
        Assert.True(response.NormallyOpen);

        _mappingRepository.Verify(r => r.UpdateAsync(mapping, It.IsAny<CancellationToken>()), Times.Once);
    }
}

