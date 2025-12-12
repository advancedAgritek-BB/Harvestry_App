using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.AiModels.API.Controllers;

/// <summary>
/// API controller for graph operations including snapshots and statistics.
/// </summary>
[ApiController]
[Route("api/v1/ai/graph")]
[Authorize]
public sealed class GraphController : ControllerBase
{
    private readonly IGraphSnapshotBuilder _snapshotBuilder;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<GraphController> _logger;

    public GraphController(
        IGraphSnapshotBuilder snapshotBuilder,
        IGraphRepository graphRepository,
        ILogger<GraphController> logger)
    {
        _snapshotBuilder = snapshotBuilder;
        _graphRepository = graphRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get graph statistics for a site
    /// </summary>
    [HttpGet("site/{siteId}/statistics")]
    [ProducesResponseType(typeof(GraphStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var stats = await _graphRepository.GetStatisticsAsync(siteId, cancellationToken);

        return Ok(new GraphStatisticsDto
        {
            SiteId = stats.SiteId,
            TotalNodes = stats.TotalNodes,
            TotalEdges = stats.TotalEdges,
            LastSnapshotAt = stats.LastSnapshotAt,
            NodeCountsByType = stats.NodeCountsByType
                .Select(kv => new NodeTypeCountDto
                {
                    NodeType = kv.Key.ToString(),
                    Count = kv.Value
                })
                .OrderByDescending(x => x.Count)
                .ToList(),
            EdgeCountsByType = stats.EdgeCountsByType
                .Select(kv => new EdgeTypeCountDto
                {
                    EdgeType = kv.Key.ToString(),
                    Count = kv.Value
                })
                .OrderByDescending(x => x.Count)
                .ToList()
        });
    }

    /// <summary>
    /// Trigger a full graph snapshot for a site
    /// </summary>
    [HttpPost("site/{siteId}/snapshot")]
    [ProducesResponseType(typeof(SnapshotResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> TriggerSnapshot(
        [FromRoute] Guid siteId,
        [FromQuery] bool async = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual snapshot triggered for site {SiteId}", siteId);

        if (async)
        {
            // Run in background and return immediately
            _ = Task.Run(async () =>
            {
                await _snapshotBuilder.BuildFullSnapshotAsync(siteId, CancellationToken.None);
            });

            return Accepted(new { message = "Snapshot started", siteId });
        }

        var result = await _snapshotBuilder.BuildFullSnapshotAsync(siteId, cancellationToken);

        return Ok(new SnapshotResultDto
        {
            SiteId = result.SiteId,
            Success = result.Success,
            DurationMs = (int)result.Duration.TotalMilliseconds,
            NodesCreated = result.NodesCreated,
            NodesUpdated = result.NodesUpdated,
            NodesDeactivated = result.NodesDeactivated,
            EdgesCreated = result.EdgesCreated,
            EdgesUpdated = result.EdgesUpdated,
            EdgesDeactivated = result.EdgesDeactivated,
            ErrorMessage = result.ErrorMessage
        });
    }

    /// <summary>
    /// Get node neighborhood (for visualization)
    /// </summary>
    [HttpGet("nodes/{nodeId}/neighborhood")]
    [ProducesResponseType(typeof(NeighborhoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNeighborhood(
        [FromRoute] string nodeId,
        [FromQuery] int hops = 1,
        CancellationToken cancellationToken = default)
    {
        if (hops < 1 || hops > 3)
        {
            return BadRequest(new { message = "Hops must be between 1 and 3" });
        }

        var (nodes, edges) = await _graphRepository.GetNeighborhoodAsync(
            nodeId, hops, null, cancellationToken);

        if (nodes.Count == 0)
        {
            return NotFound(new { message = "Node not found" });
        }

        return Ok(new NeighborhoodDto
        {
            CenterNodeId = nodeId,
            Hops = hops,
            Nodes = nodes.Select(n => new NodeSummaryDto
            {
                NodeId = n.NodeId,
                NodeType = n.NodeType.ToString(),
                Label = n.Label,
                AnomalyScore = n.AnomalyScore,
                IsActive = n.IsActive
            }).ToList(),
            Edges = edges.Select(e => new EdgeSummaryDto
            {
                EdgeId = e.EdgeId,
                EdgeType = e.EdgeType.ToString(),
                SourceNodeId = e.SourceNodeId,
                TargetNodeId = e.TargetNodeId,
                Weight = e.Weight
            }).ToList()
        });
    }

    /// <summary>
    /// Get nodes by type for a site
    /// </summary>
    [HttpGet("site/{siteId}/nodes")]
    [ProducesResponseType(typeof(NodeListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNodesByType(
        [FromRoute] Guid siteId,
        [FromQuery] string nodeType,
        [FromQuery] int limit = 100,
        [FromQuery] float? minAnomalyScore = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<GraphNodeType>(nodeType, true, out var parsedType))
        {
            return BadRequest(new { message = $"Invalid node type: {nodeType}" });
        }

        var nodes = await _graphRepository.GetNodesByTypeAsync(
            siteId, parsedType, true, cancellationToken);

        var filtered = nodes.AsEnumerable();

        if (minAnomalyScore.HasValue)
        {
            filtered = filtered.Where(n => n.AnomalyScore >= minAnomalyScore.Value);
        }

        var resultNodes = filtered
            .OrderByDescending(n => n.AnomalyScore ?? 0)
            .Take(limit)
            .Select(n => new NodeSummaryDto
            {
                NodeId = n.NodeId,
                NodeType = n.NodeType.ToString(),
                Label = n.Label,
                AnomalyScore = n.AnomalyScore,
                IsActive = n.IsActive
            })
            .ToList();

        return Ok(new NodeListDto
        {
            SiteId = siteId,
            NodeType = nodeType,
            Nodes = resultNodes,
            TotalCount = resultNodes.Count
        });
    }
}

#region DTOs

public sealed class GraphStatisticsDto
{
    public Guid SiteId { get; init; }
    public int TotalNodes { get; init; }
    public int TotalEdges { get; init; }
    public DateTime? LastSnapshotAt { get; init; }
    public IReadOnlyList<NodeTypeCountDto> NodeCountsByType { get; init; } 
        = Array.Empty<NodeTypeCountDto>();
    public IReadOnlyList<EdgeTypeCountDto> EdgeCountsByType { get; init; } 
        = Array.Empty<EdgeTypeCountDto>();
}

public sealed class NodeTypeCountDto
{
    public string NodeType { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class EdgeTypeCountDto
{
    public string EdgeType { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class SnapshotResultDto
{
    public Guid SiteId { get; init; }
    public bool Success { get; init; }
    public int DurationMs { get; init; }
    public int NodesCreated { get; init; }
    public int NodesUpdated { get; init; }
    public int NodesDeactivated { get; init; }
    public int EdgesCreated { get; init; }
    public int EdgesUpdated { get; init; }
    public int EdgesDeactivated { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class NeighborhoodDto
{
    public string CenterNodeId { get; init; } = string.Empty;
    public int Hops { get; init; }
    public IReadOnlyList<NodeSummaryDto> Nodes { get; init; } = Array.Empty<NodeSummaryDto>();
    public IReadOnlyList<EdgeSummaryDto> Edges { get; init; } = Array.Empty<EdgeSummaryDto>();
}

public sealed class NodeSummaryDto
{
    public string NodeId { get; init; } = string.Empty;
    public string NodeType { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public float? AnomalyScore { get; init; }
    public bool IsActive { get; init; }
}

public sealed class EdgeSummaryDto
{
    public string EdgeId { get; init; } = string.Empty;
    public string EdgeType { get; init; } = string.Empty;
    public string SourceNodeId { get; init; } = string.Empty;
    public string TargetNodeId { get; init; } = string.Empty;
    public float Weight { get; init; }
}

public sealed class NodeListDto
{
    public Guid SiteId { get; init; }
    public string NodeType { get; init; } = string.Empty;
    public IReadOnlyList<NodeSummaryDto> Nodes { get; init; } = Array.Empty<NodeSummaryDto>();
    public int TotalCount { get; init; }
}

#endregion
