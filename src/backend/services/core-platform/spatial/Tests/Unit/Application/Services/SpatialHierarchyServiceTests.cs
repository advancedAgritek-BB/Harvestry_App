using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.Services;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Application.Services;

public sealed class SpatialHierarchyServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepository = new();
    private readonly Mock<IInventoryLocationRepository> _locationRepository = new();
    private readonly Mock<ILogger<SpatialHierarchyService>> _logger = new();
    private readonly SpatialHierarchyService _service;

    public SpatialHierarchyServiceTests()
    {
        _service = new SpatialHierarchyService(_roomRepository.Object, _locationRepository.Object, _logger.Object);
    }

    [Fact]
    public async Task CreateRoomAsync_CreatesRootLocation()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Guid capturedRoomId = Guid.Empty;
        var locationId = Guid.NewGuid();

        _roomRepository
            .Setup(repo => repo.GetByCodeAsync(siteId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        _roomRepository
            .Setup(repo => repo.InsertAsync(It.IsAny<Room>(), It.IsAny<CancellationToken>()))
            .Callback<Room, CancellationToken>((room, _) => capturedRoomId = room.Id)
            .ReturnsAsync((Room room, CancellationToken _) => room.Id);

        _roomRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
                Room.FromPersistence(
                    id,
                    siteId,
                    "RM-001",
                    "Flower Room",
                    RoomType.Flower,
                    null,
                    RoomStatus.Active,
                    "Primary flower room",
                    floorLevel: 1,
                    areaSqft: 1200m,
                    heightFt: 12m,
                    targetTempF: 77m,
                    targetHumidityPct: 55m,
                    targetCo2Ppm: 1200,
                    createdAt: DateTime.UtcNow.AddMinutes(-5),
                    createdByUserId: userId,
                    updatedAt: DateTime.UtcNow.AddMinutes(-5),
                    updatedByUserId: userId));

        _locationRepository
            .Setup(repo => repo.InsertAsync(It.IsAny<InventoryLocation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationId);

        _locationRepository
            .Setup(repo => repo.GetByRoomAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid roomId, CancellationToken _) =>
            {
                var roomLocation = InventoryLocation.FromPersistence(
                    locationId,
                    siteId,
                    roomId,
                    parentId: null,
                    locationType: LocationType.Room,
                    code: "RM-001",
                    name: "Flower Room",
                    barcode: null,
                    path: "Flower Room",
                    depth: 0,
                    status: LocationStatus.Active,
                    lengthFt: null,
                    widthFt: null,
                    heightFt: null,
                    plantCapacity: null,
                    currentPlantCount: 0,
                    rowNumber: null,
                    columnNumber: null,
                    weightCapacityLbs: null,
                    currentWeightLbs: 0,
                    notes: null,
                    metadataJson: null,
                    createdAt: DateTime.UtcNow.AddMinutes(-5),
                    createdByUserId: userId,
                    updatedAt: DateTime.UtcNow.AddMinutes(-5),
                    updatedByUserId: userId);

                return (IReadOnlyList<InventoryLocation>)new List<InventoryLocation> { roomLocation };
            });

        var request = new CreateRoomRequest
        {
            SiteId = siteId,
            Code = "RM-001",
            Name = "Flower Room",
            RoomType = RoomType.Flower,
            RequestedByUserId = userId
        };

        // Act
        var response = await _service.CreateRoomAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, capturedRoomId);
        Assert.Equal(capturedRoomId, response.Id);
        Assert.NotNull(response.RootLocation);
        Assert.Equal(LocationType.Room, response.RootLocation!.LocationType);
        _locationRepository.Verify(repo => repo.InsertAsync(It.IsAny<InventoryLocation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateLocationAsync_InvalidParentCombination_Throws()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var parentLocation = InventoryLocation.FromPersistence(
            parentId,
            siteId,
            roomId,
            parentId: null,
            locationType: LocationType.Zone,
            code: "ZN-1",
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
            currentWeightLbs: 0,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow,
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow,
            updatedByUserId: userId);

        _locationRepository
            .Setup(repo => repo.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentLocation);

        var request = new CreateLocationRequest
        {
            SiteId = siteId,
            ParentLocationId = parentId,
            LocationType = LocationType.Rack,
            Code = "RCK-01",
            Name = "Rack 1",
            RequestedByUserId = userId
        };

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateLocationAsync(request));
    }

    [Fact]
    public async Task ChangeRoomStatusAsync_UpdatesRootLocationStatus()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var room = Room.FromPersistence(
            roomId,
            siteId,
            "RM-001",
            "Propagation",
            RoomType.Clone,
            null,
            RoomStatus.Active,
            description: null,
            floorLevel: null,
            areaSqft: null,
            heightFt: null,
            targetTempF: null,
            targetHumidityPct: null,
            targetCo2Ppm: null,
            createdAt: DateTime.UtcNow.AddHours(-1),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddHours(-1),
            updatedByUserId: userId);

        var rootLocation = InventoryLocation.FromPersistence(
            locationId,
            siteId,
            roomId,
            parentId: null,
            locationType: LocationType.Room,
            code: "RM-001",
            name: "Propagation",
            barcode: null,
            path: "Propagation",
            depth: 0,
            status: LocationStatus.Active,
            lengthFt: null,
            widthFt: null,
            heightFt: null,
            plantCapacity: null,
            currentPlantCount: 0,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddHours(-1),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddHours(-1),
            updatedByUserId: userId);

        _roomRepository
            .Setup(repo => repo.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _locationRepository
            .Setup(repo => repo.GetByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryLocation> { rootLocation });

        var requestStatus = RoomStatus.Quarantine;

        // Act
        var response = await _service.ChangeRoomStatusAsync(siteId, roomId, requestStatus, userId);

        // Assert
        Assert.Equal(requestStatus, response.Status);
        _locationRepository.Verify(repo => repo.UpdateAsync(It.Is<InventoryLocation>(l => l.Id == locationId && l.Status == LocationStatus.Quarantine), It.IsAny<CancellationToken>()), Times.Once);
    }
}
