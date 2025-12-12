using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Detects anomalies in package movements using graph-based features.
/// Uses unsupervised anomaly detection based on:
/// - Unusual movement paths (location transitions)
/// - Abnormal quantity changes
/// - Suspicious approval patterns
/// - Atypical user behavior
/// - Lineage anomalies
/// </summary>
public sealed class MovementAnomalyDetector : IMovementAnomalyDetector
{
    private const string ModelVersion = "movement-anomaly-v1.0";
    private const float DefaultThreshold = 0.7f;

    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<MovementAnomalyDetector> _logger;

    public MovementAnomalyDetector(
        IGraphRepository graphRepository,
        ILogger<MovementAnomalyDetector> logger)
    {
        _graphRepository = graphRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AnomalyResult>> DetectMovementAnomaliesAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Detecting movement anomalies for site {SiteId}", siteId);

        // Get all movement nodes
        var movementNodes = await _graphRepository.GetNodesByTypeAsync(
            siteId, GraphNodeType.InventoryMovement, true, cancellationToken);

        if (movementNodes.Count == 0)
        {
            _logger.LogDebug("No movement nodes found for site {SiteId}", siteId);
            return Array.Empty<AnomalyResult>();
        }

        // Filter by timestamp if provided
        if (sinceTimestamp.HasValue)
        {
            movementNodes = movementNodes
                .Where(n => n.SourceCreatedAt >= sinceTimestamp.Value)
                .ToList();
        }

        // Build baseline statistics from historical data
        var baselineStats = await BuildBaselineStatisticsAsync(siteId, cancellationToken);

        var results = new List<AnomalyResult>();

        foreach (var node in movementNodes)
        {
            var (score, features) = await ScoreMovementNodeAsync(node, baselineStats, cancellationToken);

            if (score >= DefaultThreshold)
            {
                var explanation = GenerateExplanation(features);

                results.Add(new AnomalyResult
                {
                    SiteId = siteId,
                    NodeId = node.NodeId,
                    NodeType = GraphNodeType.InventoryMovement,
                    AnomalyType = "movement",
                    Score = score,
                    Explanation = explanation,
                    FeatureAttributions = features,
                    DetectedAt = DateTime.UtcNow,
                    ModelVersion = ModelVersion
                });

                // Update node with anomaly score
                node.SetAnomalyScore(score, explanation);
                await _graphRepository.UpsertNodeAsync(node, cancellationToken);
            }
        }

        _logger.LogInformation(
            "Detected {Count} movement anomalies in {Total} movements for site {SiteId}",
            results.Count, movementNodes.Count, siteId);

        return results;
    }

    public async Task<AnomalyResult> ScoreMovementAsync(
        Guid movementId,
        CancellationToken cancellationToken = default)
    {
        var nodeId = GraphNode.FormatNodeId(GraphNodeType.InventoryMovement, movementId);
        var node = await _graphRepository.GetNodeAsync(nodeId, cancellationToken);

        if (node == null)
        {
            throw new InvalidOperationException($"Movement node not found: {movementId}");
        }

        var baselineStats = await BuildBaselineStatisticsAsync(node.SiteId, cancellationToken);
        var (score, features) = await ScoreMovementNodeAsync(node, baselineStats, cancellationToken);

        var explanation = GenerateExplanation(features);

        return new AnomalyResult
        {
            SiteId = node.SiteId,
            NodeId = node.NodeId,
            NodeType = GraphNodeType.InventoryMovement,
            AnomalyType = "movement",
            Score = score,
            Explanation = explanation,
            FeatureAttributions = features,
            DetectedAt = DateTime.UtcNow,
            ModelVersion = ModelVersion
        };
    }

    private async Task<(float Score, Dictionary<string, float> Features)> ScoreMovementNodeAsync(
        GraphNode node,
        MovementBaselineStatistics baseline,
        CancellationToken cancellationToken)
    {
        var features = new Dictionary<string, float>();
        var properties = MovementNodeProperties.FromJson(node.PropertiesJson);

        if (properties == null)
        {
            return (0f, features);
        }

        // Feature 1: Unusual movement type for this item category
        var movementTypeScore = ScoreMovementType(properties, baseline);
        features["movement_type_rarity"] = movementTypeScore;

        // Feature 2: Quantity anomaly (unusually large or small)
        var quantityScore = ScoreQuantity(properties, baseline);
        features["quantity_anomaly"] = quantityScore;

        // Feature 3: Approval pattern anomaly
        var approvalScore = ScoreApprovalPattern(properties, baseline);
        features["approval_pattern"] = approvalScore;

        // Feature 4: Time-of-day anomaly
        var timeScore = ScoreTimeOfDay(node.SourceCreatedAt, baseline);
        features["time_anomaly"] = timeScore;

        // Feature 5: Location path anomaly (get edges)
        var locationScore = await ScoreLocationPathAsync(node, baseline, cancellationToken);
        features["location_path"] = locationScore;

        // Feature 6: User behavior anomaly
        var userScore = await ScoreUserBehaviorAsync(properties, baseline, cancellationToken);
        features["user_behavior"] = userScore;

        // Combine features with weighted average
        var weights = new Dictionary<string, float>
        {
            ["movement_type_rarity"] = 0.15f,
            ["quantity_anomaly"] = 0.25f,
            ["approval_pattern"] = 0.20f,
            ["time_anomaly"] = 0.10f,
            ["location_path"] = 0.15f,
            ["user_behavior"] = 0.15f
        };

        var totalScore = features.Sum(f => f.Value * weights.GetValueOrDefault(f.Key, 0.1f));
        totalScore = Math.Clamp(totalScore, 0f, 1f);

        return (totalScore, features);
    }

    private float ScoreMovementType(MovementNodeProperties properties, MovementBaselineStatistics baseline)
    {
        // Score based on rarity of this movement type
        if (baseline.MovementTypeCounts.TryGetValue(properties.MovementType, out var count))
        {
            var frequency = (float)count / baseline.TotalMovements;
            // Rare movement types get higher scores
            return 1f - Math.Min(frequency * 10f, 1f);
        }
        // Unknown movement type is highly suspicious
        return 0.9f;
    }

    private float ScoreQuantity(MovementNodeProperties properties, MovementBaselineStatistics baseline)
    {
        if (baseline.MeanQuantity <= 0 || baseline.StdDevQuantity <= 0)
            return 0f;

        // Z-score based anomaly
        var zScore = Math.Abs((float)properties.Quantity - baseline.MeanQuantity) / baseline.StdDevQuantity;

        // Convert Z-score to 0-1 range (Z > 3 is ~0.99 anomaly)
        return (float)(1.0 - Math.Exp(-zScore / 2.0));
    }

    private float ScoreApprovalPattern(MovementNodeProperties properties, MovementBaselineStatistics baseline)
    {
        float score = 0f;

        // Flag: Required approval but no approver
        if (properties.RequiresApproval && !properties.FirstApproverId.HasValue)
        {
            score += 0.5f;
        }

        // Flag: Same user created and approved
        if (properties.FirstApproverId.HasValue && 
            properties.FirstApproverId.Value == properties.CreatedByUserId)
        {
            score += 0.4f;
        }

        // Flag: Dual approval required but same approvers
        if (properties.FirstApproverId.HasValue && 
            properties.SecondApproverId.HasValue &&
            properties.FirstApproverId.Value == properties.SecondApproverId.Value)
        {
            score += 0.6f;
        }

        return Math.Min(score, 1f);
    }

    private float ScoreTimeOfDay(DateTime createdAt, MovementBaselineStatistics baseline)
    {
        var hour = createdAt.Hour;

        // Score higher for off-hours (outside 6am-8pm)
        if (hour < 6 || hour > 20)
        {
            return 0.5f + ((hour < 6 ? 6 - hour : hour - 20) * 0.1f);
        }

        return 0f;
    }

    private async Task<float> ScoreLocationPathAsync(
        GraphNode node,
        MovementBaselineStatistics baseline,
        CancellationToken cancellationToken)
    {
        // Get movement's location edges
        var edges = await _graphRepository.GetOutgoingEdgesAsync(
            node.NodeId, 
            edgeType: null, 
            activeOnly: true, 
            cancellationToken);

        var fromEdge = edges.FirstOrDefault(e => e.EdgeType == GraphEdgeType.MovedFrom);
        var toEdge = edges.FirstOrDefault(e => e.EdgeType == GraphEdgeType.MovedTo);

        if (fromEdge == null || toEdge == null)
            return 0f;

        // Check if this location transition is common
        var pathKey = $"{fromEdge.TargetNodeId}->{toEdge.TargetNodeId}";
        if (baseline.LocationTransitionCounts.TryGetValue(pathKey, out var count))
        {
            var frequency = (float)count / baseline.TotalMovements;
            return 1f - Math.Min(frequency * 20f, 1f);
        }

        // Unknown path is moderately suspicious
        return 0.6f;
    }

    private Task<float> ScoreUserBehaviorAsync(
        MovementNodeProperties properties,
        MovementBaselineStatistics baseline,
        CancellationToken cancellationToken)
    {
        // Score based on user's typical behavior
        if (baseline.UserMovementCounts.TryGetValue(properties.CreatedByUserId, out var userCount))
        {
            // Users with very few movements are more suspicious
            if (userCount < 5)
                return Task.FromResult(0.4f);
        }
        else
        {
            // Unknown user is suspicious
            return Task.FromResult(0.7f);
        }

        return Task.FromResult(0f);
    }

    private async Task<MovementBaselineStatistics> BuildBaselineStatisticsAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var stats = new MovementBaselineStatistics();

        var nodes = await _graphRepository.GetNodesByTypeAsync(
            siteId, GraphNodeType.InventoryMovement, true, cancellationToken);

        stats.TotalMovements = nodes.Count;

        if (nodes.Count == 0)
            return stats;

        var quantities = new List<float>();

        foreach (var node in nodes)
        {
            var props = MovementNodeProperties.FromJson(node.PropertiesJson);
            if (props == null) continue;

            // Movement type counts
            stats.MovementTypeCounts.TryGetValue(props.MovementType, out var mtCount);
            stats.MovementTypeCounts[props.MovementType] = mtCount + 1;

            // User counts
            stats.UserMovementCounts.TryGetValue(props.CreatedByUserId, out var uCount);
            stats.UserMovementCounts[props.CreatedByUserId] = uCount + 1;

            quantities.Add((float)props.Quantity);
        }

        // Calculate quantity statistics
        if (quantities.Count > 0)
        {
            stats.MeanQuantity = quantities.Average();
            stats.StdDevQuantity = (float)Math.Sqrt(
                quantities.Average(q => Math.Pow(q - stats.MeanQuantity, 2)));
        }

        // Get location transitions from edges
        var edges = await _graphRepository.GetEdgesByTypeAsync(
            siteId, GraphEdgeType.MovedFrom, true, cancellationToken);

        var toEdges = await _graphRepository.GetEdgesByTypeAsync(
            siteId, GraphEdgeType.MovedTo, true, cancellationToken);

        var toEdgesBySource = toEdges.ToDictionary(e => e.SourceNodeId);

        foreach (var fromEdge in edges)
        {
            if (toEdgesBySource.TryGetValue(fromEdge.SourceNodeId, out var toEdge))
            {
                var pathKey = $"{fromEdge.TargetNodeId}->{toEdge.TargetNodeId}";
                stats.LocationTransitionCounts.TryGetValue(pathKey, out var count);
                stats.LocationTransitionCounts[pathKey] = count + 1;
            }
        }

        return stats;
    }

    private string GenerateExplanation(Dictionary<string, float> features)
    {
        var significantFeatures = features
            .Where(f => f.Value > 0.3f)
            .OrderByDescending(f => f.Value)
            .Take(3)
            .ToList();

        if (significantFeatures.Count == 0)
            return "Low-level anomaly detected";

        var explanations = significantFeatures.Select(f => f.Key switch
        {
            "movement_type_rarity" => $"Rare movement type (score: {f.Value:F2})",
            "quantity_anomaly" => $"Unusual quantity (score: {f.Value:F2})",
            "approval_pattern" => $"Suspicious approval pattern (score: {f.Value:F2})",
            "time_anomaly" => $"Off-hours activity (score: {f.Value:F2})",
            "location_path" => $"Unusual location path (score: {f.Value:F2})",
            "user_behavior" => $"Atypical user behavior (score: {f.Value:F2})",
            _ => $"{f.Key}: {f.Value:F2}"
        });

        return string.Join("; ", explanations);
    }

    private sealed class MovementBaselineStatistics
    {
        public int TotalMovements { get; set; }
        public float MeanQuantity { get; set; }
        public float StdDevQuantity { get; set; }
        public Dictionary<string, int> MovementTypeCounts { get; } = new();
        public Dictionary<Guid, int> UserMovementCounts { get; } = new();
        public Dictionary<string, int> LocationTransitionCounts { get; } = new();
    }
}
