using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Harvestry.Tasks.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskPriorityEnum = Harvestry.Tasks.Domain.Enums.TaskPriority;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/tasks")]
public sealed class TasksController : ApiControllerBase
{
    private readonly ITaskLifecycleService _taskLifecycleService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteAuthorizationService _siteAuthorizationService;

    public TasksController(
        ITaskLifecycleService taskLifecycleService,
        IAuthorizationService authorizationService,
        ISiteAuthorizationService siteAuthorizationService,
        ILogger<TasksController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _taskLifecycleService = taskLifecycleService ?? throw new ArgumentNullException(nameof(taskLifecycleService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _siteAuthorizationService = siteAuthorizationService ?? throw new ArgumentNullException(nameof(siteAuthorizationService));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(
        Guid siteId,
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _taskLifecycleService
                .CreateTaskAsync(siteId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetTask), new { siteId, taskId = response.TaskId }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks(
        Guid siteId,
        [FromQuery] TaskStatusEnum? status,
        [FromQuery] Guid? assignedTo,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        var response = await _taskLifecycleService
            .GetTasksBySiteAsync(siteId, status, assignedTo, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("{taskId:guid}")]
    public async Task<ActionResult<TaskResponse>> GetTask(
        Guid siteId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        var task = await _taskLifecycleService
            .GetTaskByIdAsync(siteId, taskId, cancellationToken)
            .ConfigureAwait(false);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPut("{taskId:guid}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(
        Guid siteId,
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        // Check if user has access to the site
        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _taskLifecycleService
                .UpdateTaskAsync(siteId, taskId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPut("{taskId:guid}/assign")]
    public async Task<ActionResult<TaskResponse>> AssignTask(
        Guid siteId,
        Guid taskId,
        [FromBody] AssignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .AssignTaskAsync(siteId, taskId, request, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPost("{taskId:guid}/start")]
    public async Task<ActionResult<TaskWithGatingResponse>> StartTask(
        Guid siteId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .StartTaskAsync(siteId, taskId, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{taskId:guid}/complete")]
    public async Task<ActionResult<TaskResponse>> CompleteTask(
        Guid siteId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .CompleteTaskAsync(siteId, taskId, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{taskId:guid}/cancel")]
    public async Task<ActionResult<TaskResponse>> CancelTask(
        Guid siteId,
        Guid taskId,
        [FromBody] CancelTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .CancelTaskAsync(siteId, taskId, request, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPut("{taskId:guid}/priority")]
    public async Task<ActionResult<TaskResponse>> UpdatePriority(
        Guid siteId,
        Guid taskId,
        [FromBody] TaskPriorityEnum priority,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .UpdatePriorityAsync(siteId, taskId, priority, userId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpGet("{taskId:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<TaskStateHistoryResponse>>> GetHistory(
        Guid siteId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _taskLifecycleService
                .GetTaskHistoryAsync(siteId, taskId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{taskId:guid}/watchers")]
    public async Task<ActionResult<TaskResponse>> AddWatcher(
        Guid siteId,
        Guid taskId,
        [FromBody] ModifyWatcherRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.UserId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Invalid request", "Watcher user identifier is required."));
        }

        try
        {
            var response = await _taskLifecycleService
                .AddWatcherAsync(siteId, taskId, request.UserId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{taskId:guid}/watchers/{watcherUserId:guid}")]
    public async Task<ActionResult<TaskResponse>> RemoveWatcher(
        Guid siteId,
        Guid taskId,
        Guid watcherUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _taskLifecycleService
                .RemoveWatcherAsync(siteId, taskId, watcherUserId, cancellationToken)
                .ConfigureAwait(false);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Task not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetOverdueTasks(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var response = await _taskLifecycleService
            .GetOverdueTasksAsync(siteId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

}
