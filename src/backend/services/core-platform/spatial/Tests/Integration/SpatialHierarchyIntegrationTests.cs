using System;
using System.Linq;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class SpatialHierarchyIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task CreateRoomAsync_PersistsRootLocation()
    {
        var service = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var request = new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 8).ToUpperInvariant(),
            Name = "Integration Flower Room",
            RoomType = RoomType.Flower,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        };

        var response = await service.CreateRoomAsync(request);

        Assert.Equal(request.SiteId, response.SiteId);
        Assert.Equal(request.Name, response.Name);
        Assert.NotNull(response.RootLocation);
        Assert.Equal(LocationType.Room, response.RootLocation!.LocationType);
        Assert.Equal(0, response.RootLocation.Depth);
        Assert.Equal(response.Name, response.RootLocation.Name);
    }

    [IntegrationFact]
    public async Task CreateLocationAsync_BuildsPathHierarchy()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var roomRequest = new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 8).ToUpperInvariant(),
            Name = "Integration Veg Room",
            RoomType = RoomType.Veg,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        };

        var room = await hierarchyService.CreateRoomAsync(roomRequest);
        var rootLocationId = room.RootLocation!.Id;

        var zoneRequest = new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = rootLocationId,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant(),
            Name = "Zone A",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        };

        var zone = await hierarchyService.CreateLocationAsync(zoneRequest);

        Assert.Equal(LocationType.Zone, zone.LocationType);
        Assert.Equal(1, zone.Depth);
        Assert.Equal("Integration Veg Room>Zone A", zone.Path);

        var path = await hierarchyService.GetLocationPathAsync(SpatialTestDataSeeder.SiteId, zone.Id);
        Assert.Equal(2, path.Count);
        Assert.Equal(LocationType.Room, path[0].LocationType);
        Assert.Equal(LocationType.Zone, path[1].LocationType);
    }
}
