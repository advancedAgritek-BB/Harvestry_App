using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class SessionRepository : ISessionRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<SessionRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = SessionSelect + " WHERE session_id = @session_id LIMIT 1;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("session_id", NpgsqlDbType.Uuid).Value = sessionId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapSession(reader);
    }

    public async Task<Session?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            throw new ArgumentException("Session token cannot be null or whitespace", nameof(sessionToken));
        }

        var hashedToken = HashToken(sessionToken);
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = SessionSelect + " WHERE session_token = @token LIMIT 1;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("token", NpgsqlDbType.Varchar).Value = hashedToken;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapSession(reader);
    }

    public async Task<IEnumerable<Session>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = SessionSelect + " WHERE user_id = @user_id AND is_revoked = FALSE AND expires_at > NOW();";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        var results = new List<Session>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapSession(reader));
        }

        return results;
    }

    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        var connection = await PrepareConnectionAsync(session.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO sessions (
                session_id,
                user_id,
                site_id,
                session_token,
                refresh_token,
                device_fingerprint,
                ip_address,
                user_agent,
                login_method,
                session_start,
                session_end,
                last_activity,
                expires_at,
                is_revoked,
                revoke_reason,
                metadata)
            VALUES (
                @session_id,
                @user_id,
                @site_id,
                @session_token,
                @refresh_token,
                @device_fingerprint,
                @ip_address,
                @user_agent,
                @login_method,
                @session_start,
                @session_end,
                @last_activity,
                @expires_at,
                @is_revoked,
                @revoke_reason,
                @metadata);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, session);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        var connection = await PrepareConnectionAsync(session.SiteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE sessions SET
                user_id = @user_id,
                site_id = @site_id,
                session_token = @session_token,
                refresh_token = @refresh_token,
                device_fingerprint = @device_fingerprint,
                ip_address = @ip_address,
                user_agent = @user_agent,
                login_method = @login_method,
                session_start = @session_start,
                session_end = @session_end,
                last_activity = @last_activity,
                expires_at = @expires_at,
                is_revoked = @is_revoked,
                revoke_reason = @revoke_reason,
                metadata = @metadata
            WHERE session_id = @session_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, session);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> RevokeAllByUserIdAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required", nameof(reason));
        }

        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE sessions SET
                is_revoked = TRUE,
                revoke_reason = @reason,
                session_end = NOW()
            WHERE user_id = @user_id AND is_revoked = FALSE;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("reason", NpgsqlDbType.Text).Value = reason;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Revoked {Count} sessions for user {UserId}", affected, userId);
        return affected;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteId, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var effectiveSite = siteId ?? context.SiteId ?? Guid.Empty;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(userId, role, effectiveSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static void PopulateParameters(NpgsqlCommand command, Session session)
    {
        command.Parameters.Add("session_id", NpgsqlDbType.Uuid).Value = session.Id;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = session.UserId;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = (object?)session.SiteId ?? DBNull.Value;
        command.Parameters.Add("session_token", NpgsqlDbType.Varchar).Value = HashToken(session.SessionToken);
        command.Parameters.Add("refresh_token", NpgsqlDbType.Varchar).Value = (object?)session.RefreshToken ?? DBNull.Value;
        command.Parameters.Add("device_fingerprint", NpgsqlDbType.Text).Value = (object?)session.DeviceFingerprint ?? DBNull.Value;
        command.Parameters.Add("ip_address", NpgsqlDbType.Inet).Value = (object?)session.IpAddress ?? DBNull.Value;
        command.Parameters.Add("user_agent", NpgsqlDbType.Text).Value = (object?)session.UserAgent ?? DBNull.Value;
        command.Parameters.Add("login_method", NpgsqlDbType.Varchar).Value = session.LoginMethod.ToString().ToLowerInvariant();
        command.Parameters.Add("session_start", NpgsqlDbType.TimestampTz).Value = session.SessionStart;
        command.Parameters.Add("session_end", NpgsqlDbType.TimestampTz).Value = (object?)session.SessionEnd ?? DBNull.Value;
        command.Parameters.Add("last_activity", NpgsqlDbType.TimestampTz).Value = session.LastActivity;
        command.Parameters.Add("expires_at", NpgsqlDbType.TimestampTz).Value = session.ExpiresAt;
        command.Parameters.Add("is_revoked", NpgsqlDbType.Boolean).Value = session.IsRevoked;
        command.Parameters.Add("revoke_reason", NpgsqlDbType.Text).Value = (object?)session.RevokeReason ?? DBNull.Value;
        command.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(session.Metadata);
    }

    private Session MapSession(NpgsqlDataReader reader)
    {
        var loginMethodValue = reader.GetString(reader.GetOrdinal("login_method"));
        if (!Enum.TryParse(loginMethodValue, true, out LoginMethod loginMethod))
        {
            var sessionId = reader.GetGuid(reader.GetOrdinal("session_id"));
            var userId = reader.GetGuid(reader.GetOrdinal("user_id"));
            _logger.LogWarning("Failed to parse login_method value '{LoginMethod}' for session {SessionId}, user {UserId}. Defaulting to Password.", 
                loginMethodValue, sessionId, userId);
            loginMethod = LoginMethod.Password;
        }

        var metadataValue = reader["metadata"];

        var session = Session.Restore(
            reader.GetGuid(reader.GetOrdinal("session_id")),
            reader.GetGuid(reader.GetOrdinal("user_id")),
            reader.IsDBNull(reader.GetOrdinal("site_id")) ? null : reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetString(reader.GetOrdinal("session_token")),
            reader.IsDBNull(reader.GetOrdinal("refresh_token")) ? null : reader.GetString(reader.GetOrdinal("refresh_token")),
            reader.IsDBNull(reader.GetOrdinal("device_fingerprint")) ? null : reader.GetString(reader.GetOrdinal("device_fingerprint")),
            reader.IsDBNull(reader.GetOrdinal("ip_address")) ? null : reader.GetFieldValue<string>(reader.GetOrdinal("ip_address")),
            reader.IsDBNull(reader.GetOrdinal("user_agent")) ? null : reader.GetString(reader.GetOrdinal("user_agent")),
            loginMethod,
            reader.GetDateTime(reader.GetOrdinal("session_start")),
            reader.IsDBNull(reader.GetOrdinal("session_end")) ? null : reader.GetDateTime(reader.GetOrdinal("session_end")),
            reader.GetDateTime(reader.GetOrdinal("last_activity")),
            reader.GetDateTime(reader.GetOrdinal("expires_at")),
            reader.GetBoolean(reader.GetOrdinal("is_revoked")),
            reader.IsDBNull(reader.GetOrdinal("revoke_reason")) ? null : reader.GetString(reader.GetOrdinal("revoke_reason")),
            JsonUtilities.ToDictionary(metadataValue));

        return session;
    }

    private const string SessionSelect = @"
        SELECT
            session_id,
            user_id,
            site_id,
            session_token,
            refresh_token,
            device_fingerprint,
            ip_address::text AS ip_address,
            user_agent,
            login_method,
            session_start,
            session_end,
            last_activity,
            expires_at,
            is_revoked,
            revoke_reason,
            metadata
        FROM sessions";

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        Span<byte> hashSpan = stackalloc byte[SHA256.HashSizeInBytes];
        if (!SHA256.TryHashData(bytes, hashSpan, out _))
        {
            throw new InvalidOperationException("Failed to hash session token");
        }

        return Convert.ToHexString(hashSpan);
    }
}
