using System;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class SlackNotificationResponse
{
    public Guid SlackNotificationId { get; init; }
    public Guid SiteId { get; init; }
    public Guid SlackWorkspaceId { get; init; }
    public string ChannelId { get; init; } = string.Empty;
    public NotificationType NotificationType { get; init; }
    public string PayloadJson { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public NotificationStatus Status { get; init; }
    public int Priority { get; init; }
    public int AttemptCount { get; init; }
    public int MaxAttempts { get; init; }
    public DateTimeOffset NextAttemptAt { get; init; }
    public string? LastError { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
