using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Builds the telemetry/irrigation topology graph.
/// Creates nodes for Zone, Room, Equipment, SensorStream, IrrigationRun, AlertRule, AlertInstance
/// and edges for equipment-zone relationships, irrigation runs, and alert monitoring.
/// </summary>
public sealed class TelemetryGraphBuilder : ITelemetryGraphBuilder
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<TelemetryGraphBuilder> _logger;

    public TelemetryGraphBuilder(
        NpgsqlDataSource dataSource,
        IGraphRepository graphRepository,
        ILogger<TelemetryGraphBuilder> logger)
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
        _logger.LogDebug("Building telemetry graph for site {SiteId}, since {Since}", 
            siteId, sinceTimestamp);

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        // Build sensor stream nodes
        var streamNodes = await BuildSensorStreamNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(streamNodes);

        // Build irrigation run nodes
        var runNodes = await BuildIrrigationRunNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(runNodes);

        // Build zone emitter config nodes
        var emitterNodes = await BuildZoneEmitterNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(emitterNodes);

        // Build alert rule and instance nodes
        var alertNodes = await BuildAlertNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(alertNodes);

        // Build edges
        var streamEdges = await BuildStreamEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(streamEdges);

        var irrigationEdges = await BuildIrrigationEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(irrigationEdges);

        var alertEdges = await BuildAlertEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(alertEdges);

        // Persist to graph store
        await _graphRepository.UpsertNodesAsync(nodes, cancellationToken);
        await _graphRepository.UpsertEdgesAsync(edges, cancellationToken);

        _logger.LogInformation(
            "Built telemetry graph for site {SiteId}: {Nodes} nodes, {Edges} edges",
            siteId, nodes.Count, edges.Count);

        return (nodes.Count, edges.Count);
    }

    private async Task<List<GraphNode>> BuildSensorStreamNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, stream_type, unit, display_name, equipment_id,
                equipment_channel_id, location_id, room_id, zone_id,
                is_active, metadata, created_at, updated_at
            FROM sensor_streams
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
            var streamId = reader.GetGuid(0);
            var streamType = reader.GetInt32(1);
            var displayName = reader.IsDBNull(3) ? "Sensor" : reader.GetString(3);

            var properties = new SensorStreamNodeProperties
            {
                StreamType = streamType,
                StreamTypeName = GetStreamTypeName(streamType),
                Unit = reader.IsDBNull(2) ? "" : reader.GetString(2),
                DisplayName = displayName,
                EquipmentId = reader.GetGuid(4),
                EquipmentChannelId = reader.IsDBNull(5) ? null : reader.GetGuid(5),
                LocationId = reader.IsDBNull(6) ? null : reader.GetGuid(6),
                RoomId = reader.IsDBNull(7) ? null : reader.GetGuid(7),
                ZoneId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                IsActive = reader.GetBoolean(9)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.SensorStream,
                streamId,
                $"Stream: {displayName} ({properties.StreamTypeName})",
                reader.GetDateTime(11),
                reader.GetDateTime(12),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildIrrigationRunNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, program_id, group_id, schedule_id, status,
                total_steps, completed_steps, created_at, started_at,
                completed_at, initiated_by, initiated_by_user_id,
                interlock_type, fault_message
            FROM irrigation_runs
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
            var runId = reader.GetGuid(0);
            var status = reader.GetString(4);

            var properties = new IrrigationRunNodeProperties
            {
                ProgramId = reader.GetGuid(1),
                GroupId = reader.GetGuid(2),
                ScheduleId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
                Status = status,
                TotalSteps = reader.GetInt32(5),
                CompletedSteps = reader.GetInt32(6),
                InitiatedBy = reader.IsDBNull(10) ? "system" : reader.GetString(10),
                InitiatedByUserId = reader.IsDBNull(11) ? null : reader.GetGuid(11),
                InterlockType = reader.IsDBNull(12) ? null : reader.GetString(12),
                FaultMessage = reader.IsDBNull(13) ? null : reader.GetString(13)
            };

            var createdAt = reader.GetDateTime(7);
            var updatedAt = reader.IsDBNull(9) ? createdAt : reader.GetDateTime(9);

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.IrrigationRun,
                runId,
                $"Irrigation Run: {status}",
                createdAt,
                updatedAt,
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildZoneEmitterNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                id, zone_id, zone_name, emitter_count,
                emitter_flow_rate_liters_per_hour, emitter_type,
                emitters_per_plant, operating_pressure_kpa,
                last_calibrated_at, created_at, updated_at
            FROM zone_emitter_configurations
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
            var configId = reader.GetGuid(0);
            var zoneName = reader.IsDBNull(2) ? "Unknown Zone" : reader.GetString(2);
            var emitterFlowRate = reader.GetDecimal(4);
            var emitterCount = reader.GetInt32(3);

            var properties = new ZoneEmitterConfigNodeProperties
            {
                ZoneId = reader.GetGuid(1),
                ZoneName = zoneName,
                EmitterCount = emitterCount,
                EmitterFlowRateLitersPerHour = emitterFlowRate,
                EmitterType = reader.IsDBNull(5) ? "" : reader.GetString(5),
                EmittersPerPlant = reader.IsDBNull(6) ? 1 : reader.GetInt32(6),
                TotalZoneFlowRateLitersPerMinute = emitterCount * emitterFlowRate / 60m,
                OperatingPressureKpa = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                LastCalibratedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.ZoneEmitterConfig,
                configId,
                $"Emitter Config: {zoneName}",
                reader.GetDateTime(9),
                reader.GetDateTime(10),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildAlertNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        // Alert rules
        var rulesSql = @"
            SELECT 
                id, rule_name, rule_type, stream_ids, severity,
                is_active, created_at, updated_at
            FROM alert_rules
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            rulesSql += " AND updated_at >= @since";

        await using var rulesCmd = new NpgsqlCommand(rulesSql, connection);
        rulesCmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            rulesCmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var rulesReader = await rulesCmd.ExecuteReaderAsync(cancellationToken);

        while (await rulesReader.ReadAsync(cancellationToken))
        {
            var ruleId = rulesReader.GetGuid(0);
            var ruleName = rulesReader.GetString(1);

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.AlertRule,
                ruleId,
                $"Alert Rule: {ruleName}",
                rulesReader.GetDateTime(6),
                rulesReader.GetDateTime(7));

            nodes.Add(node);
        }

        await rulesReader.CloseAsync();

        // Alert instances
        var instancesSql = @"
            SELECT 
                id, rule_id, stream_id, severity, fired_at,
                cleared_at, current_value, threshold_value, message
            FROM alert_instances
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            instancesSql += " AND fired_at >= @since";

        await using var instancesCmd = new NpgsqlCommand(instancesSql, connection);
        instancesCmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            instancesCmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var instancesReader = await instancesCmd.ExecuteReaderAsync(cancellationToken);

        while (await instancesReader.ReadAsync(cancellationToken))
        {
            var instanceId = instancesReader.GetGuid(0);
            var severity = instancesReader.GetString(3);
            var firedAt = instancesReader.GetDateTime(4);

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.AlertInstance,
                instanceId,
                $"Alert: {severity}",
                firedAt,
                instancesReader.IsDBNull(5) ? firedAt : instancesReader.GetDateTime(5));

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphEdge>> BuildStreamEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT id, equipment_id, zone_id, room_id, created_at
            FROM sensor_streams
            WHERE site_id = @siteId";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var streamId = reader.GetGuid(0);
            var equipmentId = reader.GetGuid(1);
            var zoneId = reader.IsDBNull(2) ? (Guid?)null : reader.GetGuid(2);
            var roomId = reader.IsDBNull(3) ? (Guid?)null : reader.GetGuid(3);
            var createdAt = reader.GetDateTime(4);

            // Stream -> Equipment edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.AttachedToEquipment,
                GraphNodeType.SensorStream, streamId,
                GraphNodeType.Equipment, equipmentId,
                createdAt));

            // Stream -> Zone edge
            if (zoneId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.MeasuresZone,
                    GraphNodeType.SensorStream, streamId,
                    GraphNodeType.Zone, zoneId.Value,
                    createdAt));
            }

            // Stream -> Room edge
            if (roomId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.LocatedIn,
                    GraphNodeType.SensorStream, streamId,
                    GraphNodeType.Room, roomId.Value,
                    createdAt));
            }
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildIrrigationEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        // Link zone emitter configs to zones
        var emitterSql = @"
            SELECT id, zone_id, created_at
            FROM zone_emitter_configurations
            WHERE site_id = @siteId";

        await using var emitterCmd = new NpgsqlCommand(emitterSql, connection);
        emitterCmd.Parameters.AddWithValue("siteId", siteId);

        await using var emitterReader = await emitterCmd.ExecuteReaderAsync(cancellationToken);

        while (await emitterReader.ReadAsync(cancellationToken))
        {
            var configId = emitterReader.GetGuid(0);
            var zoneId = emitterReader.GetGuid(1);
            var createdAt = emitterReader.GetDateTime(2);

            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.HasEmitters,
                GraphNodeType.Zone, zoneId,
                GraphNodeType.ZoneEmitterConfig, configId,
                createdAt));
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildAlertEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        // Alert instance -> Rule and Stream edges
        var sql = @"
            SELECT id, rule_id, stream_id, fired_at
            FROM alert_instances
            WHERE site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND fired_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var instanceId = reader.GetGuid(0);
            var ruleId = reader.GetGuid(1);
            var streamId = reader.GetGuid(2);
            var firedAt = reader.GetDateTime(3);

            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.FiredForRule,
                GraphNodeType.AlertInstance, instanceId,
                GraphNodeType.AlertRule, ruleId,
                firedAt));

            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.TriggeredByStream,
                GraphNodeType.AlertInstance, instanceId,
                GraphNodeType.SensorStream, streamId,
                firedAt));
        }

        return edges;
    }

    private static string GetStreamTypeName(int streamType)
    {
        return streamType switch
        {
            1 => "Temperature",
            2 => "Humidity",
            3 => "CO2",
            4 => "VPD",
            5 => "Light PAR",
            6 => "Light PPFD",
            10 => "EC",
            11 => "pH",
            12 => "Dissolved Oxygen",
            13 => "Water Temp",
            14 => "Water Level",
            20 => "VWC/Soil Moisture",
            21 => "Soil Temp",
            22 => "Soil EC",
            30 => "Pressure",
            31 => "Flow Rate",
            32 => "Flow Total",
            _ => $"Type {streamType}"
        };
    }
}
