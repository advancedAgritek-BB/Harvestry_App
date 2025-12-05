using System;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class RlsEquipmentTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task EquipmentRepository_RlsBlocksAlternateSiteUser()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();
        var equipmentRepository = ServiceProvider.GetRequiredService<IEquipmentRepository>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Equip Room",
            RoomType = RoomType.Clone,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var zone = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone RLS",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var equipment = await equipmentService.CreateAsync(new CreateEquipmentRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            LocationId = zone.Id,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
            Code = $"EQ-{Guid.NewGuid():N}".Substring(0, 8),
            TypeCode = "sensor",
            CoreType = CoreEquipmentType.Sensor,
            Manufacturer = "SensorWorks",
            Model = "SW-100",
            SerialNumber = Guid.NewGuid().ToString("N").Substring(0, 12)
        });

        // Change context to outsider user/site
        SetUserContext(Guid.NewGuid(), "viewer", Guid.NewGuid());

        var fetched = await equipmentRepository.GetByIdAsync(equipment.Id);
        Assert.Null(fetched); // RLS should hide equipment
    }
}
