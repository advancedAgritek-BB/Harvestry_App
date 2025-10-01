using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Application.Services;

public sealed class EquipmentRegistryServiceTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepository = new();
    private readonly Mock<IEquipmentChannelRepository> _channelRepository = new();
    private readonly Mock<IInventoryLocationRepository> _locationRepository = new();
    private readonly EquipmentRegistryService _service;

    public EquipmentRegistryServiceTests()
    {
        var logger = Mock.Of<ILogger<EquipmentRegistryService>>();
        _service = new EquipmentRegistryService(
            _equipmentRepository.Object,
            _channelRepository.Object,
            _locationRepository.Object,
            logger);
    }

    [Fact]
    public async Task CreateAsync_WithMismatchedLocationSite_ThrowsTenantMismatch()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var otherSiteId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var request = new CreateEquipmentRequest
        {
            SiteId = siteId,
            LocationId = locationId,
            Code = "PUMP-01",
            TypeCode = "pump",
            CoreType = CoreEquipmentType.Pump,
            RequestedByUserId = Guid.NewGuid()
        };

        _locationRepository
            .Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryLocation.FromPersistence(
                locationId,
                otherSiteId,
                roomId: Guid.NewGuid(),
                parentId: Guid.NewGuid(),
                locationType: LocationType.Zone,
                code: "Z1",
                name: "Zone 1",
                barcode: null,
                path: "Zone 1",
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
                createdAt: DateTime.UtcNow,
                createdByUserId: request.RequestedByUserId,
                updatedAt: DateTime.UtcNow,
                updatedByUserId: request.RequestedByUserId));

        // Act / Assert
        await Assert.ThrowsAsync<TenantMismatchException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_WhenIncludeChannels_ReturnsChannels()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "TEMP-1",
            typeCode: "temperature_sensor",
            coreType: CoreEquipmentType.Sensor,
            status: EquipmentStatus.Active,
            locationId: Guid.NewGuid(),
            installedAt: DateTime.UtcNow,
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
            createdAt: DateTime.UtcNow,
            createdByUserId: Guid.NewGuid(),
            updatedAt: DateTime.UtcNow,
            updatedByUserId: Guid.NewGuid());

        var channel = EquipmentChannel.FromPersistence(
            Guid.NewGuid(),
            equipmentId,
            channelCode: "CH-1",
            role: "temp_probe",
            portMetaJson: null,
            enabled: true,
            assignedZoneId: null,
            notes: null,
            createdAt: DateTime.UtcNow);

        _equipmentRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _channelRepository
            .Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentChannel> { channel });

        // Act
        var result = await _service.GetByIdAsync(siteId, equipmentId, includeChannels: true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.Channels);
        Assert.Equal(channel.ChannelCode, result.Channels[0].ChannelCode);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenActivatingWithoutLocation_Throws()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "VALVE-1",
            typeCode: "valve",
            coreType: CoreEquipmentType.Valve,
            status: EquipmentStatus.Maintenance,
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
            createdAt: DateTime.UtcNow,
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow,
            updatedByUserId: userId);

        _equipmentRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ChangeStatusAsync(siteId, equipmentId, EquipmentStatus.Active, userId));
    }
}
