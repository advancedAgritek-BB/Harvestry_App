using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Genetics.Infrastructure.Persistence;

/// <summary>
/// Repository for BatchRelationship operations
/// </summary>
public class BatchRelationshipRepository : IBatchRelationshipRepository
{
    private readonly GeneticsDbContext _dbContext;
    private readonly ILogger<BatchRelationshipRepository> _logger;

    public BatchRelationshipRepository(GeneticsDbContext dbContext, ILogger<BatchRelationshipRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<BatchRelationship> CreateAsync(BatchRelationship relationship, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO genetics.batch_relationships (
                id, site_id, parent_batch_id, child_batch_id, relationship_type,
                plant_count_transferred, transfer_date, notes, created_at, created_by_user_id
            ) VALUES (
                @Id, @SiteId, @ParentBatchId, @ChildBatchId, @RelationshipType,
                @PlantCountTransferred, @TransferDate, @Notes, @CreatedAt, @CreatedByUserId
            )";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, relationship.SiteId, cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", relationship.Id);
        command.Parameters.AddWithValue("SiteId", relationship.SiteId);
        command.Parameters.AddWithValue("ParentBatchId", relationship.ParentBatchId);
        command.Parameters.AddWithValue("ChildBatchId", relationship.ChildBatchId);
        command.Parameters.AddWithValue("RelationshipType", relationship.RelationshipType.ToString());
        command.Parameters.AddWithValue("PlantCountTransferred", (object?)relationship.PlantCountTransferred ?? DBNull.Value);
        command.Parameters.AddWithValue("TransferDate", relationship.TransferDate);
        command.Parameters.AddWithValue("Notes", (object?)relationship.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("CreatedAt", relationship.CreatedAt);
        command.Parameters.AddWithValue("CreatedByUserId", relationship.CreatedByUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogDebug("Created batch relationship {RelationshipId}", relationship.Id);
        return relationship;
    }

    public async Task<IReadOnlyList<BatchRelationship>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, parent_batch_id, child_batch_id, relationship_type,
                   plant_count_transferred, transfer_date, notes, created_at, created_by_user_id
            FROM genetics.batch_relationships
            WHERE (parent_batch_id = @BatchId OR child_batch_id = @BatchId) AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var relationships = new List<BatchRelationship>();
        while (await reader.ReadAsync(cancellationToken))
        {
            relationships.Add(MapToBatchRelationship(reader));
        }

        return relationships;
    }

    public async Task<IReadOnlyList<BatchRelationship>> GetByRelationshipTypeAsync(Guid batchId, RelationshipType relationshipType, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, parent_batch_id, child_batch_id, relationship_type,
                   plant_count_transferred, transfer_date, notes, created_at, created_by_user_id
            FROM genetics.batch_relationships
            WHERE (parent_batch_id = @BatchId OR child_batch_id = @BatchId)
              AND relationship_type = @RelationshipType
              AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("BatchId", batchId);
        command.Parameters.AddWithValue("RelationshipType", relationshipType.ToString());
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var relationships = new List<BatchRelationship>();
        while (await reader.ReadAsync(cancellationToken))
        {
            relationships.Add(MapToBatchRelationship(reader));
        }

        return relationships;
    }

    public async Task<IReadOnlyList<BatchRelationship>> GetSourceRelationshipsAsync(Guid sourceBatchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, parent_batch_id, child_batch_id, relationship_type,
                   plant_count_transferred, transfer_date, notes, created_at, created_by_user_id
            FROM genetics.batch_relationships
            WHERE parent_batch_id = @SourceBatchId AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("SourceBatchId", sourceBatchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var relationships = new List<BatchRelationship>();
        while (await reader.ReadAsync(cancellationToken))
        {
            relationships.Add(MapToBatchRelationship(reader));
        }

        return relationships;
    }

    public async Task<IReadOnlyList<BatchRelationship>> GetTargetRelationshipsAsync(Guid targetBatchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id, site_id, parent_batch_id, child_batch_id, relationship_type,
                   plant_count_transferred, transfer_date, notes, created_at, created_by_user_id
            FROM genetics.batch_relationships
            WHERE child_batch_id = @TargetBatchId AND site_id = @SiteId
            ORDER BY created_at DESC";

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken);
        await _dbContext.SetRlsContextAsync(connection, siteId, cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("TargetBatchId", targetBatchId);
        command.Parameters.AddWithValue("SiteId", siteId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var relationships = new List<BatchRelationship>();
        while (await reader.ReadAsync(cancellationToken))
        {
            relationships.Add(MapToBatchRelationship(reader));
        }

        return relationships;
    }

    private static BatchRelationship MapToBatchRelationship(NpgsqlDataReader reader)
    {
        return BatchRelationship.FromPersistence(
            id: reader.GetGuid(reader.GetOrdinal("id")),
            siteId: reader.GetGuid(reader.GetOrdinal("site_id")),
            parentBatchId: reader.GetGuid(reader.GetOrdinal("parent_batch_id")),
            childBatchId: reader.GetGuid(reader.GetOrdinal("child_batch_id")),
            relationshipType: Enum.Parse<RelationshipType>(reader.GetString(reader.GetOrdinal("relationship_type"))),
            plantCountTransferred: reader.IsDBNull(reader.GetOrdinal("plant_count_transferred")) ? null : reader.GetInt32(reader.GetOrdinal("plant_count_transferred")),
            transferDate: DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("transfer_date"))),
            notes: reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
            createdAt: reader.GetDateTime(reader.GetOrdinal("created_at")),
            createdByUserId: reader.GetGuid(reader.GetOrdinal("created_by_user_id"))
        );
    }
}
