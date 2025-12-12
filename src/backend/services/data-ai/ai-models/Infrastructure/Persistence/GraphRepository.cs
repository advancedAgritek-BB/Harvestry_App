using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harvestry.AiModels.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for graph node and edge persistence.
/// </summary>
public sealed class GraphRepository : IGraphRepository
{
    private readonly GraphDbContext _context;
    private readonly ILogger<GraphRepository> _logger;

    public GraphRepository(GraphDbContext context, ILogger<GraphRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // === Node Operations ===

    public async Task<GraphNode?> GetNodeAsync(string nodeId, CancellationToken cancellationToken = default)
    {
        return await _context.Nodes
            .FirstOrDefaultAsync(n => n.NodeId == nodeId, cancellationToken);
    }

    public async Task<IReadOnlyList<GraphNode>> GetNodesByTypeAsync(
        Guid siteId,
        GraphNodeType nodeType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Nodes
            .Where(n => n.SiteId == siteId && n.NodeType == nodeType);

        if (activeOnly)
            query = query.Where(n => n.IsActive);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GraphNode>> GetNodesBySourceIdsAsync(
        Guid siteId,
        GraphNodeType nodeType,
        IEnumerable<Guid> sourceEntityIds,
        CancellationToken cancellationToken = default)
    {
        var idSet = sourceEntityIds.ToHashSet();
        return await _context.Nodes
            .Where(n => n.SiteId == siteId && n.NodeType == nodeType && idSet.Contains(n.SourceEntityId))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertNodeAsync(GraphNode node, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Nodes
            .FirstOrDefaultAsync(n => n.NodeId == node.NodeId, cancellationToken);

        if (existing == null)
        {
            _context.Nodes.Add(node);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(node);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertNodesAsync(IEnumerable<GraphNode> nodes, CancellationToken cancellationToken = default)
    {
        var nodeList = nodes.ToList();
        if (nodeList.Count == 0) return;

        var nodeIds = nodeList.Select(n => n.NodeId).ToHashSet();
        var existingNodes = await _context.Nodes
            .Where(n => nodeIds.Contains(n.NodeId))
            .ToDictionaryAsync(n => n.NodeId, cancellationToken);

        foreach (var node in nodeList)
        {
            if (existingNodes.TryGetValue(node.NodeId, out var existing))
            {
                _context.Entry(existing).CurrentValues.SetValues(node);
            }
            else
            {
                _context.Nodes.Add(node);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Upserted {Count} nodes", nodeList.Count);
    }

    public async Task DeactivateNodesNotInSetAsync(
        Guid siteId,
        GraphNodeType nodeType,
        IEnumerable<string> activeNodeIds,
        CancellationToken cancellationToken = default)
    {
        var activeSet = activeNodeIds.ToHashSet();

        var nodesToDeactivate = await _context.Nodes
            .Where(n => n.SiteId == siteId && n.NodeType == nodeType && n.IsActive && !activeSet.Contains(n.NodeId))
            .ToListAsync(cancellationToken);

        foreach (var node in nodesToDeactivate)
        {
            node.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Deactivated {Count} nodes of type {Type}", nodesToDeactivate.Count, nodeType);
    }

    // === Edge Operations ===

    public async Task<GraphEdge?> GetEdgeAsync(string edgeId, CancellationToken cancellationToken = default)
    {
        return await _context.Edges
            .FirstOrDefaultAsync(e => e.EdgeId == edgeId, cancellationToken);
    }

    public async Task<IReadOnlyList<GraphEdge>> GetEdgesByTypeAsync(
        Guid siteId,
        GraphEdgeType edgeType,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Edges
            .Where(e => e.SiteId == siteId && e.EdgeType == edgeType);

        if (activeOnly)
            query = query.Where(e => e.IsActive);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GraphEdge>> GetOutgoingEdgesAsync(
        string sourceNodeId,
        GraphEdgeType? edgeType = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Edges.Where(e => e.SourceNodeId == sourceNodeId);

        if (edgeType.HasValue)
            query = query.Where(e => e.EdgeType == edgeType.Value);

        if (activeOnly)
            query = query.Where(e => e.IsActive);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GraphEdge>> GetIncomingEdgesAsync(
        string targetNodeId,
        GraphEdgeType? edgeType = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Edges.Where(e => e.TargetNodeId == targetNodeId);

        if (edgeType.HasValue)
            query = query.Where(e => e.EdgeType == edgeType.Value);

        if (activeOnly)
            query = query.Where(e => e.IsActive);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GraphEdge>> GetEdgesBetweenNodesAsync(
        Guid siteId,
        IEnumerable<string> nodeIds,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var nodeSet = nodeIds.ToHashSet();
        var query = _context.Edges
            .Where(e => e.SiteId == siteId && 
                        nodeSet.Contains(e.SourceNodeId) && 
                        nodeSet.Contains(e.TargetNodeId));

        if (activeOnly)
            query = query.Where(e => e.IsActive);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task UpsertEdgeAsync(GraphEdge edge, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Edges
            .FirstOrDefaultAsync(e => e.EdgeId == edge.EdgeId, cancellationToken);

        if (existing == null)
        {
            _context.Edges.Add(edge);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(edge);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertEdgesAsync(IEnumerable<GraphEdge> edges, CancellationToken cancellationToken = default)
    {
        var edgeList = edges.ToList();
        if (edgeList.Count == 0) return;

        var edgeIds = edgeList.Select(e => e.EdgeId).ToHashSet();
        var existingEdges = await _context.Edges
            .Where(e => edgeIds.Contains(e.EdgeId))
            .ToDictionaryAsync(e => e.EdgeId, cancellationToken);

        foreach (var edge in edgeList)
        {
            if (existingEdges.TryGetValue(edge.EdgeId, out var existing))
            {
                _context.Entry(existing).CurrentValues.SetValues(edge);
            }
            else
            {
                _context.Edges.Add(edge);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Upserted {Count} edges", edgeList.Count);
    }

    public async Task DeactivateEdgesNotInSetAsync(
        Guid siteId,
        GraphEdgeType edgeType,
        IEnumerable<string> activeEdgeIds,
        CancellationToken cancellationToken = default)
    {
        var activeSet = activeEdgeIds.ToHashSet();

        var edgesToDeactivate = await _context.Edges
            .Where(e => e.SiteId == siteId && e.EdgeType == edgeType && e.IsActive && !activeSet.Contains(e.EdgeId))
            .ToListAsync(cancellationToken);

        foreach (var edge in edgesToDeactivate)
        {
            edge.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Deactivated {Count} edges of type {Type}", edgesToDeactivate.Count, edgeType);
    }

    // === Query Operations ===

    public async Task<(IReadOnlyList<GraphNode> Nodes, IReadOnlyList<GraphEdge> Edges)> GetNeighborhoodAsync(
        string centerNodeId,
        int hops = 1,
        IEnumerable<GraphEdgeType>? edgeTypes = null,
        CancellationToken cancellationToken = default)
    {
        var visitedNodeIds = new HashSet<string> { centerNodeId };
        var allEdges = new List<GraphEdge>();
        var currentFrontier = new HashSet<string> { centerNodeId };
        var edgeTypeSet = edgeTypes?.ToHashSet();

        for (int hop = 0; hop < hops; hop++)
        {
            var newFrontier = new HashSet<string>();

            // Get outgoing edges from current frontier
            var outgoingQuery = _context.Edges
                .Where(e => currentFrontier.Contains(e.SourceNodeId) && e.IsActive);

            if (edgeTypeSet != null)
                outgoingQuery = outgoingQuery.Where(e => edgeTypeSet.Contains(e.EdgeType));

            var outgoing = await outgoingQuery.ToListAsync(cancellationToken);
            allEdges.AddRange(outgoing);

            foreach (var edge in outgoing)
            {
                if (visitedNodeIds.Add(edge.TargetNodeId))
                    newFrontier.Add(edge.TargetNodeId);
            }

            // Get incoming edges to current frontier
            var incomingQuery = _context.Edges
                .Where(e => currentFrontier.Contains(e.TargetNodeId) && e.IsActive);

            if (edgeTypeSet != null)
                incomingQuery = incomingQuery.Where(e => edgeTypeSet.Contains(e.EdgeType));

            var incoming = await incomingQuery.ToListAsync(cancellationToken);
            allEdges.AddRange(incoming);

            foreach (var edge in incoming)
            {
                if (visitedNodeIds.Add(edge.SourceNodeId))
                    newFrontier.Add(edge.SourceNodeId);
            }

            currentFrontier = newFrontier;
            if (currentFrontier.Count == 0) break;
        }

        // Fetch all nodes
        var nodes = await _context.Nodes
            .Where(n => visitedNodeIds.Contains(n.NodeId))
            .ToListAsync(cancellationToken);

        return (nodes, allEdges.DistinctBy(e => e.EdgeId).ToList());
    }

    public async Task<IReadOnlyList<GraphNode>> GetAnomalousNodesAsync(
        Guid siteId,
        float minScore = 0.7f,
        GraphNodeType? nodeType = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Nodes
            .Where(n => n.SiteId == siteId && n.IsActive && n.AnomalyScore >= minScore);

        if (nodeType.HasValue)
            query = query.Where(n => n.NodeType == nodeType.Value);

        return await query
            .OrderByDescending(n => n.AnomalyScore)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<GraphStatistics> GetStatisticsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var nodeStats = await _context.Nodes
            .Where(n => n.SiteId == siteId && n.IsActive)
            .GroupBy(n => n.NodeType)
            .Select(g => new { NodeType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var edgeStats = await _context.Edges
            .Where(e => e.SiteId == siteId && e.IsActive)
            .GroupBy(e => e.EdgeType)
            .Select(g => new { EdgeType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var lastSnapshot = await _context.Nodes
            .Where(n => n.SiteId == siteId)
            .MaxAsync(n => (DateTime?)n.SnapshotAt, cancellationToken);

        return new GraphStatistics
        {
            SiteId = siteId,
            TotalNodes = nodeStats.Sum(s => s.Count),
            TotalEdges = edgeStats.Sum(s => s.Count),
            LastSnapshotAt = lastSnapshot,
            NodeCountsByType = nodeStats.ToDictionary(s => s.NodeType, s => s.Count),
            EdgeCountsByType = edgeStats.ToDictionary(s => s.EdgeType, s => s.Count)
        };
    }
}
