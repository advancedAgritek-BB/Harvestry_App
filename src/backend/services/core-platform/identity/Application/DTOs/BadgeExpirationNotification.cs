using System;

namespace Harvestry.Identity.Application.DTOs;

public sealed record BadgeExpirationNotification
{
    public BadgeExpirationNotification(
        Guid badgeId,
        Guid siteId,
        Guid targetUserId,
        string recipientEmail,
        string recipientName,
        string badgeCode,
        DateTime expiresAt,
        bool isManagerNotification)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("RecipientEmail cannot be null or whitespace", nameof(recipientEmail));
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new ArgumentException("RecipientName cannot be null or whitespace", nameof(recipientName));
        if (string.IsNullOrWhiteSpace(badgeCode))
            throw new ArgumentException("BadgeCode cannot be null or whitespace", nameof(badgeCode));

        BadgeId = badgeId;
        SiteId = siteId;
        TargetUserId = targetUserId;
        RecipientEmail = recipientEmail;
        RecipientName = recipientName;
        BadgeCode = badgeCode;
        ExpiresAt = expiresAt;
        IsManagerNotification = isManagerNotification;
    }

    public Guid BadgeId { get; init; }
    public Guid SiteId { get; init; }
    public Guid TargetUserId { get; init; }
    public string RecipientEmail { get; init; }
    public string RecipientName { get; init; }
    public string BadgeCode { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsManagerNotification { get; init; }
}
