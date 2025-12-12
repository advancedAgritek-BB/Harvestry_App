using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Sales.API.Controllers;

/// <summary>
/// API controller for customer management.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/sales/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// List customers with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CustomerListResponse>> List(
        [FromRoute] Guid siteId,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var result = await _customerService.ListAsync(siteId, search, isActive, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get a customer by ID.
    /// </summary>
    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<CustomerDetailDto>> GetById(
        [FromRoute] Guid siteId,
        [FromRoute] Guid customerId,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _customerService.GetByIdAsync(customerId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Customer {customerId} not found" });
        }
    }

    /// <summary>
    /// Create a new customer.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerDetailDto>> Create(
        [FromRoute] Guid siteId,
        [FromBody] CreateCustomerRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        try
        {
            var result = await _customerService.CreateAsync(siteId, request, userId, ct);
            return CreatedAtAction(nameof(GetById), new { siteId, customerId = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing customer.
    /// </summary>
    [HttpPut("{customerId:guid}")]
    public async Task<ActionResult<CustomerDetailDto>> Update(
        [FromRoute] Guid siteId,
        [FromRoute] Guid customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        try
        {
            var result = await _customerService.UpdateAsync(customerId, request, userId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Customer {customerId} not found" });
        }
    }

    /// <summary>
    /// Update a customer's license verification status.
    /// </summary>
    [HttpPost("{customerId:guid}/license-verification")]
    public async Task<ActionResult<CustomerDetailDto>> UpdateLicenseVerification(
        [FromRoute] Guid siteId,
        [FromRoute] Guid customerId,
        [FromBody] UpdateLicenseVerificationRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        try
        {
            var result = await _customerService.UpdateLicenseVerificationAsync(customerId, request, userId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Customer {customerId} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        // Fallback for development/header auth
        var headerUserId = Request.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerUserId) && Guid.TryParse(headerUserId, out var headerId))
        {
            return headerId;
        }

        return Guid.Empty;
    }
}
