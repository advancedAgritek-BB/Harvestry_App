using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlackNotificationEntity = Harvestry.Tasks.Domain.Entities.SlackNotification;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class SlackNotificationQueueRepository : ISlackNotificationQueueRepository
{
    private readonly TasksDbContext _dbContext;
    private readonly ILogger<SlackNotificationQueueRepository> _logger;

    public SlackNotificationQueueRepository(TasksDbContext dbContext, ILogger<SlackNotificationQueueRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnqueueAsync(SlackNotificationEntity notification, CancellationToken cancellationToken)
    {
        var record = ToRecord(notification);
        await _dbContext.SlackNotifications.AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(SlackNotificationEntity notification, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackNotifications
            .FirstOrDefaultAsync(x => x.SlackNotificationId == notification.Id, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            throw new InvalidOperationException($"Slack notification {notification.Id} could not be found for update.");
        }

        ApplyScalarProperties(record, notification);
    }

    public async Task<IReadOnlyList<SlackNotificationEntity>> GetPendingNotificationsAsync(int batchSize, DateTimeOffset referenceTime, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SlackNotifications
            .AsNoTracking()
            .Where(x => x.Status == (short)NotificationStatus.Pending && x.NextAttemptAt <= referenceTime)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.NextAttemptAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<SlackNotificationEntity?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.SlackNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SlackNotificationId == notificationId, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<bool> ExistsAsync(string requestId, Guid slackWorkspaceId, string channelId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request identifier is required.", nameof(requestId));
        }

        if (slackWorkspaceId == Guid.Empty)
        {
            throw new ArgumentException("Slack workspace identifier is required.", nameof(slackWorkspaceId));
        }

        if (string.IsNullOrWhiteSpace(channelId))
        {
            throw new ArgumentException("Channel identifier is required.", nameof(channelId));
        }

        return await _dbContext.SlackNotifications
            .AsNoTracking()
            .AnyAsync(
                x => x.RequestId == requestId
                    && x.SlackWorkspaceId == slackWorkspaceId
                    && x.ChannelId == channelId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SlackNotificationEntity>> GetFailedNotificationsAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.SlackNotifications
            .AsNoTracking()
            .Where(x => x.SiteId == siteId && x.Status == (short)NotificationStatus.Failed)
            .OrderByDescending(x => x.NextAttemptAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SlackNotificationEntity ToDomain(SlackNotificationRecord record)
    {
        return SlackNotificationEntity.FromPersistence(
            record.SlackNotificationId,
            record.SiteId,
            record.SlackWorkspaceId,
            record.ChannelId,
            (NotificationType)record.NotificationType,
            record.PayloadJson,
            record.RequestId,
            (NotificationStatus)record.Status,
            record.Priority,
            record.AttemptCount,
            record.MaxAttempts,
            record.NextAttemptAt,
            record.LastError,
            record.CreatedAt);
    }

    private static SlackNotificationRecord ToRecord(SlackNotificationEntity notification)
    {
        return new SlackNotificationRecord
        {
            SlackNotificationId = notification.Id,
            SiteId = notification.SiteId,
            SlackWorkspaceId = notification.SlackWorkspaceId,
            ChannelId = notification.ChannelId,
            NotificationType = (short)notification.NotificationType,
            PayloadJson = notification.PayloadJson,
            RequestId = notification.RequestId,
            Status = (short)notification.Status,
            Priority = notification.Priority,
            AttemptCount = notification.AttemptCount,
            MaxAttempts = notification.MaxAttempts,
            NextAttemptAt = notification.NextAttemptAt,
            LastError = notification.LastError,
            CreatedAt = notification.CreatedAt
        };
    }

    private static void ApplyScalarProperties(SlackNotificationRecord record, SlackNotificationEntity notification)
    {
        record.SiteId = notification.SiteId;
        record.SlackWorkspaceId = notification.SlackWorkspaceId;
        record.ChannelId = notification.ChannelId;
        record.NotificationType = (short)notification.NotificationType;
        record.PayloadJson = notification.PayloadJson;
        record.RequestId = notification.RequestId;
        record.Status = (short)notification.Status;
        record.Priority = notification.Priority;
        record.AttemptCount = notification.AttemptCount;
        record.MaxAttempts = notification.MaxAttempts;
        record.NextAttemptAt = notification.NextAttemptAt;
        record.LastError = notification.LastError;
    }
}
