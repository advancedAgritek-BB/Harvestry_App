using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

public sealed class TaskOverdueMonitorWorkerTests
{
    [Fact]
    public async SystemTask ProcessOverdueTasksAsync_SendsSlackNotificationForOverdueTask()
    {
        // Arrange
        var repositoryMock = new Mock<ITaskRepository>();
        var slackServiceMock = new Mock<ISlackNotificationService>();
        var scopeFactory = CreateScopeFactory(repositoryMock, slackServiceMock);
        var logger = new Mock<ILogger<TaskOverdueMonitorWorker>>();
        var worker = new TaskOverdueMonitorWorker(scopeFactory.Object, logger.Object);

        var now = DateTimeOffset.UtcNow;
        var siteId = Guid.NewGuid();
        var task = DomainTask.FromPersistence(
            id: Guid.NewGuid(),
            siteId: siteId,
            taskType: TaskType.Operational,
            customTaskType: null,
            title: "Inspect irrigation",
            description: null,
            createdByUserId: Guid.NewGuid(),
            assignedByUserId: Guid.NewGuid(),
            assignedToUserId: Guid.NewGuid(),
            assignedToRole: null,
            assignedAt: now.AddHours(-3),
            status: DomainTaskStatus.InProgress,
            priority: TaskPriority.High,
            createdAt: now.AddHours(-4),
            updatedAt: now.AddHours(-2),
            dueDate: now.AddMinutes(-30),
            startedAt: now.AddHours(-2),
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

        repositoryMock
            .Setup(r => r.GetOverdueAsync(It.IsAny<DateTimeOffset>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { task });

        slackServiceMock
            .Setup(s => s.SendNotificationAsync(
                siteId,
                NotificationType.TaskOverdue,
                It.Is<object>(payload => payload != null),
                9,
                It.IsAny<CancellationToken>(),
                It.Is<string>(id => id.StartsWith("task-overdue:"))))
            .ReturnsAsync(Guid.NewGuid().ToString("N"));

        var method = typeof(TaskOverdueMonitorWorker)
            .GetMethod("ProcessOverdueTasksAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate ProcessOverdueTasksAsync method");

        // Act
        await ((SystemTask)method.Invoke(worker, new object[] { CancellationToken.None })!).ConfigureAwait(false);

        // Assert
        slackServiceMock.Verify(s => s.SendNotificationAsync(
            siteId,
            NotificationType.TaskOverdue,
            It.IsAny<object>(),
            9,
            It.IsAny<CancellationToken>(),
            It.Is<string>(id => id.StartsWith("task-overdue:"))), Times.Once);
    }

    private static Mock<IServiceScopeFactory> CreateScopeFactory(
        Mock<ITaskRepository> repositoryMock,
        Mock<ISlackNotificationService> slackServiceMock)
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ITaskRepository)))
            .Returns(repositoryMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISlackNotificationService)))
            .Returns(slackServiceMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        return scopeFactoryMock;
    }
}
