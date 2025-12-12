using Harvestry.AiModels.Domain.Enums;

namespace Harvestry.AiModels.Domain.Entities;

/// <summary>
/// Canonical graph node representing any entity in the Harvestry knowledge graph.
/// All nodes are partitioned by site_id for multi-tenant isolation.
/// </summary>
public sealed class GraphNode
{
    private GraphNode() { }

    /// <summary>
    /// Unique identifier for this node (format: {NodeType}:{SourceEntityId})
    /// </summary>
    public string NodeId { get; private set; } = string.Empty;

    /// <summary>
    /// Site partition key for multi-tenant isolation
    /// </summary>
    public Guid SiteId { get; private set; }

    /// <summary>
    /// Type of node (maps to domain entity type)
    /// </summary>
    public GraphNodeType NodeType { get; private set; }

    /// <summary>
    /// Original entity ID from the source domain
    /// </summary>
    public Guid SourceEntityId { get; private set; }

    /// <summary>
    /// Human-readable label for display
    /// </summary>
    public string Label { get; private set; } = string.Empty;

    /// <summary>
    /// Node feature vector for ML (embeddings, computed features)
    /// Stored as float array for efficient vector operations
    /// </summary>
    public float[]? FeatureVector { get; private set; }

    /// <summary>
    /// JSON blob of additional properties for flexible schema
    /// </summary>
    public string? PropertiesJson { get; private set; }

    /// <summary>
    /// When the source entity was created
    /// </summary>
    public DateTime SourceCreatedAt { get; private set; }

    /// <summary>
    /// When the source entity was last updated
    /// </summary>
    public DateTime SourceUpdatedAt { get; private set; }

    /// <summary>
    /// When this node snapshot was created/refreshed
    /// </summary>
    public DateTime SnapshotAt { get; private set; }

    /// <summary>
    /// Version for optimistic concurrency
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Whether node is currently active (soft delete support)
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Anomaly score computed by ML models (0-1 scale)
    /// </summary>
    public float? AnomalyScore { get; private set; }

    /// <summary>
    /// Explanation for anomaly score (feature attributions)
    /// </summary>
    public string? AnomalyExplanation { get; private set; }

    /// <summary>
    /// Create a new graph node
    /// </summary>
    public static GraphNode Create(
        Guid siteId,
        GraphNodeType nodeType,
        Guid sourceEntityId,
        string label,
        DateTime sourceCreatedAt,
        DateTime sourceUpdatedAt,
        string? propertiesJson = null,
        float[]? featureVector = null)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (sourceEntityId == Guid.Empty)
            throw new ArgumentException("Source entity ID is required", nameof(sourceEntityId));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label is required", nameof(label));

        return new GraphNode
        {
            NodeId = FormatNodeId(nodeType, sourceEntityId),
            SiteId = siteId,
            NodeType = nodeType,
            SourceEntityId = sourceEntityId,
            Label = label,
            PropertiesJson = propertiesJson,
            FeatureVector = featureVector,
            SourceCreatedAt = sourceCreatedAt,
            SourceUpdatedAt = sourceUpdatedAt,
            SnapshotAt = DateTime.UtcNow,
            Version = 1,
            IsActive = true
        };
    }

    /// <summary>
    /// Update node with new data from source
    /// </summary>
    public void Update(
        string label,
        DateTime sourceUpdatedAt,
        string? propertiesJson = null,
        float[]? featureVector = null)
    {
        Label = label;
        SourceUpdatedAt = sourceUpdatedAt;
        PropertiesJson = propertiesJson;
        FeatureVector = featureVector;
        SnapshotAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Set anomaly score from ML model
    /// </summary>
    public void SetAnomalyScore(float score, string? explanation = null)
    {
        if (score < 0 || score > 1)
            throw new ArgumentException("Anomaly score must be between 0 and 1", nameof(score));

        AnomalyScore = score;
        AnomalyExplanation = explanation;
        SnapshotAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark node as inactive (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SnapshotAt = DateTime.UtcNow;
        Version++;
    }

    /// <summary>
    /// Format canonical node ID
    /// </summary>
    public static string FormatNodeId(GraphNodeType nodeType, Guid entityId)
        => $"{nodeType}:{entityId}";

    /// <summary>
    /// Parse node ID back to type and entity ID
    /// </summary>
    public static (GraphNodeType NodeType, Guid EntityId) ParseNodeId(string nodeId)
    {
        var parts = nodeId.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid node ID format", nameof(nodeId));

        if (!Enum.TryParse<GraphNodeType>(parts[0], out var nodeType))
            throw new ArgumentException("Invalid node type", nameof(nodeId));

        if (!Guid.TryParse(parts[1], out var entityId))
            throw new ArgumentException("Invalid entity ID", nameof(nodeId));

        return (nodeType, entityId);
    }
}
