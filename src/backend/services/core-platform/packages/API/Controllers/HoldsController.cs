using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Packages.API.Controllers;

/// <summary>
/// API controller for Hold management
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/holds")]
[Authorize]
public class HoldsController : ControllerBase
{
    private readonly IHoldService _holdService;
    private readonly ILogger<HoldsController> _logger;

    public HoldsController(IHoldService holdService, ILogger<HoldsController> logger)
    {
        _holdService = holdService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<HoldSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HoldSummaryDto>>> GetHolds([FromRoute] Guid siteId, CancellationToken cancellationToken = default)
    {
        var holds = await _holdService.GetHoldsAsync(siteId, cancellationToken);
        return Ok(holds);
    }

    [HttpPost("packages/{packageId:guid}/hold")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PackageDto>> PlaceHold([FromRoute] Guid siteId, [FromRoute] Guid packageId, [FromBody] PlaceHoldRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _holdService.PlaceHoldAsync(siteId, packageId, request, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("packages/{packageId:guid}/approve-release")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PackageDto>> ApproveRelease([FromRoute] Guid siteId, [FromRoute] Guid packageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _holdService.ApproveReleaseAsync(siteId, packageId, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("packages/{packageId:guid}/release-hold")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PackageDto>> ReleaseHold([FromRoute] Guid siteId, [FromRoute] Guid packageId, [FromBody] ReleaseHoldRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _holdService.ReleaseHoldAsync(siteId, packageId, request, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}




