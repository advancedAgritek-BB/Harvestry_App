using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// Controller for batch lifecycle management
/// </summary>
[ApiController]
[Route("api/v1/genetics/batches")]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IBatchLifecycleService _batchService;
    private readonly ILogger<BatchesController> _logger;

    public BatchesController(IBatchLifecycleService batchService, ILogger<BatchesController> logger)
    {
        _batchService = batchService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new batch
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchResponse>> CreateBatch(
        [FromBody] CreateBatchRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.CreateBatchAsync(request, siteId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetBatchById), new { id = batch.Id }, batch);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create batch: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get batch by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> GetBatchById(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.GetBatchByIdAsync(id, siteId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get batches by strain
    /// </summary>
    [HttpGet("by-strain/{strainId}")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatchesByStrain(
        Guid strainId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var batches = await _batchService.GetBatchesByStrainAsync(strainId, siteId, cancellationToken);
        return Ok(batches);
    }

    /// <summary>
    /// Get batches by stage
    /// </summary>
    [HttpGet("by-stage/{stageId}")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatchesByStage(
        Guid stageId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var batches = await _batchService.GetBatchesByStageAsync(stageId, siteId, cancellationToken);
        return Ok(batches);
    }

    /// <summary>
    /// Get batches by status
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatchesByStatus(
        BatchStatus status,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var batches = await _batchService.GetBatchesByStatusAsync(status, siteId, cancellationToken);
        return Ok(batches);
    }

    /// <summary>
    /// Get all active batches
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetActiveBatches(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var batches = await _batchService.GetActiveBatchesAsync(siteId, cancellationToken);
        return Ok(batches);
    }

    /// <summary>
    /// Update batch
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> UpdateBatch(
        Guid id,
        [FromBody] UpdateBatchRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.UpdateBatchAsync(id, request, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete batch
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteBatch(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _batchService.DeleteBatchAsync(id, siteId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete batch {BatchId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    // ===== Lifecycle Operations =====

    /// <summary>
    /// Transition batch to a new stage
    /// </summary>
    [HttpPost("{id}/transition")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchResponse>> TransitionBatchStage(
        Guid id,
        [FromBody] TransitionBatchStageRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.TransitionBatchStageAsync(id, request, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to transition batch {BatchId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update batch plant count
    /// </summary>
    [HttpPost("{id}/plant-count")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> UpdatePlantCount(
        Guid id,
        [FromBody] UpdatePlantCountRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.UpdatePlantCountAsync(id, request, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Complete batch (harvest and finalize)
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> CompleteBatch(
        Guid id,
        [FromQuery] DateOnly? actualHarvestDate,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.CompleteBatchAsync(id, actualHarvestDate, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Terminate batch (destroy)
    /// </summary>
    [HttpPost("{id}/terminate")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> TerminateBatch(
        Guid id,
        [FromBody] TerminateBatchRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.TerminateBatchAsync(id, request.Reason, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ===== Split/Merge Operations =====

    /// <summary>
    /// Split batch into two batches
    /// </summary>
    [HttpPost("{id}/split")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> SplitBatch(
        Guid id,
        [FromBody] SplitBatchRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var (originalBatch, splitBatch) = await _batchService.SplitBatchAsync(id, request, siteId, userId, cancellationToken);
            return Ok(new { originalBatch, splitBatch });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to split batch {BatchId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Merge multiple batches into one
    /// </summary>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchResponse>> MergeBatches(
        [FromBody] MergeBatchesRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _batchService.MergeBatchesAsync(request, siteId, userId, cancellationToken);
            return Ok(batch);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Failed to merge batches: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to merge batches: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    // ===== Relationships & Events =====

    /// <summary>
    /// Get batch relationships
    /// </summary>
    [HttpGet("{id}/relationships")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchRelationshipResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchRelationshipResponse>>> GetBatchRelationships(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var relationships = await _batchService.GetBatchRelationshipsAsync(id, siteId, cancellationToken);
        return Ok(relationships);
    }

    /// <summary>
    /// Get batch events
    /// </summary>
    [HttpGet("{id}/events")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchEventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchEventResponse>>> GetBatchEvents(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var events = await _batchService.GetBatchEventsAsync(id, siteId, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get batch stage history
    /// </summary>
    [HttpGet("{id}/stage-history")]
    [ProducesResponseType(typeof(IReadOnlyList<StageHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StageHistoryResponse>>> GetBatchStageHistory(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var history = await _batchService.GetBatchStageHistoryAsync(id, siteId, cancellationToken);
        return Ok(history);
    }

    // ===== Genealogy =====

    /// <summary>
    /// Get batch descendants (children, grandchildren, etc.)
    /// </summary>
    [HttpGet("{id}/descendants")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchResponse>>> GetBatchDescendants(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var descendants = await _batchService.GetBatchDescendantsAsync(id, siteId, cancellationToken);
        return Ok(descendants);
    }

    /// <summary>
    /// Get batch parent
    /// </summary>
    [HttpGet("{id}/parent")]
    [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchResponse>> GetBatchParent(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var parent = await _batchService.GetBatchParentAsync(id, siteId, cancellationToken);
        if (parent == null)
            return NotFound();

        return Ok(parent);
    }
}
