using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Application.Services;

public sealed class SlackWebhookHandler : ISlackWebhookHandler
{
    private readonly ISlackNotificationService _notificationService;
    private readonly ILogger<SlackWebhookHandler> _logger;

    public SlackWebhookHandler(
        ISlackNotificationService notificationService,
        ILogger<SlackWebhookHandler> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SlackWebhookResult> HandleAsync(SlackWebhookRequest request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.Type, "url_verification", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(request.Challenge))
        {
            return new SlackWebhookChallenge(request.Challenge);
        }

        if (!string.Equals(request.Type, "event_callback", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unsupported Slack webhook type: {Type}", request.Type);
            return new SlackWebhookAck();
        }

        var payload = request.Event;

        if (payload.TryGetProperty("type", out var eventTypeProperty)
            && string.Equals(eventTypeProperty.GetString(), "app_mention", StringComparison.OrdinalIgnoreCase))
        {
            var text = payload.TryGetProperty("text", out var textProperty) ? textProperty.GetString() ?? string.Empty : string.Empty;
            var channel = payload.TryGetProperty("channel", out var channelProperty) ? channelProperty.GetString() ?? string.Empty : string.Empty;
            var siteId = payload.TryGetProperty("team", out var teamProperty) ? teamProperty.GetString() : null;

            if (!string.IsNullOrWhiteSpace(channel) && !string.IsNullOrWhiteSpace(siteId) && Guid.TryParse(siteId, out var siteGuid))
            {
                await _notificationService
                    .SendNotificationAsync(siteGuid, NotificationType.ConversationMention, new { text, channel }, priority: 5, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new SlackWebhookAck();
    }
}
