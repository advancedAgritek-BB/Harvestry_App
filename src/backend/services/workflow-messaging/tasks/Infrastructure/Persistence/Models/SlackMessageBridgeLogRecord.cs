using System;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class SlackMessageBridgeLogRecord
{
    public Guid SlackMessageBridgeLogId { get; set; }
    public Guid SiteId { get; set; }
    public Guid SlackWorkspaceId { get; set; }
    public Guid? InternalMessageId { get; set; }
    public string InternalMessageType { get; set; } = string.Empty;
    public string SlackChannelId { get; set; } = string.Empty;
    public string? SlackMessageTs { get; set; }
    public string? SlackThreadTs { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public SlackMessageBridgeStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}
