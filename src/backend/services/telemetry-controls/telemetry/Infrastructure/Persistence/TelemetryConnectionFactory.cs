using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Telemetry.Infrastructure.Persistence;

/// <summary>
/// Opens Npgsql connections configured with telemetry RLS session variables.
/// </summary>
public sealed class TelemetryConnectionFactory : ITelemetryConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<TelemetryConnectionFactory> _logger;

    public TelemetryConnectionFactory(
        NpgsqlDataSource dataSource,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        ILogger<TelemetryConnectionFactory> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await ConfigureConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "site_manager",
        "operator",
        "viewer",
        "service_account"
    };

    private static string NormalizeAndValidateRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return "service_account";
        }

        var normalizedRole = role.Trim().ToLowerInvariant();

        if (!AllowedRoles.Contains(normalizedRole))
        {
            throw new InvalidOperationException($"Role '{normalizedRole}' is not allowed. Allowed roles are: {string.Join(", ", AllowedRoles)}");
        }

        return normalizedRole;
    }

    public async Task ConfigureConnectionAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default)
    {
        var context = _rlsContextAccessor.Current;
        var role = NormalizeAndValidateRole(context.Role);
        var siteId = context.SiteId ?? Guid.Empty;
        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                set_config('app.current_user_id', @userId::text, false),
                set_config('app.user_role', @role, false),
                set_config('app.site_id', @siteId::text, false);
        ";

        command.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("role", NpgsqlDbType.Text).Value = role;
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId;

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "Failed to configure telemetry RLS context (user: {UserId}, role: {Role}, site: {SiteId})", userId, role, siteId);
            throw;
        }
    }
}
