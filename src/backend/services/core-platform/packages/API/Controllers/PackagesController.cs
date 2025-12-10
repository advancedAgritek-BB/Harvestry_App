using Harvestry.Packages.Application.DTOs;
using Harvestry.Packages.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Packages.API.Controllers;

/// <summary>
/// API controller for Package (Lot) management
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/packages")]
[Authorize]
public class PackagesController : ControllerBase
{
    private readonly IPackageService _packageService;
    private readonly ILogger<PackagesController> _logger;

    public PackagesController(IPackageService packageService, ILogger<PackagesController> logger)
    {
        _packageService = packageService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PackageListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageListResponse>> GetPackages(
        [FromRoute] Guid siteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null, [FromQuery] string? labTestingState = null,
        [FromQuery] string? inventoryCategory = null, [FromQuery] string? itemCategory = null,
        [FromQuery] Guid? locationId = null, [FromQuery] string? search = null,
        [FromQuery] bool? onHold = null, [FromQuery] bool? expiringSoon = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _packageService.GetPackagesAsync(siteId, page, pageSize, status, labTestingState,
            inventoryCategory, itemCategory, locationId, search, onHold, expiringSoon, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> GetPackage([FromRoute] Guid siteId, [FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var package = await _packageService.GetByIdAsync(siteId, id, cancellationToken);
        return package == null ? NotFound() : Ok(package);
    }

    [HttpGet("by-label/{label}")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> GetPackageByLabel([FromRoute] Guid siteId, [FromRoute] string label, CancellationToken cancellationToken = default)
    {
        var package = await _packageService.GetByLabelAsync(siteId, label, cancellationToken);
        return package == null ? NotFound() : Ok(package);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PackageDto>> CreatePackage([FromRoute] Guid siteId, [FromBody] CreatePackageRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var package = await _packageService.CreateAsync(siteId, request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetPackage), new { siteId, id = package.Id }, package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> UpdatePackage([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] UpdatePackageRequest request, CancellationToken cancellationToken = default)
    {
        var package = await _packageService.UpdateAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
        return package == null ? NotFound() : Ok(package);
    }

    [HttpPost("{id:guid}/adjust")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> AdjustPackage([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] AdjustPackageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _packageService.AdjustAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/move")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> MovePackage([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] MovePackageRequest request, CancellationToken cancellationToken = default)
    {
        var package = await _packageService.MoveAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
        return package == null ? NotFound() : Ok(package);
    }

    [HttpPost("{id:guid}/reserve")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> ReserveQuantity([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] ReserveQuantityRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _packageService.ReserveAsync(siteId, id, request, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/unreserve")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageDto>> UnreserveQuantity([FromRoute] Guid siteId, [FromRoute] Guid id, [FromBody] decimal quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _packageService.UnreserveAsync(siteId, id, quantity, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : Ok(package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("split")]
    [ProducesResponseType(typeof(List<PackageDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<List<PackageDto>>> SplitPackage([FromRoute] Guid siteId, [FromBody] SplitPackageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var packages = await _packageService.SplitAsync(siteId, request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetPackages), new { siteId }, packages);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("merge")]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PackageDto>> MergePackages([FromRoute] Guid siteId, [FromBody] MergePackagesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var package = await _packageService.MergeAsync(siteId, request, GetCurrentUserId(), cancellationToken);
            return package == null ? NotFound() : CreatedAtAction(nameof(GetPackage), new { siteId, id = package.Id }, package);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}/lineage")]
    [ProducesResponseType(typeof(PackageLineageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageLineageDto>> GetLineage([FromRoute] Guid siteId, [FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var lineage = await _packageService.GetLineageAsync(siteId, id, cancellationToken);
        return lineage == null ? NotFound() : Ok(lineage);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(PackageSummaryStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageSummaryStatsDto>> GetSummary([FromRoute] Guid siteId, CancellationToken cancellationToken = default)
    {
        var summary = await _packageService.GetSummaryStatsAsync(siteId, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("expiring")]
    [ProducesResponseType(typeof(List<ExpiringPackageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ExpiringPackageDto>>> GetExpiring([FromRoute] Guid siteId, [FromQuery] int withinDays = 30, CancellationToken cancellationToken = default)
    {
        var expiring = await _packageService.GetExpiringAsync(siteId, withinDays, cancellationToken);
        return Ok(expiring);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}



