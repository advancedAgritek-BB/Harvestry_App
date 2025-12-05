using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class SlackNotification : Entity<Guid>
{
    private SlackNotification(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        NotificationType notificationType,
        string payloadJson,
        string requestId,
        NotificationStatus status,
        int priority,
        int attemptCount,
        int maxAttempts,
        DateTimeOffset nextAttemptAt,
        string? lastError,
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

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ArgumentException("Payload is required.", nameof(payloadJson));
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request identifier is required.", nameof(requestId));
        }

        if (attemptCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptCount), "Attempt count cannot be negative.");
        }

        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Maximum attempts must be positive.");
        }

        if (attemptCount > maxAttempts)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptCount), "Attempt count cannot exceed maximum attempts.");
        }

        SiteId = siteId;
        SlackWorkspaceId = slackWorkspaceId;
        ChannelId = channelId.Trim();
        NotificationType = notificationType;
        PayloadJson = payloadJson;
        RequestId = requestId.Trim();
        Status = status;
        Priority = priority;
        AttemptCount = attemptCount;
        MaxAttempts = maxAttempts;
        NextAttemptAt = nextAttemptAt;
        LastError = lastError;
        CreatedAt = createdAt;
    }

    public Guid SiteId { get; }
    public Guid SlackWorkspaceId { get; }
    public string ChannelId { get; }
    public NotificationType NotificationType { get; }
    public string PayloadJson { get; }
    public string RequestId { get; }
    public NotificationStatus Status { get; private set; }
    public int Priority { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static SlackNotification Create(
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        NotificationType notificationType,
        string payloadJson,
        string requestId,
        int priority,
        int maxAttempts)
    {
        var createdAt = DateTimeOffset.UtcNow;

        // Default undefined notification types to TaskCreated for backward compatibility.
        // This fallback ensures notifications are processed even when callers don't specify a type.
        // TaskCreated is used as it's the most common notification type.
        var resolvedNotificationType = notificationType == NotificationType.Undefined ? NotificationType.TaskCreated : notificationType;

        return new SlackNotification(
            Guid.NewGuid(),
            siteId,
            slackWorkspaceId,
            channelId,
            resolvedNotificationType,
            payloadJson,
            requestId,
            NotificationStatus.Pending,
            priority,
            attemptCount: 0,
            maxAttempts: maxAttempts,
            nextAttemptAt: createdAt,
            lastError: null,
            createdAt: createdAt);
    }

    public static SlackNotification FromPersistence(
        Guid id,
        Guid siteId,
        Guid slackWorkspaceId,
        string channelId,
        NotificationType notificationType,
        string payloadJson,
        string requestId,
        NotificationStatus status,
        int priority,
        int attemptCount,
        int maxAttempts,
        DateTimeOffset nextAttemptAt,
        string? lastError,
        DateTimeOffset createdAt)
    {
        return new SlackNotification(
            id,
            siteId,
            slackWorkspaceId,
            channelId,
            notificationType,
            payloadJson,
            requestId,
            status,
            priority,
            attemptCount,
            maxAttempts,
            nextAttemptAt,
            lastError,
            createdAt);
    }

    public void MarkProcessing()
    {
        Status = NotificationStatus.Processing;
    }

    public void MarkSent()
    {
        Status = NotificationStatus.Sent;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Status = NotificationStatus.Failed;
        LastError = string.IsNullOrWhiteSpace(error) ? "Unknown error" : error;
    }

    public void MarkDeadLetter(string? error = null)
    {
        Status = NotificationStatus.DeadLetter;
        LastError = string.IsNullOrWhiteSpace(error) ? LastError : error;
    }

    public void IncrementAttempt()
    {
        AttemptCount++;
    }

    public bool CanRetry() => AttemptCount < MaxAttempts && Status is NotificationStatus.Pending or NotificationStatus.Failed or NotificationStatus.Processing;

    public void ScheduleRetry(TimeSpan backoff)
    {
        if (backoff <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(backoff), "Backoff duration must be positive.");
        }

        if (Status != NotificationStatus.Failed)
        {
            throw new InvalidOperationException($"Cannot schedule retry for notification in status {Status}. Only Failed notifications can be retried.");
        }

        NextAttemptAt = DateTimeOffset.UtcNow.Add(backoff);
        Status = NotificationStatus.Pending;
    }

    public void UpdatePriority(int priority)
    {
        Priority = priority;
    }

    public void UpdateMaxAttempts(int maxAttempts)
    {
        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be greater than zero.");
        }

        MaxAttempts = maxAttempts;
    }
}
