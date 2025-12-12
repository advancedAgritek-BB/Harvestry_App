using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Sales.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/sales/shipments")]
[Authorize]
public sealed class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ShipmentListResponse>> GetBySite(
        [FromRoute] Guid siteId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null,
        [FromQuery] Guid? salesOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.GetBySiteAsync(siteId, page, pageSize, status, salesOrderId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{shipmentId:guid}")]
    public async Task<ActionResult<ShipmentDto>> GetById(
        [FromRoute] Guid siteId,
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _shipmentService.GetByIdAsync(siteId, shipmentId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> CreateFromAllocations(
        [FromRoute] Guid siteId,
        [FromQuery] Guid salesOrderId,
        [FromBody] CreateShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _shipmentService.CreateFromAllocationsAsync(siteId, salesOrderId, request, userId.Value, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { siteId, shipmentId = result.Id }, result);
    }

    [HttpPost("{shipmentId:guid}/start-picking")]
    public async Task<ActionResult<ShipmentDto>> StartPicking(
        [FromRoute] Guid siteId,
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _shipmentService.StartPickingAsync(siteId, shipmentId, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{shipmentId:guid}/pack")]
    public async Task<ActionResult<ShipmentDto>> Pack(
        [FromRoute] Guid siteId,
        [FromRoute] Guid shipmentId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _shipmentService.MarkPackedAsync(siteId, shipmentId, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{shipmentId:guid}/ship")]
    public async Task<ActionResult<ShipmentDto>> Ship(
        [FromRoute] Guid siteId,
        [FromRoute] Guid shipmentId,
        [FromBody] MarkShipmentShippedRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _shipmentService.MarkShippedAsync(siteId, shipmentId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{shipmentId:guid}/cancel")]
    public async Task<ActionResult<ShipmentDto>> Cancel(
        [FromRoute] Guid siteId,
        [FromRoute] Guid shipmentId,
        [FromBody] CancelShipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _shipmentService.CancelAsync(siteId, shipmentId, request, userId.Value, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }
}

