using Harvestry.AiModels.Domain.Enums;

namespace Harvestry.AiModels.Application.Interfaces;

/// <summary>
/// Interface for graph-based anomaly detection services.
/// Supports multiple anomaly detection models for different domains.
/// </summary>
public interface IAnomalyDetectionService
{
    /// <summary>
    /// Score anomalies for all nodes of a given type in a site
    /// </summary>
    Task<AnomalyBatchResult> ScoreAnomaliesAsync(
        Guid siteId,
        GraphNodeType nodeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Score anomaly for a specific node
    /// </summary>
    Task<AnomalyResult> ScoreNodeAnomalyAsync(
        string nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Score anomalies for new/updated nodes (near real-time)
    /// </summary>
    Task<IReadOnlyList<AnomalyResult>> ScoreIncrementalAsync(
        Guid siteId,
        IEnumerable<string> nodeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top anomalies for a site
    /// </summary>
    Task<IReadOnlyList<AnomalyResult>> GetTopAnomaliesAsync(
        Guid siteId,
        int limit = 50,
        GraphNodeType? nodeType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledge an anomaly (mark as reviewed)
    /// </summary>
    Task AcknowledgeAnomalyAsync(
        Guid anomalyId,
        Guid acknowledgedByUserId,
        string? resolutionNotes = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of scoring anomalies for a batch of nodes
/// </summary>
public sealed record AnomalyBatchResult
{
    public Guid SiteId { get; init; }
    public GraphNodeType NodeType { get; init; }
    public int TotalScored { get; init; }
    public int AnomaliesDetected { get; init; }
    public float ThresholdUsed { get; init; }
    public TimeSpan Duration { get; init; }
    public string ModelVersion { get; init; } = string.Empty;
}

/// <summary>
/// Individual anomaly detection result
/// </summary>
public sealed record AnomalyResult
{
    public Guid? Id { get; init; }
    public Guid SiteId { get; init; }
    public string NodeId { get; init; } = string.Empty;
    public string? EdgeId { get; init; }
    public GraphNodeType NodeType { get; init; }
    public string AnomalyType { get; init; } = string.Empty;
    public float Score { get; init; }
    public string Explanation { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, float> FeatureAttributions { get; init; } 
        = new Dictionary<string, float>();
    public DateTime DetectedAt { get; init; }
    public string ModelVersion { get; init; } = string.Empty;
    public bool IsAcknowledged { get; init; }
}

/// <summary>
/// Interface for package/movement anomaly detection
/// </summary>
public interface IMovementAnomalyDetector
{
    /// <summary>
    /// Detect anomalies in package movements
    /// </summary>
    Task<IReadOnlyList<AnomalyResult>> DetectMovementAnomaliesAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Score a single movement for anomaly
    /// </summary>
    Task<AnomalyResult> ScoreMovementAsync(
        Guid movementId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for irrigation response anomaly detection
/// </summary>
public interface IIrrigationAnomalyDetector
{
    /// <summary>
    /// Detect anomalies in irrigation response (VWC not responding to feed)
    /// </summary>
    Task<IReadOnlyList<AnomalyResult>> DetectIrrigationAnomaliesAsync(
        Guid siteId,
        DateTime? sinceTimestamp = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Score a single irrigation run for response anomaly
    /// </summary>
    Task<AnomalyResult> ScoreIrrigationRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for task assignment and ETA prediction
/// </summary>
public interface ITaskPredictionService
{
    /// <summary>
    /// Predict recommended assignee for a task
    /// </summary>
    Task<AssigneeRecommendation> PredictAssigneeAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict estimated completion time for a task
    /// </summary>
    Task<EtaPrediction> PredictEtaAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find critical path tasks that are blocking others
    /// </summary>
    Task<IReadOnlyList<CriticalPathTask>> FindCriticalPathAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Recommended assignee for a task
/// </summary>
public sealed record AssigneeRecommendation
{
    public Guid TaskId { get; init; }
    public Guid RecommendedUserId { get; init; }
    public string RecommendedUserName { get; init; } = string.Empty;
    public float Confidence { get; init; }
    public string Reasoning { get; init; } = string.Empty;
    public IReadOnlyList<AlternateAssignee> Alternatives { get; init; } = Array.Empty<AlternateAssignee>();
}

/// <summary>
/// Alternative assignee option
/// </summary>
public sealed record AlternateAssignee(
    Guid UserId,
    string UserName,
    float Confidence,
    string Reasoning);

/// <summary>
/// ETA prediction for a task
/// </summary>
public sealed record EtaPrediction
{
    public Guid TaskId { get; init; }
    public DateTime PredictedCompletionAt { get; init; }
    public TimeSpan PredictedDuration { get; init; }
    public float Confidence { get; init; }
    public TimeSpan ConfidenceIntervalLow { get; init; }
    public TimeSpan ConfidenceIntervalHigh { get; init; }
    public IReadOnlyList<string> RiskFactors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Task on the critical path
/// </summary>
public sealed record CriticalPathTask
{
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int DependentTaskCount { get; init; }
    public TimeSpan TotalBlockedTime { get; init; }
    public float ImpactScore { get; init; }
}
