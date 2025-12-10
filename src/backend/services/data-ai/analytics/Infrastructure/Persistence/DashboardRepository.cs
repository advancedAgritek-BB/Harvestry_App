using System.Text.Json;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Harvestry.Analytics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Analytics.Infrastructure.Persistence;

public class DashboardRepository : IDashboardRepository
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(AnalyticsDbContext dbContext, ILogger<DashboardRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dashboard?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT dashboard_id, name, description, layout_config, is_public, owner_id FROM analytics.dashboards WHERE dashboard_id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return MapDashboard(reader);
    }

    public async Task<IEnumerable<Dashboard>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT dashboard_id, name, description, layout_config, is_public, owner_id FROM analytics.dashboards ORDER BY updated_at DESC";

        var list = new List<Dashboard>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapDashboard(reader));
        }
        return list;
    }

    public async Task AddAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO analytics.dashboards (dashboard_id, name, description, layout_config, is_public, owner_id, created_at, updated_at, created_by, updated_by)
            VALUES (@id, @name, @desc, @layout, @public, @owner, @created, @updated, @owner, @owner)";

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = dashboard.Id;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = dashboard.Name;
        command.Parameters.Add("desc", NpgsqlDbType.Text).Value = (object?)dashboard.Description ?? DBNull.Value;
        command.Parameters.Add("layout", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(dashboard.LayoutConfig);
        command.Parameters.Add("public", NpgsqlDbType.Boolean).Value = dashboard.IsPublic;
        command.Parameters.Add("owner", NpgsqlDbType.Uuid).Value = dashboard.OwnerId;
        command.Parameters.Add("created", NpgsqlDbType.TimestampTz).Value = dashboard.CreatedAt;
        command.Parameters.Add("updated", NpgsqlDbType.TimestampTz).Value = dashboard.UpdatedAt;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(Dashboard dashboard, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE analytics.dashboards 
            SET name=@name, description=@desc, layout_config=@layout, is_public=@public, updated_at=@updated, updated_by=@owner 
            WHERE dashboard_id=@id";

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = dashboard.Id;
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value = dashboard.Name;
        command.Parameters.Add("desc", NpgsqlDbType.Text).Value = (object?)dashboard.Description ?? DBNull.Value;
        command.Parameters.Add("layout", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(dashboard.LayoutConfig);
        command.Parameters.Add("public", NpgsqlDbType.Boolean).Value = dashboard.IsPublic;
        command.Parameters.Add("updated", NpgsqlDbType.TimestampTz).Value = dashboard.UpdatedAt;
        command.Parameters.Add("owner", NpgsqlDbType.Uuid).Value = dashboard.UpdatedByUserId;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM analytics.dashboards WHERE dashboard_id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Dashboard MapDashboard(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var name = reader.GetString(1);
        var desc = reader.IsDBNull(2) ? null : reader.GetString(2);
        var layoutJson = reader.GetString(3);
        var isPublic = reader.GetBoolean(4);
        var ownerId = reader.GetGuid(5);

        var layout = JsonSerializer.Deserialize<List<DashboardWidget>>(layoutJson) ?? new List<DashboardWidget>();
        
        return Dashboard.FromPersistence(id, name, desc, layout, isPublic, ownerId);
    }
}




