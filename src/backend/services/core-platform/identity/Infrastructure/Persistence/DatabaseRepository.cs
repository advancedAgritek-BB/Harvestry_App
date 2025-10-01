using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Application.DTOs;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Harvestry.Identity.Infrastructure.Persistence;

public sealed class DatabaseRepository : IDatabaseRepository
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRlsContextAccessor _rlsContextAccessor;
    private readonly ILogger<DatabaseRepository> _logger;

    public DatabaseRepository(
        IdentityDbContext dbContext,
        IRlsContextAccessor rlsContextAccessor,
        ILogger<DatabaseRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(bool granted, bool requiresTwoPersonApproval, string? denyReason)> CheckAbacPermissionAsync(
        Guid userId,
        string action,
        string resourceType,
        Guid siteId,
        IReadOnlyDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required", nameof(action));
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ArgumentException("Resource type is required", nameof(resourceType));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required", nameof(siteId));

        await using var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT granted, requires_two_person, deny_reason
            FROM check_abac_permission(
                @user_id,
                @action,
                @resource_type,
                @site_id,
                @context,
                TRUE);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("action", NpgsqlDbType.Varchar).Value = action;
        command.Parameters.Add("resource_type", NpgsqlDbType.Varchar).Value = resourceType;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;
        var payload = context is null
            ? new Dictionary<string, object>()
            : context.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        command.Parameters.Add("context", NpgsqlDbType.Jsonb).Value = JsonUtilities.SerializeDictionary(payload);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("check_abac_permission returned no rows for user {UserId} action {Action}", userId, action);
            return (false, false, "Permission evaluation failed");
        }

        var granted = reader.GetBoolean(reader.GetOrdinal("granted"));
        var requiresTwoPerson = reader.GetBoolean(reader.GetOrdinal("requires_two_person"));
        var denyReason = reader.IsDBNull(reader.GetOrdinal("deny_reason")) ? null : reader.GetString(reader.GetOrdinal("deny_reason"));
        return (granted, requiresTwoPerson, denyReason);
    }

    public async Task<(bool isAllowed, List<TaskGatingRequirement> missingRequirements)> CheckTaskGatingAsync(
        Guid userId,
        string taskType,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required", nameof(userId));
        if (string.IsNullOrWhiteSpace(taskType))
            throw new ArgumentException("Task type is required", nameof(taskType));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required", nameof(siteId));

        await using var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT is_allowed, missing_requirements
            FROM check_task_gating(@user_id, @task_type, @site_id);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("user_id", NpgsqlDbType.Uuid).Value = userId;
        command.Parameters.Add("task_type", NpgsqlDbType.Varchar).Value = taskType;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("check_task_gating returned no rows for user {UserId} task {TaskType}", userId, taskType);
            return (false, new List<TaskGatingRequirement>
            {
                new("system", null, "Task gating evaluation failed")
            });
        }

        var isAllowed = reader.GetBoolean(reader.GetOrdinal("is_allowed"));
        var missingJson = reader["missing_requirements"];
        var requirements = ParseMissingRequirements(missingJson);
        return (isAllowed, requirements);
    }

    public async Task<IReadOnlyCollection<TaskGatingRequirement>> GetTaskGatingRequirementsAsync(
        string taskType,
        Guid? siteId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(taskType))
        {
            throw new ArgumentException("Task type is required", nameof(taskType));
        }

        var connection = await PrepareConnectionAsync(siteId, cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT required_sop_id, required_module_id, required_permission_action
            FROM task_gating_requirements
            WHERE task_type = @task_type
              AND is_active = TRUE
              AND (@site_id IS NULL OR site_id IS NULL OR site_id = @site_id);
        ";

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("task_type", NpgsqlDbType.Varchar).Value = taskType;
        command.Parameters.Add("site_id", NpgsqlDbType.Uuid).Value = siteId ?? (object)DBNull.Value;

        var requirements = new List<TaskGatingRequirement>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (!reader.IsDBNull(reader.GetOrdinal("required_sop_id")))
            {
                requirements.Add(new TaskGatingRequirement("sop", reader.GetGuid(reader.GetOrdinal("required_sop_id")), "SOP signoff required"));
            }

            if (!reader.IsDBNull(reader.GetOrdinal("required_module_id")))
            {
                requirements.Add(new TaskGatingRequirement("training", reader.GetGuid(reader.GetOrdinal("required_module_id")), "Training completion required"));
            }

            if (!reader.IsDBNull(reader.GetOrdinal("required_permission_action")))
            {
                var action = reader.GetString(reader.GetOrdinal("required_permission_action"));
                requirements.Add(new TaskGatingRequirement("permission", null, $"Permission '{action}' required"));
            }
        }

        return requirements;
    }

    private async Task<NpgsqlConnection> PrepareConnectionAsync(Guid? siteScope, CancellationToken cancellationToken)
    {
        var context = _rlsContextAccessor.Current;
        var currentUser = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;
        var role = string.IsNullOrWhiteSpace(context.Role) ? "service_account" : context.Role;
        var effectiveSite = siteScope ?? context.SiteId ?? Guid.Empty;

        var connection = await _dbContext.GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await _dbContext.SetRlsContextAsync(currentUser, role, effectiveSite, cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static List<TaskGatingRequirement> ParseMissingRequirements(object? value)
    {
        var results = new List<TaskGatingRequirement>();
        if (value is null)
        {
            return results;
        }

        var json = value switch
        {
            string s => s,
            System.Text.Json.JsonDocument doc => doc.RootElement.GetRawText(),
            System.Text.Json.JsonElement element => element.GetRawText(),
            _ => value.ToString() ?? "[]"
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            return results;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                return results;
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                var requirement = ConvertElement(element);
                if (requirement != null)
                {
                    results.Add(requirement);
                }
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // Treat as no additional requirements if parsing fails
        }

        return results;
    }

    private static TaskGatingRequirement? ConvertElement(System.Text.Json.JsonElement element)
    {
        var type = element.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        string reason = element.TryGetProperty("reason", out var reasonElement) ? reasonElement.GetString() ?? string.Empty : string.Empty;
        Guid? requirementId = null;

        if (element.TryGetProperty("sop_id", out var sopElement) && sopElement.ValueKind == System.Text.Json.JsonValueKind.String && Guid.TryParse(sopElement.GetString(), out var sopId))
        {
            requirementId = sopId;
        }
        else if (element.TryGetProperty("module_id", out var moduleElement) && moduleElement.ValueKind == System.Text.Json.JsonValueKind.String && Guid.TryParse(moduleElement.GetString(), out var moduleId))
        {
            requirementId = moduleId;
        }

        if (element.TryGetProperty("action", out var actionElement) && actionElement.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            var actionValue = actionElement.GetString();
            if (string.Equals(type, "permission", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(actionValue))
            {
                reason = string.IsNullOrEmpty(reason)
                    ? $"Permission '{actionValue}' not granted"
                    : reason;
            }
        }

        return new TaskGatingRequirement(type!, requirementId, string.IsNullOrWhiteSpace(reason) ? "Requirement not met" : reason);
    }
}
