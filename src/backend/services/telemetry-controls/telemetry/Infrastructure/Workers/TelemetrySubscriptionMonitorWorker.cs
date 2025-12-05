using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Periodically logs subscription health and prunes stale SignalR connections.
/// </summary>
public sealed class TelemetrySubscriptionMonitorWorker : BackgroundService
{
    private readonly ITelemetrySubscriptionRegistry _registry;
    private readonly TelemetrySubscriptionMonitorOptions _options;
    private readonly ILogger<TelemetrySubscriptionMonitorWorker> _logger;

    public TelemetrySubscriptionMonitorWorker(
        ITelemetrySubscriptionRegistry registry,
        IOptions<TelemetrySubscriptionMonitorOptions> options,
        ILogger<TelemetrySubscriptionMonitorWorker> logger)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Telemetry subscription monitor disabled via configuration.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Clamp(_options.MonitorIntervalSeconds, 5, 300));
        var staleThreshold = TimeSpan.FromSeconds(Math.Clamp(_options.StaleConnectionSeconds, 30, 3600));
        var topN = Math.Clamp(_options.TopStreamsToLog, 1, 50);

        _logger.LogInformation(
            "Telemetry subscription monitor started (interval={IntervalSeconds}s, pruneAfter={PruneSeconds}s).",
            interval.TotalSeconds,
            staleThreshold.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var snapshot = _registry.GetSnapshot();
                var pruned = _registry.PruneStaleConnections(staleThreshold);

                if (pruned > 0)
                {
                    _logger.LogWarning("Pruned {Pruned} stale telemetry connections (>{ThresholdSeconds}s idle).", pruned, staleThreshold.TotalSeconds);
                }

                _logger.LogInformation(
                    "Telemetry subscriptions: Connections={Connections}, ActiveStreams={Streams}, TotalSubscriptions={Subscriptions}",
                    snapshot.TotalConnections,
                    snapshot.ActiveStreamCount,
                    snapshot.TotalSubscriptions);

                foreach (var entry in snapshot.StreamCounts
                             .OrderByDescending(kvp => kvp.Value)
                             .ThenBy(kvp => kvp.Key)
                             .Take(topN))
                {
                    _logger.LogDebug("Stream {StreamId} => {SubscriberCount} subscribers", entry.Key, entry.Value);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telemetry subscription monitor encountered an error.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Telemetry subscription monitor stopping.");
    }
}
