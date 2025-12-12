using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;

namespace Harvestry.AiModels.Application.Interfaces;

/// <summary>
/// Repository interface for graph node and edge persistence.
/// Supports batch operations for efficient snapshot building.
/// </summary>
public interface IGraphRepository
{
    // === Node Operations ===

    /// <summary>
    /// Get node by ID
    /// </summary>
    Task<GraphNode?> GetNodeAsync(string nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get nodes by type for a site
    /// </summary>
    Task<IReadOnlyList<GraphNode>> GetNodesByTypeAsync(
        Guid siteId,
        GraphNodeType nodeType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get nodes by source entity IDs
    /// </summary>
    Task<IReadOnlyList<GraphNode>> GetNodesBySourceIdsAsync(
        Guid siteId,
        GraphNodeType nodeType,
        IEnumerable<Guid> sourceEntityIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert a single node
    /// </summary>
    Task UpsertNodeAsync(GraphNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert nodes in batch
    /// </summary>
    Task UpsertNodesAsync(IEnumerable<GraphNode> nodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate nodes not in the provided ID set (for cleanup)
    /// </summary>
    Task DeactivateNodesNotInSetAsync(
        Guid siteId,
        GraphNodeType nodeType,
        IEnumerable<string> activeNodeIds,
        CancellationToken cancellationToken = default);

    // === Edge Operations ===

    /// <summary>
    /// Get edge by ID
    /// </summary>
    Task<GraphEdge?> GetEdgeAsync(string edgeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get edges by type for a site
    /// </summary>
    Task<IReadOnlyList<GraphEdge>> GetEdgesByTypeAsync(
        Guid siteId,
        GraphEdgeType edgeType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get outgoing edges from a node
    /// </summary>
    Task<IReadOnlyList<GraphEdge>> GetOutgoingEdgesAsync(
        string sourceNodeId,
        GraphEdgeType? edgeType = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get incoming edges to a node
    /// </summary>
    Task<IReadOnlyList<GraphEdge>> GetIncomingEdgesAsync(
        string targetNodeId,
        GraphEdgeType? edgeType = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get edges between two node sets (for subgraph extraction)
    /// </summary>
    Task<IReadOnlyList<GraphEdge>> GetEdgesBetweenNodesAsync(
        Guid siteId,
        IEnumerable<string> nodeIds,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert a single edge
    /// </summary>
    Task UpsertEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upsert edges in batch
    /// </summary>
    Task UpsertEdgesAsync(IEnumerable<GraphEdge> edges, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate edges not in the provided ID set
    /// </summary>
    Task DeactivateEdgesNotInSetAsync(
        Guid siteId,
        GraphEdgeType edgeType,
        IEnumerable<string> activeEdgeIds,
        CancellationToken cancellationToken = default);

    // === Query Operations ===

    /// <summary>
    /// Get k-hop neighborhood around a node
    /// </summary>
    Task<(IReadOnlyList<GraphNode> Nodes, IReadOnlyList<GraphEdge> Edges)> GetNeighborhoodAsync(
        string centerNodeId,
        int hops = 1,
        IEnumerable<GraphEdgeType>? edgeTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get nodes with high anomaly scores
    /// </summary>
    Task<IReadOnlyList<GraphNode>> GetAnomalousNodesAsync(
        Guid siteId,
        float minScore = 0.7f,
        GraphNodeType? nodeType = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get graph statistics for a site
    /// </summary>
    Task<GraphStatistics> GetStatisticsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Graph statistics summary
/// </summary>
public sealed record GraphStatistics
{
    public Guid SiteId { get; init; }
    public int TotalNodes { get; init; }
    public int TotalEdges { get; init; }
    public DateTime? LastSnapshotAt { get; init; }
    public Dictionary<GraphNodeType, int> NodeCountsByType { get; init; } = new();
    public Dictionary<GraphEdgeType, int> EdgeCountsByType { get; init; } = new();
}
