using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using SlackNotificationEntity = Harvestry.Tasks.Domain.Entities.SlackNotification;
using SlackChannelMappingEntity = Harvestry.Tasks.Domain.Entities.SlackChannelMapping;
using SlackWorkspaceEntity = Harvestry.Tasks.Domain.Entities.SlackWorkspace;
using Xunit;

namespace Harvestry.Tasks.Application.Tests.Slack;

public sealed class SlackNotificationServiceTests
{
    private readonly Mock<ISlackWorkspaceRepository> _workspaceRepository = new();
    private readonly Mock<ISlackChannelMappingRepository> _channelMappingRepository = new();
    private readonly Mock<ISlackNotificationQueueRepository> _queueRepository = new();
    private readonly SlackNotificationService _service;

    public SlackNotificationServiceTests()
    {
        var logger = new Mock<ILogger<SlackNotificationService>>();
        _service = new SlackNotificationService(
            _workspaceRepository.Object,
            _channelMappingRepository.Object,
            _queueRepository.Object,
            logger.Object);
    }

    [Fact]
    public async Task SendNotificationAsync_EnqueuesNotification()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var channelMapping = SlackChannelMappingEntity.Create(siteId, workspaceId, "C123", "task-alerts", "task_created", Guid.NewGuid());

        _channelMappingRepository
            .Setup(r => r.GetBySiteAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { channelMapping });

        var workspace = SlackWorkspaceEntity.Create(siteId, "T123", "Test Workspace", "token", null, Guid.NewGuid());
        _workspaceRepository
            .Setup(r => r.GetByIdAsync(siteId, workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        _queueRepository
            .Setup(r => r.EnqueueAsync(It.IsAny<SlackNotificationEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _queueRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var payload = new { taskId = Guid.NewGuid(), title = "Test" };

        // Act
        var requestId = await _service.SendNotificationAsync(siteId, NotificationType.TaskCreated, payload, priority: 1, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(requestId));
        _queueRepository.Verify(r => r.EnqueueAsync(It.IsAny<SlackNotificationEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _queueRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_WithExistingRequestId_SkipsDuplicateEnqueue()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var mapping = SlackChannelMappingEntity.Create(siteId, workspaceId, "C999", "alerts", "task_overdue", Guid.NewGuid());

        _channelMappingRepository
            .Setup(r => r.GetBySiteAsync(siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { mapping });

        var workspace = SlackWorkspaceEntity.Create(siteId, "T999", "Ops", "token", null, Guid.NewGuid());
        _workspaceRepository
            .Setup(r => r.GetByIdAsync(siteId, workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        const string requestId = "task-overdue:request";
        _queueRepository
            .Setup(r => r.ExistsAsync(requestId, workspaceId, mapping.ChannelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var payload = new { taskId = Guid.NewGuid(), title = "Test" };

        // Act
        var result = await _service.SendNotificationAsync(siteId, NotificationType.TaskOverdue, payload, priority: 3, CancellationToken.None, requestId);

        // Assert
        Assert.Equal(requestId, result);
        _queueRepository.Verify(r => r.EnqueueAsync(It.IsAny<SlackNotificationEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _queueRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
