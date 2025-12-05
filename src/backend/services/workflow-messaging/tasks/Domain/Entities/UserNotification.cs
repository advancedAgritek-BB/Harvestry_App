using System;
using Harvestry.Shared.Kernel.Domain;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Domain.Entities;

/// <summary>
/// In-app notification entity for users. Stores notifications that appear
/// in the notification bell/panel in the frontend.
/// </summary>
public sealed class UserNotification : AggregateRoot<Guid>
{
    private UserNotification(
        Guid id,
        Guid userId,
        Guid siteId,
        NotificationType notificationType,
        string title,
        string? message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        bool isRead,
        DateTimeOffset? readAt,
        DateTimeOffset createdAt) : base(id)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User identifier is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        UserId = userId;
        SiteId = siteId;
        NotificationType = notificationType;
        Title = title.Trim();
        Message = message?.Trim();
        RelatedEntityType = string.IsNullOrWhiteSpace(relatedEntityType) ? null : relatedEntityType.Trim();
        RelatedEntityId = relatedEntityId;
        IsRead = isRead;
        ReadAt = readAt;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; }
    public Guid SiteId { get; }
    public NotificationType NotificationType { get; }
    public string Title { get; }
    public string? Message { get; }
    public string? RelatedEntityType { get; }
    public Guid? RelatedEntityId { get; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public static UserNotification Create(
        Guid userId,
        Guid siteId,
        NotificationType notificationType,
        string title,
        string? message,
        string? relatedEntityType,
        Guid? relatedEntityId)
    {
        return new UserNotification(
            Guid.NewGuid(),
            userId,
            siteId,
            notificationType,
            title,
            message,
            relatedEntityType,
            relatedEntityId,
            isRead: false,
            readAt: null,
            DateTimeOffset.UtcNow);
    }

    public static UserNotification FromPersistence(
        Guid id,
        Guid userId,
        Guid siteId,
        NotificationType notificationType,
        string title,
        string? message,
        string? relatedEntityType,
        Guid? relatedEntityId,
        bool isRead,
        DateTimeOffset? readAt,
        DateTimeOffset createdAt)
    {
        return new UserNotification(
            id,
            userId,
            siteId,
            notificationType,
            title,
            message,
            relatedEntityType,
            relatedEntityId,
            isRead,
            readAt,
            createdAt);
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}

