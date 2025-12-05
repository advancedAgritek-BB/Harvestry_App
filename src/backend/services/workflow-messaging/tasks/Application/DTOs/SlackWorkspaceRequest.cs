using System;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class SlackWorkspaceRequest
{
    /// <summary>
    /// Slack-provided workspace identifier (e.g. T12345).
    /// </summary>
    public string WorkspaceId { get; init; } = string.Empty;

    /// <summary>
    /// Friendly workspace name displayed inside Slack.
    /// </summary>
    public string WorkspaceName { get; init; } = string.Empty;

    /// <summary>
    /// Optional encrypted bot token payload; if omitted the existing secret is retained.
    /// </summary>
    public string? BotToken { get; init; }

    /// <summary>
    /// Optional encrypted refresh token payload; if omitted the existing secret is retained.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Indicates whether the workspace should be active after the update.
    /// </summary>
    public bool IsActive { get; init; } = true;
}

public sealed class SlackWorkspaceResponse
{
    public Guid SlackWorkspaceId { get; init; }
    public Guid SiteId { get; init; }
    public string WorkspaceId { get; init; } = string.Empty;
    public string WorkspaceName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset InstalledAt { get; init; }
    public Guid InstalledBy { get; init; }
    public DateTimeOffset? LastVerifiedAt { get; init; }
}
