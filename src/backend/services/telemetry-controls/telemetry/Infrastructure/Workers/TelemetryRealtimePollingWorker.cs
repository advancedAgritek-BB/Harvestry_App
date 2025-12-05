using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Polls sensor readings for new ingestion events and dispatches them to real-time clients.
/// Serves as a fallback when logical replication is unavailable.
/// </summary>
public sealed class TelemetryRealtimePollingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryRealtimePollingWorker> _logger;
    private readonly TimeSpan _pollInterval;
    private DateTimeOffset _lastIngestionTimestamp;

    public TelemetryRealtimePollingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<TelemetryRealtimePollingWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pollInterval = TimeSpan.FromSeconds(2);
        _lastIngestionTimestamp = DateTimeOffset.UtcNow.AddSeconds(-5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry real-time polling worker started with {Interval} interval", _pollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var queryRepository = scope.ServiceProvider.GetRequiredService<ITelemetryQueryRepository>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<ITelemetryRealtimeDispatcher>();
                var rlsAccessor = scope.ServiceProvider.GetRequiredService<ITelemetryRlsContextAccessor>();

                var readings = await queryRepository
                    .GetReadingsSinceAsync(_lastIngestionTimestamp, 500, stoppingToken)
                    .ConfigureAwait(false);

                if (readings.Count > 0)
                {
                    var maxTimestamp = readings.Max(r => r.Reading.IngestionTimestamp);
                    foreach (var siteGroup in readings.GroupBy(r => r.SiteId))
                    {
                        var contextAdjusted = EnsureSiteContext(rlsAccessor, siteGroup.Key, out var originalContext);
                        try
                        {
                            await dispatcher.PublishAsync(siteGroup.Select(tuple => tuple.Reading), stoppingToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            RestoreSiteContext(rlsAccessor, originalContext, contextAdjusted);
                        }
                    }

                    _lastIngestionTimestamp = maxTimestamp;
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry real-time polling worker encountered an error");
            }

            try
            {
                await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation during delay
            }
        }

        _logger.LogInformation("Telemetry real-time polling worker stopping");
    }

    private static bool EnsureSiteContext(
        ITelemetryRlsContextAccessor accessor,
        Guid siteId,
        out TelemetryRlsContext originalContext)
    {
        originalContext = accessor.Current;
        if (originalContext.SiteId == siteId)
        {
            return false;
        }

        accessor.Set(originalContext with { SiteId = siteId });
        return true;
    }

    private static void RestoreSiteContext(
        ITelemetryRlsContextAccessor accessor,
        TelemetryRlsContext originalContext,
        bool contextAdjusted)
    {
        if (contextAdjusted)
        {
            accessor.Set(originalContext);
        }
    }
}
