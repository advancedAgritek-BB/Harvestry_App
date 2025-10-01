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

public sealed class CalibrationIntegrationTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task RecordAsync_PersistsCalibrationAndUpdatesEquipment()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();
        var calibrationService = ServiceProvider.GetRequiredService<ICalibrationService>();
        var equipmentRepository = ServiceProvider.GetRequiredService<IEquipmentRepository>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Calibration Room",
            RoomType = RoomType.Flower,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        }).ConfigureAwait(false);

        var zone = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone 1",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        }).ConfigureAwait(false);

        var equipment = await equipmentService.CreateAsync(new CreateEquipmentRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            LocationId = zone.Id,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
            Code = $"EQ-{Guid.NewGuid():N}".Substring(0, 6),
            TypeCode = "controller",
            CoreType = CoreEquipmentType.Controller,
            Manufacturer = "TestCo",
            Model = "ModelX",
            SerialNumber = Guid.NewGuid().ToString("N").Substring(0, 12)
        }).ConfigureAwait(false);

        var calibration = await calibrationService.RecordAsync(
            SpatialTestDataSeeder.SiteId,
            equipment.Id,
            new CreateCalibrationRequest
            {
                SiteId = SpatialTestDataSeeder.SiteId,
                EquipmentId = equipment.Id,
                PerformedByUserId = SpatialTestDataSeeder.ManagerUserId,
                Method = CalibrationMethod.Single,
                ReferenceValue = 100.0m,
                MeasuredValue = 99.5m,
                Result = CalibrationResult.WithinTolerance,
                Notes = "Routine calibration",
                IntervalDaysOverride = 14
            }).ConfigureAwait(false);

        Assert.Equal(equipment.Id, calibration.EquipmentId);
        Assert.Equal(14, calibration.IntervalDays);
        Assert.True(calibration.NextDueAt.HasValue);
        Assert.True(calibration.NextDueAt > calibration.PerformedAt);

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);
        var reloadedEquipment = await equipmentRepository.GetByIdAsync(equipment.Id).ConfigureAwait(false);
        Assert.NotNull(reloadedEquipment);
        Assert.Equal(calibration.PerformedAt, reloadedEquipment!.LastCalibrationAt);
        Assert.Equal(calibration.NextDueAt, reloadedEquipment.NextCalibrationDueAt);
        Assert.Equal(14, reloadedEquipment.CalibrationIntervalDays);

        var history = await calibrationService.GetHistoryAsync(SpatialTestDataSeeder.SiteId, equipment.Id).ConfigureAwait(false);
        Assert.Single(history);
        Assert.Equal(calibration.Id, history.Single().Id);
    }

    [IntegrationFact]
    public async Task GetHistoryAsync_ThrowsTenantMismatchForWrongSite()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var equipmentService = ServiceProvider.GetRequiredService<IEquipmentRegistryService>();
        var calibrationService = ServiceProvider.GetRequiredService<ICalibrationService>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Tenant Guard Room",
            RoomType = RoomType.Mother,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        }).ConfigureAwait(false);

        var zone = await hierarchyService.CreateLocationAsync(new CreateLocationRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            ParentLocationId = room.RootLocation!.Id,
            LocationType = LocationType.Zone,
            Code = $"ZN-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "Zone Tenant",
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        }).ConfigureAwait(false);

        var equipment = await equipmentService.CreateAsync(new CreateEquipmentRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            LocationId = zone.Id,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId,
            Code = $"EQ-{Guid.NewGuid():N}".Substring(0, 6),
            TypeCode = "sensor",
            CoreType = CoreEquipmentType.Sensor
        }).ConfigureAwait(false);

        await Assert.ThrowsAsync<TenantMismatchException>(() =>
            calibrationService.GetHistoryAsync(Guid.NewGuid(), equipment.Id));
    }
}
