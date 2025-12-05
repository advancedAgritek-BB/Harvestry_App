using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Telemetry.Infrastructure.Persistence;

/// <summary>
/// Ensures EF Core connections adopt the telemetry RLS session context when opened.
/// </summary>
public sealed class TelemetryConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITelemetryConnectionFactory _connectionFactory;
    private readonly ILogger<TelemetryConnectionInterceptor> _logger;

    public TelemetryConnectionInterceptor(
        ITelemetryConnectionFactory connectionFactory,
        ILogger<TelemetryConnectionInterceptor> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await ConfigureAsync(connection, cancellationToken).ConfigureAwait(false);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken).ConfigureAwait(false);
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ConfigureAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    private async Task ConfigureAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is not NpgsqlConnection npgsqlConnection)
        {
            return;
        }

        try
        {
            await _connectionFactory.ConfigureConnectionAsync(npgsqlConnection, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure telemetry RLS context for EF connection");
            throw;
        }
    }
}
