using System;
using System.Linq;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class ValveZoneMappingIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task CreateUpdateDeleteValveMapping_FlowsSuccessfully()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();
        var valveService = ServiceProvider.GetRequiredService<IValveZoneMappingService>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        // Arrange: Create base room/zone structure
        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Valve Room",
            RoomType = RoomType.Flower,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var zoneA = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone A",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var zoneB = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone B",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var equipment = await equipmentService.CreateAsync(new CreateEquipmentRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            LocationId = zoneA.Id,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
            Code = $"VALVE-{Guid.NewGuid():N}".Substring(0, 8),
            TypeCode = "valve",
            CoreType = CoreEquipmentType.Valve
        });

        // Act: Create mapping
        var mapping = await valveService.CreateAsync(
            SpatialTestDataSeeder.SiteId,
            equipment.Id,
            new CreateValveZoneMappingRequest
            {
                SiteId = SpatialTestDataSeeder.SiteId,
                EquipmentId = equipment.Id,
                ZoneLocationId = zoneA.Id,
                RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
                Priority = 5,
                NormallyOpen = false,
                InterlockGroup = "MIX-1",
                Notes = "Initial mapping"
            });

        Assert.Equal(equipment.Id, mapping.ValveEquipmentId);
        Assert.Equal(zoneA.Id, mapping.ZoneLocationId);
        Assert.Equal(5, mapping.Priority);
        Assert.False(mapping.NormallyOpen);
        Assert.Equal("MIX-1", mapping.InterlockGroup);

        // Verify retrieval by valve
        var byValve = await valveService.GetByValveAsync(SpatialTestDataSeeder.SiteId, equipment.Id);
        Assert.Single(byValve);
        Assert.Equal(mapping.Id, byValve.Single().Id);

        // Update mapping to point to second zone and change properties
        var updated = await valveService.UpdateAsync(
            SpatialTestDataSeeder.SiteId,
            mapping.Id,
            new UpdateValveZoneMappingRequest
            {
                RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
                ZoneLocationId = zoneB.Id,
                Priority = 2,
                NormallyOpen = true,
                InterlockGroup = "MIX-PRIMARY",
                Notes = "Switched to zone B",
                Enabled = true
            });

        Assert.Equal(zoneB.Id, updated.ZoneLocationId);
        Assert.Equal(2, updated.Priority);
        Assert.True(updated.NormallyOpen);
        Assert.Equal("MIX-PRIMARY", updated.InterlockGroup);
        Assert.Equal("Switched to zone B", updated.Notes);

        // Fetch by zone should include new mapping
        var byZone = await valveService.GetByZoneAsync(SpatialTestDataSeeder.SiteId, zoneB.Id);
        Assert.Single(byZone);
        Assert.Equal(mapping.Id, byZone.Single().Id);

        // Delete mapping
        await valveService.DeleteAsync(SpatialTestDataSeeder.SiteId, mapping.Id, SpatialTestDataSeeder.ManagerUserId);

        var remaining = await valveService.GetByValveAsync(SpatialTestDataSeeder.SiteId, equipment.Id);
        Assert.Empty(remaining);
    }
}

