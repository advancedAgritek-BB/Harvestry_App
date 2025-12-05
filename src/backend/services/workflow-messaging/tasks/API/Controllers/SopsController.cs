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
[Route("api/v1/orgs/{orgId:guid}/sops")]
public sealed class SopsController : ApiControllerBase
{
    private readonly ISopService _sopService;

    public SopsController(
        ISopService sopService,
        ILogger<SopsController> logger,
        IConfiguration configuration)
        : base(logger, configuration)
    {
        _sopService = sopService ?? throw new ArgumentNullException(nameof(sopService));
    }

    [HttpPost]
    public async Task<ActionResult<SopResponse>> CreateSop(
        Guid orgId,
        [FromBody] CreateSopRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _sopService
                .CreateSopAsync(orgId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetSop), new { orgId, sopId = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SopSummaryResponse>>> GetSops(
        Guid orgId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var response = await _sopService
            .GetSopsByOrgAsync(orgId, activeOnly, category, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("{sopId:guid}")]
    public async Task<ActionResult<SopResponse>> GetSop(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        var sop = await _sopService
            .GetSopByIdAsync(orgId, sopId, cancellationToken)
            .ConfigureAwait(false);

        if (sop is null)
        {
            return NotFound();
        }

        return Ok(sop);
    }

    [HttpPut("{sopId:guid}")]
    public async Task<ActionResult<SopResponse>> UpdateSop(
        Guid orgId,
        Guid sopId,
        [FromBody] UpdateSopRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _sopService
                .UpdateSopAsync(orgId, sopId, request, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("SOP not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPost("{sopId:guid}/activate")]
    public async Task<ActionResult<SopResponse>> ActivateSop(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _sopService
                .ActivateSopAsync(orgId, sopId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("SOP not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpPost("{sopId:guid}/deactivate")]
    public async Task<ActionResult<SopResponse>> DeactivateSop(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        try
        {
            var response = await _sopService
                .DeactivateSopAsync(orgId, sopId, userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("SOP not found", ex.Message, StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{sopId:guid}")]
    public async Task<ActionResult> DeleteSop(
        Guid orgId,
        Guid sopId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized(CreateProblem("Unauthorized", "Valid user credentials are required."));
        }

        await _sopService
            .DeleteSopAsync(orgId, sopId, userId, cancellationToken)
            .ConfigureAwait(false);

        return NoContent();
    }
}

