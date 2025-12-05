using System;
using System.Collections.Generic;
using System.Linq;
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
[Route("api/v1/sites/{siteId:guid}/equipment")]
public sealed class EquipmentController : ControllerBase
{
    private readonly IEquipmentRegistryService _equipmentRegistryService;
    private readonly IValveZoneMappingService _valveZoneMappingService;

    public EquipmentController(
        IEquipmentRegistryService equipmentRegistryService,
        IValveZoneMappingService valveZoneMappingService)
    {
        _equipmentRegistryService = equipmentRegistryService ?? throw new ArgumentNullException(nameof(equipmentRegistryService));
        _valveZoneMappingService = valveZoneMappingService ?? throw new ArgumentNullException(nameof(valveZoneMappingService));
    }

    [HttpPost]
    public async Task<ActionResult<EquipmentResponse>> CreateEquipment(
        Guid siteId,
        [FromBody] CreateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with { SiteId = siteId, RequestedByUserId = userId };
            var equipment = await _equipmentRegistryService.CreateAsync(command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetEquipment), new { siteId, equipmentId = equipment.Id }, equipment);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Tenant mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<EquipmentListResponse>> GetEquipmentList(
        Guid siteId,
        [FromQuery] EquipmentQueryParameters query,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _equipmentRegistryService.GetListAsync(siteId, query.ToListQuery(), cancellationToken).ConfigureAwait(false);
            return Ok(response);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Tenant mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, CreateProblem("Internal error", "An error occurred while retrieving equipment list", StatusCodes.Status500InternalServerError));
        }
    }

    [HttpGet("{equipmentId:guid}")]
    public async Task<ActionResult<EquipmentResponse>> GetEquipment(
        Guid siteId,
        Guid equipmentId,
        [FromQuery] bool includeChannels = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await _equipmentRegistryService.GetByIdAsync(siteId, equipmentId, includeChannels, cancellationToken).ConfigureAwait(false);
            if (equipment is null)
            {
                return NotFound();
            }

            return Ok(equipment);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPut("{equipmentId:guid}")]
    public async Task<ActionResult<EquipmentResponse>> UpdateEquipment(
        Guid siteId,
        Guid equipmentId,
        [FromBody] UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with { RequestedByUserId = userId };
            var equipment = await _equipmentRegistryService.UpdateAsync(siteId, equipmentId, command, cancellationToken).ConfigureAwait(false);
            return Ok(equipment);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPatch("{equipmentId:guid}/status")]
    public async Task<ActionResult<EquipmentResponse>> ChangeStatus(
        Guid siteId,
        Guid equipmentId,
        [FromBody] UpdateEquipmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var equipment = await _equipmentRegistryService.ChangeStatusAsync(siteId, equipmentId, request.Status, userId, cancellationToken).ConfigureAwait(false);
            return Ok(equipment);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPost("{equipmentId:guid}/heartbeat")]
    public async Task<IActionResult> RecordHeartbeat(
        Guid siteId,
        Guid equipmentId,
        [FromBody] RecordHeartbeatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _equipmentRegistryService.RecordHeartbeatAsync(siteId, equipmentId, request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPut("{equipmentId:guid}/network")]
    public async Task<IActionResult> UpdateNetwork(
        Guid siteId,
        Guid equipmentId,
        [FromBody] UpdateNetworkRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with { RequestedByUserId = userId };
            await _equipmentRegistryService.UpdateNetworkAsync(siteId, equipmentId, command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpPost("{equipmentId:guid}/channels")]
    public async Task<ActionResult<EquipmentChannelResponse>> CreateChannel(
        Guid siteId,
        Guid equipmentId,
        [FromBody] CreateEquipmentChannelRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }
        
        // Populate request user ID early and validate it matches authenticated user if provided
        if (request.RequestedByUserId != Guid.Empty && request.RequestedByUserId != userId)
        {
            return BadRequest(CreateProblem("User ID mismatch", "Request body user ID must match authenticated user."));
        }
        
        // Ensure request uses authenticated user ID
        request = request with { RequestedByUserId = userId };
        
        try
        {
            var channel = await _equipmentRegistryService.AddChannelAsync(siteId, equipmentId, request, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetChannels), new { siteId, equipmentId }, channel);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(CreateProblem("Not found", ex.Message, StatusCodes.Status404NotFound));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid request", ex.Message));
        }
    }

    [HttpPost("{equipmentId:guid}/valve-mappings")]
    public async Task<ActionResult<ValveZoneMappingResponse>> CreateValveMapping(
        Guid siteId,
        Guid equipmentId,
        [FromBody] CreateValveZoneMappingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with
            {
                SiteId = siteId,
                EquipmentId = equipmentId,
                RequestedByUserId = userId
            };

            var mapping = await _valveZoneMappingService.CreateAsync(siteId, equipmentId, command, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetValveMappings), new { siteId, equipmentId }, mapping);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid valve mapping request", ex.Message));
        }
    }

    [HttpGet("{equipmentId:guid}/valve-mappings")]
    public async Task<ActionResult<IReadOnlyList<ValveZoneMappingResponse>>> GetValveMappings(
        Guid siteId,
        Guid equipmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var mappings = await _valveZoneMappingService.GetByValveAsync(siteId, equipmentId, cancellationToken).ConfigureAwait(false);
            return Ok(mappings);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("~/api/sites/{siteId:guid}/valve-mappings/{mappingId:guid}")]
    public async Task<ActionResult<ValveZoneMappingResponse>> UpdateValveMapping(
        Guid siteId,
        Guid mappingId,
        [FromBody] UpdateValveZoneMappingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            var command = request with { RequestedByUserId = userId };
            var mapping = await _valveZoneMappingService.UpdateAsync(siteId, mappingId, command, cancellationToken).ConfigureAwait(false);
            return Ok(mapping);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Valve mapping belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(CreateProblem("Invalid valve mapping request", ex.Message));
        }
    }

    [HttpDelete("~/api/sites/{siteId:guid}/valve-mappings/{mappingId:guid}")]
    public async Task<IActionResult> DeleteValveMapping(
        Guid siteId,
        Guid mappingId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            await _valveZoneMappingService.DeleteAsync(siteId, mappingId, userId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Valve mapping belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{equipmentId:guid}/channels")]
    public async Task<ActionResult<IReadOnlyList<EquipmentChannelResponse>>> GetChannels(
        Guid siteId,
        Guid equipmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var channels = await _equipmentRegistryService.GetChannelsAsync(siteId, equipmentId, cancellationToken).ConfigureAwait(false);
            return Ok(channels);
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    [HttpDelete("{equipmentId:guid}/channels/{channelId:guid}")]
    public async Task<IActionResult> DeleteChannel(
        Guid siteId,
        Guid equipmentId,
        Guid channelId,
        CancellationToken cancellationToken)
    {
        var userId = ResolveUserId();
        if (userId == Guid.Empty)
        {
            return BadRequest(CreateProblem("Missing user identifier", "Provide an X-User-Id header with a valid GUID."));
        }

        try
        {
            await _equipmentRegistryService.RemoveChannelAsync(siteId, equipmentId, channelId, userId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (TenantMismatchException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, CreateProblem(
                "Site mismatch",
                $"Equipment belongs to site {ex.ActualSiteId} but request targeted site {ex.ExpectedSiteId}.",
                StatusCodes.Status403Forbidden));
        }
    }

    private Guid ResolveUserId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var values))
        {
            var headerValue = values.FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(headerValue) && Guid.TryParse(headerValue, out var headerId))
            {
                return headerId;
            }
        }

        var claim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("oid")?.Value;
        return Guid.TryParse(claim, out var parsed) ? parsed : Guid.Empty;
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

public sealed record EquipmentQueryParameters
{
    public EquipmentStatus? Status { get; init; }
    public CoreEquipmentType? CoreType { get; init; }
    public Guid? LocationId { get; init; }
    public DateTime? CalibrationDueBefore { get; init; }
    public bool IncludeChannels { get; init; }
    
    private int _page = 1;
    private int _pageSize = 50;
    
    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }
    
    [System.ComponentModel.DataAnnotations.Range(1, 1000)]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 1 : (value > 1000 ? 1000 : value);
    }

    public EquipmentListQuery ToListQuery() => new()
    {
        Status = Status,
        CoreType = CoreType,
        LocationId = LocationId,
        CalibrationDueBefore = CalibrationDueBefore,
        IncludeChannels = IncludeChannels,
        Page = Page,
        PageSize = PageSize
    };
}
