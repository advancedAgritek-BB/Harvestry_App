using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

/// <summary>
/// Lightweight database context responsible for opening Npgsql connections,
/// setting Row-Level Security (RLS) session variables, and providing
/// transaction support for the identity service.
/// </summary>
public sealed class IdentityDbContext : IAsyncDisposable, IDisposable
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
    private readonly ILogger<IdentityDbContext> _logger;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    private NpgsqlConnection? _connection;
    private bool _disposed;

    public IdentityDbContext(NpgsqlDataSource dataSource, ILogger<IdentityDbContext> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets an open connection, opening it with retry semantics when necessary.
    /// </summary>
    public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { State: ConnectionState.Open })
        {
            return _connection;
        }

        await _connectionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
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

    /// <summary>
    /// Sets the PostgreSQL session variables used by RLS policies.
    /// </summary>
    public async Task SetRlsContextAsync(Guid userId, string userRole, Guid siteId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userRole))
        {
            throw new ArgumentException("User role cannot be null or whitespace", nameof(userRole));
        }

        var normalizedRole = userRole.Trim().ToLowerInvariant();
        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT 
                set_config('{CurrentUserKey}', @userId::text, false),
                set_config('{UserRoleKey}', @userRole, false),
                set_config('{SiteKey}', @siteId::text, false);
        ";

        command.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("userRole", NpgsqlDbType.Text).Value = normalizedRole;
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "Set RLS context for user {UserId} (role: {Role}) on site {SiteId}",
            userId, normalizedRole, siteId);
    }

    /// <summary>
    /// Clears the RLS session variables. Useful after background tasks.
    /// </summary>
    public async Task ResetRlsContextAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT 
                set_config('{CurrentUserKey}', NULL, false),
                set_config('{UserRoleKey}', NULL, false),
                set_config('{SiteKey}', NULL, false);
        ";

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Begins a PostgreSQL transaction with the provided isolation level.
    /// </summary>
    public async Task<NpgsqlTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
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
                _logger.LogDebug("Opened PostgreSQL connection on attempt {Attempt}", attempt);
                return connection;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));
                _logger.LogWarning(
                    ex,
                    "Transient failure opening PostgreSQL connection (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}...",
                    attempt,
                    maxAttempts,
                    delay);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static bool IsTransient(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            PostgresException postgresException when TransientSqlStates.Contains(postgresException.SqlState) => true,
            NpgsqlException npgsqlException when npgsqlException.InnerException is not null && IsTransient(npgsqlException.InnerException) => true,
            _ => false
        };
    }

    /// <summary>
    /// Disposes resources synchronously. Note: DisposeAsync() is the preferred method
    /// as it properly handles async connection cleanup. This synchronous method uses
    /// blocking disposal which may not be optimal for all scenarios.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Dispose semaphore independently with its own exception handling
        try
        {
            _connectionSemaphore.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed – no action required.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while disposing connection semaphore");
        }

        // Dispose connection independently with its own exception handling
        if (_connection is not null)
        {
            try
            {
                _connection.Close();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while closing PostgreSQL connection during synchronous disposal");
            }

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing PostgreSQL connection during synchronous disposal");
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

        // Dispose semaphore independently with its own exception handling
        try
        {
            _connectionSemaphore.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed – no action required.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error while disposing connection semaphore");
        }

        // Dispose connection independently with its own exception handling
        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while closing PostgreSQL connection during async disposal");
            }

            try
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing PostgreSQL connection during async disposal");
            }
            finally
            {
                _connection = null;
            }
        }
    }
}
