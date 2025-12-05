using System;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class SlackChannelMappingRecord
{
    public Guid SlackChannelMappingId { get; set; }
    public Guid SiteId { get; set; }
    public Guid SlackWorkspaceId { get; set; }
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public SlackWorkspaceRecord SlackWorkspace { get; set; } = null!;
}
