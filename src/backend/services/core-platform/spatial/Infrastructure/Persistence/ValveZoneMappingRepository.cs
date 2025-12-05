using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Spatial.Infrastructure.Persistence;

public sealed class ValveZoneMappingRepository : IValveZoneMappingRepository
{
    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<ValveZoneMappingRepository> _logger;

    private const string BaseSelect = @"
SELECT
    id,
    site_id,
    valve_equipment_id,
    valve_channel_code,
    zone_location_id,
    priority,
    normally_open,
    interlock_group,
    enabled,
    notes,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM valve_zone_mappings";

    public ValveZoneMappingRepository(
        SpatialDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<ValveZoneMappingRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> InsertAsync(ValveZoneMapping mapping, CancellationToken cancellationToken = default)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO valve_zone_mappings (
    id,
    site_id,
    valve_equipment_id,
    valve_channel_code,
    zone_location_id,
    priority,
    normally_open,
    interlock_group,
    enabled,
    notes,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @valve_equipment_id,
    @valve_channel_code,
    @zone_location_id,
    @priority,
    @normally_open,
    @interlock_group,
    @enabled,
    @notes,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id
);";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, mapping);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return mapping.Id;
    }

    public async Task UpdateAsync(ValveZoneMapping mapping, CancellationToken cancellationToken = default)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE valve_zone_mappings
SET
    valve_channel_code = @valve_channel_code,
    zone_location_id = @zone_location_id,
    priority = @priority,
    normally_open = @normally_open,
    interlock_group = @interlock_group,
    enabled = @enabled,
    notes = @notes,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, mapping);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (affectedRows == 0)
        {
            throw new InvalidOperationException($"Valve zone mapping {mapping.Id} not found.");
        }
    }

    public async Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM valve_zone_mappings WHERE id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = mappingId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ValveZoneMapping?> GetByIdAsync(Guid mappingId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " WHERE id = @id LIMIT 1";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = mappingId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapMapping(reader);
    }

    public async Task<IReadOnlyList<ValveZoneMapping>> GetByValveAsync(Guid valveEquipmentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " WHERE valve_equipment_id = @equipmentId ORDER BY priority, created_at";
        command.Parameters.Add("equipmentId", NpgsqlDbType.Uuid).Value = valveEquipmentId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mappings = new List<ValveZoneMapping>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mappings.Add(MapMapping(reader));
        }

        return mappings;
    }

    public async Task<IReadOnlyList<ValveZoneMapping>> GetByZoneAsync(Guid zoneLocationId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = BaseSelect + " WHERE zone_location_id = @zoneId ORDER BY priority, created_at";
        command.Parameters.Add("zoneId", NpgsqlDbType.Uuid).Value = zoneLocationId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mappings = new List<ValveZoneMapping>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mappings.Add(MapMapping(reader));
        }

        return mappings;
    }

    public async Task<bool> AnyWithInterlockAsync(Guid siteId, string interlockGroup, CancellationToken cancellationToken = default)
    {
        if (interlockGroup == null)
        {
            throw new ArgumentNullException(nameof(interlockGroup));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);
        var trimmedGroup = interlockGroup.Trim();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM valve_zone_mappings WHERE site_id = @siteId AND interlock_group = @group AND enabled = true)";
        command.Parameters.Add("siteId", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("group", NpgsqlDbType.Varchar).Value = trimmedGroup;

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is bool value && value;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static ValveZoneMapping MapMapping(NpgsqlDataReader reader)
    {
        return ValveZoneMapping.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("valve_equipment_id")),
            reader.IsDBNull(reader.GetOrdinal("valve_channel_code")) ? null : reader.GetString(reader.GetOrdinal("valve_channel_code")),
            reader.GetGuid(reader.GetOrdinal("zone_location_id")),
            reader.GetInt32(reader.GetOrdinal("priority")),
            reader.GetBoolean(reader.GetOrdinal("normally_open")),
            reader.IsDBNull(reader.GetOrdinal("interlock_group")) ? null : reader.GetString(reader.GetOrdinal("interlock_group")),
            reader.GetBoolean(reader.GetOrdinal("enabled")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")),
            reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }

    private static void PopulateParameters(NpgsqlCommand command, ValveZoneMapping mapping)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = mapping.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = mapping.SiteId;
        command.Parameters.Add("valve_equipment_id", NpgsqlDbType.Uuid).Value = mapping.ValveEquipmentId;
        command.Parameters.Add("valve_channel_code", NpgsqlDbType.Varchar).Value = (object?)mapping.ValveChannelCode ?? DBNull.Value;
        command.Parameters.Add("zone_location_id", NpgsqlDbType.Uuid).Value = mapping.ZoneLocationId;
        command.Parameters.Add("priority", NpgsqlDbType.Integer).Value = mapping.Priority;
        command.Parameters.Add("normally_open", NpgsqlDbType.Boolean).Value = mapping.NormallyOpen;
        command.Parameters.Add("interlock_group", NpgsqlDbType.Varchar).Value = (object?)mapping.InterlockGroup ?? DBNull.Value;
        command.Parameters.Add("enabled", NpgsqlDbType.Boolean).Value = mapping.Enabled;
        command.Parameters.Add("notes", NpgsqlDbType.Text).Value = (object?)mapping.Notes ?? DBNull.Value;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = mapping.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = mapping.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = mapping.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = mapping.UpdatedByUserId;
    }
}
