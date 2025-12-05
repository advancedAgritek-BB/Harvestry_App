using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Harvestry.Tasks.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/task-generation")]
public sealed class TaskGenerationController : ApiControllerBase
{
    private readonly ITaskGenerationService _generationService;
    private readonly ISiteAuthorizationService _siteAuthorizationService;

    public TaskGenerationController(
        ITaskGenerationService generationService,
        ISiteAuthorizationService siteAuthorizationService,
        ILogger<TaskGenerationController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _generationService = generationService ?? throw new ArgumentNullException(nameof(generationService));
        _siteAuthorizationService = siteAuthorizationService ?? throw new ArgumentNullException(nameof(siteAuthorizationService));
    }

    /// <summary>
    /// Manually triggers task generation based on blueprints for a batch at a specific phase.
    /// Useful for testing or manual task generation.
    /// </summary>
    [HttpPost("trigger")]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> TriggerTaskGeneration(
        Guid siteId,
        [FromBody] TriggerTaskGenerationRequest request,
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
            var tasks = await _generationService
                .ManuallyGenerateTasksAsync(
                    siteId,
                    request.BatchId,
                    request.StrainId,
                    request.Phase,
                    request.RoomType,
                    userId,
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(tasks);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }
}

