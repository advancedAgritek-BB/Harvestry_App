using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Domain.Enums;
using Moq;
using Xunit;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;

namespace Harvestry.Tasks.Application.Tests;

public sealed class TaskGatingResolverServiceTests
{
    private readonly Mock<IUserReadinessProvider> _readinessProviderMock = new();
    private readonly TaskGatingResolverService _service;

    public TaskGatingResolverServiceTests()
    {
        _service = new TaskGatingResolverService(_readinessProviderMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WhenNoRequirements_ReturnsNotGated()
    {
        // Arrange
        var task = DomainTask.Create(
            siteId: Guid.NewGuid(),
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Inspect irrigation",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            priority: TaskPriority.Normal,
            requiredSopIds: Array.Empty<Guid>(),
            requiredTrainingIds: Array.Empty<Guid>(),
            relatedEntityType: null,
            relatedEntityId: null);

        var userId = Guid.NewGuid();

        // Act
        var result = await _service.EvaluateAsync(task, userId, CancellationToken.None);

        // Assert
        Assert.False(result.IsGated);
        Assert.Empty(result.MissingSopIds);
        Assert.Empty(result.MissingTrainingIds);
        Assert.Empty(result.Reasons);
        _readinessProviderMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EvaluateAsync_WhenRequirementsMissing_ReturnsGated()
    {
        // Arrange
        var sopId = Guid.NewGuid();
        var trainingId = Guid.NewGuid();

        var task = DomainTask.Create(
            siteId: Guid.NewGuid(),
            taskType: TaskType.Compliance,
            customTaskType: null,
            title: "Run smoke test",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            priority: TaskPriority.High,
            requiredSopIds: new[] { sopId },
            requiredTrainingIds: new[] { trainingId },
            relatedEntityType: null,
            relatedEntityId: null);

        var userId = Guid.NewGuid();

        _readinessProviderMock
            .Setup(p => p.GetCompletedSopIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        _readinessProviderMock
            .Setup(p => p.GetCompletedTrainingIdsAsync(userId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { trainingId });

        // Act
        var result = await _service.EvaluateAsync(task, userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsGated);
        Assert.Contains(sopId, result.MissingSopIds);
        Assert.Empty(result.MissingTrainingIds);
        Assert.Contains(result.Reasons, reason => reason.Contains("SOP", StringComparison.OrdinalIgnoreCase));
    }
}
