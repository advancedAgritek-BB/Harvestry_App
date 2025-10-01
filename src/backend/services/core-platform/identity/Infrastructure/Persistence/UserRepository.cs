using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<UserRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                user_id,
                email,
                email_verified,
                phone_number,
                phone_verified,
                first_name,
                last_name,
                display_name,
                password_hash,
                password_salt,
                failed_login_attempts,
                locked_until,
                last_login_at,
                profile_photo_url,
                language_preference,
                timezone,
                status,
                metadata,
                created_at,
                updated_at,
                created_by,
                updated_by
            FROM users
            WHERE user_id = @user_id
            LIMIT 1;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        MappedUser user;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            user = MapUser(reader);
        }
        
        var sites = await LoadUserSitesAsync(connection, user.Id, cancellationToken).ConfigureAwait(false);

        return User.Restore(
            user.Id,
            user.Email,
            user.EmailVerified,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.PhoneVerified,
            user.DisplayName,
            user.PasswordHash,
            user.PasswordSalt,
            user.FailedLoginAttempts,
            user.LockedUntil,
            user.LastLoginAt,
            user.ProfilePhotoUrl,
            user.LanguagePreference,
            user.Timezone,
            user.Status,
            user.Metadata,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedBy,
            user.UpdatedBy,
            sites);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        if (email == null) throw new ArgumentNullException(nameof(email));

        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT
                user_id,
                email,
                email_verified,
                phone_number,
                phone_verified,
                first_name,
                last_name,
                display_name,
                password_hash,
                password_salt,
                failed_login_attempts,
                locked_until,
                last_login_at,
                profile_photo_url,
                language_preference,
                timezone,
                status,
                metadata,
                created_at,
                updated_at,
                created_by,
                updated_by
            FROM users
            WHERE email = @email
            LIMIT 1;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("email", NpgsqlDbType.Varchar).Value = (string)email;

        MappedUser user;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            user = MapUser(reader);
        }
        
        var sites = await LoadUserSitesAsync(connection, user.Id, cancellationToken).ConfigureAwait(false);

        return User.Restore(
            user.Id,
            user.Email,
            user.EmailVerified,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.PhoneVerified,
            user.DisplayName,
            user.PasswordHash,
            user.PasswordSalt,
            user.FailedLoginAttempts,
            user.LockedUntil,
            user.LastLoginAt,
            user.ProfilePhotoUrl,
            user.LanguagePreference,
            user.Timezone,
            user.Status,
            user.Metadata,
            user.CreatedAt,
            user.UpdatedAt,
            user.CreatedBy,
            user.UpdatedBy,
            sites);
    }

    public async Task<IEnumerable<User>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT DISTINCT
                u.user_id,
                u.email,
                u.email_verified,
                u.phone_number,
                u.phone_verified,
                u.first_name,
                u.last_name,
                u.display_name,
                u.password_hash,
                u.password_salt,
                u.failed_login_attempts,
                u.locked_until,
                u.last_login_at,
                u.profile_photo_url,
                u.language_preference,
                u.timezone,
                u.status,
                u.metadata,
                u.created_at,
                u.updated_at,
                u.created_by,
                u.updated_by
            FROM users u
            INNER JOIN user_sites us ON us.user_id = u.user_id
            WHERE us.site_id = @site_id
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var results = new List<User>();
        var userRecords = new List<MappedUser>();

        await using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                userRecords.Add(MapUser(reader));
            }
        }
        
        // Batch load all user sites to avoid N+1 query
        var userIds = userRecords.Select(r => r.Id).ToList();
        var sitesByUser = await LoadUserSitesBatchAsync(connection, userIds, cancellationToken).ConfigureAwait(false);

        foreach (var record in userRecords)
        {
            var sites = sitesByUser.ContainsKey(record.Id)
                ? sitesByUser[record.Id]
                : new List<UserSite>();

            var user = User.Restore(
                record.Id,
                record.Email,
                record.EmailVerified,
                record.FirstName,
                record.LastName,
                record.PhoneNumber,
                record.PhoneVerified,
                record.DisplayName,
                record.PasswordHash,
                record.PasswordSalt,
                record.FailedLoginAttempts,
                record.LockedUntil,
                record.LastLoginAt,
                record.ProfilePhotoUrl,
                record.LanguagePreference,
                record.Timezone,
                record.Status,
                record.Metadata,
                record.CreatedAt,
                record.UpdatedAt,
                record.CreatedBy,
                record.UpdatedBy,
                sites);

            results.Add(user);
        }

        return results;
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var connection = await PrepareConnectionAsync(user.UserSites.FirstOrDefault()?.SiteId, cancellationToken).ConfigureAwait(false);

        await using var transaction = await _dbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            const string insertSql = @"
                INSERT INTO users (
                    user_id,
                    email,
                    email_verified,
                    phone_number,
                    phone_verified,
                    first_name,
                    last_name,
                    display_name,
                    password_hash,
                    password_salt,
                    failed_login_attempts,
                    locked_until,
                    last_login_at,
                    profile_photo_url,
                    language_preference,
                    timezone,
                    status,
                    metadata,
                    created_at,
                    updated_at,
                    created_by,
                    updated_by)
                VALUES (
                    @user_id,
                    @email,
                    @email_verified,
                    @phone_number,
                    @phone_verified,
                    @first_name,
                    @last_name,
                    @display_name,
                    @password_hash,
                    @password_salt,
                    @failed_login_attempts,
                    @locked_until,
                    @last_login_at,
                    @profile_photo_url,
                    @language_preference,
                    @timezone,
                    @status,
                    @metadata,
                    @created_at,
                    @updated_at,
                    @created_by,
                    @updated_by);
            ";

            await using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = insertSql;
                insertCommand.Transaction = transaction;
                PopulateUserParameters(insertCommand, user);
                await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            if (user.UserSites.Any())
            {
                const string userSiteSql = @"
                    INSERT INTO user_sites (
                        user_site_id,
                        user_id,
                        site_id,
                        role_id,
                        is_primary_site,
                        assigned_at,
                        assigned_by,
                        revoked_at,
                        revoked_by,
                        revoke_reason)
                    VALUES (
                        @user_site_id,
                        @user_id,
                        @site_id,
                        @role_id,
                        @is_primary_site,
                        @assigned_at,
                        @assigned_by,
                        @revoked_at,
                        @revoked_by,
                        @revoke_reason)
                    ON CONFLICT (user_id, site_id) DO UPDATE SET
                        role_id = EXCLUDED.role_id,
                        is_primary_site = EXCLUDED.is_primary_site,
                        assigned_at = EXCLUDED.assigned_at,
                        assigned_by = EXCLUDED.assigned_by,
                        revoked_at = EXCLUDED.revoked_at,
                        revoked_by = EXCLUDED.revoked_by,
                        revoke_reason = EXCLUDED.revoke_reason;
                ";

                foreach (var assignment in user.UserSites)
                {
                    await using var assignmentCommand = connection.CreateCommand();
                    assignmentCommand.CommandText = userSiteSql;
                    assignmentCommand.Transaction = transaction;

                    assignmentCommand.Parameters.Add("user_site_id", NpgsqlDbType.Uuid).Value = assignment.Id;
                    assignmentCommand.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = assignment.UserId;
                    assignmentCommand.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = assignment.SiteId;
                    assignmentCommand.Parameters.Add("role_id", NpgsqlDbType.Uuid).Value = assignment.RoleId;
                    assignmentCommand.Parameters.Add("is_primary_site", NpgsqlDbType.Boolean).Value = assignment.IsPrimarySite;
                    assignmentCommand.Parameters.Add("assigned_at", NpgsqlDbType.TimestampTz).Value = assignment.AssignedAt;
                    assignmentCommand.Parameters.Add("assigned_by", NpgsqlDbType.Uuid).Value = assignment.AssignedBy;
                    assignmentCommand.Parameters.Add("revoked_at", NpgsqlDbType.TimestampTz).Value = (object?)assignment.RevokedAt ?? DBNull.Value;
                    assignmentCommand.Parameters.Add("revoked_by", NpgsqlDbType.Uuid).Value = (object?)assignment.RevokedBy ?? DBNull.Value;
                    assignmentCommand.Parameters.Add("revoke_reason", NpgsqlDbType.Text).Value = (object?)assignment.RevokeReason ?? DBNull.Value;

                    await assignmentCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return user;
        }
        catch (PostgresException ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "Failed to insert user {UserId}", user.Id);
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var primarySite = user.UserSites.FirstOrDefault(us => us.IsPrimarySite) ?? user.UserSites.FirstOrDefault();
        var connection = await PrepareConnectionAsync(primarySite?.SiteId, cancellationToken).ConfigureAwait(false);

        await using var transaction = await _dbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            const string updateSql = @"
                UPDATE users SET
                    email = @email,
                    email_verified = @email_verified,
                    phone_number = @phone_number,
                    phone_verified = @phone_verified,
                    first_name = @first_name,
                    last_name = @last_name,
                    display_name = @display_name,
                    password_hash = @password_hash,
                    password_salt = @password_salt,
                    failed_login_attempts = @failed_login_attempts,
                    locked_until = @locked_until,
                    last_login_at = @last_login_at,
                    profile_photo_url = @profile_photo_url,
                    language_preference = @language_preference,
                    timezone = @timezone,
                    status = @status,
                    metadata = @metadata,
                    updated_at = @updated_at,
                    updated_by = @updated_by
                WHERE user_id = @user_id;
            ";

            await using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = updateSql;
                updateCommand.Transaction = transaction;
                PopulateUserParameters(updateCommand, user, isUpdate: true);

                var affected = await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                if (affected == 0)
                {
                    throw new InvalidOperationException($"User {user.Id} was not updated (no rows affected)");
                }
            }

            // Delete assignments that are no longer in the user's collection
            var currentSiteIds = user.UserSites.Select(us => us.SiteId).ToList();
            if (currentSiteIds.Count > 0)
            {
                const string deleteSql = @"
                    DELETE FROM user_sites
                    WHERE user_id = @user_id
                    AND site_id != ALL(@site_ids);
                ";

                await using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = deleteSql;
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = user.Id;
                deleteCommand.Parameters.Add("site_ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value = currentSiteIds.ToArray();
                await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // If no assignments, delete all for this user
                const string deleteAllSql = @"
                    DELETE FROM user_sites
                    WHERE user_id = @user_id;
                ";

                await using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = deleteAllSql;
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = user.Id;
                await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            // Sync user site assignments (upsert existing, insert new)
            const string upsertSql = @"
                INSERT INTO user_sites (
                    user_site_id,
                    user_id,
                    site_id,
                    role_id,
                    is_primary_site,
                    assigned_at,
                    assigned_by,
                    revoked_at,
                    revoked_by,
                    revoke_reason)
                VALUES (
                    @user_site_id,
                    @user_id,
                    @site_id,
                    @role_id,
                    @is_primary_site,
                    @assigned_at,
                    @assigned_by,
                    @revoked_at,
                    @revoked_by,
                    @revoke_reason)
                ON CONFLICT (user_id, site_id) DO UPDATE SET
                    role_id = EXCLUDED.role_id,
                    is_primary_site = EXCLUDED.is_primary_site,
                    assigned_at = EXCLUDED.assigned_at,
                    assigned_by = EXCLUDED.assigned_by,
                    revoked_at = EXCLUDED.revoked_at,
                    revoked_by = EXCLUDED.revoked_by,
                    revoke_reason = EXCLUDED.revoke_reason;
            ";

            foreach (var assignment in user.UserSites)
            {
                await using var upsertCommand = connection.CreateCommand();
                upsertCommand.CommandText = upsertSql;
                upsertCommand.Transaction = transaction;

                upsertCommand.Parameters.Add("user_site_id", NpgsqlDbType.Uuid).Value = assignment.Id;
                upsertCommand.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = assignment.UserId;
                upsertCommand.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = assignment.SiteId;
                upsertCommand.Parameters.Add("role_id", NpgsqlDbType.Uuid).Value = assignment.RoleId;
                upsertCommand.Parameters.Add("is_primary_site", NpgsqlDbType.Boolean).Value = assignment.IsPrimarySite;
                upsertCommand.Parameters.Add("assigned_at", NpgsqlDbType.TimestampTz).Value = assignment.AssignedAt;
                upsertCommand.Parameters.Add("assigned_by", NpgsqlDbType.Uuid).Value = assignment.AssignedBy;
                upsertCommand.Parameters.Add("revoked_at", NpgsqlDbType.TimestampTz).Value = (object?)assignment.RevokedAt ?? DBNull.Value;
                upsertCommand.Parameters.Add("revoked_by", NpgsqlDbType.Uuid).Value = (object?)assignment.RevokedBy ?? DBNull.Value;
                upsertCommand.Parameters.Add("revoke_reason", NpgsqlDbType.Text).Value = (object?)assignment.RevokeReason ?? DBNull.Value;

                await upsertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = "SELECT 1 FROM users WHERE user_id = @user_id LIMIT 1;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteId, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;

        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var scopeSite = siteId ?? context.SiteId ?? Guid.Empty;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(userId, role, scopeSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static void PopulateUserParameters(NpgsqlCommand command, User user, bool isUpdate = false)
    {
        if (!isUpdate)
        {
            command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = user.Id;
            command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = user.CreatedAt;
            command.Parameters.Add("created_by", NpgsqlDbType.Uuid).Value = (object?)user.CreatedBy ?? DBNull.Value;
        }
        else
        {
            command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = user.Id;
        }

        command.Parameters.Add("email", NpgsqlDbType.Varchar).Value = (string)user.Email;
        command.Parameters.Add("email_verified", NpgsqlDbType.Boolean).Value = user.EmailVerified;
        command.Parameters.Add("phone_number", NpgsqlDbType.Varchar).Value = user.PhoneNumber is null ? DBNull.Value : user.PhoneNumber.Value;
        command.Parameters.Add("phone_verified", NpgsqlDbType.Boolean).Value = user.PhoneVerified;
        command.Parameters.Add("first_name", NpgsqlDbType.Varchar).Value = user.FirstName;
        command.Parameters.Add("last_name", NpgsqlDbType.Varchar).Value = user.LastName;
        command.Parameters.Add("display_name", NpgsqlDbType.Varchar).Value = user.DisplayName;
        command.Parameters.Add("password_hash", NpgsqlDbType.Varchar).Value = (object?)user.PasswordHash ?? DBNull.Value;
        command.Parameters.Add("password_salt", NpgsqlDbType.Varchar).Value = (object?)user.PasswordSalt ?? DBNull.Value;
        command.Parameters.Add("failed_login_attempts", NpgsqlDbType.Integer).Value = user.FailedLoginAttempts;
        command.Parameters.Add("locked_until", NpgsqlDbType.TimestampTz).Value = (object?)user.LockedUntil ?? DBNull.Value;
        command.Parameters.Add("last_login_at", NpgsqlDbType.TimestampTz).Value = (object?)user.LastLoginAt ?? DBNull.Value;
        command.Parameters.Add("profile_photo_url", NpgsqlDbType.Text).Value = (object?)user.ProfilePhotoUrl ?? DBNull.Value;
        command.Parameters.Add("language_preference", NpgsqlDbType.Varchar).Value = user.LanguagePreference;
        command.Parameters.Add("timezone", NpgsqlDbType.Varchar).Value = user.Timezone;
        command.Parameters.Add("status", NpgsqlDbType.Varchar).Value = Enum.GetName(typeof(UserStatus), user.Status)!.ToLowerInvariant();
        command.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(user.Metadata);
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = user.UpdatedAt;
        command.Parameters.Add("updated_by", NpgsqlDbType.Uuid).Value = (object?)user.UpdatedBy ?? DBNull.Value;
    }

    private static MappedUser MapUser(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(reader.GetOrdinal("user_id"));
        var email = Email.Create(GetString(reader, "email"));
        var phoneRaw = GetNullableString(reader, "phone_number");
        PhoneNumber? phoneNumber = null;
        if (!string.IsNullOrWhiteSpace(phoneRaw))
        {
            phoneNumber = PhoneNumber.Create(phoneRaw!);
        }

        var statusValue = GetString(reader, "status");
        if (!Enum.TryParse(statusValue, true, out UserStatus status))
        {
            status = UserStatus.Active;
        }

        var metadataValue = reader["metadata"];

        return new MappedUser(
            id,
            email,
            reader.GetBoolean(reader.GetOrdinal("email_verified")),
            phoneNumber,
            reader.GetBoolean(reader.GetOrdinal("phone_verified")),
            GetString(reader, "first_name"),
            GetString(reader, "last_name"),
            GetString(reader, "display_name"),
            GetNullableString(reader, "password_hash"),
            GetNullableString(reader, "password_salt"),
            reader.GetInt32(reader.GetOrdinal("failed_login_attempts")),
            GetNullableDateTime(reader, "locked_until"),
            GetNullableDateTime(reader, "last_login_at"),
            GetNullableString(reader, "profile_photo_url"),
            GetString(reader, "language_preference"),
            GetString(reader, "timezone"),
            status,
            JsonUtilities.ToDictionary(metadataValue),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")),
            GetNullableGuid(reader, "created_by"),
            GetNullableGuid(reader, "updated_by"));
    }

    private static async Task<IReadOnlyList<UserSite>> LoadUserSitesAsync(
        NpgsqlConnection connection,
        Guid userId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                user_site_id,
                user_id,
                site_id,
                role_id,
                is_primary_site,
                assigned_at,
                assigned_by,
                revoked_at,
                revoked_by,
                revoke_reason
            FROM user_sites
            WHERE user_id = @user_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;

        var assignments = new List<UserSite>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var assignment = UserSite.Restore(
                reader.GetGuid(reader.GetOrdinal("user_site_id")),
                reader.GetGuid(reader.GetOrdinal("user_id")),
                reader.GetGuid(reader.GetOrdinal("site_id")),
                reader.GetGuid(reader.GetOrdinal("role_id")),
                reader.GetBoolean(reader.GetOrdinal("is_primary_site")),
                reader.GetDateTime(reader.GetOrdinal("assigned_at")),
                reader.GetGuid(reader.GetOrdinal("assigned_by")),
                reader.IsDBNull(reader.GetOrdinal("revoked_at")) ? null : reader.GetDateTime(reader.GetOrdinal("revoked_at")),
                reader.IsDBNull(reader.GetOrdinal("revoked_by")) ? null : reader.GetGuid(reader.GetOrdinal("revoked_by")),
                reader.IsDBNull(reader.GetOrdinal("revoke_reason")) ? null : reader.GetString(reader.GetOrdinal("revoke_reason")));

            assignments.Add(assignment);
        }

        return assignments;
    }

    private static async Task<Dictionary<Guid, List<UserSite>>> LoadUserSitesBatchAsync(
        NpgsqlConnection connection,
        List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds == null || userIds.Count == 0)
            return new Dictionary<Guid, List<UserSite>>();

        const string sql = @"
            SELECT
                user_site_id,
                user_id,
                site_id,
                role_id,
                is_primary_site,
                assigned_at,
                assigned_by,
                revoked_at,
                revoked_by,
                revoke_reason
            FROM user_sites
            WHERE user_id = ANY(@user_ids);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid).Value = userIds.ToArray();

        var result = new Dictionary<Guid, List<UserSite>>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var userId = reader.GetGuid(reader.GetOrdinal("user_id"));
            var assignment = UserSite.Restore(
                reader.GetGuid(reader.GetOrdinal("user_site_id")),
                userId,
                reader.GetGuid(reader.GetOrdinal("site_id")),
                reader.GetGuid(reader.GetOrdinal("role_id")),
                reader.GetBoolean(reader.GetOrdinal("is_primary_site")),
                reader.GetDateTime(reader.GetOrdinal("assigned_at")),
                reader.GetGuid(reader.GetOrdinal("assigned_by")),
                GetNullableDateTime(reader, "revoked_at"),
                GetNullableGuid(reader, "revoked_by"),
                GetNullableString(reader, "revoke_reason"));

            if (!result.ContainsKey(userId))
            {
                result[userId] = new List<UserSite>();
            }
            result[userId].Add(assignment);
        }

        return result;
    }

    private readonly record struct MappedUser(
        Guid Id,
        Email Email,
        bool EmailVerified,
        PhoneNumber? PhoneNumber,
        bool PhoneVerified,
        string FirstName,
        string LastName,
        string DisplayName,
        string? PasswordHash,
        string? PasswordSalt,
        int FailedLoginAttempts,
        DateTime? LockedUntil,
        DateTime? LastLoginAt,
        string? ProfilePhotoUrl,
        string LanguagePreference,
        string Timezone,
        UserStatus Status,
        Dictionary<string, object> Metadata,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid? CreatedBy,
        Guid? UpdatedBy);

    private static string GetString(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.GetString(ordinal);
    }

    private static string? GetNullableString(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static DateTime? GetNullableDateTime(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static Guid? GetNullableGuid(NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetGuid(ordinal);
    }
}
