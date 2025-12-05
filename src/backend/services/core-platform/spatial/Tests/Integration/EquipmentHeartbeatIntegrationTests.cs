using System;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class EquipmentHeartbeatIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task RecordHeartbeatAsync_UpdatesOnlineStatus()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        // Arrange hierarchy
        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Heartbeat Room",
            RoomType = RoomType.Veg,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });

        var zone = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Heartbeat Zone",
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
            Model = "SW-200",
            SerialNumber = Guid.NewGuid().ToString("N").Substring(0, 12)
        });

        var heartbeatAt = DateTime.UtcNow;

        await equipmentService.RecordHeartbeatAsync(
            SpatialTestDataSeeder.SiteId,
            equipment.Id,
            new RecordHeartbeatRequest
            {
                HeartbeatAt = heartbeatAt,
                SignalStrengthDbm = -55,
                BatteryPercent = 90,
                UptimeSeconds = 1200
            });

        // Reapply context for retrieval
        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var refreshed = await equipmentService.GetByIdAsync(SpatialTestDataSeeder.SiteId, equipment.Id, includeChannels: false);

        Assert.NotNull(refreshed);
        Assert.Equal(heartbeatAt, refreshed!.LastHeartbeatAt);
        Assert.Equal(90, refreshed.BatteryPercent);
        Assert.True(refreshed.Online);
    }
}
