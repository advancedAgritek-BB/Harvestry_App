using System.Text.Json;
using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Domain.Entities;
using Harvestry.Integration.Growlink.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Integration.Growlink.Infrastructure.Persistence;

/// <summary>
/// Repository for Growlink credentials using direct SQL.
/// </summary>
public sealed class GrowlinkCredentialRepository : IGrowlinkCredentialRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<GrowlinkCredentialRepository> _logger;

    public GrowlinkCredentialRepository(
        NpgsqlDataSource dataSource,
        ILogger<GrowlinkCredentialRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GrowlinkCredential?> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, growlink_account_id, access_token, refresh_token,
                   token_expires_at, status, last_sync_at, last_sync_error,
                   consecutive_failures, created_at, updated_at
            FROM growlink_credentials
            WHERE site_id = @SiteId";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToCredential(reader);
    }

    public async Task<List<GrowlinkCredential>> GetActiveCredentialsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, growlink_account_id, access_token, refresh_token,
                   token_expires_at, status, last_sync_at, last_sync_error,
                   consecutive_failures, created_at, updated_at
            FROM growlink_credentials
            WHERE status IN (@Connected, @TokenExpired)";

        var credentials = new List<GrowlinkCredential>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Connected", GrowlinkConnectionStatus.Connected.ToString());
        command.Parameters.AddWithValue("@TokenExpired", GrowlinkConnectionStatus.TokenExpired.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            credentials.Add(MapToCredential(reader));
        }

        return credentials;
    }

    public async Task CreateAsync(
        GrowlinkCredential credential,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO growlink_credentials
                (id, site_id, growlink_account_id, access_token, refresh_token,
                 token_expires_at, status, last_sync_at, last_sync_error,
                 consecutive_failures, created_at, updated_at)
            VALUES
                (@Id, @SiteId, @GrowlinkAccountId, @AccessToken, @RefreshToken,
                 @TokenExpiresAt, @Status, @LastSyncAt, @LastSyncError,
                 @ConsecutiveFailures, @CreatedAt, @UpdatedAt)";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        AddCredentialParameters(command, credential);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogDebug("Created Growlink credential for site {SiteId}", credential.SiteId);
    }

    public async Task UpdateAsync(
        GrowlinkCredential credential,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE growlink_credentials
            SET access_token = @AccessToken,
                refresh_token = @RefreshToken,
                token_expires_at = @TokenExpiresAt,
                status = @Status,
                last_sync_at = @LastSyncAt,
                last_sync_error = @LastSyncError,
                consecutive_failures = @ConsecutiveFailures,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Id", credential.Id);
        command.Parameters.AddWithValue("@AccessToken", credential.AccessToken);
        command.Parameters.AddWithValue("@RefreshToken", credential.RefreshToken);
        command.Parameters.AddWithValue("@TokenExpiresAt", credential.TokenExpiresAt);
        command.Parameters.AddWithValue("@Status", credential.Status.ToString());
        command.Parameters.AddWithValue("@LastSyncAt", (object?)credential.LastSyncAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastSyncError", (object?)credential.LastSyncError ?? DBNull.Value);
        command.Parameters.AddWithValue("@ConsecutiveFailures", credential.ConsecutiveFailures);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM growlink_credentials WHERE id = @Id";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogDebug("Deleted Growlink credential {Id}", id);
    }

    private static void AddCredentialParameters(NpgsqlCommand command, GrowlinkCredential credential)
    {
        command.Parameters.AddWithValue("@Id", credential.Id);
        command.Parameters.AddWithValue("@SiteId", credential.SiteId);
        command.Parameters.AddWithValue("@GrowlinkAccountId", credential.GrowlinkAccountId);
        command.Parameters.AddWithValue("@AccessToken", credential.AccessToken);
        command.Parameters.AddWithValue("@RefreshToken", credential.RefreshToken);
        command.Parameters.AddWithValue("@TokenExpiresAt", credential.TokenExpiresAt);
        command.Parameters.AddWithValue("@Status", credential.Status.ToString());
        command.Parameters.AddWithValue("@LastSyncAt", (object?)credential.LastSyncAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@LastSyncError", (object?)credential.LastSyncError ?? DBNull.Value);
        command.Parameters.AddWithValue("@ConsecutiveFailures", credential.ConsecutiveFailures);
        command.Parameters.AddWithValue("@CreatedAt", credential.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", credential.UpdatedAt);
    }

    private static GrowlinkCredential MapToCredential(NpgsqlDataReader reader)
    {
        var statusStr = reader.GetString(6);
        var status = Enum.TryParse<GrowlinkConnectionStatus>(statusStr, out var parsed)
            ? parsed
            : GrowlinkConnectionStatus.NotConnected;

        return GrowlinkCredential.FromPersistence(
            id: reader.GetGuid(0),
            siteId: reader.GetGuid(1),
            growlinkAccountId: reader.GetString(2),
            accessToken: reader.GetString(3),
            refreshToken: reader.GetString(4),
            tokenExpiresAt: reader.GetFieldValue<DateTimeOffset>(5),
            status: status,
            lastSyncAt: reader.IsDBNull(7) ? null : reader.GetFieldValue<DateTimeOffset>(7),
            lastSyncError: reader.IsDBNull(8) ? null : reader.GetString(8),
            consecutiveFailures: reader.GetInt32(9),
            createdAt: reader.GetFieldValue<DateTimeOffset>(10),
            updatedAt: reader.GetFieldValue<DateTimeOffset>(11));
    }
}




