using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Utilities;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using SlackChannelMappingEntity = Harvestry.Tasks.Domain.Entities.SlackChannelMapping;
using SlackNotificationEntity = Harvestry.Tasks.Domain.Entities.SlackNotification;

namespace Harvestry.Tasks.Application.Services;

public sealed class SlackNotificationService : ISlackNotificationService
{
    private readonly ISlackWorkspaceRepository _workspaceRepository;
    private readonly ISlackChannelMappingRepository _channelMappingRepository;
    private readonly ISlackNotificationQueueRepository _queueRepository;
    private readonly ILogger<SlackNotificationService> _logger;

    public SlackNotificationService(
        ISlackWorkspaceRepository workspaceRepository,
        ISlackChannelMappingRepository channelMappingRepository,
        ISlackNotificationQueueRepository queueRepository,
        ILogger<SlackNotificationService> logger)
    {
        _workspaceRepository = workspaceRepository ?? throw new ArgumentNullException(nameof(workspaceRepository));
        _channelMappingRepository = channelMappingRepository ?? throw new ArgumentNullException(nameof(channelMappingRepository));
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> SendNotificationAsync(Guid siteId, NotificationType notificationType, object payload, int priority, CancellationToken cancellationToken, string? requestId = null)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (notificationType == NotificationType.Undefined)
        {
            throw new ArgumentException("Notification type must be defined.", nameof(notificationType));
        }

        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (priority < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be non-negative.");
        }

        var mappings = await _channelMappingRepository
            .GetBySiteAsync(siteId, cancellationToken)
            .ConfigureAwait(false);

        var notificationKey = ToNotificationKey(notificationType);
        var targetMappings = mappings
            .Where(m => m.IsActive && string.Equals(m.NotificationType, notificationKey, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (targetMappings.Length == 0)
        {
            _logger.LogWarning("No Slack channel mappings configured for site {SiteId} and notification type {NotificationType}", siteId, notificationType);
            return string.Empty;
        }

        var payloadJson = SlackMessageFormatter.FormatNotification(notificationType, payload);
        var hasCustomRequest = !string.IsNullOrWhiteSpace(requestId);
        var effectiveRequestId = hasCustomRequest
            ? requestId!.Trim()
            : Guid.NewGuid().ToString("N");

        var enqueued = false;
        var alreadyQueued = false;

        foreach (var mapping in targetMappings)
        {
            if (hasCustomRequest)
            {
                var exists = await _queueRepository
                    .ExistsAsync(effectiveRequestId, mapping.SlackWorkspaceId, mapping.ChannelId, cancellationToken)
                    .ConfigureAwait(false);

                if (exists)
                {
                    alreadyQueued = true;
                    continue;
                }
            }

            var workspace = await _workspaceRepository
                .GetByIdAsync(siteId, mapping.SlackWorkspaceId, cancellationToken)
                .ConfigureAwait(false);

            if (workspace is null || !workspace.IsActive)
            {
                _logger.LogWarning(
                    "Slack workspace {WorkspaceId} inactive or missing for site {SiteId}",
                    mapping.SlackWorkspaceId,
                    siteId);
                continue;
            }

            var notification = SlackNotificationEntity.Create(
                siteId,
                mapping.SlackWorkspaceId,
                mapping.ChannelId,
                notificationType,
                payloadJson,
                effectiveRequestId,
                priority,
                maxAttempts: 5);

            await _queueRepository.EnqueueAsync(notification, cancellationToken).ConfigureAwait(false);
            enqueued = true;
        }

        if (enqueued)
        {
            await _queueRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return effectiveRequestId;
        }

        if (alreadyQueued)
        {
            return effectiveRequestId;
        }

        return string.Empty;
    }

    public async Task<IReadOnlyList<SlackChannelMappingResponse>> GetChannelMappingsAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var mappings = await _channelMappingRepository.GetBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
        return mappings.Select(ToChannelMappingResponse).ToArray();
    }

    public async Task<IReadOnlyList<SlackNotificationResponse>> GetFailedNotificationsAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var notifications = await _queueRepository
            .GetFailedNotificationsAsync(siteId, cancellationToken)
            .ConfigureAwait(false);

        return notifications.Select(ToNotificationResponse).ToArray();
    }

    public async Task RetryFailedNotificationAsync(Guid siteId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await _queueRepository
            .GetByIdAsync(notificationId, cancellationToken)
            .ConfigureAwait(false);

        if (notification is null || notification.SiteId != siteId)
        {
            throw new KeyNotFoundException($"Slack notification {notificationId} not found for site {siteId}.");
        }

        if (!notification.CanRetry())
        {
            throw new InvalidOperationException($"Slack notification {notificationId} has exceeded max attempts.");
        }

        notification.ScheduleRetry(TimeSpan.FromMinutes(1));
        await _queueRepository.UpdateAsync(notification, cancellationToken).ConfigureAwait(false);
        await _queueRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static SlackChannelMappingResponse ToChannelMappingResponse(SlackChannelMappingEntity mapping)
    {
        return new SlackChannelMappingResponse
        {
            SlackChannelMappingId = mapping.Id,
            SlackWorkspaceId = mapping.SlackWorkspaceId,
            SiteId = mapping.SiteId,
            ChannelId = mapping.ChannelId,
            ChannelName = mapping.ChannelName,
            NotificationType = FromNotificationKey(mapping.NotificationType),
            IsActive = mapping.IsActive,
            CreatedAt = mapping.CreatedAt,
            CreatedBy = mapping.CreatedByUserId
        };
    }

    private static SlackNotificationResponse ToNotificationResponse(SlackNotificationEntity notification)
    {
        return new SlackNotificationResponse
        {
            SlackNotificationId = notification.Id,
            SiteId = notification.SiteId,
            SlackWorkspaceId = notification.SlackWorkspaceId,
            ChannelId = notification.ChannelId,
            NotificationType = notification.NotificationType,
            PayloadJson = notification.PayloadJson,
            RequestId = notification.RequestId,
            Status = notification.Status,
            Priority = notification.Priority,
            AttemptCount = notification.AttemptCount,
            MaxAttempts = notification.MaxAttempts,
            NextAttemptAt = notification.NextAttemptAt,
            LastError = notification.LastError,
            CreatedAt = notification.CreatedAt
        };
    }

    private static string ToNotificationKey(NotificationType notificationType) => notificationType switch
    {
        NotificationType.TaskCreated => "task_created",
        NotificationType.TaskAssigned => "task_assigned",
        NotificationType.TaskCompleted => "task_completed",
        NotificationType.TaskOverdue => "task_overdue",
        NotificationType.TaskBlocked => "task_blocked",
        NotificationType.ConversationMention => "conversation_mention",
        NotificationType.AlertCritical => "alert_critical",
        NotificationType.AlertWarning => "alert_warning",
        _ => "task_created"
    };

    private static NotificationType FromNotificationKey(string key) => key?.ToLowerInvariant() switch
    {
        "task_created" => NotificationType.TaskCreated,
        "task_assigned" => NotificationType.TaskAssigned,
        "task_completed" => NotificationType.TaskCompleted,
        "task_overdue" => NotificationType.TaskOverdue,
        "task_blocked" => NotificationType.TaskBlocked,
        "conversation_mention" => NotificationType.ConversationMention,
        "alert_critical" => NotificationType.AlertCritical,
        "alert_warning" => NotificationType.AlertWarning,
        _ => NotificationType.Undefined
    };
}
