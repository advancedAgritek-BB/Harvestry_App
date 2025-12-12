using Harvestry.AiModels.Domain.Enums;

namespace Harvestry.AiModels.Domain.Entities;

/// <summary>
/// Canonical graph edge representing a relationship between two nodes.
/// Edges are directed and timestamped for temporal graph analysis.
/// </summary>
public sealed class GraphEdge
{
    private GraphEdge() { }

    /// <summary>
    /// Unique identifier for this edge (format: {EdgeType}:{SourceNodeId}:{TargetNodeId})
    /// </summary>
    public string EdgeId { get; private set; } = string.Empty;

    /// <summary>
    /// Site partition key for multi-tenant isolation
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// Type of relationship
    /// </summary>
    public GraphEdgeType EdgeType { get; private set; }

    /// <summary>
    /// Source node ID (from node)
    /// </summary>
    public string SourceNodeId { get; private set; } = string.Empty;

    /// <summary>
    /// Target node ID (to node)
    /// </summary>
    public string TargetNodeId { get; private set; } = string.Empty;

    /// <summary>
    /// Edge weight for weighted graph algorithms (default 1.0)
    /// </summary>
    public float Weight { get; private set; } = 1.0f;

    /// <summary>
    /// JSON blob of additional edge properties
    /// </summary>
    public string? PropertiesJson { get; private set; }

    /// <summary>
    /// When the relationship was established in the source system
    /// </summary>
    public DateTime RelationshipCreatedAt { get; private set; }

    /// <summary>
    /// When this edge snapshot was created/refreshed
    /// </summary>
    public DateTime SnapshotAt { get; private set; }

    /// <summary>
    /// Version for optimistic concurrency
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Whether edge is currently active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Anomaly score for this edge (unusual connection pattern)
    /// </summary>
    public float? AnomalyScore { get; private set; }

    /// <summary>
    /// Create a new graph edge
    /// </summary>
    public static GraphEdge Create(
        Guid siteId,
        GraphEdgeType edgeType,
        string sourceNodeId,
        string targetNodeId,
        DateTime relationshipCreatedAt,
        float weight = 1.0f,
        string? propertiesJson = null)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(sourceNodeId))
            throw new ArgumentException("Source node ID is required", nameof(sourceNodeId));
        if (string.IsNullOrWhiteSpace(targetNodeId))
            throw new ArgumentException("Target node ID is required", nameof(targetNodeId));

        return new GraphEdge
        {
            EdgeId = FormatEdgeId(edgeType, sourceNodeId, targetNodeId),
            SiteId = siteId,
            EdgeType = edgeType,
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            Weight = weight,
            PropertiesJson = propertiesJson,
            RelationshipCreatedAt = relationshipCreatedAt,
            SnapshotAt = DateTime.UtcNow,
            Version = 1,
            IsActive = true
        };
    }

    /// <summary>
    /// Create edge using source entity IDs (convenience method)
    /// </summary>
    public static GraphEdge Create(
        Guid siteId,
        GraphEdgeType edgeType,
        GraphNodeType sourceNodeType,
        Guid sourceEntityId,
        GraphNodeType targetNodeType,
        Guid targetEntityId,
        DateTime relationshipCreatedAt,
        float weight = 1.0f,
        string? propertiesJson = null)
    {
        var sourceNodeId = GraphNode.FormatNodeId(sourceNodeType, sourceEntityId);
        var targetNodeId = GraphNode.FormatNodeId(targetNodeType, targetEntityId);

        return Create(siteId, edgeType, sourceNodeId, targetNodeId, 
            relationshipCreatedAt, weight, propertiesJson);
    }

    /// <summary>
    /// Update edge properties
    /// </summary>
    public void Update(float weight, string? propertiesJson = null)
    {
        Weight = weight;
        PropertiesJson = propertiesJson;
        SnapshotAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Set anomaly score for edge
    /// </summary>
    public void SetAnomalyScore(float score)
    {
        if (score < 0 || score > 1)
            throw new ArgumentException("Anomaly score must be between 0 and 1", nameof(score));

        AnomalyScore = score;
        SnapshotAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate edge (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SnapshotAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Format canonical edge ID
    /// </summary>
    public static string FormatEdgeId(GraphEdgeType edgeType, string sourceNodeId, string targetNodeId)
        => $"{edgeType}:{sourceNodeId}->{targetNodeId}";
}
