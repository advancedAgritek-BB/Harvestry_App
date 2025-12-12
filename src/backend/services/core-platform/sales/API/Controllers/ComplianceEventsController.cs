using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Sales.API.Controllers;

/// <summary>
/// API controller for compliance event audit trail.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/sites/{siteId:guid}/sales/compliance-events")]
public sealed class ComplianceEventsController : ControllerBase
{
    private readonly IComplianceEventService _complianceEventService;

    public ComplianceEventsController(IComplianceEventService complianceEventService)
    {
        _complianceEventService = complianceEventService;
    }

    /// <summary>
    /// List compliance events with optional filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ComplianceEventListResponse>> List(
        [FromRoute] Guid siteId,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] string? eventType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var result = await _complianceEventService.ListAsync(
            siteId, entityType, entityId, eventType, fromDate, toDate, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get compliance events for a specific entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ComplianceEventDto>>> GetByEntity(
        [FromRoute] Guid siteId,
        [FromRoute] string entityType,
        [FromRoute] Guid entityId,
        CancellationToken ct = default)
    {
        var result = await _complianceEventService.GetByEntityAsync(siteId, entityType, entityId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Log a compliance event (for internal use or manual audit entries).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ComplianceEventDto>> LogEvent(
        [FromRoute] Guid siteId,
        [FromBody] LogComplianceEventRequest request,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _complianceEventService.LogEventAsync(siteId, request, userId, ct);
        return CreatedAtAction(nameof(GetByEntity), new { siteId, entityType = request.EntityType, entityId = request.EntityId }, result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        var headerUserId = Request.Headers["X-User-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerUserId) && Guid.TryParse(headerUserId, out var headerId))
        {
            return headerId;
        }

        return Guid.Empty;
    }
}
