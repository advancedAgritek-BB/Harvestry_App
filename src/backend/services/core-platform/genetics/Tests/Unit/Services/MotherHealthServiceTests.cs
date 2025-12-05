using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Services;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Genetics.Tests.Unit.Services;

public sealed class MotherHealthServiceTests
{
    private readonly Mock<IMotherPlantRepository> _motherPlantRepository = new();
    private readonly Mock<IMotherHealthLogRepository> _healthLogRepository = new();
    private readonly Mock<IPropagationSettingsRepository> _propagationSettingsRepository = new();
    private readonly Mock<IPropagationOverrideRequestRepository> _overrideRepository = new();
    private readonly Mock<IBatchRepository> _batchRepository = new();
    private readonly Mock<IStrainRepository> _strainRepository = new();
    private readonly MotherHealthService _service;

    public MotherHealthServiceTests()
    {
        var logger = Mock.Of<ILogger<MotherHealthService>>();
        _service = new MotherHealthService(
            _motherPlantRepository.Object,
            _healthLogRepository.Object,
            _propagationSettingsRepository.Object,
            _overrideRepository.Object,
            _batchRepository.Object,
            _strainRepository.Object,
            logger);
    }

    [Fact]
    public async Task CreateMotherPlantAsync_PersistsAggregate()
    {
        var siteId = Guid.NewGuid();
        var batchId = Guid.NewGuid();
        var strainId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var batch = CreateBatch(siteId, strainId, batchId, userId);

        _batchRepository
            .Setup(repo => repo.GetByIdAsync(batchId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);

        _strainRepository
            .Setup(repo => repo.ExistsAsync(strainId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _motherPlantRepository
            .Setup(repo => repo.GetByPlantTagAsync(siteId, "MP-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MotherPlant?)null);

        MotherPlant? captured = null;
        _motherPlantRepository
            .Setup(repo => repo.AddAsync(It.IsAny<MotherPlant>(), It.IsAny<CancellationToken>()))
            .Callback<MotherPlant, CancellationToken>((mother, _) => captured = mother)
            .Returns(Task.CompletedTask);

        var request = new CreateMotherPlantRequest(
            batchId,
            strainId,
            "MP-001",
            DateOnly.FromDateTime(DateTime.UtcNow));

        var response = await _service.CreateMotherPlantAsync(siteId, request, userId, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("MP-001", captured!.PlantId.Value);
        Assert.Equal(response.Id, captured.Id);
        _motherPlantRepository.Verify(repo => repo.AddAsync(It.IsAny<MotherPlant>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPropagationAsync_WithinLimits_IncrementsCount()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mother = MotherPlant.Create(
            siteId,
            Guid.NewGuid(),
            PlantId.Create("MP-002"),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            userId);

        _motherPlantRepository
            .Setup(repo => repo.GetByIdAsync(siteId, mother.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mother);

        _propagationSettingsRepository
            .Setup(repo => repo.GetBySiteAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropagationSettings?)null);

        _propagationSettingsRepository
            .Setup(repo => repo.UpsertAsync(It.IsAny<PropagationSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropagationSettings settings, CancellationToken _) => settings);

        _motherPlantRepository
            .Setup(repo => repo.UpdatePropagationAsync(siteId, mother.Id, It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<int>(), userId, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _motherPlantRepository
            .Setup(repo => repo.UpdateAsync(mother, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterPropagationRequest(5);
        var response = await _service.RegisterPropagationAsync(siteId, mother.Id, request, userId, CancellationToken.None);

        Assert.Equal(5, response.PropagationCount);
    }

    [Fact]
    public async Task RequestPropagationOverrideAsync_PersistsRequest()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        PropagationOverrideRequest? captured = null;

        _overrideRepository
            .Setup(repo => repo.AddAsync(It.IsAny<PropagationOverrideRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PropagationOverrideRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync((PropagationOverrideRequest request, CancellationToken _) => request);

        var request = new CreatePropagationOverrideRequest(Guid.NewGuid(), null, 25, "Urgent clone run");
        var response = await _service.RequestPropagationOverrideAsync(siteId, request, userId, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(response.Id, captured!.Id);
        Assert.Equal(PropagationOverrideStatus.Pending, captured.Status);
    }

    private static Batch CreateBatch(Guid siteId, Guid strainId, Guid batchId, Guid userId)
    {
        var batchCode = BatchCode.Create("BATCH-001");
        var batch = Batch.Create(
            siteId,
            strainId,
            batchCode,
            "Mother Batch",
            BatchType.MotherPlant,
            BatchSourceType.Propagation,
            10,
            Guid.NewGuid(),
            userId);

        return Batch.FromPersistence(
            batchId,
            siteId,
            strainId,
            batchCode,
            batch.BatchName,
            batch.BatchType,
            batch.SourceType,
            null,
            batch.Generation,
            batch.PlantCount,
            batch.TargetPlantCount,
            batch.CurrentStageId,
            batch.StageStartedAt,
            batch.ExpectedHarvestDate,
            batch.ActualHarvestDate,
            batch.LocationId,
            batch.RoomId,
            batch.ZoneId,
            batch.Status,
            batch.Notes,
            new Dictionary<string, object>(),
            DateTime.UtcNow,
            userId,
            DateTime.UtcNow,
            userId);
    }
}
