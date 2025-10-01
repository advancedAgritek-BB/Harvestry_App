using System;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Domain;

public sealed class InventoryLocationTests
{
    private static readonly Guid SiteId = Guid.NewGuid();
    private static readonly Guid RoomId = Guid.NewGuid();
    private static readonly Guid ParentId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void AddPlants_AtCapacity_SetsStatusToFull()
    {
        var location = InventoryLocation.FromPersistence(
            Guid.NewGuid(),
            SiteId,
            roomId: RoomId,
            parentId: ParentId,
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
            plantCapacity: 10,
            currentPlantCount: 5,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0m,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-1),
            createdByUserId: UserId,
            updatedAt: DateTime.UtcNow.AddHours(-2),
            updatedByUserId: UserId);

        location.AddPlants(5);

        Assert.Equal(10, location.CurrentPlantCount);
        Assert.Equal(LocationStatus.Full, location.Status);
    }

    [Fact]
    public void AddPlants_BeyondCapacity_Throws()
    {
        var location = InventoryLocation.FromPersistence(
            Guid.NewGuid(),
            SiteId,
            roomId: RoomId,
            parentId: ParentId,
            locationType: LocationType.Zone,
            code: "ZONE-2",
            name: "Zone 2",
            barcode: null,
            path: "Room>Zone 2",
            depth: 1,
            status: LocationStatus.Active,
            lengthFt: null,
            widthFt: null,
            heightFt: null,
            plantCapacity: 5,
            currentPlantCount: 4,
            rowNumber: null,
            columnNumber: null,
            weightCapacityLbs: null,
            currentWeightLbs: 0m,
            notes: null,
            metadataJson: null,
            createdAt: DateTime.UtcNow.AddDays(-1),
            createdByUserId: UserId,
            updatedAt: DateTime.UtcNow.AddHours(-2),
            updatedByUserId: UserId);

        Assert.Throws<InvalidOperationException>(() => location.AddPlants(2));
    }
}
