using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Integration.Growlink.Infrastructure.Persistence;

/// <summary>
/// Repository for Growlink stream mappings using direct SQL.
/// </summary>
public sealed class GrowlinkStreamMappingRepository : IGrowlinkStreamMappingRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<GrowlinkStreamMappingRepository> _logger;

    public GrowlinkStreamMappingRepository(
        NpgsqlDataSource dataSource,
        ILogger<GrowlinkStreamMappingRepository> logger)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<GrowlinkStreamMapping>> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, growlink_device_id, growlink_sensor_id,
                   growlink_sensor_name, growlink_sensor_type, harvestry_stream_id,
                   is_active, auto_created, created_at, updated_at
            FROM growlink_stream_mappings
            WHERE site_id = @SiteId AND is_active = true";

        var mappings = new List<GrowlinkStreamMapping>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            mappings.Add(MapToStreamMapping(reader));
        }

        return mappings;
    }

    public async Task<GrowlinkStreamMapping?> GetByGrowlinkSensorAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, growlink_device_id, growlink_sensor_id,
                   growlink_sensor_name, growlink_sensor_type, harvestry_stream_id,
                   is_active, auto_created, created_at, updated_at
            FROM growlink_stream_mappings
            WHERE site_id = @SiteId
              AND growlink_device_id = @DeviceId
              AND growlink_sensor_id = @SensorId";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SiteId", siteId);
        command.Parameters.AddWithValue("@DeviceId", growlinkDeviceId);
        command.Parameters.AddWithValue("@SensorId", growlinkSensorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToStreamMapping(reader);
    }

    public async Task<GrowlinkStreamMapping?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, growlink_device_id, growlink_sensor_id,
                   growlink_sensor_name, growlink_sensor_type, harvestry_stream_id,
                   is_active, auto_created, created_at, updated_at
            FROM growlink_stream_mappings
            WHERE id = @Id";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToStreamMapping(reader);
    }

    public async Task CreateAsync(
        GrowlinkStreamMapping mapping,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO growlink_stream_mappings
                (id, site_id, growlink_device_id, growlink_sensor_id,
                 growlink_sensor_name, growlink_sensor_type, harvestry_stream_id,
                 is_active, auto_created, created_at, updated_at)
            VALUES
                (@Id, @SiteId, @DeviceId, @SensorId,
                 @SensorName, @SensorType, @StreamId,
                 @IsActive, @AutoCreated, @CreatedAt, @UpdatedAt)";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        AddMappingParameters(command, mapping);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogDebug("Created Growlink stream mapping {Id}", mapping.Id);
    }

    public async Task CreateManyAsync(
        IEnumerable<GrowlinkStreamMapping> mappings,
        CancellationToken cancellationToken = default)
    {
        var mappingList = mappings.ToList();
        if (mappingList.Count == 0)
            return;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            const string sql = @"
                INSERT INTO growlink_stream_mappings
                    (id, site_id, growlink_device_id, growlink_sensor_id,
                     growlink_sensor_name, growlink_sensor_type, harvestry_stream_id,
                     is_active, auto_created, created_at, updated_at)
                VALUES
                    (@Id, @SiteId, @DeviceId, @SensorId,
                     @SensorName, @SensorType, @StreamId,
                     @IsActive, @AutoCreated, @CreatedAt, @UpdatedAt)";

            foreach (var mapping in mappingList)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                AddMappingParameters(command, mapping);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogDebug("Created {Count} Growlink stream mappings", mappingList.Count);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(
        GrowlinkStreamMapping mapping,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE growlink_stream_mappings
            SET harvestry_stream_id = @StreamId,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Id", mapping.Id);
        command.Parameters.AddWithValue("@StreamId", mapping.HarvestryStreamId);
        command.Parameters.AddWithValue("@IsActive", mapping.IsActive);
        command.Parameters.AddWithValue("@UpdatedAt", DateTimeOffset.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM growlink_stream_mappings WHERE id = @Id";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogDebug("Deleted Growlink stream mapping {Id}", id);
    }

    public async Task DeleteBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM growlink_stream_mappings WHERE site_id = @SiteId";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SiteId", siteId);

        var deleted = await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogDebug("Deleted {Count} Growlink stream mappings for site {SiteId}", deleted, siteId);
    }

    private static void AddMappingParameters(NpgsqlCommand command, GrowlinkStreamMapping mapping)
    {
        command.Parameters.AddWithValue("@Id", mapping.Id);
        command.Parameters.AddWithValue("@SiteId", mapping.SiteId);
        command.Parameters.AddWithValue("@DeviceId", mapping.GrowlinkDeviceId);
        command.Parameters.AddWithValue("@SensorId", mapping.GrowlinkSensorId);
        command.Parameters.AddWithValue("@SensorName", mapping.GrowlinkSensorName);
        command.Parameters.AddWithValue("@SensorType", mapping.GrowlinkSensorType);
        command.Parameters.AddWithValue("@StreamId", mapping.HarvestryStreamId);
        command.Parameters.AddWithValue("@IsActive", mapping.IsActive);
        command.Parameters.AddWithValue("@AutoCreated", mapping.AutoCreated);
        command.Parameters.AddWithValue("@CreatedAt", mapping.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", mapping.UpdatedAt);
    }

    private static GrowlinkStreamMapping MapToStreamMapping(NpgsqlDataReader reader)
    {
        return GrowlinkStreamMapping.FromPersistence(
            id: reader.GetGuid(0),
            siteId: reader.GetGuid(1),
            growlinkDeviceId: reader.GetString(2),
            growlinkSensorId: reader.GetString(3),
            growlinkSensorName: reader.GetString(4),
            growlinkSensorType: reader.GetString(5),
            harvestryStreamId: reader.GetGuid(6),
            isActive: reader.GetBoolean(7),
            autoCreated: reader.GetBoolean(8),
            createdAt: reader.GetFieldValue<DateTimeOffset>(9),
            updatedAt: reader.GetFieldValue<DateTimeOffset>(10));
    }
}
