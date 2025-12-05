using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class SlackChannelMapping : Entity<Guid>
{
    private SlackChannelMapping(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        string channelName,
        string notificationType,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt) : base(id)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (slackWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Slack workspace identifier is required.", nameof(slackWorkspaceId));
        }

        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel identifier is required.", nameof(channelId));
        }

        if (string.IsNullOrWhiteSpace(notificationType))
        {
            throw new ArgumentException("Notification type is required.", nameof(notificationType));
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException("CreatedBy user identifier is required.", nameof(createdByUserId));
        }

        SiteId = siteId;
        SlackWorkspaceId = slackWorkspaceId;
        ChannelId = channelId.Trim();
        ChannelName = string.IsNullOrWhiteSpace(channelName) ? ChannelId : channelName.Trim();
        NotificationType = NormalizeNotificationType(notificationType);
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    public Guid SiteId { get; }
    public Guid SlackWorkspaceId { get; }
    public string ChannelId { get; private set; }
    public string ChannelName { get; private set; }
    public string NotificationType { get; private set; }
    public bool IsActive { get; private set; }
    public Guid CreatedByUserId { get; }
    public DateTimeOffset CreatedAt { get; }

    public static SlackChannelMapping Create(
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        string channelName,
        string notificationType,
        Guid createdByUserId,
        DateTimeOffset? createdAt = null)
    {
        return new SlackChannelMapping(
            Guid.NewGuid(),
            siteId,
            slackWorkspaceId,
            channelId,
            channelName,
            notificationType,
            isActive: true,
            createdByUserId,
            createdAt ?? DateTimeOffset.UtcNow);
    }

    public static SlackChannelMapping FromPersistence(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        string channelName,
        string notificationType,
        bool isActive,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new SlackChannelMapping(
            id,
            siteId,
            slackWorkspaceId,
            channelId,
            channelName,
            notificationType,
            isActive,
            createdByUserId,
            createdAt);
    }

    public void Update(string channelName, bool isActive)
    {
        ChannelName = string.IsNullOrWhiteSpace(channelName) ? ChannelName : channelName.Trim();
        IsActive = isActive;
    }

    private static string NormalizeNotificationType(string notificationType)
    {
        if (string.IsNullOrWhiteSpace(notificationType))
        {
            throw new ArgumentException("Notification type is required.", nameof(notificationType));
        }

        return notificationType.Trim().ToLowerInvariant();
    }
}
