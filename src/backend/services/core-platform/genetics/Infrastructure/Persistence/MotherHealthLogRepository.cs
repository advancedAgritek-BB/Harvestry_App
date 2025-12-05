using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL repository for mother plant health logs.
/// </summary>
public sealed class MotherHealthLogRepository : IMotherHealthLogRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<MotherHealthLogRepository> _logger;

    public MotherHealthLogRepository(GeneticsDbContext dbContext, ILogger<MotherHealthLogRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddAsync(MotherHealthLog healthLog, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.mother_health_logs (
                id, site_id, mother_plant_id, log_date, health_status, pest_pressure,
                disease_pressure, nutrient_deficiencies, observations, treatments_applied,
                environmental_notes, photo_urls, logged_by_user_id, created_at
            ) VALUES (
                @Id, @SiteId, @MotherPlantId, @LogDate, @HealthStatus, @PestPressure,
                @DiseasePressure, @NutrientDeficiencies, @Observations, @TreatmentsApplied,
                @EnvironmentalNotes, @PhotoUrls, @LoggedByUserId, @CreatedAt
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, healthLog.SiteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Id", healthLog.Id);
        command.Parameters.AddWithValue("SiteId", healthLog.SiteId);
        command.Parameters.AddWithValue("MotherPlantId", healthLog.MotherPlantId);
        command.Parameters.AddWithValue("LogDate", healthLog.LogDate);
        command.Parameters.AddWithValue("HealthStatus", healthLog.HealthStatus.ToString());
        command.Parameters.AddWithValue("PestPressure", healthLog.PestPressure.ToString());
        command.Parameters.AddWithValue("DiseasePressure", healthLog.DiseasePressure.ToString());
        command.Parameters.AddWithValue("NutrientDeficiencies", healthLog.NutrientDeficiencies);
        command.Parameters.AddWithValue("Observations", healthLog.Observations is null ? DBNull.Value : healthLog.Observations);
        command.Parameters.AddWithValue("TreatmentsApplied", healthLog.TreatmentsApplied is null ? DBNull.Value : healthLog.TreatmentsApplied);
        command.Parameters.AddWithValue("EnvironmentalNotes", healthLog.EnvironmentalNotes is null ? DBNull.Value : healthLog.EnvironmentalNotes);
        command.Parameters.AddWithValue("PhotoUrls", healthLog.PhotoUrls);
        command.Parameters.AddWithValue("LoggedByUserId", healthLog.LoggedByUserId);
        command.Parameters.AddWithValue("CreatedAt", healthLog.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Inserted mother health log {LogId} for mother {MotherPlantId}", healthLog.Id, healthLog.MotherPlantId);
    }

    public async Task<IReadOnlyList<MotherHealthLog>> GetByMotherPlantAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, mother_plant_id, log_date, health_status, pest_pressure,
                   disease_pressure, nutrient_deficiencies, observations, treatments_applied,
                   environmental_notes, photo_urls, logged_by_user_id, created_at
            FROM genetics.mother_health_logs
            WHERE site_id = @SiteId AND mother_plant_id = @MotherPlantId
            ORDER BY log_date DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("MotherPlantId", motherPlantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var logs = new List<MotherHealthLog>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            logs.Add(Map(reader));
        }

        return logs;
    }

    public async Task<MotherHealthLog?> GetLatestAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, mother_plant_id, log_date, health_status, pest_pressure,
                   disease_pressure, nutrient_deficiencies, observations, treatments_applied,
                   environmental_notes, photo_urls, logged_by_user_id, created_at
            FROM genetics.mother_health_logs
            WHERE site_id = @SiteId AND mother_plant_id = @MotherPlantId
            ORDER BY log_date DESC
            LIMIT 1";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SiteId", siteId);
        command.Parameters.AddWithValue("MotherPlantId", motherPlantId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        return Map(reader);
    }

    private static MotherHealthLog Map(NpgsqlDataReader reader)
    {
        var nutrientDeficiencies = reader.IsDBNull(reader.GetOrdinal("nutrient_deficiencies"))
            ? Array.Empty<string>()
            : reader.GetFieldValue<string[]>(reader.GetOrdinal("nutrient_deficiencies"));

        var photoUrls = reader.IsDBNull(reader.GetOrdinal("photo_urls"))
            ? Array.Empty<string>()
            : reader.GetFieldValue<string[]>(reader.GetOrdinal("photo_urls"));

        return MotherHealthLog.FromPersistence(
            reader.GetGuid(reader.GetOrdinal("id")),
            reader.GetGuid(reader.GetOrdinal("site_id")),
            reader.GetGuid(reader.GetOrdinal("mother_plant_id")),
            DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("log_date"))),
            Enum.Parse<HealthStatus>(reader.GetString(reader.GetOrdinal("health_status"))),
            Enum.Parse<PressureLevel>(reader.GetString(reader.GetOrdinal("pest_pressure"))),
            Enum.Parse<PressureLevel>(reader.GetString(reader.GetOrdinal("disease_pressure"))),
            nutrientDeficiencies,
            reader.IsDBNull(reader.GetOrdinal("observations")) ? null : reader.GetString(reader.GetOrdinal("observations")),
            reader.IsDBNull(reader.GetOrdinal("treatments_applied")) ? null : reader.GetString(reader.GetOrdinal("treatments_applied")),
            reader.IsDBNull(reader.GetOrdinal("environmental_notes")) ? null : reader.GetString(reader.GetOrdinal("environmental_notes")),
            photoUrls,
            reader.GetGuid(reader.GetOrdinal("logged_by_user_id")),
            reader.GetDateTime(reader.GetOrdinal("created_at"))
        );
    }
}
