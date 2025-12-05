using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Services;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Tasks.Application.Tests.Slack;

public sealed class SlackWebhookHandlerTests
{
    private readonly Mock<ISlackNotificationService> _notificationService = new();
    private readonly SlackWebhookHandler _handler;

    public SlackWebhookHandlerTests()
    {
        var logger = new Mock<ILogger<SlackWebhookHandler>>();
        _handler = new SlackWebhookHandler(_notificationService.Object, logger.Object);
    }

    [Fact]
    public async Task HandleAsync_ReturnsChallenge_WhenUrlVerification()
    {
        // Arrange
        var request = new SlackWebhookRequest
        {
            Type = "url_verification",
            Challenge = "abc-123"
        };

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        var challenge = Assert.IsType<SlackWebhookChallenge>(result);
        Assert.Equal("abc-123", challenge.Challenge);
    }

    [Fact]
    public async Task HandleAsync_EnqueuesNotification_OnAppMention()
    {
        // Arrange
        var payload = JsonSerializer.SerializeToElement(new
        {
            type = "app_mention",
            text = "hello",
            channel = "C1",
            team = Guid.NewGuid().ToString()
        });
        var request = new SlackWebhookRequest
        {
            Type = "event_callback",
            Event = payload
        };

        _notificationService
            .Setup(s => s.SendNotificationAsync(
                It.IsAny<Guid>(),
                NotificationType.ConversationMention,
                It.IsAny<object>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string?>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.IsType<SlackWebhookAck>(result);
        _notificationService.Verify(s => s.SendNotificationAsync(
            It.IsAny<Guid>(),
            NotificationType.ConversationMention,
            It.IsAny<object>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>()), Times.Once);
    }
}
