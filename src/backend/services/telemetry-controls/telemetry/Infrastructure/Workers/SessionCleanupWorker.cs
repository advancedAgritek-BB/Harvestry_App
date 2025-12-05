using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Periodically ends ingestion sessions that have not sent a heartbeat within the stale threshold.
/// </summary>
public sealed class SessionCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupWorker> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _staleThreshold;

    public SessionCleanupWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pollInterval = TimeSpan.FromMinutes(1);
        _staleThreshold = TimeSpan.FromMinutes(5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup worker starting with interval {Interval} and stale threshold {Threshold}", _pollInterval, _staleThreshold);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IIngestionSessionRepository>();
                var ended = await repository.EndStaleSessionsAsync(_staleThreshold, DateTimeOffset.UtcNow, stoppingToken).ConfigureAwait(false);

                if (ended > 0)
                {
                    _logger.LogInformation("Ended {Count} stale ingestion sessions", ended);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session cleanup worker encountered an error");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Session cleanup worker stopping");
    }
}
