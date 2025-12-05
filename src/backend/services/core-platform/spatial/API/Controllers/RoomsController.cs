using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.API.Contracts;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Exceptions;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Application.ViewModels;
using Harvestry.Spatial.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Spatial.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly ISpatialHierarchyService _spatialHierarchyService;
    public RoomsController(ISpatialHierarchyService spatialHierarchyService)
    {
        _spatialHierarchyService = spatialHierarchyService ?? throw new ArgumentNullException(nameof(spatialHierarchyService));
    }

    [HttpPost]
    public async Task<ActionResult<RoomResponse>> CreateRoom(
        Guid siteId,
        [FromBody] CreateRoomRequest request,
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

        var room = await _spatialHierarchyService.CreateRoomAsync(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetRoom), new { siteId, roomId = room.Id }, room);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> GetRooms(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }
        
        try
        {
            // Authorization is performed by the service layer (same pattern as CreateRoom)
            var rooms = await _spatialHierarchyService.GetRoomsBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
            return Ok(rooms);
        }
        catch (TenantMismatchException)
        {
            return Forbid();
        }
    }

    [HttpGet("{roomId:guid}")]
    public async Task<ActionResult<RoomResponse>> GetRoom(
        Guid siteId,
        Guid roomId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var room = await _spatialHierarchyService.GetRoomWithHierarchyAsync(siteId, roomId, cancellationToken).ConfigureAwait(false);

            if (room is null)
            {
                return NotFound();
            }

            return Ok(room);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Room belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPut("{roomId:guid}")]
    public async Task<ActionResult<RoomResponse>> UpdateRoom(
        Guid siteId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
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
            var room = await _spatialHierarchyService.UpdateRoomAsync(siteId, roomId, command, cancellationToken).ConfigureAwait(false);
            return Ok(room);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Room belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPatch("{roomId:guid}/status")]
    public async Task<ActionResult<RoomResponse>> ChangeRoomStatus(
        Guid siteId,
        Guid roomId,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var room = await _spatialHierarchyService.ChangeRoomStatusAsync(siteId, roomId, request.Status, userId, cancellationToken).ConfigureAwait(false);
            return Ok(room);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Room belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
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
