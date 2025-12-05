using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Tasks.Domain.Entities;

public sealed class MessageReadReceipt : Entity<Guid>
{
    private MessageReadReceipt(
        Guid id,
        Guid messageId,
        Guid siteId,
        Guid userId,
        DateTimeOffset readAt) : base(id)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("Message identifier is required.", nameof(messageId));
        }

        if (siteId == Guid.Empty)
        {
            throw new ArgumentException("Site identifier is required.", nameof(siteId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        MessageId = messageId;
        SiteId = siteId;
        UserId = userId;
        ReadAt = readAt;
    }

    public Guid MessageId { get; }
    public Guid SiteId { get; }
    public Guid UserId { get; }
    public DateTimeOffset ReadAt { get; private set; }

    public static MessageReadReceipt Create(
        Guid messageId,
        Guid siteId,
        Guid userId,
        DateTimeOffset? readAt = null)
    {
        return new MessageReadReceipt(
            Guid.NewGuid(),
            messageId,
            siteId,
            userId,
            readAt ?? DateTimeOffset.UtcNow);
    }

    public static MessageReadReceipt FromPersistence(
        Guid id,
        Guid messageId,
        Guid siteId,
        Guid userId,
        DateTimeOffset readAt)
    {
        return new MessageReadReceipt(id, messageId, siteId, userId, readAt);
    }

    public void UpdateReadAt(DateTimeOffset readAt)
    {
        ReadAt = readAt;
    }
}
