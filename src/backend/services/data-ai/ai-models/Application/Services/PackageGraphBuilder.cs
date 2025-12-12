using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Builds the package/movement traceability graph from the packages domain.
/// Creates nodes for Package, InventoryMovement, Location, Item, User
/// and edges for movement flows, approvals, lineage.
/// </summary>
public sealed class PackageGraphBuilder : IPackageGraphBuilder
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<PackageGraphBuilder> _logger;

    public PackageGraphBuilder(
        NpgsqlDataSource dataSource,
        IGraphRepository graphRepository,
        ILogger<PackageGraphBuilder> logger)
    {
        _dataSource = dataSource;
        _graphRepository = graphRepository;
        _logger = logger;
    }

    public async Task<(int Nodes, int Edges)> BuildAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Building package graph for site {SiteId}, since {Since}", 
            siteId, sinceTimestamp);

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        // Build package nodes
        var packageNodes = await BuildPackageNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(packageNodes);

        // Build movement nodes
        var movementNodes = await BuildMovementNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(movementNodes);

        // Build location nodes
        var locationNodes = await BuildLocationNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(locationNodes);

        // Build edges
        var movementEdges = await BuildMovementEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(movementEdges);

        var lineageEdges = await BuildLineageEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(lineageEdges);

        // Persist to graph store
        await _graphRepository.UpsertNodesAsync(nodes, cancellationToken);
        await _graphRepository.UpsertEdgesAsync(edges, cancellationToken);

        _logger.LogInformation(
            "Built package graph for site {SiteId}: {Nodes} nodes, {Edges} edges",
            siteId, nodes.Count, edges.Count);

        return (nodes.Count, edges.Count);
    }

    private async Task<List<GraphNode>> BuildPackageNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, package_label, item_id, item_name, item_category,
                quantity, initial_quantity, unit_of_measure,
                location_id, location_name, status, lab_testing_state,
                thc_percent, cbd_percent, generation_depth, root_ancestor_id,
                hold_reason_code, metrc_sync_status, unit_cost, grade,
                created_at, updated_at
            FROM packages
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND updated_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var packageId = reader.GetGuid(0);
            var packageLabel = reader.GetString(1);

            var properties = new PackageNodeProperties
            {
                PackageLabel = packageLabel,
                ItemName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ItemCategory = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Quantity = reader.GetDecimal(5),
                InitialQuantity = reader.GetDecimal(6),
                UnitOfMeasure = reader.IsDBNull(7) ? "" : reader.GetString(7),
                LocationId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                LocationName = reader.IsDBNull(9) ? null : reader.GetString(9),
                Status = reader.IsDBNull(10) ? "" : reader.GetString(10),
                LabTestingState = reader.IsDBNull(11) ? null : reader.GetString(11),
                ThcPercent = reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                CbdPercent = reader.IsDBNull(13) ? null : reader.GetDecimal(13),
                GenerationDepth = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                RootAncestorId = reader.IsDBNull(15) ? null : reader.GetGuid(15),
                HoldReasonCode = reader.IsDBNull(16) ? null : reader.GetString(16),
                MetrcSyncStatus = reader.IsDBNull(17) ? null : reader.GetString(17),
                UnitCost = reader.IsDBNull(18) ? null : reader.GetDecimal(18),
                Grade = reader.IsDBNull(19) ? null : reader.GetString(19)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.Package,
                packageId,
                $"Package: {packageLabel}",
                reader.GetDateTime(20),
                reader.GetDateTime(21),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildMovementNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, movement_type, status, package_id, package_label,
                quantity, unit_of_measure, from_location_id, from_location_path,
                to_location_id, to_location_path, reason_code, requires_approval,
                first_approver_id, second_approver_id, verified_by_user_id,
                created_by_user_id, sales_order_id, transfer_id, processing_job_id,
                created_at
            FROM inventory_movements
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND created_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var movementId = reader.GetGuid(0);

            var properties = new MovementNodeProperties
            {
                MovementType = reader.GetString(1),
                Status = reader.GetString(2),
                PackageId = reader.GetGuid(3),
                PackageLabel = reader.IsDBNull(4) ? null : reader.GetString(4),
                Quantity = reader.GetDecimal(5),
                UnitOfMeasure = reader.IsDBNull(6) ? "" : reader.GetString(6),
                FromLocationId = reader.IsDBNull(7) ? null : reader.GetGuid(7),
                FromLocationPath = reader.IsDBNull(8) ? null : reader.GetString(8),
                ToLocationId = reader.IsDBNull(9) ? null : reader.GetGuid(9),
                ToLocationPath = reader.IsDBNull(10) ? null : reader.GetString(10),
                ReasonCode = reader.IsDBNull(11) ? null : reader.GetString(11),
                RequiresApproval = reader.IsDBNull(12) ? false : reader.GetBoolean(12),
                FirstApproverId = reader.IsDBNull(13) ? null : reader.GetGuid(13),
                SecondApproverId = reader.IsDBNull(14) ? null : reader.GetGuid(14),
                VerifiedByUserId = reader.IsDBNull(15) ? null : reader.GetGuid(15),
                CreatedByUserId = reader.GetGuid(16),
                SalesOrderId = reader.IsDBNull(17) ? null : reader.GetGuid(17),
                TransferId = reader.IsDBNull(18) ? null : reader.GetGuid(18),
                ProcessingJobId = reader.IsDBNull(19) ? null : reader.GetGuid(19)
            };

            var createdAt = reader.GetDateTime(20);

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.InventoryMovement,
                movementId,
                $"Movement: {properties.MovementType} - {properties.PackageLabel}",
                createdAt,
                createdAt,
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildLocationNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        // Get unique locations from packages
        var sql = @"
            SELECT DISTINCT location_id, location_name
            FROM packages
            WHERE site_id = @siteId AND location_id IS NOT NULL";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var locationId = reader.GetGuid(0);
            var locationName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.Location,
                locationId,
                $"Location: {locationName}",
                DateTime.UtcNow,
                DateTime.UtcNow);

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphEdge>> BuildMovementEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT 
                id, package_id, from_location_id, to_location_id,
                created_by_user_id, verified_by_user_id, first_approver_id,
                second_approver_id, sales_order_id, transfer_id, processing_job_id,
                created_at
            FROM inventory_movements
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND created_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var movementId = reader.GetGuid(0);
            var packageId = reader.GetGuid(1);
            var fromLocationId = reader.IsDBNull(2) ? (Guid?)null : reader.GetGuid(2);
            var toLocationId = reader.IsDBNull(3) ? (Guid?)null : reader.GetGuid(3);
            var createdByUserId = reader.GetGuid(4);
            var verifiedByUserId = reader.IsDBNull(5) ? (Guid?)null : reader.GetGuid(5);
            var firstApproverId = reader.IsDBNull(6) ? (Guid?)null : reader.GetGuid(6);
            var secondApproverId = reader.IsDBNull(7) ? (Guid?)null : reader.GetGuid(7);
            var salesOrderId = reader.IsDBNull(8) ? (Guid?)null : reader.GetGuid(8);
            var transferId = reader.IsDBNull(9) ? (Guid?)null : reader.GetGuid(9);
            var processingJobId = reader.IsDBNull(10) ? (Guid?)null : reader.GetGuid(10);
            var createdAt = reader.GetDateTime(11);

            // Movement -> Package edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.InvolvesPackage,
                GraphNodeType.InventoryMovement, movementId,
                GraphNodeType.Package, packageId,
                createdAt));

            // Movement -> From Location edge
            if (fromLocationId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.MovedFrom,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.Location, fromLocationId.Value,
                    createdAt));
            }

            // Movement -> To Location edge
            if (toLocationId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.MovedTo,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.Location, toLocationId.Value,
                    createdAt));
            }

            // Movement -> CreatedBy User edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.CreatedBy,
                GraphNodeType.InventoryMovement, movementId,
                GraphNodeType.User, createdByUserId,
                createdAt));

            // Approval edges
            if (verifiedByUserId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.VerifiedBy,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.User, verifiedByUserId.Value,
                    createdAt));
            }

            if (firstApproverId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.ApprovedBy,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.User, firstApproverId.Value,
                    createdAt));
            }

            if (secondApproverId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.SecondApprovedBy,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.User, secondApproverId.Value,
                    createdAt));
            }

            // Related entity edges
            if (salesOrderId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.PartOfSalesOrder,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.SalesOrder, salesOrderId.Value,
                    createdAt));
            }

            if (transferId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.PartOfTransfer,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.Transfer, transferId.Value,
                    createdAt));
            }

            if (processingJobId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.PartOfProcessingJob,
                    GraphNodeType.InventoryMovement, movementId,
                    GraphNodeType.ProcessingJob, processingJobId.Value,
                    createdAt));
            }
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildLineageEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        // Get packages with ancestry
        var sql = @"
            SELECT id, root_ancestor_id, created_at
            FROM packages
            WHERE site_id = @siteId 
              AND root_ancestor_id IS NOT NULL 
              AND root_ancestor_id != id";

        if (sinceTimestamp.HasValue)
            sql += " AND updated_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var packageId = reader.GetGuid(0);
            var rootAncestorId = reader.GetGuid(1);
            var createdAt = reader.GetDateTime(2);

            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.DerivedFrom,
                GraphNodeType.Package, packageId,
                GraphNodeType.Package, rootAncestorId,
                createdAt));
        }

        return edges;
    }
}
