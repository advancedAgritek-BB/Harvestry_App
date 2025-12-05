using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<UserNotificationRepository> _logger;

    public UserNotificationRepository(TasksDbContext dbContext, ILogger<UserNotificationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        if (notification is null)
            throw new ArgumentNullException(nameof(notification));

        _logger.LogDebug("Adding notification {NotificationId} for user {UserId}", notification.Id, notification.UserId);
        var record = ToRecord(notification);
        await _dbContext.UserNotifications.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserNotification?> GetByIdAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.UserNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<UserNotification>> GetByUserAsync(
        Guid userId,
        bool? unreadOnly,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.UserNotifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (unreadOnly == true)
        {
            query = query.Where(x => !x.IsRead);
        }

        var records = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserNotifications
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        if (notification is null)
            throw new ArgumentNullException(nameof(notification));

        var record = await _dbContext.UserNotifications
            .FirstOrDefaultAsync(x => x.NotificationId == notification.Id && x.UserId == notification.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            throw new InvalidOperationException($"Notification {notification.Id} could not be found for update.");

        record.IsRead = notification.IsRead;
        record.ReadAt = notification.ReadAt;
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, now), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UserNotification ToDomain(UserNotificationRecord record)
    {
        return UserNotification.FromPersistence(
            id: record.NotificationId,
            userId: record.UserId,
            siteId: record.SiteId,
            notificationType: (NotificationType)record.NotificationType,
            title: record.Title,
            message: record.Message,
            relatedEntityType: record.RelatedEntityType,
            relatedEntityId: record.RelatedEntityId,
            isRead: record.IsRead,
            readAt: record.ReadAt,
            createdAt: record.CreatedAt);
    }

    private static UserNotificationRecord ToRecord(UserNotification notification)
    {
        return new UserNotificationRecord
        {
            NotificationId = notification.Id,
            UserId = notification.UserId,
            SiteId = notification.SiteId,
            NotificationType = (int)notification.NotificationType,
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

