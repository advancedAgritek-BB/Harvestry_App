using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class SlackNotificationRecord
{
    public Guid SlackNotificationId { get; set; }
    public Guid SiteId { get; set; }
    public Guid SlackWorkspaceId { get; set; }
    public string ChannelId { get; set; } = string.Empty;
    public short NotificationType { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public short Status { get; set; }
    public int Priority { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTimeOffset NextAttemptAt { get; set; }
    public string? LastError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
