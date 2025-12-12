using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Sales.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/sales/orders")]
[Authorize]
public sealed class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _salesOrderService;

    public SalesOrdersController(ISalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<SalesOrderListResponse>> GetBySite(
        [FromRoute] Guid siteId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _salesOrderService.GetBySiteAsync(siteId, page, pageSize, status, search, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{salesOrderId:guid}")]
    public async Task<ActionResult<SalesOrderDto>> GetById(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _salesOrderService.GetByIdAsync(siteId, salesOrderId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrderDto>> CreateDraft(
        [FromRoute] Guid siteId,
        [FromBody] CreateSalesOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _salesOrderService.CreateDraftAsync(siteId, request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { siteId, salesOrderId = result.Id }, result);
    }

    [HttpPost("{salesOrderId:guid}/lines")]
    public async Task<ActionResult<SalesOrderDto>> AddLine(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        [FromBody] AddSalesOrderLineRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _salesOrderService.AddLineAsync(siteId, salesOrderId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{salesOrderId:guid}/submit")]
    public async Task<ActionResult<SalesOrderDto>> Submit(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _salesOrderService.SubmitAsync(siteId, salesOrderId, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{salesOrderId:guid}/cancel")]
    public async Task<ActionResult<SalesOrderDto>> Cancel(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        [FromBody] CancelSalesOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _salesOrderService.CancelAsync(siteId, salesOrderId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}

