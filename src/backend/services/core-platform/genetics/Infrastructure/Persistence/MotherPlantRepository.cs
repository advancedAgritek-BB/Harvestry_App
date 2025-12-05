using System.Text.Json;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL-backed repository for mother plant aggregates.
/// </summary>
public sealed class MotherPlantRepository : IMotherPlantRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<MotherPlantRepository> _logger;

    public MotherPlantRepository(GeneticsDbContext dbContext, ILogger<MotherPlantRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddAsync(MotherPlant motherPlant, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.mother_plants (
                id, site_id, batch_id, strain_id, plant_tag, status,
                location_id, room_id, date_established, last_propagation_date,
                propagation_count, max_propagation_count, notes, metadata,
                created_at, created_by_user_id, updated_at, updated_by_user_id
            ) VALUES (
                @Id, @SiteId, @BatchId, @StrainId, @PlantTag, @Status,
                @LocationId, @RoomId, @DateEstablished, @LastPropagationDate,
                @PropagationCount, @MaxPropagationCount, @Notes, @Metadata::jsonb,
                @CreatedAt, @CreatedByUserId, @UpdatedAt, @UpdatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, motherPlant.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", motherPlant.Id);
        command.Parameters.AddWithValue("SiteId", motherPlant.SiteId);
        command.Parameters.AddWithValue("BatchId", motherPlant.BatchId);
        command.Parameters.AddWithValue("StrainId", motherPlant.StrainId);
        command.Parameters.AddWithValue("PlantTag", motherPlant.PlantId.Value);
        command.Parameters.AddWithValue("Status", motherPlant.Status.ToString());
        command.Parameters.AddWithValue("LocationId", motherPlant.LocationId is null ? DBNull.Value : motherPlant.LocationId);
        command.Parameters.AddWithValue("RoomId", motherPlant.RoomId is null ? DBNull.Value : motherPlant.RoomId);
        command.Parameters.AddWithValue("DateEstablished", motherPlant.DateEstablished);
        command.Parameters.AddWithValue("LastPropagationDate", motherPlant.LastPropagationDate is null ? DBNull.Value : motherPlant.LastPropagationDate);
        command.Parameters.AddWithValue("PropagationCount", motherPlant.PropagationCount);
        command.Parameters.AddWithValue("MaxPropagationCount", motherPlant.MaxPropagationCount is null ? DBNull.Value : motherPlant.MaxPropagationCount);
        command.Parameters.AddWithValue("Notes", motherPlant.Notes is null ? DBNull.Value : motherPlant.Notes);
        command.Parameters.AddWithValue("Metadata", JsonSerializer.Serialize(motherPlant.Metadata, SerializerOptions));
        command.Parameters.AddWithValue("CreatedAt", motherPlant.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", motherPlant.CreatedByUserId);
        command.Parameters.AddWithValue("UpdatedAt", motherPlant.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", motherPlant.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Inserted mother plant {MotherPlantId} for site {SiteId}", motherPlant.Id, motherPlant.SiteId);
    }

    public async Task<MotherPlant?> GetByIdAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, strain_id, plant_tag, status,
                   location_id, room_id, date_established, last_propagation_date,
                   propagation_count, max_propagation_count, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.mother_plants
            WHERE site_id = @SiteId AND id = @Id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("Id", motherPlantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task<MotherPlant?> GetByPlantTagAsync(Guid siteId, string plantTag, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, strain_id, plant_tag, status,
                   location_id, room_id, date_established, last_propagation_date,
                   propagation_count, max_propagation_count, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.mother_plants
            WHERE site_id = @SiteId AND lower(plant_tag) = lower(@PlantTag)";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("PlantTag", plantTag);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task<IReadOnlyList<MotherPlant>> GetBySiteAsync(Guid siteId, MotherPlantStatus? status, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT id, site_id, batch_id, strain_id, plant_tag, status,
                   location_id, room_id, date_established, last_propagation_date,
                   propagation_count, max_propagation_count, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.mother_plants
            WHERE site_id = @SiteId";

        if (status.HasValue)
        {
            sql += " AND status = @Status";
        }

        sql += " ORDER BY plant_tag";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        if (status.HasValue)
        {
            command.Parameters.AddWithValue("Status", status.Value.ToString());
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mothers = new List<MotherPlant>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mothers.Add(Map(reader));
        }

        return mothers;
    }

    public async Task<IReadOnlyList<MotherPlant>> GetByStrainAsync(Guid siteId, Guid strainId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, strain_id, plant_tag, status,
                   location_id, room_id, date_established, last_propagation_date,
                   propagation_count, max_propagation_count, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.mother_plants
            WHERE site_id = @SiteId AND strain_id = @StrainId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("StrainId", strainId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mothers = new List<MotherPlant>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mothers.Add(Map(reader));
        }

        return mothers;
    }

    public async Task<IReadOnlyList<MotherPlant>> GetByLocationAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, batch_id, strain_id, plant_tag, status,
                   location_id, room_id, date_established, last_propagation_date,
                   propagation_count, max_propagation_count, notes, metadata,
                   created_at, created_by_user_id, updated_at, updated_by_user_id
            FROM genetics.mother_plants
            WHERE site_id = @SiteId AND location_id = @LocationId";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("LocationId", locationId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mothers = new List<MotherPlant>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mothers.Add(Map(reader));
        }

        return mothers;
    }

    public async Task UpdateAsync(MotherPlant motherPlant, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE genetics.mother_plants SET
                strain_id = @StrainId,
                status = @Status,
                location_id = @LocationId,
                room_id = @RoomId,
                last_propagation_date = @LastPropagationDate,
                propagation_count = @PropagationCount,
                max_propagation_count = @MaxPropagationCount,
                notes = @Notes,
                metadata = @Metadata::jsonb,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE site_id = @SiteId AND id = @Id";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, motherPlant.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", motherPlant.SiteId);
        command.Parameters.AddWithValue("Id", motherPlant.Id);
        command.Parameters.AddWithValue("StrainId", motherPlant.StrainId);
        command.Parameters.AddWithValue("Status", motherPlant.Status.ToString());
        command.Parameters.AddWithValue("LocationId", motherPlant.LocationId is null ? DBNull.Value : motherPlant.LocationId);
        command.Parameters.AddWithValue("RoomId", motherPlant.RoomId is null ? DBNull.Value : motherPlant.RoomId);
        command.Parameters.AddWithValue("LastPropagationDate", motherPlant.LastPropagationDate is null ? DBNull.Value : motherPlant.LastPropagationDate);
        command.Parameters.AddWithValue("PropagationCount", motherPlant.PropagationCount);
        command.Parameters.AddWithValue("MaxPropagationCount", motherPlant.MaxPropagationCount is null ? DBNull.Value : motherPlant.MaxPropagationCount);
        command.Parameters.AddWithValue("Notes", motherPlant.Notes is null ? DBNull.Value : motherPlant.Notes);
        command.Parameters.AddWithValue("Metadata", JsonSerializer.Serialize(motherPlant.Metadata, SerializerOptions));
        command.Parameters.AddWithValue("UpdatedAt", motherPlant.UpdatedAt);
        command.Parameters.AddWithValue("UpdatedByUserId", motherPlant.UpdatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Updated mother plant {MotherPlantId}", motherPlant.Id);
    }

    public async Task UpdatePropagationAsync(Guid siteId, Guid motherPlantId, int newTotalCount, DateOnly lastPropagationDate, int propagatedCount, Guid userId, string? notes, CancellationToken cancellationToken = default)
    {
        const string updateSql = @"
            UPDATE genetics.mother_plants SET
                propagation_count = @PropagationCount,
                last_propagation_date = @LastPropagationDate,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE site_id = @SiteId AND id = @Id";

        const string eventSql = @"
            INSERT INTO genetics.mother_propagation_events (
                id, site_id, mother_plant_id, propagated_count, recorded_on, notes, created_at, created_by_user_id
            ) VALUES (
                @EventId, @SiteId, @MotherPlantId, @PropagatedCount, @RecordedOn, @Notes, @CreatedAt, @CreatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using (var updateCommand = new NpgsqlCommand(updateSql, connection, transaction))
            {
                updateCommand.Parameters.AddWithValue("SiteId", siteId);
                updateCommand.Parameters.AddWithValue("Id", motherPlantId);
                updateCommand.Parameters.AddWithValue("PropagationCount", newTotalCount);
                updateCommand.Parameters.AddWithValue("LastPropagationDate", lastPropagationDate);
                var utcNow = DateTime.UtcNow;
                updateCommand.Parameters.AddWithValue("UpdatedAt", utcNow);
                updateCommand.Parameters.AddWithValue("UpdatedByUserId", userId);

                await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var eventCommand = new NpgsqlCommand(eventSql, connection, transaction))
            {
                eventCommand.Parameters.AddWithValue("EventId", Guid.NewGuid());
                eventCommand.Parameters.AddWithValue("SiteId", siteId);
                eventCommand.Parameters.AddWithValue("MotherPlantId", motherPlantId);
                eventCommand.Parameters.AddWithValue("PropagatedCount", propagatedCount);
                eventCommand.Parameters.AddWithValue("RecordedOn", lastPropagationDate);
                eventCommand.Parameters.AddWithValue("Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);
                eventCommand.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
                eventCommand.Parameters.AddWithValue("CreatedByUserId", userId);

                await eventCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "Failed to update propagation count for mother plant {MotherPlantId}", motherPlantId);
            throw;
        }
    }

    public async Task<bool> TryUpdatePropagationWithLimitsAsync(Guid siteId, Guid motherPlantId, int newTotalCount, DateOnly lastPropagationDate, int propagatedCount, Guid userId, string? notes, int? dailyLimit, int? weeklyLimit, CancellationToken cancellationToken = default)
    {
        const string checkLimitsSql = @"
            SELECT
                (CASE WHEN @DailyLimit IS NOT NULL THEN
                    (SELECT COALESCE(SUM(propagated_count), 0)
                     FROM genetics.mother_propagation_events
                     WHERE site_id = @SiteId AND recorded_on = @Today) + @PropagatedCount
                 ELSE 0 END) > @DailyLimit AS daily_limit_exceeded,
                (CASE WHEN @WeeklyLimit IS NOT NULL THEN
                    (SELECT COALESCE(SUM(propagated_count), 0)
                     FROM genetics.mother_propagation_events
                     WHERE site_id = @SiteId AND recorded_on >= @WeekStart AND recorded_on <= @Today) + @PropagatedCount
                 ELSE 0 END) > @WeeklyLimit AS weekly_limit_exceeded";

        const string updateSql = @"
            UPDATE genetics.mother_plants SET
                propagation_count = @PropagationCount,
                last_propagation_date = @LastPropagationDate,
                updated_at = @UpdatedAt,
                updated_by_user_id = @UpdatedByUserId
            WHERE site_id = @SiteId AND id = @Id";

        const string eventSql = @"
            INSERT INTO genetics.mother_propagation_events (
                id, site_id, mother_plant_id, propagated_count, recorded_on, notes, created_at, created_by_user_id
            ) VALUES (
                @EventId, @SiteId, @MotherPlantId, @PropagatedCount, @RecordedOn, @Notes, @CreatedAt, @CreatedByUserId
            )";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-6);

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Check limits first
            await using (var checkCommand = new NpgsqlCommand(checkLimitsSql, connection, transaction))
            {
                checkCommand.Parameters.AddWithValue("SiteId", siteId);
                checkCommand.Parameters.AddWithValue("Today", today);
                checkCommand.Parameters.AddWithValue("WeekStart", weekStart);
                checkCommand.Parameters.AddWithValue("PropagatedCount", propagatedCount);
                checkCommand.Parameters.AddWithValue("DailyLimit", dailyLimit ?? (object)DBNull.Value);
                checkCommand.Parameters.AddWithValue("WeeklyLimit", weeklyLimit ?? (object)DBNull.Value);

                await using var reader = await checkCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var dailyExceeded = dailyLimit.HasValue && reader.GetInt32(0) == 1;
                    var weeklyExceeded = weeklyLimit.HasValue && reader.GetInt32(1) == 1;

                    if (dailyExceeded || weeklyExceeded)
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                        return false;
                    }
                }
            }

            // Perform the update if limits not exceeded
            await using (var updateCommand = new NpgsqlCommand(updateSql, connection, transaction))
            {
                updateCommand.Parameters.AddWithValue("SiteId", siteId);
                updateCommand.Parameters.AddWithValue("Id", motherPlantId);
                updateCommand.Parameters.AddWithValue("PropagationCount", newTotalCount);
                updateCommand.Parameters.AddWithValue("LastPropagationDate", lastPropagationDate);
                var utcNow = DateTime.UtcNow;
                updateCommand.Parameters.AddWithValue("UpdatedAt", utcNow);
                updateCommand.Parameters.AddWithValue("UpdatedByUserId", userId);

                await updateCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var eventCommand = new NpgsqlCommand(eventSql, connection, transaction))
            {
                eventCommand.Parameters.AddWithValue("EventId", Guid.NewGuid());
                eventCommand.Parameters.AddWithValue("SiteId", siteId);
                eventCommand.Parameters.AddWithValue("MotherPlantId", motherPlantId);
                eventCommand.Parameters.AddWithValue("PropagatedCount", propagatedCount);
                eventCommand.Parameters.AddWithValue("RecordedOn", lastPropagationDate);
                eventCommand.Parameters.AddWithValue("Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes);
                eventCommand.Parameters.AddWithValue("CreatedAt", DateTime.UtcNow);
                eventCommand.Parameters.AddWithValue("CreatedByUserId", userId);

                await eventCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "Failed to atomically update propagation with limits for mother plant {MotherPlantId}", motherPlantId);
            throw;
        }
    }

    public async Task<IReadOnlyList<MotherPlant>> GetOverdueForHealthCheckAsync(Guid siteId, TimeSpan threshold, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            WITH latest_logs AS (
                SELECT mother_plant_id, MAX(log_date) AS last_log_date
                FROM genetics.mother_health_logs
                WHERE site_id = @SiteId
                GROUP BY mother_plant_id
            )
            SELECT mp.id, mp.site_id, mp.batch_id, mp.strain_id, mp.plant_tag, mp.status,
                   mp.location_id, mp.room_id, mp.date_established, mp.last_propagation_date,
                   mp.propagation_count, mp.max_propagation_count, mp.notes, mp.metadata,
                   mp.created_at, mp.created_by_user_id, mp.updated_at, mp.updated_by_user_id
            FROM genetics.mother_plants mp
            LEFT JOIN latest_logs ll ON ll.mother_plant_id = mp.id
            WHERE mp.site_id = @SiteId
              AND (
                    ll.last_log_date IS NULL
                    OR ll.last_log_date < (CURRENT_DATE - @ThresholdDays)
                  )
            ORDER BY mp.plant_tag";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("ThresholdDays", (int)Math.Ceiling(threshold.TotalDays));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var mothers = new List<MotherPlant>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            mothers.Add(Map(reader));
        }

        return mothers;
    }

    public async Task<int> GetPropagationCountForWindowAsync(Guid siteId, DateOnly windowStart, DateOnly windowEnd, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COALESCE(SUM(propagated_count), 0)
            FROM genetics.mother_propagation_events
            WHERE site_id = @SiteId
              AND recorded_on BETWEEN @Start AND @End";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("Start", windowStart);
        command.Parameters.AddWithValue("End", windowEnd);

        var scalar = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(scalar, System.Globalization.CultureInfo.InvariantCulture);
    }

    private MotherPlant Map(NpgsqlDataReader reader)
    {
        var metadataJson = reader.GetString(reader.GetOrdinal("metadata"));
        Dictionary<string, object> metadata;
        
        try
        {
            metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson, SerializerOptions) ?? new Dictionary<string, object>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize metadata JSON for mother plant. JSON: {MetadataJson}", metadataJson);
            metadata = new Dictionary<string, object>();
        }

        return MotherPlant.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("batch_id")),
            reader.GetString(reader.GetOrdinal("plant_tag")),
            reader.GetGuid(reader.GetOrdinal("strain_id")),
            reader.IsDBNull(reader.GetOrdinal("location_id")) ? null : reader.GetGuid(reader.GetOrdinal("location_id")),
            reader.IsDBNull(reader.GetOrdinal("room_id")) ? null : reader.GetGuid(reader.GetOrdinal("room_id")),
            Enum.Parse<MotherPlantStatus>(reader.GetString(reader.GetOrdinal("status"))),
            DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("date_established"))),
            reader.IsDBNull(reader.GetOrdinal("last_propagation_date")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("last_propagation_date"))),
            reader.GetInt32(reader.GetOrdinal("propagation_count")),
            reader.IsDBNull(reader.GetOrdinal("max_propagation_count")) ? null : reader.GetInt32(reader.GetOrdinal("max_propagation_count")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            metadata,
            reader.GetDateTime(reader.GetOrdinal("created_at")),
            reader.GetGuid(reader.GetOrdinal("created_by_user_id")),
            reader.GetDateTime(reader.GetOrdinal("updated_at")),
            reader.GetGuid(reader.GetOrdinal("updated_by_user_id"))
        );
    }
}
