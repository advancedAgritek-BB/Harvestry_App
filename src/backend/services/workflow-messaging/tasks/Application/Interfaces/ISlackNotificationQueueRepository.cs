using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SlackNotificationEntity = Harvestry.Tasks.Domain.Entities.SlackNotification;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackNotificationQueueRepository
{
    Task EnqueueAsync(SlackNotificationEntity notification, CancellationToken cancellationToken);
    Task UpdateAsync(SlackNotificationEntity notification, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackNotificationEntity>> GetPendingNotificationsAsync(int batchSize, DateTimeOffset referenceTime, CancellationToken cancellationToken);
    Task<SlackNotificationEntity?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackNotificationEntity>> GetFailedNotificationsAsync(Guid siteId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string requestId, Guid slackWorkspaceId, string channelId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
