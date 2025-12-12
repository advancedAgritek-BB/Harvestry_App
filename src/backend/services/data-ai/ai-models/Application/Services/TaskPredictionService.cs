using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Predicts task assignments and completion times using graph-based features.
/// Analyzes:
/// - User workload and capacity
/// - Historical completion patterns
/// - Task dependencies and critical paths
/// - User-task type affinity
/// </summary>
public sealed class TaskPredictionService : ITaskPredictionService
{
    private const string ModelVersion = "task-prediction-v1.0";

    private readonly IGraphRepository _graphRepository;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<TaskPredictionService> _logger;

    public TaskPredictionService(
        IGraphRepository graphRepository,
        NpgsqlDataSource dataSource,
        ILogger<TaskPredictionService> logger)
    {
        _graphRepository = graphRepository;
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<AssigneeRecommendation> PredictAssigneeAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var nodeId = GraphNode.FormatNodeId(GraphNodeType.Task, taskId);
        var taskNode = await _graphRepository.GetNodeAsync(nodeId, cancellationToken);

        if (taskNode == null)
        {
            throw new InvalidOperationException($"Task node not found: {taskId}");
        }

        var taskProps = TaskNodeProperties.FromJson(taskNode.PropertiesJson);
        if (taskProps == null)
        {
            throw new InvalidOperationException($"Task properties not found: {taskId}");
        }

        // Get all potential assignees (users with task history for this site)
        var userNodes = await _graphRepository.GetNodesByTypeAsync(
            taskNode.SiteId, GraphNodeType.User, true, cancellationToken);

        if (userNodes.Count == 0)
        {
            return CreateNoAssigneeResult(taskId);
        }

        // Score each potential assignee
        var candidates = new List<(GraphNode User, float Score, string Reason)>();

        foreach (var userNode in userNodes)
        {
            var (score, reason) = await ScoreAssigneeAsync(
                taskNode, taskProps, userNode, cancellationToken);
            candidates.Add((userNode, score, reason));
        }

        // Sort by score descending
        candidates = candidates.OrderByDescending(c => c.Score).ToList();

        if (candidates.Count == 0 || candidates[0].Score <= 0)
        {
            return CreateNoAssigneeResult(taskId);
        }

        var best = candidates[0];
        var userProps = UserNodeProperties.FromJson(best.User.PropertiesJson);

        return new AssigneeRecommendation
        {
            TaskId = taskId,
            RecommendedUserId = best.User.SourceEntityId,
            RecommendedUserName = userProps?.DisplayName ?? "Unknown",
            Confidence = best.Score,
            Reasoning = best.Reason,
            Alternatives = candidates
                .Skip(1)
                .Take(3)
                .Select(c =>
                {
                    var props = UserNodeProperties.FromJson(c.User.PropertiesJson);
                    return new AlternateAssignee(
                        c.User.SourceEntityId,
                        props?.DisplayName ?? "Unknown",
                        c.Score,
                        c.Reason);
                })
                .ToList()
        };
    }

    public async Task<EtaPrediction> PredictEtaAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var nodeId = GraphNode.FormatNodeId(GraphNodeType.Task, taskId);
        var taskNode = await _graphRepository.GetNodeAsync(nodeId, cancellationToken);

        if (taskNode == null)
        {
            throw new InvalidOperationException($"Task node not found: {taskId}");
        }

        var taskProps = TaskNodeProperties.FromJson(taskNode.PropertiesJson);
        if (taskProps == null)
        {
            throw new InvalidOperationException($"Task properties not found: {taskId}");
        }

        // Get historical completion times for similar tasks
        var historicalData = await GetHistoricalCompletionDataAsync(
            taskNode.SiteId, taskProps.TaskType, cancellationToken);

        // Get dependency delays
        var dependencyDelays = await GetDependencyDelaysAsync(
            taskNode, cancellationToken);

        // Calculate predicted duration
        var (predictedDuration, confidence, confidenceInterval) = 
            CalculatePredictedDuration(taskProps, historicalData);

        // Add dependency delays
        var totalDelay = TimeSpan.FromMinutes(dependencyDelays.Sum(d => d.DelayMinutes));
        var adjustedDuration = predictedDuration + totalDelay;

        // Calculate completion time
        var startTime = taskProps.StartedAt ?? DateTime.UtcNow;
        var predictedCompletion = startTime + adjustedDuration;

        // Identify risk factors
        var riskFactors = IdentifyRiskFactors(taskProps, dependencyDelays, historicalData);

        return new EtaPrediction
        {
            TaskId = taskId,
            PredictedCompletionAt = predictedCompletion,
            PredictedDuration = adjustedDuration,
            Confidence = confidence,
            ConfidenceIntervalLow = confidenceInterval.Low,
            ConfidenceIntervalHigh = confidenceInterval.High,
            RiskFactors = riskFactors
        };
    }

    public async Task<IReadOnlyList<CriticalPathTask>> FindCriticalPathAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Finding critical path tasks for site {SiteId}", siteId);

        // Get all active tasks
        var taskNodes = await _graphRepository.GetNodesByTypeAsync(
            siteId, GraphNodeType.Task, true, cancellationToken);

        var activeTasks = taskNodes
            .Where(n =>
            {
                var props = TaskNodeProperties.FromJson(n.PropertiesJson);
                return props != null && 
                       props.Status != "Completed" && 
                       props.Status != "Cancelled";
            })
            .ToList();

        if (activeTasks.Count == 0)
        {
            return Array.Empty<CriticalPathTask>();
        }

        // Get all dependency edges
        var dependencyEdges = await _graphRepository.GetEdgesByTypeAsync(
            siteId, GraphEdgeType.DependsOn, true, cancellationToken);

        // Build dependency graph and calculate impact
        var taskImpact = new Dictionary<string, (int DependentCount, TimeSpan BlockedTime)>();

        foreach (var task in activeTasks)
        {
            var dependentEdges = dependencyEdges
                .Where(e => e.TargetNodeId == task.NodeId)
                .ToList();

            var dependentCount = dependentEdges.Count;
            var blockedTime = TimeSpan.Zero;

            foreach (var edge in dependentEdges)
            {
                var dependentTask = activeTasks.FirstOrDefault(t => t.NodeId == edge.SourceNodeId);
                if (dependentTask != null)
                {
                    var props = TaskNodeProperties.FromJson(dependentTask.PropertiesJson);
                    if (props?.DueDate != null)
                    {
                        var delay = DateTime.UtcNow - props.DueDate.Value;
                        if (delay > TimeSpan.Zero)
                        {
                            blockedTime += delay;
                        }
                    }
                }
            }

            taskImpact[task.NodeId] = (dependentCount, blockedTime);
        }

        // Sort by impact and return top critical tasks
        var criticalTasks = activeTasks
            .Select(t =>
            {
                var props = TaskNodeProperties.FromJson(t.PropertiesJson);
                var (dependentCount, blockedTime) = taskImpact.GetValueOrDefault(t.NodeId, (0, TimeSpan.Zero));

                // Calculate impact score based on dependent count and blocked time
                var impactScore = (float)(dependentCount * 0.3 + 
                    Math.Min(blockedTime.TotalHours, 100) * 0.01);

                return new CriticalPathTask
                {
                    TaskId = t.SourceEntityId,
                    Title = props?.Title ?? "Unknown",
                    DependentTaskCount = dependentCount,
                    TotalBlockedTime = blockedTime,
                    ImpactScore = impactScore
                };
            })
            .Where(t => t.DependentTaskCount > 0 || t.TotalBlockedTime > TimeSpan.Zero)
            .OrderByDescending(t => t.ImpactScore)
            .Take(20)
            .ToList();

        _logger.LogInformation(
            "Found {Count} critical path tasks for site {SiteId}",
            criticalTasks.Count, siteId);

        return criticalTasks;
    }

    private async Task<(float Score, string Reason)> ScoreAssigneeAsync(
        GraphNode taskNode,
        TaskNodeProperties taskProps,
        GraphNode userNode,
        CancellationToken cancellationToken)
    {
        var userProps = UserNodeProperties.FromJson(userNode.PropertiesJson);
        if (userProps == null || !userProps.IsActive)
        {
            return (0f, "User inactive");
        }

        var scores = new List<(float Score, float Weight, string Factor)>();

        // Factor 1: Task type affinity (historical success with this task type)
        var affinityScore = await CalculateTaskTypeAffinityAsync(
            userNode.SourceEntityId, taskProps.TaskType, taskNode.SiteId, cancellationToken);
        scores.Add((affinityScore, 0.30f, "task type experience"));

        // Factor 2: Current workload (lower is better)
        var workloadScore = await CalculateWorkloadScoreAsync(
            userNode.SourceEntityId, taskNode.SiteId, cancellationToken);
        scores.Add((1f - workloadScore, 0.25f, "workload capacity"));

        // Factor 3: Role match
        var roleScore = CalculateRoleMatchScore(taskProps.AssignedToRole, userProps.PrimaryRole);
        scores.Add((roleScore, 0.20f, "role match"));

        // Factor 4: Recent performance
        var performanceScore = CalculatePerformanceScore(userProps);
        scores.Add((performanceScore, 0.15f, "past performance"));

        // Factor 5: Availability (simple - could be enhanced with schedule data)
        var availabilityScore = 0.8f; // Default to available
        scores.Add((availabilityScore, 0.10f, "availability"));

        // Calculate weighted score
        var totalScore = scores.Sum(s => s.Score * s.Weight);

        // Build reasoning
        var topFactors = scores
            .Where(s => s.Score > 0.5f)
            .OrderByDescending(s => s.Score * s.Weight)
            .Take(2)
            .Select(s => s.Factor);

        var reason = topFactors.Any()
            ? $"Recommended based on: {string.Join(", ", topFactors)}"
            : "General availability";

        return (totalScore, reason);
    }

    private async Task<float> CalculateTaskTypeAffinityAsync(
        Guid userId,
        string taskType,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT COUNT(*) as completed_count
            FROM tasks t
            JOIN task_time_entries tte ON t.task_id = tte.task_id
            WHERE t.site_id = @siteId
              AND tte.user_id = @userId
              AND t.task_type = @taskType
              AND t.status = 'Completed'
              AND t.completed_at > NOW() - INTERVAL '90 days'";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        cmd.Parameters.AddWithValue("userId", userId);
        cmd.Parameters.AddWithValue("taskType", taskType);

        try
        {
            var count = await cmd.ExecuteScalarAsync(cancellationToken) as long? ?? 0;

            // Score based on experience (more completions = higher score)
            return Math.Min((float)count / 10f, 1f);
        }
        catch
        {
            return 0.5f; // Default to neutral
        }
    }

    private async Task<float> CalculateWorkloadScoreAsync(
        Guid userId,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT COUNT(*) as active_tasks
            FROM tasks
            WHERE site_id = @siteId
              AND assigned_to_user_id = @userId
              AND status IN ('Pending', 'InProgress')";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        cmd.Parameters.AddWithValue("userId", userId);

        try
        {
            var count = await cmd.ExecuteScalarAsync(cancellationToken) as long? ?? 0;

            // Higher workload = higher score (which we invert)
            // 5+ tasks = fully loaded
            return Math.Min((float)count / 5f, 1f);
        }
        catch
        {
            return 0.5f;
        }
    }

    private float CalculateRoleMatchScore(string? requiredRole, string? userRole)
    {
        if (string.IsNullOrEmpty(requiredRole))
            return 0.5f; // No role requirement

        if (string.IsNullOrEmpty(userRole))
            return 0.3f; // User has no role

        // Exact match
        if (requiredRole.Equals(userRole, StringComparison.OrdinalIgnoreCase))
            return 1.0f;

        // Partial match (e.g., "Senior Cultivator" matches "Cultivator")
        if (userRole.Contains(requiredRole, StringComparison.OrdinalIgnoreCase) ||
            requiredRole.Contains(userRole, StringComparison.OrdinalIgnoreCase))
            return 0.7f;

        return 0.2f;
    }

    private float CalculatePerformanceScore(UserNodeProperties userProps)
    {
        // Based on historical completion rate and time
        var completedTasks = userProps.TotalTasksCompleted;
        var avgCompletionHours = userProps.AvgTaskCompletionHours ?? 24;

        // More completions = higher score
        var completionScore = Math.Min(completedTasks / 50f, 1f);

        // Faster completion = higher score (inversely related to time)
        var speedScore = Math.Max(0f, 1f - (float)(avgCompletionHours / 48f));

        return (completionScore * 0.6f) + (speedScore * 0.4f);
    }

    private async Task<List<TaskHistoricalData>> GetHistoricalCompletionDataAsync(
        Guid siteId,
        string taskType,
        CancellationToken cancellationToken)
    {
        var data = new List<TaskHistoricalData>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        var sql = @"
            SELECT 
                EXTRACT(EPOCH FROM (completed_at - started_at)) / 60 as duration_minutes
            FROM tasks
            WHERE site_id = @siteId
              AND task_type = @taskType
              AND status = 'Completed'
              AND started_at IS NOT NULL
              AND completed_at IS NOT NULL
              AND completed_at > NOW() - INTERVAL '90 days'
            ORDER BY completed_at DESC
            LIMIT 100";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        cmd.Parameters.AddWithValue("taskType", taskType);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var minutes = reader.IsDBNull(0) ? 60 : reader.GetDouble(0);
                data.Add(new TaskHistoricalData { DurationMinutes = (float)minutes });
            }
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Error fetching historical task data");
        }

        return data;
    }

    private async Task<List<DependencyDelay>> GetDependencyDelaysAsync(
        GraphNode taskNode,
        CancellationToken cancellationToken)
    {
        var delays = new List<DependencyDelay>();

        // Get dependency edges for this task
        var dependencyEdges = await _graphRepository.GetOutgoingEdgesAsync(
            taskNode.NodeId, GraphEdgeType.DependsOn, true, cancellationToken);

        foreach (var edge in dependencyEdges)
        {
            var dependencyNode = await _graphRepository.GetNodeAsync(edge.TargetNodeId, cancellationToken);
            if (dependencyNode == null) continue;

            var props = TaskNodeProperties.FromJson(dependencyNode.PropertiesJson);
            if (props == null) continue;

            if (props.Status != "Completed")
            {
                // Estimate remaining time for incomplete dependency
                var estimatedRemaining = props.DueDate.HasValue
                    ? (props.DueDate.Value - DateTime.UtcNow).TotalMinutes
                    : 60; // Default 1 hour

                delays.Add(new DependencyDelay
                {
                    TaskId = dependencyNode.SourceEntityId,
                    DelayMinutes = (int)Math.Max(0, estimatedRemaining)
                });
            }
        }

        return delays;
    }

    private (TimeSpan Duration, float Confidence, (TimeSpan Low, TimeSpan High) Interval) 
        CalculatePredictedDuration(
            TaskNodeProperties taskProps,
            List<TaskHistoricalData> historicalData)
    {
        if (historicalData.Count == 0)
        {
            // No historical data - use default
            return (
                TimeSpan.FromHours(4),
                0.3f,
                (TimeSpan.FromHours(1), TimeSpan.FromHours(8))
            );
        }

        var durations = historicalData.Select(d => d.DurationMinutes).ToList();
        var mean = durations.Average();
        var stdDev = (float)Math.Sqrt(durations.Average(d => Math.Pow(d - mean, 2)));

        // Confidence based on sample size and variance
        var confidence = Math.Min(0.9f, 0.5f + (historicalData.Count / 100f) - (stdDev / mean / 2));
        confidence = Math.Max(0.3f, confidence);

        var predictedMinutes = mean;
        var lowMinutes = mean - (1.96f * stdDev);
        var highMinutes = mean + (1.96f * stdDev);

        return (
            TimeSpan.FromMinutes(predictedMinutes),
            confidence,
            (TimeSpan.FromMinutes(Math.Max(5, lowMinutes)), TimeSpan.FromMinutes(highMinutes))
        );
    }

    private List<string> IdentifyRiskFactors(
        TaskNodeProperties taskProps,
        List<DependencyDelay> dependencyDelays,
        List<TaskHistoricalData> historicalData)
    {
        var risks = new List<string>();

        if (taskProps.IsBlocked)
        {
            risks.Add($"Task is blocked: {taskProps.BlockingReason}");
        }

        if (dependencyDelays.Count > 0)
        {
            var totalDelay = dependencyDelays.Sum(d => d.DelayMinutes);
            risks.Add($"Waiting on {dependencyDelays.Count} dependencies (~{totalDelay / 60:F1}h)");
        }

        if (taskProps.DueDate.HasValue && taskProps.DueDate.Value < DateTime.UtcNow)
        {
            risks.Add("Task is past due date");
        }

        if (historicalData.Count < 5)
        {
            risks.Add("Limited historical data for this task type");
        }

        if (!taskProps.AssignedToUserId.HasValue)
        {
            risks.Add("Task is unassigned");
        }

        return risks;
    }

    private AssigneeRecommendation CreateNoAssigneeResult(Guid taskId)
    {
        return new AssigneeRecommendation
        {
            TaskId = taskId,
            RecommendedUserId = Guid.Empty,
            RecommendedUserName = "No recommendation",
            Confidence = 0f,
            Reasoning = "No eligible assignees found",
            Alternatives = Array.Empty<AlternateAssignee>()
        };
    }

    private sealed class TaskHistoricalData
    {
        public float DurationMinutes { get; init; }
    }

    private sealed class DependencyDelay
    {
        public Guid TaskId { get; init; }
        public int DelayMinutes { get; init; }
    }
}
