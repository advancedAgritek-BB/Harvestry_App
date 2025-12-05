using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// API surface for site-wide propagation settings and overrides.
/// </summary>
[ApiController]
[Route("api/v1/genetics/propagation")]
[Authorize]
public sealed class PropagationController : ControllerBase
{
    private readonly IMotherHealthService _motherHealthService;

    public PropagationController(IMotherHealthService motherHealthService)
    {
        _motherHealthService = motherHealthService;
    }

    [HttpGet("settings")]
    [ProducesResponseType(typeof(PropagationSettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PropagationSettingsResponse>> GetSettings(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var settings = await _motherHealthService.GetPropagationSettingsAsync(siteId, cancellationToken).ConfigureAwait(false);
        return Ok(settings);
    }

    [HttpPut("settings")]
    [ProducesResponseType(typeof(PropagationSettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PropagationSettingsResponse>> UpdateSettings(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] UpdatePropagationSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await _motherHealthService.UpdatePropagationSettingsAsync(siteId, request, userId, cancellationToken).ConfigureAwait(false);
        return Ok(settings);
    }

    [HttpGet("overrides")]
    [ProducesResponseType(typeof(IReadOnlyList<PropagationOverrideResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PropagationOverrideResponse>>> GetOverrides(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromQuery] PropagationOverrideStatus? status,
        CancellationToken cancellationToken)
    {
        var overrides = await _motherHealthService.GetPropagationOverridesAsync(siteId, status, cancellationToken).ConfigureAwait(false);
        return Ok(overrides);
    }

    [HttpPost("overrides")]
    [ProducesResponseType(typeof(PropagationOverrideResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PropagationOverrideResponse>> CreateOverride(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] CreatePropagationOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.RequestPropagationOverrideAsync(siteId, request, userId, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetOverrides), new { status = response.Status }, response);
    }

    [HttpPost("overrides/{overrideId}/decision")]
    [ProducesResponseType(typeof(PropagationOverrideResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PropagationOverrideResponse>> DecideOverride(
        Guid overrideId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        [FromBody] PropagationOverrideDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _motherHealthService.DecidePropagationOverrideAsync(siteId, overrideId, request, userId, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
