using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/task-blueprints")]
public sealed class TaskBlueprintsController : ApiControllerBase
{
    private readonly ITaskBlueprintService _blueprintService;
    private readonly ISiteAuthorizationService _siteAuthorizationService;

    public TaskBlueprintsController(
        ITaskBlueprintService blueprintService,
        ISiteAuthorizationService siteAuthorizationService,
        ILogger<TaskBlueprintsController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _blueprintService = blueprintService ?? throw new ArgumentNullException(nameof(blueprintService));
        _siteAuthorizationService = siteAuthorizationService ?? throw new ArgumentNullException(nameof(siteAuthorizationService));
    }

    [HttpPost]
    public async Task<ActionResult<TaskBlueprintResponse>> CreateBlueprint(
        Guid siteId,
        [FromBody] CreateTaskBlueprintRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _blueprintService
                .CreateBlueprintAsync(siteId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetBlueprint), new { siteId, blueprintId = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskBlueprintResponse>>> GetBlueprints(
        Guid siteId,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        var response = await _blueprintService
            .GetBlueprintsBySiteAsync(siteId, activeOnly, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("{blueprintId:guid}")]
    public async Task<ActionResult<TaskBlueprintResponse>> GetBlueprint(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        var blueprint = await _blueprintService
            .GetBlueprintByIdAsync(siteId, blueprintId, cancellationToken)
            .ConfigureAwait(false);

        if (blueprint is null)
        {
            return NotFound();
        }

        return Ok(blueprint);
    }

    [HttpPut("{blueprintId:guid}")]
    public async Task<ActionResult<TaskBlueprintResponse>> UpdateBlueprint(
        Guid siteId,
        Guid blueprintId,
        [FromBody] UpdateTaskBlueprintRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _blueprintService
                .UpdateBlueprintAsync(siteId, blueprintId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Blueprint not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPost("{blueprintId:guid}/activate")]
    public async Task<ActionResult<TaskBlueprintResponse>> ActivateBlueprint(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _blueprintService
                .ActivateBlueprintAsync(siteId, blueprintId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Blueprint not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{blueprintId:guid}/deactivate")]
    public async Task<ActionResult<TaskBlueprintResponse>> DeactivateBlueprint(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        try
        {
            var response = await _blueprintService
                .DeactivateBlueprintAsync(siteId, blueprintId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Blueprint not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{blueprintId:guid}")]
    public async Task<ActionResult> DeleteBlueprint(
        Guid siteId,
        Guid blueprintId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        if (!await _siteAuthorizationService.HasSiteAccessAsync(userId, siteId, cancellationToken))
        {
            return Forbid();
        }

        await _blueprintService
            .DeleteBlueprintAsync(siteId, blueprintId, userId, cancellationToken)
            .ConfigureAwait(false);

        return NoContent();
    }
}

