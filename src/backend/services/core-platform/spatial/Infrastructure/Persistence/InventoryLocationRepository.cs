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

public sealed class InventoryLocationRepository : IInventoryLocationRepository
{
    private const string LocationSelect = @"
SELECT
    id,
    site_id,
    room_id,
    parent_id,
    location_type,
    code,
    name,
    barcode,
    path,
    depth,
    status,
    length_ft,
    width_ft,
    height_ft,
    plant_capacity,
    current_plant_count,
    row_number,
    column_number,
    weight_capacity_lbs,
    current_weight_lbs,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM inventory_locations";

    private readonly SpatialDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<InventoryLocationRepository> _logger;

    public InventoryLocationRepository(
        SpatialDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<InventoryLocationRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InventoryLocation?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = LocationSelect + " WHERE id = @id LIMIT 1;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = locationId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapLocation(reader);
    }

    public async Task<InventoryLocation?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace", nameof(code));
        }

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = LocationSelect + " WHERE site_id = @site_id AND code = @code LIMIT 1;";
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = code;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return MapLocation(reader);
    }

    public async Task<IReadOnlyList<InventoryLocation>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = LocationSelect + " WHERE parent_id = @parent_id ORDER BY name;";
        command.Parameters.Add("parent_id", NpgsqlDbType.Uuid).Value = parentId;

        return await ReadLocationsAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<InventoryLocation>> GetDescendantsAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
WITH RECURSIVE descendants AS (
    SELECT * FROM inventory_locations WHERE parent_id = @root_id
    UNION ALL
    SELECT il.* FROM inventory_locations il
    INNER JOIN descendants d ON il.parent_id = d.id
)
SELECT
    id,
    site_id,
    room_id,
    parent_id,
    location_type,
    code,
    name,
    barcode,
    path,
    depth,
    status,
    length_ft,
    width_ft,
    height_ft,
    plant_capacity,
    current_plant_count,
    row_number,
    column_number,
    weight_capacity_lbs,
    current_weight_lbs,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM descendants
ORDER BY depth;
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("root_id", NpgsqlDbType.Uuid).Value = locationId;

        return await ReadLocationsAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<InventoryLocation>> GetByRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = LocationSelect + " WHERE room_id = @room_id ORDER BY depth, name;";
        command.Parameters.Add("room_id", NpgsqlDbType.Uuid).Value = roomId;

        return await ReadLocationsAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<InventoryLocation>> GetPathAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
WITH RECURSIVE path_nodes AS (
    SELECT * FROM inventory_locations WHERE id = @location_id
    UNION ALL
    SELECT parent.* FROM inventory_locations parent
    INNER JOIN path_nodes child ON child.parent_id = parent.id
)
SELECT
    id,
    site_id,
    room_id,
    parent_id,
    location_type,
    code,
    name,
    barcode,
    path,
    depth,
    status,
    length_ft,
    width_ft,
    height_ft,
    plant_capacity,
    current_plant_count,
    row_number,
    column_number,
    weight_capacity_lbs,
    current_weight_lbs,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
FROM path_nodes;
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("location_id", NpgsqlDbType.Uuid).Value = locationId;

        return await ReadLocationsAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid> InsertAsync(InventoryLocation location, CancellationToken cancellationToken = default)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
INSERT INTO inventory_locations (
    id,
    site_id,
    room_id,
    parent_id,
    location_type,
    code,
    name,
    barcode,
    path,
    depth,
    status,
    length_ft,
    width_ft,
    height_ft,
    plant_capacity,
    current_plant_count,
    row_number,
    column_number,
    weight_capacity_lbs,
    current_weight_lbs,
    notes,
    metadata_json,
    created_at,
    created_by_user_id,
    updated_at,
    updated_by_user_id
) VALUES (
    @id,
    @site_id,
    @room_id,
    @parent_id,
    @location_type,
    @code,
    @name,
    @barcode,
    @path,
    @depth,
    @status,
    @length_ft,
    @width_ft,
    @height_ft,
    @plant_capacity,
    @current_plant_count,
    @row_number,
    @column_number,
    @weight_capacity_lbs,
    @current_weight_lbs,
    @notes,
    @metadata_json,
    @created_at,
    @created_by_user_id,
    @updated_at,
    @updated_by_user_id);
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, location);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return location.Id;
    }

    public async Task UpdateAsync(InventoryLocation location, CancellationToken cancellationToken = default)
    {
        if (location == null) throw new ArgumentNullException(nameof(location));

        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
UPDATE inventory_locations SET
    site_id = @site_id,
    room_id = @room_id,
    parent_id = @parent_id,
    location_type = @location_type,
    code = @code,
    name = @name,
    barcode = @barcode,
    path = @path,
    depth = @depth,
    status = @status,
    length_ft = @length_ft,
    width_ft = @width_ft,
    height_ft = @height_ft,
    plant_capacity = @plant_capacity,
    current_plant_count = @current_plant_count,
    row_number = @row_number,
    column_number = @column_number,
    weight_capacity_lbs = @weight_capacity_lbs,
    current_weight_lbs = @current_weight_lbs,
    notes = @notes,
    metadata_json = @metadata_json,
    updated_at = @updated_at,
    updated_by_user_id = @updated_by_user_id
WHERE id = @id;
";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        PopulateParameters(command, location);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException($"Location {location.Id} not found.");
        }
    }

    public async Task DeleteAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM inventory_locations WHERE id = @id;";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = locationId;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<InventoryLocation>> ReadLocationsAsync(NpgsqlCommand command, CancellationToken cancellationToken)
    {
        var results = new List<InventoryLocation>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(MapLocation(reader));
        }

        return results;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(context.UserId, role, context.SiteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static InventoryLocation MapLocation(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var siteId = reader.GetGuid(1);
        var roomId = reader.IsDBNull(2) ? (Guid?)null : reader.GetGuid(2);
        var parentId = reader.IsDBNull(3) ? (Guid?)null : reader.GetGuid(3);
        
        var locationTypeString = reader.IsDBNull(4) ? null : reader.GetString(4);
        if (!Enum.TryParse<LocationType>(locationTypeString, ignoreCase: true, out var locationType))
        {
            throw new InvalidOperationException($"Invalid LocationType value '{locationTypeString}' found in database for location ID {id}");
        }
        
        var code = reader.GetString(5);
        var name = reader.GetString(6);
        var barcode = reader.IsDBNull(7) ? null : reader.GetString(7);
        var path = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
        var depth = reader.GetInt32(9);
        
        var statusString = reader.IsDBNull(10) ? null : reader.GetString(10);
        if (!Enum.TryParse<LocationStatus>(statusString, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException($"Invalid LocationStatus value '{statusString}' found in database for location ID {id}");
        }
        var lengthFt = reader.IsDBNull(11) ? (decimal?)null : reader.GetDecimal(11);
        var widthFt = reader.IsDBNull(12) ? (decimal?)null : reader.GetDecimal(12);
        var heightFt = reader.IsDBNull(13) ? (decimal?)null : reader.GetDecimal(13);
        var plantCapacity = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14);
        var currentPlantCount = reader.IsDBNull(15) ? 0 : reader.GetInt32(15);
        var rowNumber = reader.IsDBNull(16) ? (int?)null : reader.GetInt32(16);
        var columnNumber = reader.IsDBNull(17) ? (int?)null : reader.GetInt32(17);
        var weightCapacity = reader.IsDBNull(18) ? (decimal?)null : reader.GetDecimal(18);
        var currentWeight = reader.IsDBNull(19) ? 0m : reader.GetDecimal(19);
        var notes = reader.IsDBNull(20) ? null : reader.GetString(20);
        var metadata = reader.IsDBNull(21) ? null : reader.GetString(21);
        var createdAt = reader.GetDateTime(22);
        var createdBy = reader.GetGuid(23);
        var updatedAt = reader.GetDateTime(24);
        var updatedBy = reader.GetGuid(25);

        return InventoryLocation.FromPersistence(
            id,
            siteId,
            roomId,
            parentId,
            locationType,
            code,
            name,
            barcode,
            path,
            depth,
            status,
            lengthFt,
            widthFt,
            heightFt,
            plantCapacity,
            currentPlantCount,
            rowNumber,
            columnNumber,
            weightCapacity,
            currentWeight,
            notes,
            metadata,
            createdAt,
            createdBy,
            updatedAt,
            updatedBy);
    }

    private static void PopulateParameters(NpgsqlCommand command, InventoryLocation location)
    {
        command.Parameters.Clear();
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = location.Id;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = location.SiteId;
        command.Parameters.Add("room_id", NpgsqlDbType.Uuid).Value = (object?)location.RoomId ?? DBNull.Value;
        command.Parameters.Add("parent_id", NpgsqlDbType.Uuid).Value = (object?)location.ParentId ?? DBNull.Value;
        command.Parameters.Add("location_type", NpgsqlDbType.Unknown).Value = location.LocationType.ToString();
        command.Parameters.Add("code", NpgsqlDbType.Varchar).Value = location.Code;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = location.Name;
        command.Parameters.Add("barcode", NpgsqlDbType.Varchar).Value = (object?)location.Barcode ?? DBNull.Value;
        command.Parameters.Add("path", NpgsqlDbType.Varchar).Value = location.Path;
        command.Parameters.Add("depth", NpgsqlDbType.Integer).Value = location.Depth;
        command.Parameters.Add("status", NpgsqlDbType.Unknown).Value = location.Status.ToString();
        command.Parameters.Add("length_ft", NpgsqlDbType.Numeric).Value = (object?)location.LengthFt ?? DBNull.Value;
        command.Parameters.Add("width_ft", NpgsqlDbType.Numeric).Value = (object?)location.WidthFt ?? DBNull.Value;
        command.Parameters.Add("height_ft", NpgsqlDbType.Numeric).Value = (object?)location.HeightFt ?? DBNull.Value;
        command.Parameters.Add("plant_capacity", NpgsqlDbType.Integer).Value = (object?)location.PlantCapacity ?? DBNull.Value;
        command.Parameters.Add("current_plant_count", NpgsqlDbType.Integer).Value = location.CurrentPlantCount;
        command.Parameters.Add("row_number", NpgsqlDbType.Integer).Value = (object?)location.RowNumber ?? DBNull.Value;
        command.Parameters.Add("column_number", NpgsqlDbType.Integer).Value = (object?)location.ColumnNumber ?? DBNull.Value;
        command.Parameters.Add("weight_capacity_lbs", NpgsqlDbType.Numeric).Value = (object?)location.WeightCapacityLbs ?? DBNull.Value;
        command.Parameters.Add("current_weight_lbs", NpgsqlDbType.Numeric).Value = location.CurrentWeightLbs;
        command.Parameters.Add("notes", NpgsqlDbType.Text).Value = (object?)location.Notes ?? DBNull.Value;
        command.Parameters.Add("metadata_json", NpgsqlDbType.Jsonb).Value = location.MetadataJson is null ? DBNull.Value : (object)location.MetadataJson;
        command.Parameters.Add("created_at", NpgsqlDbType.TimestampTz).Value = location.CreatedAt;
        command.Parameters.Add("created_by_user_id", NpgsqlDbType.Uuid).Value = location.CreatedByUserId;
        command.Parameters.Add("updated_at", NpgsqlDbType.TimestampTz).Value = location.UpdatedAt;
        command.Parameters.Add("updated_by_user_id", NpgsqlDbType.Uuid).Value = location.UpdatedByUserId;
    }
}
