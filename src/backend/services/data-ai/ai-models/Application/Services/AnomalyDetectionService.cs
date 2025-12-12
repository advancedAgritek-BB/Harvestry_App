using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Orchestrates anomaly detection across different domains and manages anomaly results.
/// </summary>
public sealed class AnomalyDetectionService : IAnomalyDetectionService
{
    private readonly IGraphRepository _graphRepository;
    private readonly IMovementAnomalyDetector _movementDetector;
    private readonly IIrrigationAnomalyDetector _irrigationDetector;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<AnomalyDetectionService> _logger;

    public AnomalyDetectionService(
        IGraphRepository graphRepository,
        IMovementAnomalyDetector movementDetector,
        IIrrigationAnomalyDetector irrigationDetector,
        NpgsqlDataSource dataSource,
        ILogger<AnomalyDetectionService> logger)
    {
        _graphRepository = graphRepository;
        _movementDetector = movementDetector;
        _irrigationDetector = irrigationDetector;
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<AnomalyBatchResult> ScoreAnomaliesAsync(
        Guid siteId,
        GraphNodeType nodeType,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("Scoring anomalies for site {SiteId}, type {NodeType}", siteId, nodeType);

        IReadOnlyList<AnomalyResult> results = nodeType switch
        {
            GraphNodeType.InventoryMovement => 
                await _movementDetector.DetectMovementAnomaliesAsync(siteId, null, cancellationToken),
            GraphNodeType.IrrigationRun => 
                await _irrigationDetector.DetectIrrigationAnomaliesAsync(siteId, null, cancellationToken),
            _ => Array.Empty<AnomalyResult>()
        };

        // Store results in database
        await StoreAnomalyResultsAsync(results, cancellationToken);

        return new AnomalyBatchResult
        {
            SiteId = siteId,
            NodeType = nodeType,
            TotalScored = results.Count,
            AnomaliesDetected = results.Count(r => r.Score >= 0.5f),
            ThresholdUsed = 0.5f,
            Duration = DateTime.UtcNow - startedAt,
            ModelVersion = results.FirstOrDefault()?.ModelVersion ?? "unknown"
        };
    }

    public async Task<AnomalyResult> ScoreNodeAnomalyAsync(
        string nodeId,
        CancellationToken cancellationToken = default)
    {
        var node = await _graphRepository.GetNodeAsync(nodeId, cancellationToken);
        if (node == null)
        {
            throw new InvalidOperationException($"Node not found: {nodeId}");
        }

        return node.NodeType switch
        {
            GraphNodeType.InventoryMovement => 
                await _movementDetector.ScoreMovementAsync(node.SourceEntityId, cancellationToken),
            GraphNodeType.IrrigationRun => 
                await _irrigationDetector.ScoreIrrigationRunAsync(node.SourceEntityId, cancellationToken),
            _ => new AnomalyResult
            {
                SiteId = node.SiteId,
                NodeId = nodeId,
                NodeType = node.NodeType,
                AnomalyType = "unsupported",
                Score = 0f,
                Explanation = "Anomaly detection not supported for this node type",
                FeatureAttributions = new Dictionary<string, float>(),
                DetectedAt = DateTime.UtcNow,
                ModelVersion = "N/A"
            }
        };
    }

    public async Task<IReadOnlyList<AnomalyResult>> ScoreIncrementalAsync(
        Guid siteId,
        IEnumerable<string> nodeIds,
        CancellationToken cancellationToken = default)
    {
        var results = new List<AnomalyResult>();

        foreach (var nodeId in nodeIds)
        {
            try
            {
                var result = await ScoreNodeAnomalyAsync(nodeId, cancellationToken);
                if (result.Score >= 0.5f)
                {
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error scoring node {NodeId}", nodeId);
            }
        }

        // Store results
        await StoreAnomalyResultsAsync(results, cancellationToken);

        return results;
    }

    public async Task<IReadOnlyList<AnomalyResult>> GetTopAnomaliesAsync(
        Guid siteId,
        int limit = 50,
        GraphNodeType? nodeType = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<AnomalyResult>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT 
                id, site_id, node_id, edge_id, anomaly_type,
                score, explanation, feature_attributions, model_version,
                detected_at, acknowledged_at
            FROM ml.anomaly_results
            WHERE site_id = @siteId
              AND acknowledged_at IS NULL";

        if (nodeType.HasValue)
        {
            sql += " AND node_id LIKE @nodeTypePrefix";
        }

        sql += " ORDER BY score DESC LIMIT @limit";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        cmd.Parameters.AddWithValue("limit", limit);
        
        if (nodeType.HasValue)
        {
            cmd.Parameters.AddWithValue("nodeTypePrefix", $"{nodeType.Value}:%");
        }

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var nodeId = reader.IsDBNull(2) ? "" : reader.GetString(2);
                var (parsedNodeType, _) = TryParseNodeId(nodeId);

                results.Add(new AnomalyResult
                {
                    Id = reader.GetGuid(0),
                    SiteId = reader.GetGuid(1),
                    NodeId = nodeId,
                    EdgeId = reader.IsDBNull(3) ? null : reader.GetString(3),
                    NodeType = parsedNodeType,
                    AnomalyType = reader.GetString(4),
                    Score = reader.GetFloat(5),
                    Explanation = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    FeatureAttributions = new Dictionary<string, float>(), // Would deserialize from JSON
                    ModelVersion = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    DetectedAt = reader.GetDateTime(9),
                    IsAcknowledged = !reader.IsDBNull(10)
                });
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01") // Table not found
        {
            _logger.LogWarning("Anomaly results table not found, returning empty list");
        }

        return results;
    }

    public async Task AcknowledgeAnomalyAsync(
        Guid anomalyId,
        Guid acknowledgedByUserId,
        string? resolutionNotes = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            UPDATE ml.anomaly_results
            SET acknowledged_at = @acknowledgedAt,
                acknowledged_by = @acknowledgedBy,
                resolution_notes = @notes
            WHERE id = @id";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("id", anomalyId);
        cmd.Parameters.AddWithValue("acknowledgedAt", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("acknowledgedBy", acknowledgedByUserId);
        cmd.Parameters.AddWithValue("notes", resolutionNotes ?? (object)DBNull.Value);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (affected == 0)
        {
            throw new InvalidOperationException($"Anomaly not found: {anomalyId}");
        }

        _logger.LogInformation("Anomaly {AnomalyId} acknowledged by user {UserId}", 
            anomalyId, acknowledgedByUserId);
    }

    private async Task StoreAnomalyResultsAsync(
        IReadOnlyList<AnomalyResult> results,
        CancellationToken cancellationToken)
    {
        if (results.Count == 0) return;

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            foreach (var result in results)
            {
                var sql = @"
                    INSERT INTO ml.anomaly_results 
                        (site_id, node_id, edge_id, anomaly_type, score, 
                         explanation, model_version, detected_at)
                    VALUES 
                        (@siteId, @nodeId, @edgeId, @anomalyType, @score,
                         @explanation, @modelVersion, @detectedAt)
                    ON CONFLICT (node_id, anomaly_type) 
                    WHERE detected_at > NOW() - INTERVAL '1 hour'
                    DO UPDATE SET 
                        score = @score,
                        explanation = @explanation,
                        detected_at = @detectedAt";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("siteId", result.SiteId);
                cmd.Parameters.AddWithValue("nodeId", result.NodeId);
                cmd.Parameters.AddWithValue("edgeId", result.EdgeId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("anomalyType", result.AnomalyType);
                cmd.Parameters.AddWithValue("score", result.Score);
                cmd.Parameters.AddWithValue("explanation", result.Explanation);
                cmd.Parameters.AddWithValue("modelVersion", result.ModelVersion);
                cmd.Parameters.AddWithValue("detectedAt", result.DetectedAt);

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            _logger.LogWarning("Anomaly results table not found, skipping storage");
        }
    }

    private static (GraphNodeType Type, Guid EntityId) TryParseNodeId(string nodeId)
    {
        try
        {
            return Domain.Entities.GraphNode.ParseNodeId(nodeId);
        }
        catch
        {
            return (GraphNodeType.Package, Guid.Empty);
        }
    }
}
