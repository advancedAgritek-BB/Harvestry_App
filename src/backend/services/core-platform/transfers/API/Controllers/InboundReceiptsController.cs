using Harvestry.Transfers.Application.DTOs;
using Harvestry.Transfers.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Transfers.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/transfers/inbound/receipts")]
[Authorize]
public sealed class InboundReceiptsController : ControllerBase
{
    private readonly IInboundTransferReceiptService _receiptService;

    public InboundReceiptsController(IInboundTransferReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpGet]
    public async Task<ActionResult<InboundReceiptListResponse>> GetBySite(
        [FromRoute] Guid siteId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _receiptService.GetBySiteAsync(siteId, page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{receiptId:guid}")]
    public async Task<ActionResult<InboundReceiptDto>> GetById(
        [FromRoute] Guid siteId,
        [FromRoute] Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var result = await _receiptService.GetByIdAsync(siteId, receiptId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InboundReceiptDto>> CreateDraft(
        [FromRoute] Guid siteId,
        [FromBody] CreateInboundReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _receiptService.CreateDraftAsync(siteId, request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { siteId, receiptId = result.Id }, result);
    }

    [HttpPost("{receiptId:guid}/accept")]
    public async Task<ActionResult<InboundReceiptDto>> Accept(
        [FromRoute] Guid siteId,
        [FromRoute] Guid receiptId,
        [FromBody] AcceptInboundReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _receiptService.AcceptAsync(siteId, receiptId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{receiptId:guid}/reject")]
    public async Task<ActionResult<InboundReceiptDto>> Reject(
        [FromRoute] Guid siteId,
        [FromRoute] Guid receiptId,
        [FromBody] RejectInboundReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _receiptService.RejectAsync(siteId, receiptId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}

