using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Builds the genetics/crop steering graph.
/// Creates nodes for Strain, CropSteeringProfile, ResponseCurve, Harvest, LabTestBatch
/// and edges for strain relationships, harvests, and test results.
/// </summary>
public sealed class GeneticsGraphBuilder : IGeneticsGraphBuilder
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<GeneticsGraphBuilder> _logger;

    public GeneticsGraphBuilder(
        NpgsqlDataSource dataSource,
        IGraphRepository graphRepository,
        ILogger<GeneticsGraphBuilder> logger)
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
        _logger.LogDebug("Building genetics graph for site {SiteId}, since {Since}", 
            siteId, sinceTimestamp);

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        // Build strain nodes
        var strainNodes = await BuildStrainNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(strainNodes);

        // Build steering profile nodes
        var profileNodes = await BuildSteeringProfileNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(profileNodes);

        // Build harvest nodes
        var harvestNodes = await BuildHarvestNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(harvestNodes);

        // Build lab test batch nodes
        var labTestNodes = await BuildLabTestNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(labTestNodes);

        // Build edges
        var strainEdges = await BuildStrainEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(strainEdges);

        var harvestEdges = await BuildHarvestEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(harvestEdges);

        var labTestEdges = await BuildLabTestEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(labTestEdges);

        // Persist to graph store
        await _graphRepository.UpsertNodesAsync(nodes, cancellationToken);
        await _graphRepository.UpsertEdgesAsync(edges, cancellationToken);

        _logger.LogInformation(
            "Built genetics graph for site {SiteId}: {Nodes} nodes, {Edges} edges",
            siteId, nodes.Count, edges.Count);

        return (nodes.Count, edges.Count);
    }

    private async Task<List<GraphNode>> BuildStrainNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                strain_id, name, breeder, seed_bank, genetic_classification,
                testing_status, nominal_thc_percent, nominal_cbd_percent,
                expected_harvest_window_days, crop_steering_profile_id,
                metrc_strain_id, created_at, updated_at
            FROM strains
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
            var strainId = reader.GetGuid(0);
            var name = reader.GetString(1);

            var properties = new StrainNodeProperties
            {
                Name = name,
                Breeder = reader.IsDBNull(2) ? null : reader.GetString(2),
                SeedBank = reader.IsDBNull(3) ? null : reader.GetString(3),
                GeneticClassification = reader.IsDBNull(4) ? "" : reader.GetString(4),
                TestingStatus = reader.IsDBNull(5) ? "" : reader.GetString(5),
                NominalThcPercent = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                NominalCbdPercent = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                ExpectedHarvestWindowDays = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                CropSteeringProfileId = reader.IsDBNull(9) ? null : reader.GetGuid(9),
                HasCustomSteeringProfile = !reader.IsDBNull(9),
                MetrcStrainId = reader.IsDBNull(10) ? null : reader.GetInt64(10)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.Strain,
                strainId,
                $"Strain: {name}",
                reader.GetDateTime(11),
                reader.GetDateTime(12),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildSteeringProfileNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, strain_id, name, target_mode, is_active,
                configuration, created_at, updated_at
            FROM crop_steering_profiles
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
            var profileId = reader.GetGuid(0);
            var name = reader.IsDBNull(2) ? "Profile" : reader.GetString(2);

            var properties = new SteeringProfileNodeProperties
            {
                Name = name,
                StrainId = reader.IsDBNull(1) ? null : reader.GetGuid(1),
                TargetMode = reader.IsDBNull(3) ? "" : reader.GetString(3),
                IsActive = reader.IsDBNull(4) ? true : reader.GetBoolean(4),
                IsSiteDefault = reader.IsDBNull(1)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.CropSteeringProfile,
                profileId,
                $"Profile: {name}",
                reader.GetDateTime(6),
                reader.GetDateTime(7),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildHarvestNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, harvest_name, harvest_type, status, current_phase,
                strain_id, strain_name, plant_count,
                total_wet_weight_grams, total_dry_weight_grams,
                metrc_harvest_id, created_at, updated_at
            FROM harvests
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
            var harvestId = reader.GetGuid(0);
            var harvestName = reader.IsDBNull(1) ? "Harvest" : reader.GetString(1);

            var properties = new HarvestNodeProperties
            {
                HarvestName = harvestName,
                HarvestType = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Phase = reader.IsDBNull(4) ? "" : reader.GetString(4),
                StrainId = reader.IsDBNull(5) ? null : reader.GetGuid(5),
                StrainName = reader.IsDBNull(6) ? null : reader.GetString(6),
                PlantCount = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                TotalWetWeightGrams = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                TotalDryWeightGrams = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                MetrcHarvestId = reader.IsDBNull(10) ? null : reader.GetInt64(10)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.Harvest,
                harvestId,
                $"Harvest: {harvestName}",
                reader.GetDateTime(11),
                reader.GetDateTime(12),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildLabTestNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, batch_number, lab_name, status, passed,
                thc_percent, cbd_percent, total_cannabinoids,
                requires_remediation, sample_collected_at, results_received_at,
                created_at, updated_at
            FROM lab_test_batches
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
            var batchId = reader.GetGuid(0);
            var batchNumber = reader.IsDBNull(1) ? "Batch" : reader.GetString(1);

            var properties = new LabTestBatchNodeProperties
            {
                BatchNumber = batchNumber,
                LabName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Status = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Passed = reader.IsDBNull(4) ? null : reader.GetBoolean(4),
                ThcPercent = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                CbdPercent = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                TotalCannabinoids = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                RequiresRemediation = reader.IsDBNull(8) ? false : reader.GetBoolean(8),
                SampleCollectedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                ResultsReceivedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.LabTestBatch,
                batchId,
                $"Lab Test: {batchNumber}",
                reader.GetDateTime(11),
                reader.GetDateTime(12),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphEdge>> BuildStrainEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        // Strain -> CropSteeringProfile edges
        var sql = @"
            SELECT strain_id, crop_steering_profile_id, updated_at
            FROM strains
            WHERE site_id = @siteId AND crop_steering_profile_id IS NOT NULL";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var strainId = reader.GetGuid(0);
            var profileId = reader.GetGuid(1);
            var updatedAt = reader.GetDateTime(2);

            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.HasSteeringProfile,
                GraphNodeType.Strain, strainId,
                GraphNodeType.CropSteeringProfile, profileId,
                updatedAt));
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildHarvestEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT id, strain_id, created_at
            FROM harvests
            WHERE site_id = @siteId AND strain_id IS NOT NULL";

        if (sinceTimestamp.HasValue)
            sql += " AND updated_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var harvestId = reader.GetGuid(0);
            var strainId = reader.GetGuid(1);
            var createdAt = reader.GetDateTime(2);

            // Harvest -> Strain edge (what strain was harvested)
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.OfStrain,
                GraphNodeType.Harvest, harvestId,
                GraphNodeType.Strain, strainId,
                createdAt));
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildLabTestEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        // This assumes there's a package_id on lab_test_batches - adjust based on actual schema
        var sql = @"
            SELECT id, package_id, created_at
            FROM lab_test_batches
            WHERE site_id = @siteId AND package_id IS NOT NULL";

        if (sinceTimestamp.HasValue)
            sql += " AND updated_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var batchId = reader.GetGuid(0);
                var packageId = reader.GetGuid(1);
                var createdAt = reader.GetDateTime(2);

                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.TestedPackage,
                    GraphNodeType.LabTestBatch, batchId,
                    GraphNodeType.Package, packageId,
                    createdAt));
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "42703") // Column not found
        {
            _logger.LogDebug("package_id column not found on lab_test_batches, skipping lab test edges");
        }

        return edges;
    }
}
