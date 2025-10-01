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

public sealed class EquipmentChannelRepository : IEquipmentChannelRepository
{
    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<EquipmentChannelRepository> _logger;

    public EquipmentChannelRepository(SpatialDbContext dbContext, IRlsContextAccessor rlsContextAccessor, ILogger<EquipmentChannelRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<EquipmentChannel>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
SELECT
    id,
    equipment_id,
    channel_code,
    role,
    port_meta_json,
    enabled,
    assigned_zone_id,
    notes,
    created_at
FROM equipment_channels
WHERE equipment_id = @equipment_id
ORDER BY channel_code;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("equipment_id", NpgsqlDbType.Uuid).Value = equipmentId;

        var results = new List<EquipmentChannel>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapChannel(reader));
        }

        return results;
    }

    public async Task<EquipmentChannel?> GetByIdAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
SELECT
    id,
    equipment_id,
    channel_code,
    role,
    port_meta_json,
    enabled,
    assigned_zone_id,
    notes,
    created_at
FROM equipment_channels
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = channelId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapChannel(reader);
    }

    public async Task<Guid> InsertAsync(EquipmentChannel channel, CancellationToken cancellationToken = default)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO equipment_channels (
    id,
    equipment_id,
    channel_code,
    role,
    port_meta_json,
    enabled,
    assigned_zone_id,
    notes,
    created_at
) VALUES (
    @id,
    @equipment_id,
    @channel_code,
    @role,
    @port_meta_json,
    @enabled,
    @assigned_zone_id,
    @notes,
    @created_at
);";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, channel);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return channel.Id;
    }

    public async Task UpdateAsync(EquipmentChannel channel, CancellationToken cancellationToken = default)
    {
        if (channel == null) throw new ArgumentNullException(nameof(channel));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE equipment_channels SET
    equipment_id = @equipment_id,
    channel_code = @channel_code,
    role = @role,
    port_meta_json = @port_meta_json,
    enabled = @enabled,
    assigned_zone_id = @assigned_zone_id,
    notes = @notes,
    created_at = @created_at
WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, channel);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Equipment channel {channel.Id} not found.");
        }
    }

    public async Task DeleteAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = "DELETE FROM equipment_channels WHERE id = @id;";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = channelId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static EquipmentChannel MapChannel(NpgsqlDataReader reader)
    {
        return EquipmentChannel.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("equipment_id")),
            reader.GetString(reader.GetOrdinal("channel_code")),
            reader.IsDBNull(reader.GetOrdinal("role")) ? null : reader.GetString(reader.GetOrdinal("role")),
            reader.IsDBNull(reader.GetOrdinal("port_meta_json")) ? null : reader.GetString(reader.GetOrdinal("port_meta_json")),
            reader.GetBoolean(reader.GetOrdinal("enabled")),
            reader.IsDBNull(reader.GetOrdinal("assigned_zone_id")) ? null : reader.GetGuid(reader.GetOrdinal("assigned_zone_id")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            reader.GetDateTime(reader.GetOrdinal("created_at"))
        );
    }

    private static void PopulateParameters(NpgsqlCommand command, EquipmentChannel channel)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = channel.Id;
        command.Parameters.Add("equipment_id", NpgsqlDbType.Uuid).Value = channel.EquipmentId;
        command.Parameters.Add("channel_code", NpgsqlDbType.Varchar).Value = channel.ChannelCode;
        command.Parameters.Add("role", NpgsqlDbType.Varchar).Value = (object?)channel.Role ?? DBNull.Value;
        command.Parameters.Add("port_meta_json", NpgsqlDbType.Jsonb).Value = (object?)channel.PortMetaJson ?? DBNull.Value;
        command.Parameters.Add("enabled", NpgsqlDbType.Boolean).Value = channel.Enabled;
        command.Parameters.Add("assigned_zone_id", NpgsqlDbType.Uuid).Value = (object?)channel.AssignedZoneId ?? DBNull.Value;
        command.Parameters.Add("notes", NpgsqlDbType.Text).Value = (object?)channel.Notes ?? DBNull.Value;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = channel.CreatedAt;
    }
}
