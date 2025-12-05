using System;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Mappers;

public static class NotificationMapper
{
    public static UserNotificationResponse ToResponse(UserNotification notification)
    {
        if (notification is null)
            throw new ArgumentNullException(nameof(notification));

        return new UserNotificationResponse
        {
            Id = notification.Id,
            SiteId = notification.SiteId,
            NotificationType = notification.NotificationType,
            Title = notification.Title,
            Message = notification.Message,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }
}

