using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class SlackWorkspaceRecord
{
    public Guid SlackWorkspaceId { get; set; }
    public Guid SiteId { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string? BotTokenEncrypted { get; set; }
    public string? RefreshTokenEncrypted { get; set; }
    public bool IsActive { get; set; }
    public Guid InstalledByUserId { get; set; }
    public DateTimeOffset InstalledAt { get; set; }
    public DateTimeOffset? LastVerifiedAt { get; set; }

    public ICollection<SlackChannelMappingRecord> ChannelMappings { get; set; } = new List<SlackChannelMappingRecord>();
}
