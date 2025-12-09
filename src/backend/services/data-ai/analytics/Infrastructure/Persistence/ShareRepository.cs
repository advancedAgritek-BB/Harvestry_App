using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Analytics.Infrastructure.Persistence;

public class ShareRepository : IShareRepository
{
    private readonly AnalyticsDbContext _dbContext;
    private readonly ILogger<ShareRepository> _logger;

    public ShareRepository(AnalyticsDbContext dbContext, ILogger<ShareRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Share>> GetByResourceAsync(string resourceType, Guid resourceId, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT share_id, resource_type, resource_id, shared_with_id, shared_with_type, permission_level, created_at, created_by FROM analytics.shares WHERE resource_type = @type AND resource_id = @id";
        command.Parameters.Add("type", NpgsqlDbType.Varchar).Value = resourceType;
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = resourceId;

        var list = new List<Share>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(Share.FromPersistence(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetGuid(2),
                reader.GetGuid(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetGuid(7),
                reader.GetDateTime(6)
            ));
        }
        return list;
    }

    public async Task AddAsync(Share share, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO analytics.shares (share_id, resource_type, resource_id, shared_with_id, shared_with_type, permission_level, created_at, created_by)
            VALUES (@id, @type, @resid, @withid, @withtype, @perm, @created, @by)";

        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = share.Id;
        command.Parameters.Add("type", NpgsqlDbType.Varchar).Value = share.ResourceType;
        command.Parameters.Add("resid", NpgsqlDbType.Uuid).Value = share.ResourceId;
        command.Parameters.Add("withid", NpgsqlDbType.Uuid).Value = share.SharedWithId;
        command.Parameters.Add("withtype", NpgsqlDbType.Varchar).Value = share.SharedWithType;
        command.Parameters.Add("perm", NpgsqlDbType.Varchar).Value = share.PermissionLevel;
        command.Parameters.Add("created", NpgsqlDbType.TimestampTz).Value = share.CreatedAt;
        command.Parameters.Add("by", NpgsqlDbType.Uuid).Value = share.CreatedBy;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM analytics.shares WHERE share_id = @id";
        command.Parameters.Add("id", NpgsqlDbType.Uuid).Value = id;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
