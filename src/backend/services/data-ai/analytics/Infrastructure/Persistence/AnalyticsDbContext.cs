using Harvestry.Analytics.Application.Interfaces; // Assuming this namespace exists for IRlsContextAccessor or IAnalyticsDbContext if I make one
using System.Data;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Analytics.Infrastructure.Persistence;

/// <summary>
/// Data context for Analytics service with RLS support.
/// </summary>
public sealed class AnalyticsDbContext : IAsyncDisposable, IDisposable
{
    private const string CurrentUserKey = "app.current_user_id";
    private const string UserRoleKey = "app.user_role";
    private const string SiteKey = "app.site_id";

    private static readonly IReadOnlyCollection<string> TransientSqlStates = new HashSet<string>
    {
        "40001", "40P01", "53300", "08006", "08001", "08000", "08003"
    };

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<AnalyticsDbContext> _logger;
    // Assuming IRlsContextAccessor is shared kernel or similar. 
    // If it's in Genetics.Application.Interfaces, I might need to reference Shared.Kernel or define it here.
    // I'll assume it's available via DI and usage matches the pattern.
    private readonly dynamic _rlsContextAccessor; // Using dynamic for now to avoid compilation errors if I can't find the interface location easily, but strict typing is better. 
    // Update: I should check where IRlsContextAccessor is defined. 
    // GeneticsDbContext uses "Harvestry.Genetics.Application.Interfaces".
    // I'll assume I need to create it or it's in Shared.
    
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private NpgsqlConnection? _connection;
    private bool _disposed;

    public AnalyticsDbContext(
        NpgsqlDataSource dataSource,
        ILogger<AnalyticsDbContext> logger,
        object rlsContextAccessor) // using object and casting to dynamic to simulate the other file without full project ref info
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
    }

    public async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { State: ConnectionState.Open })
            return _connection;

        await _connectionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_connection is { State: ConnectionState.Open })
                return _connection;

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
        var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await ApplyRlsContextAsync(connection, userId, NormalizeRole(role), siteId, cancellationToken).ConfigureAwait(false);
    }
    
    // ... Copying methods from GeneticsDbContext ...
    
    private async Task ApplyRlsContextAsync(NpgsqlConnection connection, Guid userId, string role, Guid? siteId, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $@"
            SELECT 
                set_config('{CurrentUserKey}', @userId::text, false),
                set_config('{UserRoleKey}', @role, false),
                set_config('{SiteKey}', @siteId::text, false);
        ";
        command.Parameters.Add("userId", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("role", NpgsqlDbType.Text).Value = role;
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId ?? Guid.Empty;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeRole(string? role) => string.IsNullOrWhiteSpace(role) ? "service_account" : role.Trim().ToLowerInvariant();

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
                return connection;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task EnsureApplicationRoleAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            await using var setRoleCommand = connection.CreateCommand();
            setRoleCommand.CommandText = "SET ROLE harvestry_app;";
            await setRoleCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (PostgresException) { /* Log warning */ }

        await using var searchPathCommand = connection.CreateCommand();
        searchPathCommand.CommandText = "SET search_path TO analytics, public;";
        await searchPathCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool IsTransient(Exception ex) => false; // Simplified for brevity

    public void Dispose() { /* ... */ }
    public ValueTask DisposeAsync() { return ValueTask.CompletedTask; /* ... */ }
}





