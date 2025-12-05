using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Workers;
using Harvestry.Tasks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using DomainTaskDependency = Harvestry.Tasks.Domain.Entities.TaskDependency;
using DomainTaskStateHistory = Harvestry.Tasks.Domain.Entities.TaskStateHistory;
using DomainTaskWatcher = Harvestry.Tasks.Domain.Entities.TaskWatcher;
using DomainTaskTimeEntry = Harvestry.Tasks.Domain.Entities.TaskTimeEntry;

using SystemTask = System.Threading.Tasks.Task;
using DomainTaskStatus = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.Application.Tests.Workers;

public sealed class TaskDependencyResolverWorkerTests
{
    [Fact]
    public async SystemTask ResolveDependenciesAsync_UnblocksTask_WhenDependenciesSatisfied()
    {
        // Arrange
        var repositoryMock = new Mock<ITaskRepository>();
        var slackServiceMock = new Mock<ISlackNotificationService>();
        var scopeFactory = CreateScopeFactory(repositoryMock, slackServiceMock);
        var logger = new Mock<ILogger<TaskDependencyResolverWorker>>();
        var worker = new TaskDependencyResolverWorker(scopeFactory.Object, logger.Object);

        var now = DateTimeOffset.UtcNow;
        var siteId = Guid.NewGuid();
        var dependencyTaskId = Guid.NewGuid();
        var blockedTaskId = Guid.NewGuid();

        var dependency = DomainTaskDependency.Create(blockedTaskId, dependencyTaskId);
        var blockedTask = DomainTask.FromPersistence(
            id: blockedTaskId,
            siteId: siteId,
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Harvest plot A",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            assignedToUserId: Guid.NewGuid(),
            assignedToRole: null,
            assignedAt: now.AddHours(-6),
            status: DomainTaskStatus.Blocked,
            priority: TaskPriority.Normal,
            createdAt: now.AddHours(-7),
            updatedAt: now.AddHours(-5),
            dueDate: now.AddHours(2),
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
            cancellationReason: null,
            blockingReason: "Task calibration must complete first",
            relatedEntityType: null,
            relatedEntityId: null,
            requiredSopIds: Array.Empty<Guid>(),
            requiredTrainingIds: Array.Empty<Guid>(),
            stateHistory: Array.Empty<DomainTaskStateHistory>(),
            dependencies: new[] { dependency },
            watchers: Array.Empty<DomainTaskWatcher>(),
            timeEntries: Array.Empty<DomainTaskTimeEntry>());

        var dependencyTask = DomainTask.FromPersistence(
            id: dependencyTaskId,
            siteId: siteId,
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Calibrate sensors",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            assignedToUserId: Guid.NewGuid(),
            assignedToRole: null,
            assignedAt: now.AddHours(-8),
            status: DomainTaskStatus.Completed,
            priority: TaskPriority.Normal,
            createdAt: now.AddHours(-9),
            updatedAt: now.AddHours(-4),
            dueDate: now.AddHours(-2),
            startedAt: now.AddHours(-8),
            completedAt: now.AddHours(-3),
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

        repositoryMock
            .Setup(r => r.GetBlockedWithDependenciesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { blockedTask });
        repositoryMock
            .Setup(r => r.GetByIdsAsync(siteId, It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(dependencyTaskId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { dependencyTask });
        repositoryMock
            .Setup(r => r.UpdateAsync(blockedTask, It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        slackServiceMock
            .Setup(s => s.SendNotificationAsync(
                siteId,
                NotificationType.TaskAssigned,
                It.IsAny<object>(),
                6,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync(Guid.NewGuid().ToString("N"));

        // Act
        await worker.ResolveDependenciesAsync(CancellationToken.None).ConfigureAwait(false);

        // Assert
        repositoryMock.Verify(r => r.UpdateAsync(blockedTask, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        slackServiceMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskAssigned,
            It.IsAny<object>(),
            6,
            It.IsAny<CancellationToken>(),
            null), Times.Once);
        Assert.Equal(DomainTaskStatus.Pending, blockedTask.Status);
        Assert.Null(blockedTask.BlockingReason);
    }

    private static Mock<IServiceScopeFactory> CreateScopeFactory(
        Mock<ITaskRepository> repositoryMock,
        Mock<ISlackNotificationService> slackServiceMock)
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TasksDbContext(options);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ITaskRepository)))
            .Returns(repositoryMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISlackNotificationService)))
            .Returns(slackServiceMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(TasksDbContext)))
            .Returns(dbContext);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeMock.Setup(s => s.Dispose()).Callback(() => dbContext.Dispose());

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        return scopeFactoryMock;
    }
}
