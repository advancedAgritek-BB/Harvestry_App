using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.Extensions.Logging;
using TaskTypeEnum = Harvestry.Tasks.Domain.Enums.TaskType;

namespace Harvestry.Tasks.Application.Services;

public sealed class TaskGenerationService : ITaskGenerationService
{
    private readonly ITaskBlueprintRepository _blueprintRepository;
    private readonly ITaskLifecycleService _taskLifecycleService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TaskGenerationService> _logger;

    public TaskGenerationService(
        ITaskBlueprintRepository blueprintRepository,
        ITaskLifecycleService taskLifecycleService,
        INotificationService notificationService,
        ILogger<TaskGenerationService> logger)
    {
        _blueprintRepository = blueprintRepository ?? throw new ArgumentNullException(nameof(blueprintRepository));
        _taskLifecycleService = taskLifecycleService ?? throw new ArgumentNullException(nameof(taskLifecycleService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<TaskResponse>> GenerateTasksForPhaseChangeAsync(
        Guid siteId,
        Guid batchId,
        Guid? strainId,
        GrowthPhase newPhase,
        BlueprintRoomType roomType,
        Guid triggeredByUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating tasks for phase change: Site={SiteId}, Batch={BatchId}, Phase={Phase}, RoomType={RoomType}",
            siteId, batchId, newPhase, roomType);

        // Find matching blueprints
        var blueprints = await _blueprintRepository
            .GetMatchingBlueprintsAsync(siteId, newPhase, roomType, strainId, cancellationToken)
            .ConfigureAwait(false);

        if (blueprints.Count == 0)
        {
            _logger.LogInformation("No matching blueprints found for phase change");
            return Array.Empty<TaskResponse>();
        }

        _logger.LogInformation("Found {Count} matching blueprints", blueprints.Count);

        var createdTasks = new List<TaskResponse>();

        foreach (var blueprint in blueprints)
        {
            try
            {
                // Calculate due date based on blueprint offset
                DateTimeOffset? dueDate = null;
                if (blueprint.TimeOffset > TimeSpan.Zero)
                {
                    dueDate = DateTimeOffset.UtcNow.Add(blueprint.TimeOffset);
                }

                // Create the task request from the blueprint
                var request = new CreateTaskRequest
                {
                    TaskType = TaskTypeEnum.Custom,
                    CustomTaskType = "BlueprintGenerated",
                    Title = blueprint.Title,
                    Description = blueprint.Description,
                    Priority = blueprint.Priority,
                    DueDate = dueDate,
                    AssignedToRole = blueprint.AssignedToRole,
                    RequiredSopIds = blueprint.RequiredSopIds.ToArray(),
                    RequiredTrainingIds = blueprint.RequiredTrainingIds.ToArray(),
                    RelatedEntityType = "Batch",
                    RelatedEntityId = batchId
                };

                var task = await _taskLifecycleService
                    .CreateTaskAsync(siteId, request, triggeredByUserId, cancellationToken)
                    .ConfigureAwait(false);

                createdTasks.Add(task);

                _logger.LogInformation(
                    "Created task {TaskId} from blueprint {BlueprintId} for batch {BatchId}",
                    task.TaskId, blueprint.Id, batchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create task from blueprint {BlueprintId} for batch {BatchId}",
                    blueprint.Id, batchId);
            }
        }

        _logger.LogInformation(
            "Generated {Count} tasks for phase change on batch {BatchId}",
            createdTasks.Count, batchId);

        return createdTasks;
    }

    public Task<IReadOnlyList<TaskResponse>> ManuallyGenerateTasksAsync(
        Guid siteId,
        Guid batchId,
        Guid? strainId,
        GrowthPhase phase,
        BlueprintRoomType roomType,
        Guid triggeredByUserId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Manually triggering task generation for batch {BatchId} at phase {Phase}",
            batchId, phase);

        return GenerateTasksForPhaseChangeAsync(
            siteId, batchId, strainId, phase, roomType, triggeredByUserId, cancellationToken);
    }
}

