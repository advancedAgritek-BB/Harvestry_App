using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class SiteRepository : ISiteRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<SiteRepository> _logger;

    public SiteRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<SiteRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Site?> GetByIdAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = SiteSelect + " WHERE site_id = @site_id LIMIT 1;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapSite(reader);
    }

    public async Task<Site?> GetByCodeAsync(string siteCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(siteCode))
            throw new ArgumentException("Site code cannot be null or whitespace", nameof(siteCode));

        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = SiteSelect + " WHERE site_code = @site_code LIMIT 1;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("site_code", NpgsqlDbType.Varchar).Value = siteCode;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapSite(reader);
    }

    public async Task<IEnumerable<Site>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(null, cancellationToken).ConfigureAwait(false);

        const string sql = SiteSelect + " WHERE org_id = @org_id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("org_id", NpgsqlDbType.Uuid).Value = orgId;

        var results = new List<Site>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapSite(reader));
        }

        return results;
    }

    public async Task<Site> AddAsync(Site site, CancellationToken cancellationToken = default)
    {
        if (site == null) throw new ArgumentNullException(nameof(site));

        var connection = await PrepareConnectionAsync(site.Id, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO sites (
                site_id,
                org_id,
                site_name,
                site_code,
                address_line1,
                address_line2,
                city,
                state_province,
                postal_code,
                country,
                timezone,
                license_number,
                license_type,
                license_expiration,
                site_type,
                status,
                site_policies,
                metadata,
                created_at,
                updated_at)
            VALUES (
                @site_id,
                @org_id,
                @site_name,
                @site_code,
                @address_line1,
                @address_line2,
                @city,
                @state_province,
                @postal_code,
                @country,
                @timezone,
                @license_number,
                @license_type,
                @license_expiration,
                @site_type,
                @status,
                @site_policies,
                @metadata,
                @created_at,
                @updated_at);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, site);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return site;
    }

    public async Task UpdateAsync(Site site, CancellationToken cancellationToken = default)
    {
        if (site == null) throw new ArgumentNullException(nameof(site));

        var connection = await PrepareConnectionAsync(site.Id, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            UPDATE sites SET
                org_id = @org_id,
                site_name = @site_name,
                site_code = @site_code,
                address_line1 = @address_line1,
                address_line2 = @address_line2,
                city = @city,
                state_province = @state_province,
                postal_code = @postal_code,
                country = @country,
                timezone = @timezone,
                license_number = @license_number,
                license_type = @license_type,
                license_expiration = @license_expiration,
                site_type = @site_type,
                status = @status,
                site_policies = @site_policies,
                metadata = @metadata,
                updated_at = @updated_at
            WHERE site_id = @site_id;
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, site);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteScope, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var userId = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var effectiveSite = siteScope ?? context.SiteId ?? Guid.Empty;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(userId, role, effectiveSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static void PopulateParameters(NpgsqlCommand command, Site site)
    {
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = site.Id;
        command.Parameters.Add("org_id", NpgsqlDbType.Uuid).Value = site.OrgId;
        command.Parameters.Add("site_name", NpgsqlDbType.Varchar).Value = site.SiteName;
        command.Parameters.Add("site_code", NpgsqlDbType.Varchar).Value = site.SiteCode;
        command.Parameters.Add("address_line1", NpgsqlDbType.Varchar).Value = (object?)site.AddressLine1 ?? DBNull.Value;
        command.Parameters.Add("address_line2", NpgsqlDbType.Varchar).Value = (object?)site.AddressLine2 ?? DBNull.Value;
        command.Parameters.Add("city", NpgsqlDbType.Varchar).Value = (object?)site.City ?? DBNull.Value;
        command.Parameters.Add("state_province", NpgsqlDbType.Varchar).Value = (object?)site.StateProvince ?? DBNull.Value;
        command.Parameters.Add("postal_code", NpgsqlDbType.Varchar).Value = (object?)site.PostalCode ?? DBNull.Value;
        command.Parameters.Add("country", NpgsqlDbType.Varchar).Value = site.Country;
        command.Parameters.Add("timezone", NpgsqlDbType.Varchar).Value = site.Timezone;
        command.Parameters.Add("license_number", NpgsqlDbType.Varchar).Value = (object?)site.LicenseNumber ?? DBNull.Value;
        command.Parameters.Add("license_type", NpgsqlDbType.Varchar).Value = (object?)site.LicenseType ?? DBNull.Value;
        command.Parameters.Add("license_expiration", NpgsqlDbType.Date).Value = (object?)site.LicenseExpiration ?? DBNull.Value;
        command.Parameters.Add("site_type", NpgsqlDbType.Varchar).Value = site.SiteType;
        command.Parameters.Add("status", NpgsqlDbType.Varchar).Value = site.Status.ToString().ToLowerInvariant();
        command.Parameters.Add("site_policies", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(site.SitePolicies);
        command.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(site.Metadata);
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = site.CreatedAt;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = site.UpdatedAt;
    }

    private Site MapSite(NpgsqlDataReader reader)
    {
        var statusValue = reader.GetString(reader.GetOrdinal("status"));
        if (!Enum.TryParse(statusValue, true, out SiteStatus status))
        {
            var siteId = reader.GetGuid(reader.GetOrdinal("site_id"));
            _logger.LogWarning("Failed to parse status value '{Status}' for site {SiteId}. Defaulting to Active.", 
                statusValue, siteId);
            status = SiteStatus.Active;
        }

        return Site.Restore(
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("org_id")),
            reader.GetString(reader.GetOrdinal("site_name")),
            reader.GetString(reader.GetOrdinal("site_code")),
            reader.IsDBNull(reader.GetOrdinal("address_line1")) ? null : reader.GetString(reader.GetOrdinal("address_line1")),
            reader.IsDBNull(reader.GetOrdinal("address_line2")) ? null : reader.GetString(reader.GetOrdinal("address_line2")),
            reader.IsDBNull(reader.GetOrdinal("city")) ? null : reader.GetString(reader.GetOrdinal("city")),
            reader.IsDBNull(reader.GetOrdinal("state_province")) ? null : reader.GetString(reader.GetOrdinal("state_province")),
            reader.IsDBNull(reader.GetOrdinal("postal_code")) ? null : reader.GetString(reader.GetOrdinal("postal_code")),
            reader.GetString(reader.GetOrdinal("country")),
            reader.GetString(reader.GetOrdinal("timezone")),
            reader.IsDBNull(reader.GetOrdinal("license_number")) ? null : reader.GetString(reader.GetOrdinal("license_number")),
            reader.IsDBNull(reader.GetOrdinal("license_type")) ? null : reader.GetString(reader.GetOrdinal("license_type")),
            reader.IsDBNull(reader.GetOrdinal("license_expiration")) ? null : reader.GetDateTime(reader.GetOrdinal("license_expiration")),
            reader.GetString(reader.GetOrdinal("site_type")),
            status,
            JsonUtilities.ToDictionary(reader["site_policies"]),
            JsonUtilities.ToDictionary(reader["metadata"]),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")));
    }

    private const string SiteSelect = @"
        SELECT
            site_id,
            org_id,
            site_name,
            site_code,
            address_line1,
            address_line2,
            city,
            state_province,
            postal_code,
            country,
            timezone,
            license_number,
            license_type,
            license_expiration,
            site_type,
            status,
            site_policies,
            metadata,
            created_at,
            updated_at
        FROM sites";
}
