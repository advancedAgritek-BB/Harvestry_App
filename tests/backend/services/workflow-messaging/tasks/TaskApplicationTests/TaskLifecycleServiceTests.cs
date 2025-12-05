using System;
using System.Collections.Generic;
using System.Threading;
using SystemTask = System.Threading.Tasks.Task;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Moq;
using Xunit;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using DomainTaskDependency = Harvestry.Tasks.Domain.Entities.TaskDependency;
using DomainTaskStateHistory = Harvestry.Tasks.Domain.Entities.TaskStateHistory;
using DomainTaskWatcher = Harvestry.Tasks.Domain.Entities.TaskWatcher;
using DomainTaskTimeEntry = Harvestry.Tasks.Domain.Entities.TaskTimeEntry;

namespace Harvestry.Tasks.Application.Tests;

public sealed class TaskLifecycleServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock = new();
    private readonly Mock<ITaskGatingResolverService> _gatingMock = new();
    private readonly Mock<ISlackNotificationService> _slackMock = new();
    private readonly TaskLifecycleService _service;

    public TaskLifecycleServiceTests()
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TaskLifecycleService>>();
        _slackMock
            .Setup(s => s.SendNotificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<NotificationType>(),
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string?>()))
            .ReturnsAsync(Guid.NewGuid().ToString("N"));

        _service = new TaskLifecycleService(
            _repositoryMock.Object,
            _gatingMock.Object,
            _slackMock.Object,
            logger.Object);
    }

    [Fact]
    public async SystemTask CreateTaskAsync_PersistsTaskAndReturnsResponse()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            TaskType = TaskType.Operational,
            Title = "Calibrate sensors",
            Description = "Run calibration routine",
            Priority = TaskPriority.High,
            DueDate = DateTimeOffset.UtcNow.AddHours(2)
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        // Act
        var response = await _service.CreateTaskAsync(siteId, request, userId, CancellationToken.None);

        // Assert
        Assert.Equal(siteId, response.SiteId);
        Assert.Equal(request.Title, response.Title);
        Assert.Equal(TaskStatus.Pending, response.Status);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _slackMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskCreated,
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async SystemTask StartTaskAsync_WhenGated_ReturnsGatingResponseWithoutStarting()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var task = CreateTask(siteId, taskId, TaskStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(siteId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var gating = new TaskGatingStatusResponse
        {
            IsGated = true,
            Reasons = new[] { "Training incomplete" },
            MissingSopIds = Array.Empty<Guid>(),
            MissingTrainingIds = Array.Empty<Guid>()
        };

        _gatingMock
            .Setup(g => g.EvaluateAsync(task, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gating);

        // Act
        var response = await _service.StartTaskAsync(siteId, taskId, userId, CancellationToken.None);

        // Assert
        Assert.True(response.Gating.IsGated);
        Assert.Equal(TaskStatus.Pending, response.Task.Status);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()), Times.Never);
        _slackMock.Verify(s => s.SendNotificationAsync(
            It.IsAny<Guid>(),
            It.IsAny<NotificationType>(),
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async SystemTask StartTaskAsync_WhenDependenciesBlock_TaskStaysBlocked()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var dependencyTaskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var dependency = DomainTaskDependency.FromPersistence(
            Guid.NewGuid(),
            taskId,
            dependencyTaskId,
            DependencyType.FinishToStart,
            isBlocking: true,
            minimumLag: null);

        var task = DomainTask.FromPersistence(
            id: taskId,
            siteId: siteId,
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Run task",
            description: null,
            createdByUserId: userId,
            assignedByUserId: userId,
            assignedToUserId: null,
            assignedToRole: null,
            assignedAt: null,
            status: TaskStatus.Pending,
            priority: TaskPriority.Normal,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow,
            dueDate: null,
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
            cancellationReason: null,
            blockingReason: null,
            relatedEntityType: null,
            relatedEntityId: null,
            requiredSopIds: Array.Empty<Guid>(),
            requiredTrainingIds: Array.Empty<Guid>(),
            stateHistory: Array.Empty<DomainTaskStateHistory>(),
            dependencies: new[] { dependency },
            watchers: Array.Empty<DomainTaskWatcher>(),
            timeEntries: Array.Empty<DomainTaskTimeEntry>());

        var dependencyTask = CreateTask(siteId, dependencyTaskId, TaskStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(siteId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _repositoryMock
            .Setup(r => r.GetByIdsAsync(siteId, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { dependencyTask });

        _repositoryMock
            .Setup(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        _gatingMock
            .Setup(g => g.EvaluateAsync(task, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskGatingStatusResponse { IsGated = false });

        // Act
        var response = await _service.StartTaskAsync(siteId, taskId, userId, CancellationToken.None);

        // Assert
        Assert.True(response.Gating.IsGated);
        Assert.Equal(TaskStatus.Blocked, response.Task.Status);
        _repositoryMock.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _slackMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskBlocked,
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async SystemTask AssignTaskAsync_SendsSlackNotification()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = CreateTask(siteId, taskId, TaskStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(siteId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _repositoryMock
            .Setup(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var request = new AssignTaskRequest
        {
            UserId = Guid.NewGuid(),
            Role = "Operator",
            AssignedAt = DateTimeOffset.UtcNow
        };

        // Act
        await _service.AssignTaskAsync(siteId, taskId, request, userId, CancellationToken.None);

        // Assert
        _slackMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskAssigned,
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public async SystemTask CompleteTaskAsync_SendsSlackNotification()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = CreateTask(siteId, taskId, TaskStatus.InProgress);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(siteId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _repositoryMock
            .Setup(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        // Act
        await _service.CompleteTaskAsync(siteId, taskId, userId, CancellationToken.None);

        // Assert
        _slackMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskCompleted,
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Once);
    }

    private static DomainTask CreateTask(Guid siteId, Guid taskId, TaskStatus status)
    {
        var startedAt = status == TaskStatus.InProgress ? DateTimeOffset.UtcNow.AddMinutes(-10) : (DateTimeOffset?)null;

        return DomainTask.FromPersistence(
            id: taskId,
            siteId: siteId,
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Task",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            assignedToUserId: null,
            assignedToRole: null,
            assignedAt: null,
            status: status,
            priority: TaskPriority.Normal,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow,
            dueDate: null,
            startedAt: startedAt,
            completedAt: null,
            cancelledAt: null,
            cancellationReason: null,
            blockingReason: null,
            relatedEntityType: null,
            relatedEntityId: null,
            requiredSopIds: Array.Empty<Guid>(),
            requiredTrainingIds: Array.Empty<Guid>(),
            stateHistory: Array.Empty<DomainTaskStateHistory>(),
            dependencies: Array.Empty<DomainTaskDependency>(),
            watchers: Array.Empty<DomainTaskWatcher>(),
            timeEntries: Array.Empty<DomainTaskTimeEntry>());
    }
}
