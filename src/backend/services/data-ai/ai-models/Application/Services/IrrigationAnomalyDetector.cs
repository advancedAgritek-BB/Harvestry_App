using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Detects anomalies in irrigation response patterns.
/// Identifies when VWC is not responding appropriately to irrigation events,
/// indicating potential issues with:
/// - Clogged emitters
/// - Sensor malfunction
/// - Substrate problems
/// - Zone configuration errors
/// </summary>
public sealed class IrrigationAnomalyDetector : IIrrigationAnomalyDetector
{
    private const string ModelVersion = "irrigation-anomaly-v1.0";
    private const float DefaultThreshold = 0.6f;

    private readonly IGraphRepository _graphRepository;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<IrrigationAnomalyDetector> _logger;

    public IrrigationAnomalyDetector(
        IGraphRepository graphRepository,
        NpgsqlDataSource dataSource,
        ILogger<IrrigationAnomalyDetector> logger)
    {
        _graphRepository = graphRepository;
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AnomalyResult>> DetectIrrigationAnomaliesAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Detecting irrigation anomalies for site {SiteId}", siteId);

        // Get irrigation run nodes
        var runNodes = await _graphRepository.GetNodesByTypeAsync(
            siteId, GraphNodeType.IrrigationRun, true, cancellationToken);

        if (runNodes.Count == 0)
        {
            _logger.LogDebug("No irrigation run nodes found for site {SiteId}", siteId);
            return Array.Empty<AnomalyResult>();
        }

        // Filter by timestamp if provided
        if (sinceTimestamp.HasValue)
        {
            runNodes = runNodes
                .Where(n => n.SourceCreatedAt >= sinceTimestamp.Value)
                .ToList();
        }

        // Build zone response baseline
        var zoneBaselines = await BuildZoneBaselinesAsync(siteId, cancellationToken);

        var results = new List<AnomalyResult>();

        foreach (var node in runNodes)
        {
            var props = IrrigationRunNodeProperties.FromJson(node.PropertiesJson);
            if (props == null || props.Status != "Completed") continue;

            // Get VWC response data for this run
            var responseData = await GetIrrigationResponseDataAsync(
                node.SourceEntityId, props.TargetZoneIds, cancellationToken);

            foreach (var zoneResponse in responseData)
            {
                var baseline = zoneBaselines.GetValueOrDefault(zoneResponse.ZoneId);
                var (score, features) = ScoreIrrigationResponse(zoneResponse, baseline);

                if (score >= DefaultThreshold)
                {
                    var explanation = GenerateExplanation(zoneResponse, features);

                    results.Add(new AnomalyResult
                    {
                        SiteId = siteId,
                        NodeId = node.NodeId,
                        NodeType = GraphNodeType.IrrigationRun,
                        AnomalyType = "irrigation_response",
                        Score = score,
                        Explanation = explanation,
                        FeatureAttributions = features,
                        DetectedAt = DateTime.UtcNow,
                        ModelVersion = ModelVersion
                    });
                }
            }
        }

        _logger.LogInformation(
            "Detected {Count} irrigation anomalies in {Total} runs for site {SiteId}",
            results.Count, runNodes.Count, siteId);

        return results;
    }

    public async Task<AnomalyResult> ScoreIrrigationRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default)
    {
        var nodeId = GraphNode.FormatNodeId(GraphNodeType.IrrigationRun, runId);
        var node = await _graphRepository.GetNodeAsync(nodeId, cancellationToken);

        if (node == null)
        {
            throw new InvalidOperationException($"Irrigation run node not found: {runId}");
        }

        var props = IrrigationRunNodeProperties.FromJson(node.PropertiesJson);
        if (props == null)
        {
            return CreateNoDataResult(node.SiteId, nodeId);
        }

        var zoneBaselines = await BuildZoneBaselinesAsync(node.SiteId, cancellationToken);
        var responseData = await GetIrrigationResponseDataAsync(
            runId, props.TargetZoneIds, cancellationToken);

        var aggregateScore = 0f;
        var aggregateFeatures = new Dictionary<string, float>();

        foreach (var zoneResponse in responseData)
        {
            var baseline = zoneBaselines.GetValueOrDefault(zoneResponse.ZoneId);
            var (score, features) = ScoreIrrigationResponse(zoneResponse, baseline);

            aggregateScore = Math.Max(aggregateScore, score);
            foreach (var (key, value) in features)
            {
                aggregateFeatures[$"{zoneResponse.ZoneId}_{key}"] = value;
            }
        }

        return new AnomalyResult
        {
            SiteId = node.SiteId,
            NodeId = nodeId,
            NodeType = GraphNodeType.IrrigationRun,
            AnomalyType = "irrigation_response",
            Score = aggregateScore,
            Explanation = GenerateAggregateExplanation(responseData, aggregateScore),
            FeatureAttributions = aggregateFeatures,
            DetectedAt = DateTime.UtcNow,
            ModelVersion = ModelVersion
        };
    }

    private (float Score, Dictionary<string, float> Features) ScoreIrrigationResponse(
        ZoneIrrigationResponse response,
        ZoneResponseBaseline? baseline)
    {
        var features = new Dictionary<string, float>();

        // Feature 1: VWC response ratio (actual vs expected)
        var responseRatioScore = ScoreResponseRatio(response, baseline);
        features["response_ratio"] = responseRatioScore;

        // Feature 2: Time to peak VWC
        var timeToPeakScore = ScoreTimeToPeak(response, baseline);
        features["time_to_peak"] = timeToPeakScore;

        // Feature 3: Response consistency with neighbors
        var neighborScore = ScoreNeighborConsistency(response, baseline);
        features["neighbor_consistency"] = neighborScore;

        // Feature 4: Command execution confirmation
        var commandScore = ScoreCommandExecution(response);
        features["command_execution"] = commandScore;

        // Feature 5: Sensor drift detection
        var sensorDriftScore = ScoreSensorDrift(response, baseline);
        features["sensor_drift"] = sensorDriftScore;

        // Feature 6: Historical pattern deviation
        var patternScore = ScorePatternDeviation(response, baseline);
        features["pattern_deviation"] = patternScore;

        // Weighted combination
        var weights = new Dictionary<string, float>
        {
            ["response_ratio"] = 0.30f,
            ["time_to_peak"] = 0.15f,
            ["neighbor_consistency"] = 0.15f,
            ["command_execution"] = 0.15f,
            ["sensor_drift"] = 0.10f,
            ["pattern_deviation"] = 0.15f
        };

        var totalScore = features.Sum(f => f.Value * weights.GetValueOrDefault(f.Key, 0.1f));
        totalScore = Math.Clamp(totalScore, 0f, 1f);

        return (totalScore, features);
    }

    private float ScoreResponseRatio(ZoneIrrigationResponse response, ZoneResponseBaseline? baseline)
    {
        if (response.ExpectedVwcIncrease <= 0)
            return 0f;

        var actualIncrease = response.VwcAfter - response.VwcBefore;
        var ratio = actualIncrease / response.ExpectedVwcIncrease;

        // Expected ratio is around 1.0 (actual matches expected)
        // Low ratio = VWC not responding (clog, sensor issue)
        // High ratio = over-response (unexpected, possible sensor issue)

        if (ratio < 0.3m)
        {
            // Very low response - highly anomalous
            return 0.9f;
        }
        else if (ratio < 0.5m)
        {
            return 0.7f;
        }
        else if (ratio < 0.7m)
        {
            return 0.4f;
        }
        else if (ratio > 2.0m)
        {
            // Over-response
            return 0.6f;
        }
        else if (ratio > 1.5m)
        {
            return 0.3f;
        }

        return 0f;
    }

    private float ScoreTimeToPeak(ZoneIrrigationResponse response, ZoneResponseBaseline? baseline)
    {
        if (!response.TimeToPeakSeconds.HasValue || baseline == null)
            return 0f;

        var expected = baseline.MeanTimeToPeakSeconds;
        if (expected <= 0)
            return 0f;

        var deviation = Math.Abs(response.TimeToPeakSeconds.Value - expected) / expected;

        // More than 3x expected time is very suspicious
        if (deviation > 3)
            return 0.8f;
        if (deviation > 2)
            return 0.5f;
        if (deviation > 1)
            return 0.3f;

        return 0f;
    }

    private float ScoreNeighborConsistency(ZoneIrrigationResponse response, ZoneResponseBaseline? baseline)
    {
        // If we have neighbor zone data, compare responses
        // Zones with similar setups should have similar responses
        // This would require additional data about neighboring zones

        // Placeholder - would be enhanced with actual neighbor comparison
        return 0f;
    }

    private float ScoreCommandExecution(ZoneIrrigationResponse response)
    {
        // Check if device commands were acknowledged
        if (!response.CommandAcknowledged)
        {
            return 0.7f;
        }

        // Check if flow was detected
        if (!response.FlowDetected)
        {
            return 0.8f;
        }

        return 0f;
    }

    private float ScoreSensorDrift(ZoneIrrigationResponse response, ZoneResponseBaseline? baseline)
    {
        if (baseline == null)
            return 0f;

        // Check if baseline VWC is significantly different from historical
        var baselineDeviation = Math.Abs(response.VwcBefore - baseline.MeanBaselineVwc);
        var normalRange = baseline.StdDevBaselineVwc * 2;

        if (baselineDeviation > normalRange * 2)
        {
            return 0.7f;
        }
        else if (baselineDeviation > normalRange)
        {
            return 0.3f;
        }

        return 0f;
    }

    private float ScorePatternDeviation(ZoneIrrigationResponse response, ZoneResponseBaseline? baseline)
    {
        if (baseline == null || baseline.HistoricalResponseCount < 10)
            return 0f;

        // Compare this response to historical pattern for this zone
        var responseIncrease = response.VwcAfter - response.VwcBefore;
        var expectedIncrease = baseline.MeanVwcIncrease;
        var stdDev = baseline.StdDevVwcIncrease;

        if (stdDev <= 0)
            return 0f;

        var zScore = Math.Abs((float)(responseIncrease - expectedIncrease) / stdDev);

        if (zScore > 3)
            return 0.8f;
        if (zScore > 2)
            return 0.5f;
        if (zScore > 1.5f)
            return 0.2f;

        return 0f;
    }

    private async Task<Dictionary<Guid, ZoneResponseBaseline>> BuildZoneBaselinesAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var baselines = new Dictionary<Guid, ZoneResponseBaseline>();

        // Query historical irrigation response data
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT 
                zec.zone_id,
                AVG(EXTRACT(EPOCH FROM (ir.completed_at - ir.started_at))) as mean_duration,
                STDDEV(EXTRACT(EPOCH FROM (ir.completed_at - ir.started_at))) as stddev_duration,
                COUNT(*) as response_count
            FROM zone_emitter_configurations zec
            JOIN irrigation_runs ir ON ir.site_id = zec.site_id
            WHERE zec.site_id = @siteId
              AND ir.status = 'Completed'
              AND ir.completed_at > NOW() - INTERVAL '30 days'
            GROUP BY zec.zone_id";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var zoneId = reader.GetGuid(0);
                var meanDuration = reader.IsDBNull(1) ? 0f : (float)reader.GetDouble(1);

                baselines[zoneId] = new ZoneResponseBaseline
                {
                    ZoneId = zoneId,
                    MeanTimeToPeakSeconds = meanDuration,
                    HistoricalResponseCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                };
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Error building zone baselines, using defaults");
        }

        return baselines;
    }

    private async Task<IReadOnlyList<ZoneIrrigationResponse>> GetIrrigationResponseDataAsync(
        Guid runId,
        Guid[] targetZoneIds,
        CancellationToken cancellationToken)
    {
        var responses = new List<ZoneIrrigationResponse>();

        // This would query telemetry data to get VWC readings before/after irrigation
        // For now, return placeholder data structure
        foreach (var zoneId in targetZoneIds)
        {
            responses.Add(new ZoneIrrigationResponse
            {
                ZoneId = zoneId,
                RunId = runId,
                VwcBefore = 0m,
                VwcAfter = 0m,
                ExpectedVwcIncrease = 0m,
                CommandAcknowledged = true,
                FlowDetected = true
            });
        }

        return responses;
    }

    private string GenerateExplanation(ZoneIrrigationResponse response, Dictionary<string, float> features)
    {
        var issues = new List<string>();

        if (features.GetValueOrDefault("response_ratio") > 0.5f)
        {
            var actualIncrease = response.VwcAfter - response.VwcBefore;
            issues.Add($"VWC response {actualIncrease:F1}% vs expected {response.ExpectedVwcIncrease:F1}%");
        }

        if (features.GetValueOrDefault("command_execution") > 0.5f)
        {
            issues.Add("Irrigation command may not have executed properly");
        }

        if (features.GetValueOrDefault("sensor_drift") > 0.5f)
        {
            issues.Add("Possible sensor calibration issue");
        }

        if (issues.Count == 0)
        {
            return "Irrigation response anomaly detected";
        }

        return string.Join("; ", issues);
    }

    private string GenerateAggregateExplanation(
        IReadOnlyList<ZoneIrrigationResponse> responses,
        float score)
    {
        var zonesWithIssues = responses.Count(r => r.VwcAfter <= r.VwcBefore);

        if (zonesWithIssues > 0)
        {
            return $"{zonesWithIssues} of {responses.Count} zones showed no VWC response to irrigation";
        }

        return $"Irrigation response anomaly detected (score: {score:F2})";
    }

    private AnomalyResult CreateNoDataResult(Guid siteId, string nodeId)
    {
        return new AnomalyResult
        {
            SiteId = siteId,
            NodeId = nodeId,
            NodeType = GraphNodeType.IrrigationRun,
            AnomalyType = "irrigation_response",
            Score = 0f,
            Explanation = "Insufficient data for anomaly detection",
            FeatureAttributions = new Dictionary<string, float>(),
            DetectedAt = DateTime.UtcNow,
            ModelVersion = ModelVersion
        };
    }

    private sealed class ZoneIrrigationResponse
    {
        public Guid ZoneId { get; init; }
        public Guid RunId { get; init; }
        public decimal VwcBefore { get; init; }
        public decimal VwcAfter { get; init; }
        public decimal ExpectedVwcIncrease { get; init; }
        public int? TimeToPeakSeconds { get; init; }
        public bool CommandAcknowledged { get; init; }
        public bool FlowDetected { get; init; }
    }

    private sealed class ZoneResponseBaseline
    {
        public Guid ZoneId { get; init; }
        public float MeanTimeToPeakSeconds { get; set; }
        public float MeanBaselineVwc { get; set; }
        public float StdDevBaselineVwc { get; set; }
        public float MeanVwcIncrease { get; set; }
        public float StdDevVwcIncrease { get; set; }
        public int HistoricalResponseCount { get; set; }
    }
}
