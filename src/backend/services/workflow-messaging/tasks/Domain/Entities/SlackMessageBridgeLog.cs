using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class SlackMessageBridgeLog : Entity<Guid>
{
    private SlackMessageBridgeLog(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        Guid? internalMessageId,
        string internalMessageType,
        string slackChannelId,
        string requestId,
        SlackMessageBridgeStatus status,
        int attemptCount,
        DateTimeOffset createdAt,
        DateTimeOffset? lastAttemptAt,
        string? slackMessageTs,
        string? slackThreadTs,
        string? errorMessage,
        DateTimeOffset? sentAt) : base(id)
    {
        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (slackWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Slack workspace identifier is required.", nameof(slackWorkspaceId));
        }

        if (string.IsNullOrWhiteSpace(internalMessageType))
        {
            throw new ArgumentException("Internal message type is required.", nameof(internalMessageType));
        }

        if (string.IsNullOrWhiteSpace(slackChannelId))
        {
            throw new ArgumentException("Slack channel identifier is required.", nameof(slackChannelId));
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request identifier is required.", nameof(requestId));
        }

        SiteId = siteId;
        SlackWorkspaceId = slackWorkspaceId;
        InternalMessageId = internalMessageId;
        InternalMessageType = internalMessageType.Trim();
        SlackChannelId = slackChannelId.Trim();
        RequestId = requestId.Trim();
        Status = status;
        AttemptCount = attemptCount;
        CreatedAt = createdAt;
        LastAttemptAt = lastAttemptAt;
        SlackMessageTs = slackMessageTs;
        SlackThreadTs = slackThreadTs;
        ErrorMessage = errorMessage;
        SentAt = sentAt;
    }

    public Guid SiteId { get; }
    public Guid SlackWorkspaceId { get; }
    public Guid? InternalMessageId { get; }
    public string InternalMessageType { get; }
    public string SlackChannelId { get; private set; }
    public string? SlackMessageTs { get; private set; }
    public string? SlackThreadTs { get; private set; }
    public string RequestId { get; }
    public SlackMessageBridgeStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    public static SlackMessageBridgeLog Create(
        Guid siteId,
        Guid slackWorkspaceId,
        Guid? internalMessageId,
        string internalMessageType,
        string slackChannelId,
        string requestId)
    {
        var timestamp = DateTimeOffset.UtcNow;
        return new SlackMessageBridgeLog(
            Guid.NewGuid(),
            siteId,
            slackWorkspaceId,
            internalMessageId,
            internalMessageType,
            slackChannelId,
            requestId,
            SlackMessageBridgeStatus.Pending,
            attemptCount: 0,
            createdAt: timestamp,
            lastAttemptAt: null,
            slackMessageTs: null,
            slackThreadTs: null,
            errorMessage: null,
            sentAt: null);
    }

    public static SlackMessageBridgeLog FromPersistence(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        Guid? internalMessageId,
        string internalMessageType,
        string slackChannelId,
        string? slackMessageTs,
        string? slackThreadTs,
        string requestId,
        SlackMessageBridgeStatus status,
        int attemptCount,
        DateTimeOffset createdAt,
        DateTimeOffset? lastAttemptAt,
        string? errorMessage,
        DateTimeOffset? sentAt)
    {
        return new SlackMessageBridgeLog(
            id,
            siteId,
            slackWorkspaceId,
            internalMessageId,
            internalMessageType,
            slackChannelId,
            requestId,
            status,
            attemptCount,
            createdAt,
            lastAttemptAt,
            slackMessageTs,
            slackThreadTs,
            errorMessage,
            sentAt);
    }

    public void RecordAttempt(DateTimeOffset attemptAt, SlackMessageBridgeStatus status, string slackChannelId, string? slackMessageTs, string? slackThreadTs, string? errorMessage)
    {
        if (attemptAt == default)
        {
            throw new ArgumentException("Attempt timestamp is required.", nameof(attemptAt));
        }

        AttemptCount++;
        LastAttemptAt = attemptAt;
        Status = status;
        SlackChannelId = string.IsNullOrWhiteSpace(slackChannelId) ? SlackChannelId : slackChannelId.Trim();
        SlackMessageTs = slackMessageTs;
        SlackThreadTs = slackThreadTs;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage;
    }

    public void MarkSent(DateTimeOffset sentAt, string slackChannelId, string slackMessageTs, string? slackThreadTs)
    {
        SentAt = sentAt;
        Status = SlackMessageBridgeStatus.Sent;
        SlackChannelId = string.IsNullOrWhiteSpace(slackChannelId) ? SlackChannelId : slackChannelId.Trim();
        SlackMessageTs = slackMessageTs;
        SlackThreadTs = slackThreadTs;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = SlackMessageBridgeStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Unknown Slack error" : errorMessage.Trim();
    }
}
