using System;
using System.Linq;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class EquipmentRegistryIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task CreateAndRetrieveEquipment_SucceedsWithTenantGuards()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        // Arrange hierarchy: room -> zone (for location assignment)
        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Equip Integration Room",
            RoomType = RoomType.Veg,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var zone = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone Int",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        // Act: create equipment
        var created = await equipmentService.CreateAsync(new CreateEquipmentRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            LocationId = zone.Id,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
            Code = $"EQ-{Guid.NewGuid():N}".Substring(0, 8),
            TypeCode = "controller",
            CoreType = CoreEquipmentType.Controller,
            Manufacturer = "HydroCore",
            Model = "HC-200",
            SerialNumber = Guid.NewGuid().ToString("N").Substring(0, 12)
        });

        // Retrieve list (without channels)
        var list = await equipmentService.GetListAsync(SpatialTestDataSeeder.SiteId, new EquipmentListQuery
        {
            IncludeChannels = false,
            Page = 1,
            PageSize = 10
        });

        Assert.Contains(list.Items, e => e.Id == created.Id);
        Assert.Empty(list.Items.First(e => e.Id == created.Id).Channels);

        // Retrieve with channels flag
        var detailed = await equipmentService.GetByIdAsync(SpatialTestDataSeeder.SiteId, created.Id, includeChannels: true);
        Assert.NotNull(detailed);
        Assert.Equal(created.Id, detailed!.Id);
        Assert.Equal(zone.Id, detailed.LocationId);
        Assert.Empty(detailed.Channels);

        // Tenant mismatch guard when querying with wrong site id
        await Assert.ThrowsAsync<TenantMismatchException>(() =>
            equipmentService.GetByIdAsync(Guid.NewGuid(), created.Id, includeChannels: false));
    }
}
