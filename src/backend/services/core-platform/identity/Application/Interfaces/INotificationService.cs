using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;

namespace Harvestry.Identity.Application.Interfaces;

public interface INotificationService
{
    Task NotifyBadgeExpirationAsync(BadgeExpirationNotification notification, CancellationToken cancellationToken = default);
}
