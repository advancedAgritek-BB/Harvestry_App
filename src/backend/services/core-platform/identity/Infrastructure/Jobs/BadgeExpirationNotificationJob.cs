using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Infrastructure.Jobs;

public sealed class BadgeExpirationNotificationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BadgeExpirationNotificationJob> _logger;

    public BadgeExpirationNotificationJob(IServiceProvider serviceProvider, ILogger<BadgeExpirationNotificationJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Schedule for 08:00 UTC (change to LocalTime if local timezone is intended)
                var now = DateTimeOffset.UtcNow;
                var nextRun = now.Date.AddHours(8); // 08:00 UTC today
                if (nextRun <= now)
                {
                    nextRun = nextRun.AddDays(1); // If already past 08:00, schedule for tomorrow
                }
                var delay = nextRun - now;
                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.Zero;
                }

                await Task.Delay(delay, stoppingToken);
                await NotifyExpiringBadgesAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Badge expiration notification job failed");
            }
        }
    }

    private async Task NotifyExpiringBadgesAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await db.SetRlsContextAsync(Guid.Empty, "service_account", Guid.Empty, cancellationToken);
        await using var connection = await db.GetOpenConnectionAsync(cancellationToken);

        var notificationsDispatched = 0;

        try
        {
            const string sql = @"
            SELECT 
                b.badge_id,
                b.user_id,
                b.site_id,
                b.badge_code,
                b.expires_at,
                u.email            AS user_email,
                COALESCE(NULLIF(TRIM(u.first_name || ' ' || u.last_name), ''), u.first_name, u.last_name, u.email) AS user_name,
                manager.user_id    AS manager_user_id,
                manager.email      AS manager_email,
                manager.display_name AS manager_name
            FROM badges b
            JOIN users u ON u.user_id = b.user_id
            LEFT JOIN LATERAL (
                SELECT 
                    mu.user_id,
                    mu.email,
                    COALESCE(NULLIF(TRIM(mu.first_name || ' ' || mu.last_name), ''), mu.first_name, mu.last_name, mu.email) AS display_name
                FROM user_sites ms
                JOIN roles r ON r.role_id = ms.role_id
                JOIN users mu ON mu.user_id = ms.user_id
                WHERE ms.site_id = b.site_id
                  AND r.role_name IN ('manager', 'admin')
                ORDER BY CASE r.role_name WHEN 'manager' THEN 0 ELSE 1 END, mu.user_id
                LIMIT 1
            ) manager ON TRUE
            WHERE b.expires_at IS NOT NULL
              AND b.expires_at BETWEEN NOW() AND NOW() + INTERVAL '7 days';
        ";

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var badgeIdOrdinal = reader.GetOrdinal("badge_id");
            var userIdOrdinal = reader.GetOrdinal("user_id");
            var siteIdOrdinal = reader.GetOrdinal("site_id");
            var badgeCodeOrdinal = reader.GetOrdinal("badge_code");
            var expiresAtOrdinal = reader.GetOrdinal("expires_at");
            var userEmailOrdinal = reader.GetOrdinal("user_email");
            var userNameOrdinal = reader.GetOrdinal("user_name");
            var managerUserIdOrdinal = reader.GetOrdinal("manager_user_id");
            var managerEmailOrdinal = reader.GetOrdinal("manager_email");
            var managerNameOrdinal = reader.GetOrdinal("manager_name");

            while (await reader.ReadAsync(cancellationToken))
            {
                var badgeId = reader.GetGuid(badgeIdOrdinal);
                var userId = reader.GetGuid(userIdOrdinal);
                var siteId = reader.GetGuid(siteIdOrdinal);
                var badgeCode = reader.GetString(badgeCodeOrdinal);
                var expiresAt = reader.GetDateTime(expiresAtOrdinal);
                var userEmail = reader.GetString(userEmailOrdinal);
                var userName = reader.IsDBNull(userNameOrdinal) ? userEmail : reader.GetString(userNameOrdinal);

                var ownerNotification = new BadgeExpirationNotification(
                    badgeId,
                    siteId,
                    userId,
                    userEmail,
                    userName,
                    badgeCode,
                    expiresAt,
                    IsManagerNotification: false);

                try
                {
                    await notificationService.NotifyBadgeExpirationAsync(ownerNotification, cancellationToken).ConfigureAwait(false);
                    notificationsDispatched++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send badge expiration notification for badge {BadgeId}, user {UserId}, email {Email}",
                        badgeId, userId, userEmail);
                }

                if (!reader.IsDBNull(managerEmailOrdinal) && !reader.IsDBNull(managerUserIdOrdinal))
                {
                    var managerUserId = reader.GetGuid(managerUserIdOrdinal);
                    if (managerUserId != userId)
                    {
                        var managerEmail = reader.GetString(managerEmailOrdinal);
                        var managerName = reader.IsDBNull(managerNameOrdinal) ? managerEmail : reader.GetString(managerNameOrdinal);

                        var managerNotification = ownerNotification with
                        {
                            TargetUserId = managerUserId,
                            RecipientEmail = managerEmail,
                            RecipientName = managerName,
                            IsManagerNotification = true
                        };

                        try
                        {
                            await notificationService.NotifyBadgeExpirationAsync(managerNotification, cancellationToken).ConfigureAwait(false);
                            notificationsDispatched++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send manager badge expiration notification for badge {BadgeId}, manager {ManagerUserId}, email {ManagerEmail}",
                                badgeId, managerUserId, managerEmail);
                        }
                    }
                }
            }
        }
        finally
        {
            await db.ResetRlsContextAsync(cancellationToken);
        }

        _logger.LogInformation("Badge expiration notification job dispatched {Count} notifications", notificationsDispatched);
    }
}
