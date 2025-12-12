using Harvestry.Items.Application.DTOs;
using Harvestry.Items.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Items.API.Controllers;

/// <summary>
/// API controller for Item/Product management
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get paginated list of items
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ItemListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ItemListResponse>> GetItems(
        [FromRoute] Guid siteId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? inventoryCategory = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isSellable = null,
        [FromQuery] bool? isPurchasable = null,
        [FromQuery] bool? isProducible = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _itemService.GetItemsAsync(
            siteId, page, pageSize, category, status, inventoryCategory, search,
            isSellable, isPurchasable, isProducible, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get item by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItem(
        [FromRoute] Guid siteId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await _itemService.GetByIdAsync(siteId, id, cancellationToken);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Get item by SKU
    /// </summary>
    [HttpGet("by-sku/{sku}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItemBySku(
        [FromRoute] Guid siteId,
        [FromRoute] string sku,
        CancellationToken cancellationToken = default)
    {
        var item = await _itemService.GetBySkuAsync(siteId, sku, cancellationToken);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Get item by barcode
    /// </summary>
    [HttpGet("by-barcode/{barcode}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> GetItemByBarcode(
        [FromRoute] Guid siteId,
        [FromRoute] string barcode,
        CancellationToken cancellationToken = default)
    {
        var item = await _itemService.GetByBarcodeAsync(siteId, barcode, cancellationToken);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Create a new item
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ItemDto>> CreateItem(
        [FromRoute] Guid siteId,
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        try
        {
            var item = await _itemService.CreateAsync(siteId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetItem), new { siteId, id = item.Id }, item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing item
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> UpdateItem(
        [FromRoute] Guid siteId,
        [FromRoute] Guid id,
        [FromBody] UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();

        try
        {
            var item = await _itemService.UpdateAsync(siteId, id, request, userId, cancellationToken);
            if (item == null)
                return NotFound();

            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an item
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        [FromRoute] Guid siteId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _itemService.DeleteAsync(siteId, id, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Activate an item
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> ActivateItem(
        [FromRoute] Guid siteId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var item = await _itemService.ActivateAsync(siteId, id, userId, cancellationToken);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Deactivate an item
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDto>> DeactivateItem(
        [FromRoute] Guid siteId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var item = await _itemService.DeactivateAsync(siteId, id, userId, cancellationToken);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(List<LowStockAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LowStockAlertDto>>> GetLowStockAlerts(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _itemService.GetLowStockAlertsAsync(siteId, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Get category summary
    /// </summary>
    [HttpGet("categories/summary")]
    [ProducesResponseType(typeof(List<ItemCategorySummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemCategorySummaryDto>>> GetCategorySummary(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _itemService.GetCategorySummaryAsync(siteId, cancellationToken);
        return Ok(summary);
    }

    private Guid GetCurrentUserId()
    {
        // Extract user ID from claims - implementation depends on auth setup
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("user_id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        // Fallback for development
        _logger.LogWarning("Could not extract user ID from claims, using default");
        return Guid.Empty;
    }
}




