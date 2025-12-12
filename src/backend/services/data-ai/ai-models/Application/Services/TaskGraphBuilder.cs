using Harvestry.AiModels.Application.Interfaces;
using Harvestry.AiModels.Domain.Entities;
using Harvestry.AiModels.Domain.Enums;
using Harvestry.AiModels.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Builds the task/work graph from the tasks domain.
/// Creates nodes for Task, TimeEntry, User, Team, SOP
/// and edges for dependencies, assignments, time logging.
/// </summary>
public sealed class TaskGraphBuilder : ITaskGraphBuilder
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IGraphRepository _graphRepository;
    private readonly ILogger<TaskGraphBuilder> _logger;

    public TaskGraphBuilder(
        NpgsqlDataSource dataSource,
        IGraphRepository graphRepository,
        ILogger<TaskGraphBuilder> logger)
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
        _logger.LogDebug("Building task graph for site {SiteId}, since {Since}", 
            siteId, sinceTimestamp);

        var nodes = new List<GraphNode>();
        var edges = new List<GraphEdge>();

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        // Build task nodes
        var taskNodes = await BuildTaskNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(taskNodes);

        // Build time entry nodes
        var timeEntryNodes = await BuildTimeEntryNodesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        nodes.AddRange(timeEntryNodes);

        // Build user nodes (unique users from tasks)
        var userNodes = await BuildUserNodesAsync(connection, siteId, cancellationToken);
        nodes.AddRange(userNodes);

        // Build edges
        var dependencyEdges = await BuildDependencyEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(dependencyEdges);

        var assignmentEdges = await BuildAssignmentEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(assignmentEdges);

        var timeEntryEdges = await BuildTimeEntryEdgesAsync(connection, siteId, sinceTimestamp, cancellationToken);
        edges.AddRange(timeEntryEdges);

        // Persist to graph store
        await _graphRepository.UpsertNodesAsync(nodes, cancellationToken);
        await _graphRepository.UpsertEdgesAsync(edges, cancellationToken);

        _logger.LogInformation(
            "Built task graph for site {SiteId}: {Nodes} nodes, {Edges} edges",
            siteId, nodes.Count, edges.Count);

        return (nodes.Count, edges.Count);
    }

    private async Task<List<GraphNode>> BuildTaskNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                task_id, task_type, custom_task_type, title, status,
                priority, assigned_to_user_id, assigned_to_role,
                due_date, started_at, completed_at,
                related_entity_type, related_entity_id, blocking_reason,
                created_at, updated_at
            FROM tasks
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
            var taskId = reader.GetGuid(0);
            var title = reader.IsDBNull(3) ? "Untitled Task" : reader.GetString(3);
            var status = reader.IsDBNull(4) ? "Unknown" : reader.GetString(4);

            var properties = new TaskNodeProperties
            {
                TaskType = reader.IsDBNull(1) ? "" : reader.GetString(1),
                CustomTaskType = reader.IsDBNull(2) ? null : reader.GetString(2),
                Title = title,
                Status = status,
                Priority = reader.IsDBNull(5) ? "" : reader.GetString(5),
                AssignedToUserId = reader.IsDBNull(6) ? null : reader.GetGuid(6),
                AssignedToRole = reader.IsDBNull(7) ? null : reader.GetString(7),
                DueDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                StartedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                CompletedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                RelatedEntityType = reader.IsDBNull(11) ? null : reader.GetString(11),
                RelatedEntityId = reader.IsDBNull(12) ? null : reader.GetGuid(12),
                BlockingReason = reader.IsDBNull(13) ? null : reader.GetString(13),
                IsBlocked = !reader.IsDBNull(13)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.Task,
                taskId,
                $"Task: {title}",
                reader.GetDateTime(14),
                reader.GetDateTime(15),
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildTimeEntryNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        var sql = @"
            SELECT 
                tte.task_time_entry_id, tte.task_id, tte.user_id,
                tte.started_at, tte.ended_at, tte.notes
            FROM task_time_entries tte
            JOIN tasks t ON tte.task_id = t.task_id
            WHERE t.site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND tte.started_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var entryId = reader.GetGuid(0);
            var startedAt = reader.GetDateTime(3);
            var endedAt = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4);

            var durationMinutes = endedAt.HasValue
                ? (int)(endedAt.Value - startedAt).TotalMinutes
                : 0;

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.TimeEntry,
                entryId,
                $"Time Entry: {durationMinutes} min",
                startedAt,
                endedAt ?? startedAt);

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphNode>> BuildUserNodesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var nodes = new List<GraphNode>();

        // Get unique users from tasks
        var sql = @"
            SELECT DISTINCT u.user_id, u.display_name, u.email, u.is_active
            FROM users u
            JOIN tasks t ON (t.assigned_to_user_id = u.user_id 
                OR t.created_by_user_id = u.user_id 
                OR t.assigned_by_user_id = u.user_id)
            WHERE t.site_id = @siteId";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var userId = reader.GetGuid(0);
            var displayName = reader.IsDBNull(1) ? "Unknown User" : reader.GetString(1);

            var properties = new UserNodeProperties
            {
                DisplayName = displayName,
                Email = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsActive = reader.IsDBNull(3) ? true : reader.GetBoolean(3)
            };

            var node = GraphNode.Create(
                siteId,
                GraphNodeType.User,
                userId,
                $"User: {displayName}",
                DateTime.UtcNow,
                DateTime.UtcNow,
                properties.ToJson());

            nodes.Add(node);
        }

        return nodes;
    }

    private async Task<List<GraphEdge>> BuildDependencyEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT 
                td.task_id, td.depends_on_task_id, td.dependency_type,
                td.is_blocking, t.created_at
            FROM task_dependencies td
            JOIN tasks t ON td.task_id = t.task_id
            WHERE t.site_id = @siteId";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var taskId = reader.GetGuid(0);
            var dependsOnTaskId = reader.GetGuid(1);
            var dependencyType = reader.IsDBNull(2) ? "FinishToStart" : reader.GetString(2);
            var isBlocking = reader.IsDBNull(3) ? false : reader.GetBoolean(3);
            var createdAt = reader.GetDateTime(4);

            var properties = new TaskDependencyEdgeProperties
            {
                DependencyType = dependencyType,
                IsBlocking = isBlocking
            };

            var edge = GraphEdge.Create(
                siteId, GraphEdgeType.DependsOn,
                GraphNodeType.Task, taskId,
                GraphNodeType.Task, dependsOnTaskId,
                createdAt,
                weight: isBlocking ? 2.0f : 1.0f,
                propertiesJson: properties.ToJson());

            edges.Add(edge);
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildAssignmentEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT 
                task_id, assigned_to_user_id, assigned_by_user_id,
                created_by_user_id, assigned_at, created_at
            FROM tasks
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
            var taskId = reader.GetGuid(0);
            var assignedToUserId = reader.IsDBNull(1) ? (Guid?)null : reader.GetGuid(1);
            var assignedByUserId = reader.IsDBNull(2) ? (Guid?)null : reader.GetGuid(2);
            var createdByUserId = reader.GetGuid(3);
            var assignedAt = reader.IsDBNull(4) ? reader.GetDateTime(5) : reader.GetDateTime(4);
            var createdAt = reader.GetDateTime(5);

            // Task -> AssignedTo User edge
            if (assignedToUserId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.AssignedTo,
                    GraphNodeType.Task, taskId,
                    GraphNodeType.User, assignedToUserId.Value,
                    assignedAt));
            }

            // Task -> AssignedBy User edge
            if (assignedByUserId.HasValue)
            {
                edges.Add(GraphEdge.Create(
                    siteId, GraphEdgeType.AssignedBy,
                    GraphNodeType.Task, taskId,
                    GraphNodeType.User, assignedByUserId.Value,
                    assignedAt));
            }

            // Task -> CreatedBy User edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.CreatedBy,
                GraphNodeType.Task, taskId,
                GraphNodeType.User, createdByUserId,
                createdAt));
        }

        return edges;
    }

    private async Task<List<GraphEdge>> BuildTimeEntryEdgesAsync(
        NpgsqlConnection connection,
        Guid siteId,
        DateTime? sinceTimestamp,
        CancellationToken cancellationToken)
    {
        var edges = new List<GraphEdge>();

        var sql = @"
            SELECT 
                tte.task_time_entry_id, tte.task_id, tte.user_id, tte.started_at
            FROM task_time_entries tte
            JOIN tasks t ON tte.task_id = t.task_id
            WHERE t.site_id = @siteId";

        if (sinceTimestamp.HasValue)
            sql += " AND tte.started_at >= @since";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("siteId", siteId);
        if (sinceTimestamp.HasValue)
            cmd.Parameters.AddWithValue("since", sinceTimestamp.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var entryId = reader.GetGuid(0);
            var taskId = reader.GetGuid(1);
            var userId = reader.GetGuid(2);
            var startedAt = reader.GetDateTime(3);

            // TimeEntry -> Task edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.TimeEntryFor,
                GraphNodeType.TimeEntry, entryId,
                GraphNodeType.Task, taskId,
                startedAt));

            // TimeEntry -> User edge
            edges.Add(GraphEdge.Create(
                siteId, GraphEdgeType.LoggedBy,
                GraphNodeType.TimeEntry, entryId,
                GraphNodeType.User, userId,
                startedAt));
        }

        return edges;
    }
}
