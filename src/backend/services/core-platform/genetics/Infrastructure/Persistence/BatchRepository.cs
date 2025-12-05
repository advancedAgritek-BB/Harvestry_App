using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for Batch aggregate operations with RLS support
/// </summary>
public class BatchRepository : IBatchRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchRepository> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public BatchRepository(
        GeneticsDbContext dbContext,
        ILogger<BatchRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Batch> CreateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        var connection = await PrepareConnectionAsync(batch.SiteId, cancellationToken);

        const string sql = @"
            INSERT INTO genetics.batches (
                id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                parent_batch_id, generation, plant_count, target_plant_count,
                current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                location_id, room_id, zone_id, status, notes, metadata,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @StrainId, @BatchCode, @BatchName, @BatchType, @SourceType,
                @ParentBatchId, @Generation, @PlantCount, @TargetPlantCount,
                @CurrentStageId, @StageStartedAt, @ExpectedHarvestDate, @ActualHarvestDate,
                @LocationId, @RoomId, @ZoneId, @Status, @Notes, @Metadata::jsonb,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )";

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", batch.Id);
        command.Parameters.AddWithValue("SiteId", batch.SiteId);
        command.Parameters.AddWithValue("StrainId", batch.StrainId);
        command.Parameters.AddWithValue("BatchCode", batch.BatchCode.Value);
        command.Parameters.AddWithValue("BatchName", batch.BatchName);
        command.Parameters.AddWithValue("BatchType", batch.BatchType.ToString());
        command.Parameters.AddWithValue("SourceType", batch.SourceType.ToString());
        command.Parameters.AddWithValue("ParentBatchId", (object?)batch.ParentBatchId ?? DBNull.Value);
        command.Parameters.AddWithValue("Generation", batch.Generation);
        command.Parameters.AddWithValue("PlantCount", batch.PlantCount);
        command.Parameters.AddWithValue("TargetPlantCount", batch.TargetPlantCount);
        command.Parameters.AddWithValue("CurrentStageId", batch.CurrentStageId);
        command.Parameters.AddWithValue("StageStartedAt", batch.StageStartedAt);
        command.Parameters.AddWithValue("ExpectedHarvestDate", (object?)batch.ExpectedHarvestDate ?? DBNull.Value);
        command.Parameters.AddWithValue("ActualHarvestDate", (object?)batch.ActualHarvestDate ?? DBNull.Value);
        command.Parameters.AddWithValue("LocationId", (object?)batch.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("RoomId", (object?)batch.RoomId ?? DBNull.Value);
        command.Parameters.AddWithValue("ZoneId", (object?)batch.ZoneId ?? DBNull.Value);
        command.Parameters.AddWithValue("Status", batch.Status.ToString());
        command.Parameters.AddWithValue("Notes", (object?)batch.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("Metadata", JsonSerializer.Serialize(batch.Metadata, JsonOptions));
        command.Parameters.AddWithValue("CreatedAt", batch.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", batch.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", batch.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", batch.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created batch {BatchId} with code {BatchCode}", batch.Id, batch.BatchCode.Value);
        return batch;
    }

    public async Task<Batch?> GetByIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                   parent_batch_id, generation, plant_count, target_plant_count,
                   current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                   location_id, room_id, zone_id, status, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batches
            WHERE id = @BatchId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatch(reader);
    }

    public async Task<IReadOnlyList<Batch>> GetByStrainIdAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                   parent_batch_id, generation, plant_count, target_plant_count,
                   current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                   location_id, room_id, zone_id, status, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batches
            WHERE strain_id = @StrainId AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("StrainId", strainId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var batches = new List<Batch>();
        while (await reader.ReadAsync(cancellationToken))
        {
            batches.Add(MapToBatch(reader));
        }

        return batches;
    }

    public async Task<IReadOnlyList<Batch>> GetByStageIdAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                   parent_batch_id, generation, plant_count, target_plant_count,
                   current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                   location_id, room_id, zone_id, status, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batches
            WHERE current_stage_id = @StageId AND site_id = @SiteId
            ORDER BY stage_started_at";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("StageId", stageId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var batches = new List<Batch>();
        while (await reader.ReadAsync(cancellationToken))
        {
            batches.Add(MapToBatch(reader));
        }

        return batches;
    }

    public async Task<IReadOnlyList<Batch>> GetByStatusAsync(BatchStatus status, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                   parent_batch_id, generation, plant_count, target_plant_count,
                   current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                   location_id, room_id, zone_id, status, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.batches
            WHERE status = @Status AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Status", status.ToString());
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var batches = new List<Batch>();
        while (await reader.ReadAsync(cancellationToken))
        {
            batches.Add(MapToBatch(reader));
        }

        return batches;
    }

    public async Task<IReadOnlyList<Batch>> GetActiveAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(BatchStatus.Active, siteId, cancellationToken);
    }

    public async Task<Batch> UpdateAsync(Batch batch, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.batches SET
                batch_name = @BatchName,
                plant_count = @PlantCount,
                target_plant_count = @TargetPlantCount,
                current_stage_id = @CurrentStageId,
                stage_started_at = @StageStartedAt,
                expected_harvest_date = @ExpectedHarvestDate,
                actual_harvest_date = @ActualHarvestDate,
                location_id = @LocationId,
                room_id = @RoomId,
                zone_id = @ZoneId,
                status = @Status,
                notes = @Notes,
                metadata = @Metadata::jsonb,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE id = @Id AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, batch.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", batch.Id);
        command.Parameters.AddWithValue("SiteId", batch.SiteId);
        command.Parameters.AddWithValue("BatchName", batch.BatchName);
        command.Parameters.AddWithValue("PlantCount", batch.PlantCount);
        command.Parameters.AddWithValue("TargetPlantCount", batch.TargetPlantCount);
        command.Parameters.AddWithValue("CurrentStageId", batch.CurrentStageId);
        command.Parameters.AddWithValue("StageStartedAt", batch.StageStartedAt);
        command.Parameters.AddWithValue("ExpectedHarvestDate", (object?)batch.ExpectedHarvestDate ?? DBNull.Value);
        command.Parameters.AddWithValue("ActualHarvestDate", (object?)batch.ActualHarvestDate ?? DBNull.Value);
        command.Parameters.AddWithValue("LocationId", (object?)batch.LocationId ?? DBNull.Value);
        command.Parameters.AddWithValue("RoomId", (object?)batch.RoomId ?? DBNull.Value);
        command.Parameters.AddWithValue("ZoneId", (object?)batch.ZoneId ?? DBNull.Value);
        command.Parameters.AddWithValue("Status", batch.Status.ToString());
        command.Parameters.AddWithValue("Notes", (object?)batch.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("Metadata", JsonSerializer.Serialize(batch.Metadata, JsonOptions));
        command.Parameters.AddWithValue("UpdatedAt", batch.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", batch.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Updated batch {BatchId}", batch.Id);
        return batch;
    }

    public async Task DeleteAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM genetics.batches WHERE id = @BatchId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Deleted batch {BatchId}", batchId);
    }

    public async Task<IReadOnlyList<Batch>> GetDescendantsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            WITH RECURSIVE descendants AS (
                SELECT id, site_id, strain_id, batch_code, batch_name, batch_type, source_type,
                       parent_batch_id, generation, plant_count, target_plant_count,
                       current_stage_id, stage_started_at, expected_harvest_date, actual_harvest_date,
                       location_id, room_id, zone_id, status, notes, metadata,
                       created_at, created_by_user_id, updated_at, updated_by_user_id
                FROM genetics.batches
                WHERE parent_batch_id = @BatchId AND site_id = @SiteId
                
                UNION ALL
                
                SELECT b.id, b.site_id, b.strain_id, b.batch_code, b.batch_name, b.batch_type, b.source_type,
                       b.parent_batch_id, b.generation, b.plant_count, b.target_plant_count,
                       b.current_stage_id, b.stage_started_at, b.expected_harvest_date, b.actual_harvest_date,
                       b.location_id, b.room_id, b.zone_id, b.status, b.notes, b.metadata,
                       b.created_at, b.created_by_user_id, b.updated_at, b.updated_by_user_id
                FROM genetics.batches b
                INNER JOIN descendants d ON b.parent_batch_id = d.id
                WHERE b.site_id = @SiteId
            )
            SELECT * FROM descendants
            ORDER BY generation, created_at";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var batches = new List<Batch>();
        while (await reader.ReadAsync(cancellationToken))
        {
            batches.Add(MapToBatch(reader));
        }

        return batches;
    }

    public async Task<Batch?> GetParentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT p.id, p.site_id, p.strain_id, p.batch_code, p.batch_name, p.batch_type, p.source_type,
                   p.parent_batch_id, p.generation, p.plant_count, p.target_plant_count,
                   p.current_stage_id, p.stage_started_at, p.expected_harvest_date, p.actual_harvest_date,
                   p.location_id, p.room_id, p.zone_id, p.status, p.notes, p.metadata,
                   p.created_at, p.created_by_user_id, p.updated_at, p.updated_by_user_id
            FROM genetics.batches b
            INNER JOIN genetics.batches p ON b.parent_batch_id = p.id
            WHERE b.id = @BatchId AND b.site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapToBatch(reader);
    }

    public async Task<bool> BatchCodeExistsAsync(string batchCode, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM genetics.batches WHERE batch_code = @BatchCode AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("BatchCode", batchCode);
        command.Parameters.AddWithValue("SiteId", siteId);

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    public async Task<bool> ExistsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM genetics.batches WHERE id = @BatchId AND site_id = @SiteId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        var count = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 0L);
        return count > 0;
    }

    private static Batch MapToBatch(NpgsqlDataReader reader)
    {
        var metadataJson = reader.GetString(reader.GetOrdinal("metadata"));
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson, JsonOptions) 
                       ?? new Dictionary<string, object>();

        return Batch.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            strainId: reader.GetGuid(reader.GetOrdinal("strain_id")),
            batchCode: BatchCode.Create(reader.GetString(reader.GetOrdinal("batch_code"))),
            batchName: reader.GetString(reader.GetOrdinal("batch_name")),
            batchType: Enum.Parse<BatchType>(reader.GetString(reader.GetOrdinal("batch_type"))),
            sourceType: Enum.Parse<BatchSourceType>(reader.GetString(reader.GetOrdinal("source_type"))),
            parentBatchId: reader.IsDBNull(reader.GetOrdinal("parent_batch_id")) ? null : reader.GetGuid(reader.GetOrdinal("parent_batch_id")),
            generation: reader.GetInt32(reader.GetOrdinal("generation")),
            plantCount: reader.GetInt32(reader.GetOrdinal("plant_count")),
            targetPlantCount: reader.GetInt32(reader.GetOrdinal("target_plant_count")),
            currentStageId: reader.GetGuid(reader.GetOrdinal("current_stage_id")),
            stageStartedAt: reader.GetDateTime(reader.GetOrdinal("stage_started_at")),
            expectedHarvestDate: reader.IsDBNull(reader.GetOrdinal("expected_harvest_date")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("expected_harvest_date"))),
            actualHarvestDate: reader.IsDBNull(reader.GetOrdinal("actual_harvest_date")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("actual_harvest_date"))),
            locationId: reader.IsDBNull(reader.GetOrdinal("location_id")) ? null : reader.GetGuid(reader.GetOrdinal("location_id")),
            roomId: reader.IsDBNull(reader.GetOrdinal("room_id")) ? null : reader.GetGuid(reader.GetOrdinal("room_id")),
            zoneId: reader.IsDBNull(reader.GetOrdinal("zone_id")) ? null : reader.GetGuid(reader.GetOrdinal("zone_id")),
            status: Enum.Parse<BatchStatus>(reader.GetString(reader.GetOrdinal("status"))),
            notes: reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            metadata: metadata,
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            updatedAt: reader.GetDateTime(reader.GetOrdinal("updated_at")),
            updatedByUserId: reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
