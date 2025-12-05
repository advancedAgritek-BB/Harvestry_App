using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a user (in-app and optionally Slack).
    /// </summary>
    Task<UserNotificationResponse> SendNotificationAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies a user when they are assigned a task.
    /// </summary>
    Task NotifyTaskAssignedAsync(
        Guid assigneeUserId,
        Guid siteId,
        Guid taskId,
        string taskTitle,
        Guid assignedByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies relevant users when a comment is posted on a task.
    /// </summary>
    Task NotifyTaskCommentAsync(
        Guid siteId,
        Guid taskId,
        string taskTitle,
        Guid commentAuthorUserId,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies relevant users when a task status changes.
    /// </summary>
    Task NotifyTaskStatusChangedAsync(
        Guid siteId,
        Guid taskId,
        string taskTitle,
        string newStatus,
        IReadOnlyCollection<Guid> recipientUserIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for the current user.
    /// </summary>
    Task<IReadOnlyList<UserNotificationResponse>> GetNotificationsAsync(
        Guid userId,
        bool? unreadOnly,
        int limit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a user.
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

