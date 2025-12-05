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

public sealed class CalibrationServiceTests
{
    private readonly Mock<ICalibrationRepository> _calibrationRepository = new();
    private readonly Mock<IEquipmentRepository> _equipmentRepository = new();
    private readonly CalibrationService _service;

    public CalibrationServiceTests()
    {
        var logger = Mock.Of<ILogger<CalibrationService>>();
        _service = new CalibrationService(
            _calibrationRepository.Object,
            _equipmentRepository.Object,
            logger);
    }

    [Fact]
    public async Task RecordAsync_WithOverrideInterval_SetsNextDueAndUpdatesEquipment()
    {
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var intervalOverride = 21;

        var equipment = Equipment.FromPersistence(
            equipmentId,
            siteId,
            code: "EQ-01",
            typeCode: "sensor",
            coreType: CoreEquipmentType.Sensor,
            status: EquipmentStatus.Active,
            locationId: Guid.NewGuid(),
            installedAt: DateTime.UtcNow.AddDays(-30),
            decommissionedAt: null,
            manufacturer: "Test",
            model: "Model",
            serialNumber: "SN123",
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
            createdAt: DateTime.UtcNow.AddDays(-60),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        _equipmentRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        Calibration? inserted = null;
        _calibrationRepository
            .Setup(r => r.InsertAsync(It.IsAny<Calibration>(), It.IsAny<CancellationToken>()))
            .Callback<Calibration, CancellationToken>((c, _) => inserted = c)
            .ReturnsAsync(Guid.NewGuid());

        _equipmentRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var response = await _service.RecordAsync(siteId, equipmentId, new CreateCalibrationRequest
        {
            SiteId = siteId,
            EquipmentId = equipmentId,
            PerformedByUserId = userId,
            Method = CalibrationMethod.Single,
            ReferenceValue = 100m,
            MeasuredValue = 99.8m,
            Result = CalibrationResult.WithinTolerance,
            IntervalDaysOverride = intervalOverride
        });

        Assert.NotNull(inserted);
        Assert.Equal(intervalOverride, response.IntervalDays);
        Assert.Equal(inserted!.PerformedAt.AddDays(intervalOverride), inserted.NextDueAt);
        Assert.Equal(intervalOverride, equipment.CalibrationIntervalDays);
        Assert.Equal(inserted.PerformedAt, equipment.LastCalibrationAt);
        Assert.Equal(inserted.NextDueAt, equipment.NextCalibrationDueAt);

        _equipmentRepository.Verify(r => r.UpdateAsync(equipment, It.IsAny<CancellationToken>()), Times.Once);
        _calibrationRepository.Verify(r => r.InsertAsync(inserted, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordAsync_WithSiteMismatch_ThrowsTenantMismatch()
    {
        var siteId = Guid.NewGuid();
        var otherSiteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var equipment = Equipment.FromPersistence(
            equipmentId,
            otherSiteId,
            code: "EQ-02",
            typeCode: "sensor",
            coreType: CoreEquipmentType.Sensor,
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
            createdAt: DateTime.UtcNow.AddMonths(-1),
            createdByUserId: userId,
            updatedAt: DateTime.UtcNow.AddDays(-1),
            updatedByUserId: userId);

        _equipmentRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        await Assert.ThrowsAsync<TenantMismatchException>(() =>
            _service.RecordAsync(siteId, equipmentId, new CreateCalibrationRequest
            {
                SiteId = siteId,
                EquipmentId = equipmentId,
                PerformedByUserId = userId,
                Method = CalibrationMethod.Single,
                ReferenceValue = 1m,
                MeasuredValue = 1m,
                Result = CalibrationResult.Pass
            }));
    }

    [Fact]
    public async Task GetOverdueAsync_DefaultsDueDateAndFlagsCritical()
    {
        var siteId = Guid.NewGuid();
        var performedAt = DateTime.UtcNow.AddDays(-40);
        var nextDue = DateTime.UtcNow.AddDays(-10);

        var calibration = Calibration.FromPersistence(
            Guid.NewGuid(),
            Guid.NewGuid(),
            channelCode: null,
            method: CalibrationMethod.Single,
            referenceValue: 1m,
            measuredValue: 1.2m,
            result: CalibrationResult.OutOfTolerance,
            deviation: 0.2m,
            deviationPct: 20m,
            performedAt: performedAt,
            performedByUserId: Guid.NewGuid(),
            nextDueAt: nextDue,
            notes: null,
            attachmentUrl: null,
            coefficientsJson: null,
            firmwareVersionAtCalibration: null);

        DateTime? capturedDueBefore = null;
        _calibrationRepository
            .Setup(r => r.GetOverdueAsync(siteId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, DateTime, CancellationToken>((_, dueBefore, _) => capturedDueBefore = dueBefore)
            .ReturnsAsync(new List<Calibration> { calibration });

        var results = await _service.GetOverdueAsync(siteId, null);

        Assert.NotNull(capturedDueBefore);
        Assert.True((DateTime.UtcNow - capturedDueBefore!.Value).TotalSeconds < 5);
        Assert.Single(results);
        Assert.True(results[0].IsCritical);
    }
}
