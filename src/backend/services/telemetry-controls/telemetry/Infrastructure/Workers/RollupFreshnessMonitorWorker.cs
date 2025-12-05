using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Monitors TimescaleDB continuous aggregate freshness and logs warnings if lag exceeds the threshold.
/// </summary>
public sealed class RollupFreshnessMonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RollupFreshnessMonitorWorker> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(1);
    private readonly TimeSpan _stalenessThreshold = TimeSpan.FromMinutes(2);

    public RollupFreshnessMonitorWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<RollupFreshnessMonitorWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Rollup freshness monitor starting with poll interval {Interval}", _pollInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var connectionFactory = scope.ServiceProvider.GetRequiredService<ITelemetryConnectionFactory>();

                var staleAggregates = await GetStaleAggregatesAsync(connectionFactory, stoppingToken).ConfigureAwait(false);
                foreach (var aggregate in staleAggregates)
                {
                    _logger.LogWarning(
                        "Continuous aggregate {ViewName} is stale by {Lag} (threshold {Threshold})",
                        aggregate.ViewName,
                        aggregate.Lag,
                        _stalenessThreshold);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rollup freshness monitor encountered an unexpected error");
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

        _logger.LogInformation("Rollup freshness monitor stopping");
    }

    private async Task<List<(string ViewName, TimeSpan Lag)>> GetStaleAggregatesAsync(
        ITelemetryConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        const string query = @"
            SELECT ca.view_schema || '.' || ca.view_name AS view_name,
                   CASE WHEN j.last_successful_finish IS NOT NULL
                        THEN now() - j.last_successful_finish
                        ELSE NULL END AS lag
            FROM timescaledb_information.continuous_aggregates ca
            LEFT JOIN timescaledb_information.jobs j
              ON ca.job_id = j.job_id";

        var stale = new List<(string, TimeSpan)>();

        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (reader.IsDBNull(1))
            {
                continue;
            }

            var lag = reader.GetTimeSpan(1);
            if (lag > _stalenessThreshold)
            {
                stale.Add((reader.GetString(0), lag));
            }
        }

        return stale;
    }
}
