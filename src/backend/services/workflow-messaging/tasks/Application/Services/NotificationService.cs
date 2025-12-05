using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Application.Mappers;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IUserNotificationRepository _notificationRepository;
    private readonly ISlackNotificationService _slackNotificationService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUserNotificationRepository notificationRepository,
        ISlackNotificationService slackNotificationService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _slackNotificationService = slackNotificationService ?? throw new ArgumentNullException(nameof(slackNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserNotificationResponse> SendNotificationAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        _logger.LogInformation(
            "Sending notification to user {UserId}: {Title}",
            request.UserId,
            request.Title);

        var notification = UserNotification.Create(
            request.UserId,
            request.SiteId,
            request.NotificationType,
            request.Title,
            request.Message,
            request.RelatedEntityType,
            request.RelatedEntityId);

        await _notificationRepository.AddAsync(notification, cancellationToken).ConfigureAwait(false);
        await _notificationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Also send to Slack if requested
        if (request.SendSlack)
        {
            try
            {
                await _slackNotificationService.SendNotificationAsync(
                    request.SiteId,
                    request.NotificationType,
                    new { Title = request.Title, Message = request.Message, UserId = request.UserId },
                    priority: 5,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Slack notification for user {UserId}", request.UserId);
            }
        }

        return NotificationMapper.ToResponse(notification);
    }

    public async Task NotifyTaskAssignedAsync(
        Guid assigneeUserId,
        Guid siteId,
        Guid taskId,
        string taskTitle,
        Guid assignedByUserId,
        CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(new SendNotificationRequest
        {
            UserId = assigneeUserId,
            SiteId = siteId,
            NotificationType = NotificationType.TaskAssigned,
            Title = $"Task Assigned: {taskTitle}",
            Message = "You have been assigned a new task.",
            RelatedEntityType = "Task",
            RelatedEntityId = taskId,
            SendSlack = true
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task NotifyTaskCommentAsync(
        Guid siteId,
        Guid taskId,
        string taskTitle,
        Guid commentAuthorUserId,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in recipientUserIds.Where(id => id != commentAuthorUserId))
        {
            await SendNotificationAsync(new SendNotificationRequest
            {
                UserId = userId,
                SiteId = siteId,
                NotificationType = NotificationType.ConversationMention,
                Title = $"New comment on: {taskTitle}",
                Message = "A new comment has been added to a task you're watching.",
                RelatedEntityType = "Task",
                RelatedEntityId = taskId,
                SendSlack = true
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task NotifyTaskStatusChangedAsync(
        Guid siteId,
        Guid taskId,
        string taskTitle,
        string newStatus,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        var notificationType = newStatus switch
        {
            "Completed" => NotificationType.TaskCompleted,
            "Blocked" => NotificationType.TaskBlocked,
            _ => NotificationType.TaskCreated
        };

        foreach (var userId in recipientUserIds)
        {
            await SendNotificationAsync(new SendNotificationRequest
            {
                UserId = userId,
                SiteId = siteId,
                NotificationType = notificationType,
                Title = $"Task {newStatus}: {taskTitle}",
                Message = $"Task status has changed to {newStatus}.",
                RelatedEntityType = "Task",
                RelatedEntityId = taskId,
                SendSlack = true
            }, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyList<UserNotificationResponse>> GetNotificationsAsync(
        Guid userId,
        bool? unreadOnly,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository
            .GetByUserAsync(userId, unreadOnly, limit, cancellationToken)
            .ConfigureAwait(false);

        return notifications.Select(NotificationMapper.ToResponse).ToArray();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository
            .GetUnreadCountAsync(userId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository
            .GetByIdAsync(userId, notificationId, cancellationToken)
            .ConfigureAwait(false);

        if (notification is null)
        {
            _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
            return;
        }

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification, cancellationToken).ConfigureAwait(false);
        await _notificationRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}

