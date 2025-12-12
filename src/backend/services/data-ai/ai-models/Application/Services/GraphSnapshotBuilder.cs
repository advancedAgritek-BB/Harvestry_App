using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Orchestrates graph snapshot building across all domain-specific builders.
/// Supports full refresh and incremental updates.
/// </summary>
public sealed class GraphSnapshotBuilder : IGraphSnapshotBuilder
{
    private readonly IPackageGraphBuilder _packageBuilder;
    private readonly ITaskGraphBuilder _taskBuilder;
    private readonly ITelemetryGraphBuilder _telemetryBuilder;
    private readonly IGeneticsGraphBuilder _geneticsBuilder;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<GraphSnapshotBuilder> _logger;

    public GraphSnapshotBuilder(
        IPackageGraphBuilder packageBuilder,
        ITaskGraphBuilder taskBuilder,
        ITelemetryGraphBuilder telemetryBuilder,
        IGeneticsGraphBuilder geneticsBuilder,
        IGraphRepository graphRepository,
        ILogger<GraphSnapshotBuilder> logger)
    {
        _packageBuilder = packageBuilder;
        _taskBuilder = taskBuilder;
        _telemetryBuilder = telemetryBuilder;
        _geneticsBuilder = geneticsBuilder;
        _graphRepository = graphRepository;
        _logger = logger;
    }

    public async Task<GraphSnapshotResult> BuildFullSnapshotAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("Starting full graph snapshot for site {SiteId}", siteId);

        var result = new GraphSnapshotResult
        {
            SiteId = siteId,
            StartedAt = startedAt,
            NodeCountsByType = new Dictionary<GraphNodeType, int>(),
            EdgeCountsByType = new Dictionary<GraphEdgeType, int>()
        };

        try
        {
            // Build all domain graphs in parallel
            var packageTask = _packageBuilder.BuildAsync(siteId, cancellationToken: cancellationToken);
            var taskTask = _taskBuilder.BuildAsync(siteId, cancellationToken: cancellationToken);
            var telemetryTask = _telemetryBuilder.BuildAsync(siteId, cancellationToken: cancellationToken);
            var geneticsTask = _geneticsBuilder.BuildAsync(siteId, cancellationToken: cancellationToken);

            await Task.WhenAll(packageTask, taskTask, telemetryTask, geneticsTask);

            var packageResult = await packageTask;
            var taskResult = await taskTask;
            var telemetryResult = await telemetryTask;
            var geneticsResult = await geneticsTask;

            var totalNodes = packageResult.Nodes + taskResult.Nodes + 
                            telemetryResult.Nodes + geneticsResult.Nodes;
            var totalEdges = packageResult.Edges + taskResult.Edges + 
                            telemetryResult.Edges + geneticsResult.Edges;

            result = result with
            {
                Success = true,
                CompletedAt = DateTime.UtcNow,
                NodesCreated = totalNodes,
                EdgesCreated = totalEdges
            };

            _logger.LogInformation(
                "Completed full graph snapshot for site {SiteId}: {Nodes} nodes, {Edges} edges in {Duration}ms",
                siteId, totalNodes, totalEdges, result.Duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build graph snapshot for site {SiteId}", siteId);
            result = result with
            {
                Success = false,
                CompletedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }

        return result;
    }

    public async Task<GraphSnapshotResult> BuildPartialSnapshotAsync(
        Guid siteId,
        IEnumerable<GraphNodeType> nodeTypes,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var nodeTypeSet = nodeTypes.ToHashSet();
        
        _logger.LogInformation(
            "Starting partial graph snapshot for site {SiteId}, types: {Types}",
            siteId, string.Join(", ", nodeTypeSet));

        var result = new GraphSnapshotResult
        {
            SiteId = siteId,
            StartedAt = startedAt,
            NodeCountsByType = new Dictionary<GraphNodeType, int>(),
            EdgeCountsByType = new Dictionary<GraphEdgeType, int>()
        };

        try
        {
            var tasks = new List<Task<(int Nodes, int Edges)>>();

            // Determine which builders to run based on node types
            if (nodeTypeSet.Any(t => t is GraphNodeType.Package or GraphNodeType.InventoryMovement 
                or GraphNodeType.Location or GraphNodeType.Item))
            {
                tasks.Add(_packageBuilder.BuildAsync(siteId, cancellationToken: cancellationToken));
            }

            if (nodeTypeSet.Any(t => t is GraphNodeType.Task or GraphNodeType.TimeEntry 
                or GraphNodeType.Sop or GraphNodeType.Team))
            {
                tasks.Add(_taskBuilder.BuildAsync(siteId, cancellationToken: cancellationToken));
            }

            if (nodeTypeSet.Any(t => t is GraphNodeType.Zone or GraphNodeType.Room 
                or GraphNodeType.Equipment or GraphNodeType.SensorStream 
                or GraphNodeType.IrrigationRun or GraphNodeType.AlertRule))
            {
                tasks.Add(_telemetryBuilder.BuildAsync(siteId, cancellationToken: cancellationToken));
            }

            if (nodeTypeSet.Any(t => t is GraphNodeType.Strain or GraphNodeType.CropSteeringProfile 
                or GraphNodeType.ResponseCurve or GraphNodeType.Harvest 
                or GraphNodeType.LabTestBatch or GraphNodeType.Plant))
            {
                tasks.Add(_geneticsBuilder.BuildAsync(siteId, cancellationToken: cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            var totalNodes = results.Sum(r => r.Nodes);
            var totalEdges = results.Sum(r => r.Edges);

            result = result with
            {
                Success = true,
                CompletedAt = DateTime.UtcNow,
                NodesCreated = totalNodes,
                EdgesCreated = totalEdges
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build partial graph snapshot for site {SiteId}", siteId);
            result = result with
            {
                Success = false,
                CompletedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }

        return result;
    }

    public async Task<GraphSnapshotResult> ApplyIncrementalUpdatesAsync(
        Guid siteId,
        IEnumerable<GraphUpdate> updates,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var updateList = updates.ToList();
        
        _logger.LogInformation(
            "Applying {Count} incremental updates for site {SiteId}",
            updateList.Count, siteId);

        var result = new GraphSnapshotResult
        {
            SiteId = siteId,
            StartedAt = startedAt,
            NodeCountsByType = new Dictionary<GraphNodeType, int>(),
            EdgeCountsByType = new Dictionary<GraphEdgeType, int>()
        };

        try
        {
            // Group updates by domain for efficient processing
            var packageUpdates = updateList.Where(u => 
                u.NodeType is GraphNodeType.Package or GraphNodeType.InventoryMovement).ToList();
            
            var taskUpdates = updateList.Where(u => 
                u.NodeType is GraphNodeType.Task or GraphNodeType.TimeEntry).ToList();

            var telemetryUpdates = updateList.Where(u => 
                u.NodeType is GraphNodeType.IrrigationRun or GraphNodeType.SensorStream 
                or GraphNodeType.AlertInstance).ToList();

            var geneticsUpdates = updateList.Where(u => 
                u.NodeType is GraphNodeType.Strain or GraphNodeType.Harvest 
                or GraphNodeType.LabTestBatch).ToList();

            // Process updates by domain
            var tasks = new List<Task<(int Nodes, int Edges)>>();

            if (packageUpdates.Count > 0)
            {
                var sinceTime = packageUpdates.Min(u => u.OccurredAt);
                tasks.Add(_packageBuilder.BuildAsync(siteId, sinceTime, cancellationToken));
            }

            if (taskUpdates.Count > 0)
            {
                var sinceTime = taskUpdates.Min(u => u.OccurredAt);
                tasks.Add(_taskBuilder.BuildAsync(siteId, sinceTime, cancellationToken));
            }

            if (telemetryUpdates.Count > 0)
            {
                var sinceTime = telemetryUpdates.Min(u => u.OccurredAt);
                tasks.Add(_telemetryBuilder.BuildAsync(siteId, sinceTime, cancellationToken));
            }

            if (geneticsUpdates.Count > 0)
            {
                var sinceTime = geneticsUpdates.Min(u => u.OccurredAt);
                tasks.Add(_geneticsBuilder.BuildAsync(siteId, sinceTime, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            
            result = result with
            {
                Success = true,
                CompletedAt = DateTime.UtcNow,
                NodesUpdated = results.Sum(r => r.Nodes),
                EdgesUpdated = results.Sum(r => r.Edges)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply incremental updates for site {SiteId}", siteId);
            result = result with
            {
                Success = false,
                CompletedAt = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }

        return result;
    }
}
