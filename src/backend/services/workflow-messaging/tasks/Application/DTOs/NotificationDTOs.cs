using System;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

/// <summary>
/// Response payload for a user notification.
/// </summary>
public sealed class UserNotificationResponse
{
    public Guid Id { get; init; }
    public Guid SiteId { get; init; }
    public NotificationType NotificationType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Response for unread count.
/// </summary>
public sealed class NotificationCountResponse
{
    public int UnreadCount { get; init; }
}

/// <summary>
/// Request to send a notification to a user.
/// </summary>
public sealed class SendNotificationRequest
{
    public Guid UserId { get; init; }
    public Guid SiteId { get; init; }
    public NotificationType NotificationType { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public bool SendSlack { get; init; } = true;
}

