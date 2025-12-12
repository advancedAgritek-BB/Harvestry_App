using Harvestry.AiModels.Domain.Enums;

namespace Harvestry.AiModels.Application.Interfaces;

/// <summary>
/// Interface for building graph snapshots from domain data.
/// Supports both full refresh and incremental updates.
/// </summary>
public interface IGraphSnapshotBuilder
{
    /// <summary>
    /// Build a full graph snapshot for a site (all node types)
    /// </summary>
    Task<GraphSnapshotResult> BuildFullSnapshotAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Build a snapshot for specific node types only
    /// </summary>
    Task<GraphSnapshotResult> BuildPartialSnapshotAsync(
        Guid siteId,
        IEnumerable<GraphNodeType> nodeTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply incremental updates from domain events
    /// </summary>
    Task<GraphSnapshotResult> ApplyIncrementalUpdatesAsync(
        Guid siteId,
        IEnumerable<GraphUpdate> updates,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a graph snapshot operation
/// </summary>
public sealed record GraphSnapshotResult
{
    public Guid SiteId { get; init; }
    public bool Success { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime CompletedAt { get; init; }
    public TimeSpan Duration => CompletedAt - StartedAt;
    public int NodesCreated { get; init; }
    public int NodesUpdated { get; init; }
    public int NodesDeactivated { get; init; }
    public int EdgesCreated { get; init; }
    public int EdgesUpdated { get; init; }
    public int EdgesDeactivated { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<GraphNodeType, int> NodeCountsByType { get; init; } = new();
    public Dictionary<GraphEdgeType, int> EdgeCountsByType { get; init; } = new();
}

/// <summary>
/// Represents an incremental graph update from a domain event
/// </summary>
public sealed record GraphUpdate
{
    public GraphUpdateType UpdateType { get; init; }
    public GraphNodeType? NodeType { get; init; }
    public GraphEdgeType? EdgeType { get; init; }
    public Guid EntityId { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public DateTime OccurredAt { get; init; }
    public string? PayloadJson { get; init; }
}

/// <summary>
/// Type of graph update operation
/// </summary>
public enum GraphUpdateType
{
    NodeCreated,
    NodeUpdated,
    NodeDeleted,
    EdgeCreated,
    EdgeDeleted
}

/// <summary>
/// Domain-specific snapshot builders for each bounded context
/// </summary>
public interface IPackageGraphBuilder
{
    /// <summary>
    /// Build package/movement traceability graph for a site
    /// </summary>
    Task<(int Nodes, int Edges)> BuildAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);
}

public interface ITaskGraphBuilder
{
    /// <summary>
    /// Build task dependency and work graph for a site
    /// </summary>
    Task<(int Nodes, int Edges)> BuildAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);
}

public interface ITelemetryGraphBuilder
{
    /// <summary>
    /// Build telemetry/irrigation topology graph for a site
    /// </summary>
    Task<(int Nodes, int Edges)> BuildAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);
}

public interface IGeneticsGraphBuilder
{
    /// <summary>
    /// Build genetics/crop steering graph for a site
    /// </summary>
    Task<(int Nodes, int Edges)> BuildAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);
}
