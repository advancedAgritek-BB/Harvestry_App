using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Packages.API.Controllers;

/// <summary>
/// API controller for Inventory Movement tracking
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly IMovementService _movementService;
    private readonly ILogger<MovementsController> _logger;

    public MovementsController(IMovementService movementService, ILogger<MovementsController> logger)
    {
        _movementService = movementService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MovementListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MovementListResponse>> GetMovements(
        [FromRoute] Guid siteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? movementType = null, [FromQuery] string? status = null,
        [FromQuery] Guid? packageId = null, [FromQuery] Guid? locationId = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null,
        [FromQuery] string? syncStatus = null, CancellationToken cancellationToken = default)
    {
        var result = await _movementService.GetMovementsAsync(siteId, page, pageSize, movementType, status,
            packageId, locationId, fromDate, toDate, syncStatus, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovementDto>> GetMovement([FromRoute] Guid siteId, [FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var movement = await _movementService.GetByIdAsync(siteId, id, cancellationToken);
        return movement == null ? NotFound() : Ok(movement);
    }

    [HttpGet("by-package/{packageId:guid}")]
    [ProducesResponseType(typeof(List<MovementSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovementSummaryDto>>> GetByPackage([FromRoute] Guid siteId, [FromRoute] Guid packageId, CancellationToken cancellationToken = default)
    {
        var movements = await _movementService.GetByPackageAsync(siteId, packageId, cancellationToken);
        return Ok(movements);
    }

    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<MovementSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MovementSummaryDto>>> GetRecent([FromRoute] Guid siteId, [FromQuery] int count = 20, CancellationToken cancellationToken = default)
    {
        var movements = await _movementService.GetRecentAsync(siteId, count, cancellationToken);
        return Ok(movements);
    }

    [HttpPost("batch")]
    [ProducesResponseType(typeof(List<MovementDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<MovementDto>>> CreateBatch([FromRoute] Guid siteId, [FromBody] BatchMovementRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var movements = await _movementService.CreateBatchAsync(siteId, request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetMovements), new { siteId }, movements);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}



