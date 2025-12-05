using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using SystemTask = System.Threading.Tasks.Task;

namespace Harvestry.Tasks.Application.Tests.Workers;

public sealed class SlackNotificationWorkerTests
{
    [Fact]
    public async SystemTask ProcessBatchAsync_PublishesBridgeLog_OnSuccess()
    {
        // Arrange
        var queueRepository = new Mock<ISlackNotificationQueueRepository>();
        var workspaceRepository = new Mock<ISlackWorkspaceRepository>();
        var bridgeLogRepository = new Mock<ISlackMessageBridgeLogRepository>();
        var slackApiClient = new Mock<ISlackApiClient>();
        var logger = new Mock<ILogger<SlackNotificationWorker>>();

        var notification = SlackNotification.Create(
            siteId: Guid.NewGuid(),
            slackWorkspaceId: Guid.NewGuid(),
            channelId: "C123",
            notificationType: NotificationType.TaskCreated,
            payloadJson: "{}",
            requestId: Guid.NewGuid().ToString("N"),
            priority: 5,
            maxAttempts: 3);

        queueRepository
            .Setup(r => r.GetPendingNotificationsAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { notification });
        queueRepository
            .Setup(r => r.UpdateAsync(It.IsAny<SlackNotification>(), It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        queueRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var workspace = SlackWorkspace.Create(
            siteId: notification.SiteId,
            workspaceId: "T123",
            workspaceName: "Test",
            encryptedBotToken: "xoxb-token",
            encryptedRefreshToken: null,
            installedByUserId: Guid.NewGuid());

        workspaceRepository
            .Setup(r => r.GetByIdAsync(notification.SiteId, notification.SlackWorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        slackApiClient
            .Setup(c => c.SendMessageAsync(
                workspace.EncryptedBotToken!,
                notification.ChannelId,
                notification.PayloadJson,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlackMessageResponse(notification.ChannelId, "123.456", null, DateTimeOffset.UtcNow));

        bridgeLogRepository
            .Setup(r => r.GetByRequestIdAsync(notification.RequestId, notification.SlackWorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SlackMessageBridgeLog?)null);

        bridgeLogRepository
            .Setup(r => r.AddAsync(It.Is<SlackMessageBridgeLog>(log =>
                log.Status == SlackMessageBridgeStatus.Sent &&
                log.AttemptCount == 1 &&
                log.RequestId == notification.RequestId), It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask)
            .Verifiable();
        bridgeLogRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var worker = new SlackNotificationWorker(CreateScopeFactory(
            queueRepository,
            workspaceRepository,
            bridgeLogRepository,
            slackApiClient), logger.Object);

        var processMethod = typeof(SlackNotificationWorker)
            .GetMethod("ProcessBatchAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate ProcessBatchAsync method.");

        // Act
        await ((SystemTask)processMethod.Invoke(worker, new object[] { CancellationToken.None })!).ConfigureAwait(false);

        // Assert
        bridgeLogRepository.Verify();
        queueRepository.Verify(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        queueRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async SystemTask ProcessBatchAsync_RecordsFailure_OnSlackError()
    {
        // Arrange
        var queueRepository = new Mock<ISlackNotificationQueueRepository>();
        var workspaceRepository = new Mock<ISlackWorkspaceRepository>();
        var bridgeLogRepository = new Mock<ISlackMessageBridgeLogRepository>();
        var slackApiClient = new Mock<ISlackApiClient>();
        var logger = new Mock<ILogger<SlackNotificationWorker>>();

        var notification = SlackNotification.Create(
            siteId: Guid.NewGuid(),
            slackWorkspaceId: Guid.NewGuid(),
            channelId: "C124",
            notificationType: NotificationType.TaskCreated,
            payloadJson: "{}",
            requestId: Guid.NewGuid().ToString("N"),
            priority: 5,
            maxAttempts: 3);

        queueRepository
            .Setup(r => r.GetPendingNotificationsAsync(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { notification });
        queueRepository
            .Setup(r => r.UpdateAsync(It.IsAny<SlackNotification>(), It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);
        queueRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var workspace = SlackWorkspace.Create(
            siteId: notification.SiteId,
            workspaceId: "T321",
            workspaceName: "Test",
            encryptedBotToken: "xoxb-token",
            encryptedRefreshToken: null,
            installedByUserId: Guid.NewGuid());

        workspaceRepository
            .Setup(r => r.GetByIdAsync(notification.SiteId, notification.SlackWorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        slackApiClient
            .Setup(c => c.SendMessageAsync(
                workspace.EncryptedBotToken!,
                notification.ChannelId,
                notification.PayloadJson,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("invalid_auth"));

        bridgeLogRepository
            .Setup(r => r.GetByRequestIdAsync(notification.RequestId, notification.SlackWorkspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SlackMessageBridgeLog?)null);

        bridgeLogRepository
            .Setup(r => r.AddAsync(It.Is<SlackMessageBridgeLog>(log =>
                log.Status == SlackMessageBridgeStatus.Failed &&
                log.AttemptCount == 1 &&
                log.ErrorMessage == "invalid_auth"), It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask)
            .Verifiable();
        bridgeLogRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(SystemTask.CompletedTask);

        var worker = new SlackNotificationWorker(CreateScopeFactory(
            queueRepository,
            workspaceRepository,
            bridgeLogRepository,
            slackApiClient), logger.Object);

        var processMethod = typeof(SlackNotificationWorker)
            .GetMethod("ProcessBatchAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Unable to locate ProcessBatchAsync method.");

        // Act
        await ((SystemTask)processMethod.Invoke(worker, new object[] { CancellationToken.None })!).ConfigureAwait(false);

        // Assert
        bridgeLogRepository.Verify();
        queueRepository.Verify(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        queueRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static IServiceScopeFactory CreateScopeFactory(
        Mock<ISlackNotificationQueueRepository> queueRepository,
        Mock<ISlackWorkspaceRepository> workspaceRepository,
        Mock<ISlackMessageBridgeLogRepository> bridgeLogRepository,
        Mock<ISlackApiClient> slackApiClient)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(ISlackNotificationQueueRepository)))
            .Returns(queueRepository.Object);
        serviceProvider
            .Setup(sp => sp.GetService(typeof(ISlackWorkspaceRepository)))
            .Returns(workspaceRepository.Object);
        serviceProvider
            .Setup(sp => sp.GetService(typeof(ISlackMessageBridgeLogRepository)))
            .Returns(bridgeLogRepository.Object);
        serviceProvider
            .Setup(sp => sp.GetService(typeof(ISlackApiClient)))
            .Returns(slackApiClient.Object);

        var scope = new Mock<IServiceScope>();
        scope.SetupGet(s => s.ServiceProvider).Returns(serviceProvider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        return scopeFactory.Object;
    }
}
