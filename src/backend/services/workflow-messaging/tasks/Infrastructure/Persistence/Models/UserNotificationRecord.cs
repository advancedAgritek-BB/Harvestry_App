using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class UserNotificationRecord
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
    public Guid SiteId { get; set; }
    public int NotificationType { get; set; }
    public string Title { get; set; } = null!;
    public string? Message { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

