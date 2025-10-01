using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Infrastructure.Jobs;

public sealed class SessionCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupJob> _logger;

    public SessionCleanupJob(IServiceProvider serviceProvider, ILogger<SessionCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run cleanup immediately on startup
        try
        {
            await CleanupAsync(stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Shutdown during initial cleanup
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial session cleanup failed");
        }

        // Then run on hourly schedule
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                await CleanupAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session cleanup failed");
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.SetRlsContextAsync(Guid.Empty, "service_account", Guid.Empty, cancellationToken);
        await using var connection = await db.GetOpenConnectionAsync(cancellationToken);

        var deleted = 0;
        try
        {
            const string sql = @"
                DELETE FROM sessions
                WHERE expires_at < NOW() - INTERVAL '7 days';
            ";

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            deleted = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            await db.ResetRlsContextAsync(cancellationToken);
        }

        _logger.LogInformation("Session cleanup removed {Count} rows", deleted);
    }
}
