using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Transfers.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/transfers/outbound")]
[Authorize]
public sealed class OutboundTransfersController : ControllerBase
{
    private readonly IOutboundTransferService _transferService;

    public OutboundTransfersController(IOutboundTransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<ActionResult<OutboundTransferListResponse>> GetBySite(
        [FromRoute] Guid siteId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _transferService.GetBySiteAsync(siteId, page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{transferId:guid}")]
    public async Task<ActionResult<OutboundTransferDto>> GetById(
        [FromRoute] Guid siteId,
        [FromRoute] Guid transferId,
        CancellationToken cancellationToken = default)
    {
        var result = await _transferService.GetByIdAsync(siteId, transferId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("create-from-shipment")]
    public async Task<ActionResult<OutboundTransferDto>> CreateFromShipment(
        [FromRoute] Guid siteId,
        [FromBody] CreateOutboundTransferFromShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _transferService.CreateFromShipmentAsync(siteId, request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { siteId, transferId = result.Id }, result);
    }

    [HttpPost("{transferId:guid}/ready")]
    public async Task<ActionResult<OutboundTransferDto>> MarkReady(
        [FromRoute] Guid siteId,
        [FromRoute] Guid transferId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _transferService.MarkReadyAsync(siteId, transferId, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{transferId:guid}/submit-to-metrc")]
    public async Task<ActionResult<OutboundTransferDto>> SubmitToMetrc(
        [FromRoute] Guid siteId,
        [FromRoute] Guid transferId,
        [FromBody] SubmitOutboundTransferToMetrcRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _transferService.SubmitToMetrcAsync(siteId, transferId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{transferId:guid}/void")]
    public async Task<ActionResult<OutboundTransferDto>> Void(
        [FromRoute] Guid siteId,
        [FromRoute] Guid transferId,
        [FromBody] VoidOutboundTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _transferService.VoidAsync(siteId, transferId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}

