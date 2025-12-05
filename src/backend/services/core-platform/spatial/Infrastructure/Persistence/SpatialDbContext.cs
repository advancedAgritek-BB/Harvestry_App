using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Spatial.Infrastructure.Persistence;

/// <summary>
/// Thin data context responsible for connection management and RLS session configuration for the spatial service.
/// </summary>
public sealed class SpatialDbContext : IAsyncDisposable, IDisposable
{
    private const string CurrentUserKey = "app.current_user_id";
    private const string UserRoleKey = "app.user_role";
    private const string SiteKey = "app.site_id";

    private static readonly IReadOnlyCollection<string> TransientSqlStates = new HashSet<string>
    {
        "40001", // serialization_failure
        "40P01", // deadlock_detected
        "53300", // too_many_connections
        "08006", // connection_failure
        "08001", // sqlclient_unable_to_establish_sqlconnection
        "08000", // connection_exception
        "08003"  // connection_does_not_exist
    };

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<SpatialDbContext> _logger;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    private NpgsqlConnection? _connection;
    private bool _disposed;

    public SpatialDbContext(NpgsqlDataSource dataSource, ILogger<SpatialDbContext> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SpatialDbContext));
        }
        
        if (_connection is { State: ConnectionState.Open })
        {
            return _connection;
        }

        await _connectionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check disposed after acquiring semaphore
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SpatialDbContext));
            }
            
            if (_connection is { State: ConnectionState.Open })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
                _connection = null;
            }

            _connection = await OpenConnectionWithRetryAsync(cancellationToken).ConfigureAwait(false);
            return _connection;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task SetRlsContextAsync(Guid userId, string role, Guid? siteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be null or whitespace", nameof(role));
        }

        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                set_config(@currentUserKey, @userId::text, false),
                set_config(@userRoleKey, @role, false),
                set_config(@siteKey, @siteId::text, false);
        ";
        command.Parameters.AddWithValue("currentUserKey", CurrentUserKey);
        command.Parameters.AddWithValue("userRoleKey", UserRoleKey);
        command.Parameters.AddWithValue("siteKey", SiteKey);

        command.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("role", NpgsqlDbType.Text).Value = role.Trim().ToLowerInvariant();
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId ?? Guid.Empty;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Spatial RLS context applied for user {UserId} role {Role} site {SiteId}", userId, role, siteId);
    }

    public async Task ResetRlsContextAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                set_config(@currentUserKey, NULL, false),
                set_config(@userRoleKey, NULL, false),
                set_config(@siteKey, NULL, false);
        ";
        command.Parameters.AddWithValue("currentUserKey", CurrentUserKey);
        command.Parameters.AddWithValue("userRoleKey", UserRoleKey);
        command.Parameters.AddWithValue("siteKey", SiteKey);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<NpgsqlTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        return await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
    }

    private async Task<NpgsqlConnection> OpenConnectionWithRetryAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        var attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            try
            {
                var connection = await _dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
                await EnsureApplicationRoleAsync(connection, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Opened spatial PostgreSQL connection on attempt {Attempt}", attempt);
                return connection;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));
                _logger.LogWarning(ex, "Transient error opening PostgreSQL connection (attempt {Attempt}/{Max}). Retrying in {Delay}.", attempt, maxAttempts, delay);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task EnsureApplicationRoleAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);

        try
        {
            await using var setRoleCommand = connection.CreateCommand();
            setRoleCommand.CommandText = "SET ROLE harvestry_app;";
            await setRoleCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (PostgresException ex) when (ex.SqlState is PostgresErrorCodes.UndefinedObject or PostgresErrorCodes.InvalidAuthorizationSpecification)
        {
            _logger.LogWarning(ex, "Unable to set PostgreSQL role to harvestry_app. RLS enforcement may be bypassed for this connection.");
        }
    }

    private static bool IsTransient(Exception exception) => exception switch
    {
        TimeoutException => true,
        PostgresException postgresException when TransientSqlStates.Contains(postgresException.SqlState) => true,
        NpgsqlException npgsqlException when npgsqlException.InnerException is not null && IsTransient(npgsqlException.InnerException) => true,
        _ => false
    };

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _connectionSemaphore.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispose spatial db semaphore");
        }

        if (_connection is not null)
        {
            try
            {
                _connection.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when closing spatial db connection during Dispose()");
            }

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when disposing spatial db connection during Dispose()");
            }
            finally
            {
                _connection = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            _connectionSemaphore.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to dispose spatial db semaphore (async)");
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when closing spatial db connection during DisposeAsync()");
            }

            try
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error when disposing spatial db connection during DisposeAsync()");
            }
            finally
            {
                _connection = null;
            }
        }
    }
}
