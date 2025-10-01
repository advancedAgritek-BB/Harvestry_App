using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Spatial.API.Controllers;

[ApiController]
[Route("api/sites/{siteId:guid}/locations")]
public sealed class LocationsController : ControllerBase
{
    private readonly ISpatialHierarchyService _spatialHierarchyService;
    private readonly IValveZoneMappingService _valveZoneMappingService;

    public LocationsController(
        ISpatialHierarchyService spatialHierarchyService,
        IValveZoneMappingService valveZoneMappingService)
    {
        _spatialHierarchyService = spatialHierarchyService ?? throw new ArgumentNullException(nameof(spatialHierarchyService));
        _valveZoneMappingService = valveZoneMappingService ?? throw new ArgumentNullException(nameof(valveZoneMappingService));
    }

    [HttpPost]
    public async Task<ActionResult<InventoryLocationNodeResponse>> CreateLocation(
        Guid siteId,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        var command = request with
        {
            SiteId = siteId,
            RequestedByUserId = userId
        };

        var location = await _spatialHierarchyService.CreateLocationAsync(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetLocationChildren), new { siteId, locationId = location.Id }, location);
    }

    [HttpPut("{locationId:guid}")]
    public async Task<ActionResult<InventoryLocationNodeResponse>> UpdateLocation(
        Guid siteId,
        Guid locationId,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        var command = request with { RequestedByUserId = userId };
        try
        {
            var location = await _spatialHierarchyService.UpdateLocationAsync(siteId, locationId, command, cancellationToken).ConfigureAwait(false);
            return Ok(location);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Location belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpGet("{locationId:guid}/children")]
    public async Task<ActionResult<IReadOnlyList<InventoryLocationNodeResponse>>> GetLocationChildren(
        Guid siteId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var children = await _spatialHierarchyService.GetLocationChildrenAsync(siteId, locationId, cancellationToken).ConfigureAwait(false);
            return Ok(children);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Location belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpGet("{locationId:guid}/path")]
    public async Task<ActionResult<IReadOnlyList<LocationPathSegment>>> GetLocationPath(
        Guid siteId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var path = await _spatialHierarchyService.GetLocationPathAsync(siteId, locationId, cancellationToken).ConfigureAwait(false);
            return Ok(path);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Location belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpGet("{locationId:guid}/valve-mappings")]
    public async Task<ActionResult<IReadOnlyList<ValveZoneMappingResponse>>> GetValveMappingsForLocation(
        Guid siteId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var mappings = await _valveZoneMappingService.GetByZoneAsync(siteId, locationId, cancellationToken).ConfigureAwait(false);
            return Ok(mappings);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Location belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem(
                "Resource not found",
                ex.Message,
                StatusCodes.Status404NotFound));
        }
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<IActionResult> DeleteLocation(
        Guid siteId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            await _spatialHierarchyService.DeleteLocationAsync(siteId, locationId, userId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Location belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    private Guid ResolveUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var values) && Guid.TryParse(values, out var headerId))
        {
            return headerId;
        }

        var claim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("oid")?.Value;
        if (Guid.TryParse(claim, out var claimId))
        {
            return claimId;
        }

        return Guid.Empty;
    }

    private ProblemDetails CreateProblem(string title, string detail, int statusCode = StatusCodes.Status400BadRequest)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode
        };

        if (HttpContext != null)
        {
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        }

        return problem;
    }

}
