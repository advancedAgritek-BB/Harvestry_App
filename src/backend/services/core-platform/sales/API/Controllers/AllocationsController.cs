using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Shared.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Sales.API.Controllers;

[ApiController]
[Route("api/v1/sites/{siteId:guid}/sales/orders/{salesOrderId:guid}/allocations")]
[Authorize]
public sealed class AllocationsController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationsController(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesAllocationDto>>> GetAllocations(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _allocationService.GetAllocationsAsync(siteId, salesOrderId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("allocate")]
    public async Task<ActionResult<List<SalesAllocationDto>>> Allocate(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        [FromBody] AllocateSalesOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _allocationService.AllocateAsync(siteId, salesOrderId, request, userId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpPost("unallocate")]
    public async Task<ActionResult<List<SalesAllocationDto>>> Unallocate(
        [FromRoute] Guid siteId,
        [FromRoute] Guid salesOrderId,
        [FromBody] UnallocateSalesOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _allocationService.UnallocateAsync(siteId, salesOrderId, request, userId.Value, cancellationToken);
        return Ok(result);
    }
}

