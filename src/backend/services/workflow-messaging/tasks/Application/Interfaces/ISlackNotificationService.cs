using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackNotificationService
{
    Task<string> SendNotificationAsync(Guid siteId, NotificationType notificationType, object payload, int priority, CancellationToken cancellationToken, string? requestId = null);
    Task<IReadOnlyList<SlackChannelMappingResponse>> GetChannelMappingsAsync(Guid siteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SlackNotificationResponse>> GetFailedNotificationsAsync(Guid siteId, CancellationToken cancellationToken);
    Task RetryFailedNotificationAsync(Guid siteId, Guid notificationId, CancellationToken cancellationToken);
}
