using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Infrastructure.External;

public sealed class LoggingNotificationService : INotificationService
{
    private readonly ILogger<LoggingNotificationService> _logger;

    public LoggingNotificationService(ILogger<LoggingNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task NotifyBadgeExpirationAsync(BadgeExpirationNotification notification, CancellationToken cancellationToken = default)
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        _logger.LogInformation(
            "Badge {BadgeId} for site {SiteId} expiring at {ExpiresAt}. Notifying {RecipientEmail} ({RecipientType})",
            notification.BadgeId,
            notification.SiteId,
            notification.ExpiresAt,
            notification.RecipientEmail,
            notification.IsManagerNotification ? "manager" : "owner");

        return Task.CompletedTask;
    }
}
