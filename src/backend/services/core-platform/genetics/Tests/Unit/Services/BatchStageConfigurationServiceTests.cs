using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Services;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Genetics.Tests.Unit.Services;

public sealed class BatchStageConfigurationServiceTests
{
    private readonly Mock<IBatchStageDefinitionRepository> _stageRepository = new(MockBehavior.Strict);
    private readonly Mock<IBatchStageTransitionRepository> _transitionRepository = new(MockBehavior.Strict);
    private readonly BatchStageConfigurationService _sut;

    public BatchStageConfigurationServiceTests()
    {
        _sut = new BatchStageConfigurationService(
            _stageRepository.Object,
            _transitionRepository.Object,
            Mock.Of<ILogger<BatchStageConfigurationService>>());
    }

    [Fact]
    public async Task CreateTransitionAsync_FromStageMissing_ThrowsInvalidOperationException()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateStageTransitionRequest(Guid.NewGuid(), Guid.NewGuid());

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.FromStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.ToStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Func<Task> action = () => _sut.CreateTransitionAsync(request, siteId, userId, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"From stage {request.FromStageId} not found");

        _transitionRepository.Verify(repo => repo.CreateAsync(It.IsAny<BatchStageTransition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTransitionAsync_ExistingTransition_ThrowsInvalidOperationException()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateStageTransitionRequest(Guid.NewGuid(), Guid.NewGuid());
        var existing = BatchStageTransition.Create(siteId, request.FromStageId, request.ToStageId, userId);

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.FromStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.ToStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _transitionRepository
            .Setup(repo => repo.GetTransitionAsync(request.FromStageId, request.ToStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        Func<Task> action = () => _sut.CreateTransitionAsync(request, siteId, userId, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Transition from {request.FromStageId} to {request.ToStageId} already exists");

        _transitionRepository.Verify(repo => repo.CreateAsync(It.IsAny<BatchStageTransition>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTransitionAsync_Success_ReturnsResponse()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateStageTransitionRequest(Guid.NewGuid(), Guid.NewGuid(), true, true, "manager");
        var created = BatchStageTransition.Create(siteId, request.FromStageId, request.ToStageId, userId, request.AutoAdvance, request.RequiresApproval, request.ApprovalRole);

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.FromStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _stageRepository
            .Setup(repo => repo.ExistsAsync(request.ToStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _transitionRepository
            .Setup(repo => repo.GetTransitionAsync(request.FromStageId, request.ToStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchStageTransition?)null);

        _transitionRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<BatchStageTransition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _sut.CreateTransitionAsync(request, siteId, userId, CancellationToken.None);

        response.SiteId.Should().Be(siteId);
        response.FromStageId.Should().Be(request.FromStageId);
        response.ToStageId.Should().Be(request.ToStageId);
        response.AutoAdvance.Should().BeTrue();
        response.RequiresApproval.Should().BeTrue();
        response.ApprovalRole.Should().Be("manager");

        _transitionRepository.Verify(repo => repo.CreateAsync(It.IsAny<BatchStageTransition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStageAsync_WithTransitions_ThrowsInvalidOperationException()
    {
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stageId = Guid.NewGuid();
        var stage = BatchStageDefinition.Create(siteId, StageKey.Create("VEG"), "Veg", 1, userId);

        _stageRepository
            .Setup(repo => repo.GetByIdAsync(stageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stage);

        _transitionRepository
            .Setup(repo => repo.GetFromStageAsync(stageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BatchStageTransition> { BatchStageTransition.Create(siteId, stageId, Guid.NewGuid(), userId) });

        _transitionRepository
            .Setup(repo => repo.GetToStageAsync(stageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BatchStageTransition>());

        Func<Task> action = () => _sut.DeleteStageAsync(stageId, siteId, userId, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Cannot delete stage {stageId} because it has 1 transitions");

        _stageRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CanTransitionAsync_ReturnsTrueWhenTransitionExists()
    {
        var siteId = Guid.NewGuid();
        var fromStageId = Guid.NewGuid();
        var toStageId = Guid.NewGuid();

        _transitionRepository
            .Setup(repo => repo.GetTransitionAsync(fromStageId, toStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BatchStageTransition.Create(siteId, fromStageId, toStageId, Guid.NewGuid()));

        var result = await _sut.CanTransitionAsync(fromStageId, toStageId, siteId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanTransitionAsync_ReturnsFalseWhenTransitionMissing()
    {
        var siteId = Guid.NewGuid();
        var fromStageId = Guid.NewGuid();
        var toStageId = Guid.NewGuid();

        _transitionRepository
            .Setup(repo => repo.GetTransitionAsync(fromStageId, toStageId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchStageTransition?)null);

        var result = await _sut.CanTransitionAsync(fromStageId, toStageId, siteId, CancellationToken.None);

        result.Should().BeFalse();
    }
}
