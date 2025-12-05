using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Interfaces;

public interface IUserNotificationRepository
{
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken);
    Task<UserNotification?> GetByIdAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserNotification>> GetByUserAsync(Guid userId, bool? unreadOnly, int limit, CancellationToken cancellationToken);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
    Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

