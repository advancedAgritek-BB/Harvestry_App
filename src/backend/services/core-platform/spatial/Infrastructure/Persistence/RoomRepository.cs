using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Spatial.Infrastructure.Persistence;

public sealed class RoomRepository : IRoomRepository
{
    private const string RoomSelect = @"
SELECT
    id,
    site_id,
    code,
    name,
    room_type,
    custom_room_type,
    status,
    description,
    floor_level,
    area_sqft,
    height_ft,
    target_temp_f,
    target_humidity_pct,
    target_co2_ppm,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM rooms";

    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<RoomRepository> _logger;

    public RoomRepository(SpatialDbContext dbContext, IRlsContextAccessor rlsContextAccessor, ILogger<RoomRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Room?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = RoomSelect + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = roomId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapRoom(reader);
    }

    public async Task<Room?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace", nameof(code));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = RoomSelect + " WHERE site_id = @site_id AND code = @code LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = code;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapRoom(reader);
    }

    public async Task<IReadOnlyList<Room>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = RoomSelect + " WHERE site_id = @site_id ORDER BY name;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        var rooms = new List<Room>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rooms.Add(MapRoom(reader));
        }

        return rooms;
    }

    public async Task<Guid> InsertAsync(Room room, CancellationToken cancellationToken = default)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO rooms (
    id,
    site_id,
    code,
    name,
    room_type,
    custom_room_type,
    status,
    description,
    floor_level,
    area_sqft,
    height_ft,
    target_temp_f,
    target_humidity_pct,
    target_co2_ppm,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @code,
    @name,
    @room_type,
    @custom_room_type,
    @status,
    @description,
    @floor_level,
    @area_sqft,
    @height_ft,
    @target_temp_f,
    @target_humidity_pct,
    @target_co2_ppm,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, room);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return room.Id;
    }

    public async Task UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        if (room == null) throw new ArgumentNullException(nameof(room));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE rooms SET
    site_id = @site_id,
    code = @code,
    name = @name,
    room_type = @room_type,
    custom_room_type = @custom_room_type,
    status = @status,
    description = @description,
    floor_level = @floor_level,
    area_sqft = @area_sqft,
    height_ft = @height_ft,
    target_temp_f = @target_temp_f,
    target_humidity_pct = @target_humidity_pct,
    target_co2_ppm = @target_co2_ppm,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, room);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Room {room.Id} not found.");
        }
    }

    public async Task DeleteAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM rooms WHERE id = @id;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = roomId;

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

    private static Room MapRoom(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var siteId = reader.GetGuid(1);
        var code = reader.GetString(2);
        var name = reader.GetString(3);
        
        var roomTypeString = reader.IsDBNull(4) ? null : reader.GetString(4);
        if (!Enum.TryParse<RoomType>(roomTypeString, ignoreCase: true, out var roomType))
        {
            throw new InvalidOperationException($"Invalid RoomType value '{roomTypeString}' found in database for room ID {id}");
        }
        
        var customRoomType = reader.IsDBNull(5) ? null : reader.GetString(5);
        
        var statusString = reader.IsDBNull(6) ? null : reader.GetString(6);
        if (!Enum.TryParse<RoomStatus>(statusString, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException($"Invalid RoomStatus value '{statusString}' found in database for room ID {id}");
        }
        var description = reader.IsDBNull(7) ? null : reader.GetString(7);
        var floorLevel = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8);
        var areaSqft = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9);
        var heightFt = reader.IsDBNull(10) ? (decimal?)null : reader.GetDecimal(10);
        var targetTempF = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11);
        var targetHumidityPct = reader.IsDBNull(12) ? (decimal?)null : reader.GetDecimal(12);
        var targetCo2Ppm = reader.IsDBNull(13) ? (int?)null : reader.GetInt32(13);
        var createdAt = reader.GetDateTime(14);
        var createdBy = reader.GetGuid(15);
        var updatedAt = reader.GetDateTime(16);
        var updatedBy = reader.GetGuid(17);

        return Room.FromPersistence(
            id,
            siteId,
            code,
            name,
            roomType,
            customRoomType,
            status,
            description,
            floorLevel,
            areaSqft,
            heightFt,
            targetTempF,
            targetHumidityPct,
            targetCo2Ppm,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy);
    }

    private static void PopulateParameters(NpgsqlCommand command, Room room)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = room.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = room.SiteId;
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = room.Code;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = room.Name;
        command.Parameters.Add("room_type", NpgsqlDbType.Unknown).Value = room.RoomType.ToString();
        command.Parameters.Add("custom_room_type", NpgsqlDbType.Varchar).Value = (object?)room.CustomRoomType ?? DBNull.Value;
        command.Parameters.Add("status", NpgsqlDbType.Unknown).Value = room.Status.ToString();
        command.Parameters.Add("description", NpgsqlDbType.Text).Value = (object?)room.Description ?? DBNull.Value;
        command.Parameters.Add("floor_level", NpgsqlDbType.Integer).Value = (object?)room.FloorLevel ?? DBNull.Value;
        command.Parameters.Add("area_sqft", NpgsqlDbType.Numeric).Value = (object?)room.AreaSqft ?? DBNull.Value;
        command.Parameters.Add("height_ft", NpgsqlDbType.Numeric).Value = (object?)room.HeightFt ?? DBNull.Value;
        command.Parameters.Add("target_temp_f", NpgsqlDbType.Numeric).Value = (object?)room.TargetTempF ?? DBNull.Value;
        command.Parameters.Add("target_humidity_pct", NpgsqlDbType.Numeric).Value = (object?)room.TargetHumidityPct ?? DBNull.Value;
        command.Parameters.Add("target_co2_ppm", NpgsqlDbType.Integer).Value = (object?)room.TargetCo2Ppm ?? DBNull.Value;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = room.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = room.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = room.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = room.UpdatedByUserId;
    }
}
