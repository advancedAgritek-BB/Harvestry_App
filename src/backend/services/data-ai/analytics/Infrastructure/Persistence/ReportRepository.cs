using System.Text.Json;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Harvestry.Analytics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Analytics.Infrastructure.Persistence;

public class ReportRepository : IReportRepository
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(AnalyticsDbContext dbContext, ILogger<ReportRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT report_id, name, description, config, visualization_config, is_public, owner_id FROM analytics.reports WHERE report_id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return MapReport(reader);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        // Just return all visible reports (RLS handles visibility)
        command.CommandText = "SELECT report_id, name, description, config, visualization_config, is_public, owner_id FROM analytics.reports ORDER BY updated_at DESC";

        var list = new List<Report>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapReport(reader));
        }
        return list;
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO analytics.reports (report_id, name, description, config, visualization_config, is_public, owner_id, created_at, updated_at, created_by, updated_by)
            VALUES (@id, @name, @desc, @config, @vis, @public, @owner, @created, @updated, @owner, @owner)";

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = report.Id;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = report.Name;
        command.Parameters.Add("desc", NpgsqlDbType.Text).Value = (object?)report.Description ?? DBNull.Value;
        command.Parameters.Add("config", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(report.Config);
        command.Parameters.Add("vis", NpgsqlDbType.Jsonb).Value = report.VisualizationConfigJson;
        command.Parameters.Add("public", NpgsqlDbType.Boolean).Value = report.IsPublic;
        command.Parameters.Add("owner", NpgsqlDbType.Uuid).Value = report.OwnerId;
        command.Parameters.Add("created", NpgsqlDbType.TimestampTz).Value = report.CreatedAt;
        command.Parameters.Add("updated", NpgsqlDbType.TimestampTz).Value = report.UpdatedAt;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE analytics.reports 
            SET name=@name, description=@desc, config=@config, visualization_config=@vis, is_public=@public, updated_at=@updated, updated_by=@owner 
            WHERE report_id=@id";

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = report.Id;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = report.Name;
        command.Parameters.Add("desc", NpgsqlDbType.Text).Value = (object?)report.Description ?? DBNull.Value;
        command.Parameters.Add("config", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(report.Config);
        command.Parameters.Add("vis", NpgsqlDbType.Jsonb).Value = report.VisualizationConfigJson;
        command.Parameters.Add("public", NpgsqlDbType.Boolean).Value = report.IsPublic;
        command.Parameters.Add("updated", NpgsqlDbType.TimestampTz).Value = report.UpdatedAt;
        command.Parameters.Add("owner", NpgsqlDbType.Uuid).Value = report.UpdatedByUserId;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM analytics.reports WHERE report_id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Report MapReport(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var name = reader.GetString(1);
        var desc = reader.IsDBNull(2) ? null : reader.GetString(2);
        var configJson = reader.GetString(3);
        var visJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4);
        var isPublic = reader.GetBoolean(5);
        var ownerId = reader.GetGuid(6);

        var config = JsonSerializer.Deserialize<ReportConfig>(configJson) ?? ReportConfig.Create("");
        
        return Report.FromPersistence(id, name, desc, config, visJson, isPublic, ownerId);
    }
}





