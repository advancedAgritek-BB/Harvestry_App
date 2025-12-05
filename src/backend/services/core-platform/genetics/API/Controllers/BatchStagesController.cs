using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// Controller for batch stage configuration
/// </summary>
[ApiController]
[Route("api/v1/genetics/batch-stages")]
[Authorize]
public class BatchStagesController : ControllerBase
{
    private readonly IBatchStageConfigurationService _stageService;
    private readonly ILogger<BatchStagesController> _logger;

    public BatchStagesController(IBatchStageConfigurationService stageService, ILogger<BatchStagesController> logger)
    {
        _stageService = stageService;
        _logger = logger;
    }

    // ===== Stage Definition Operations =====

    /// <summary>
    /// Create a new batch stage definition
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BatchStageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchStageResponse>> CreateStage(
        [FromBody] CreateBatchStageRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stage = await _stageService.CreateStageAsync(request, siteId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetStageById), new { id = stage.Id }, stage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create stage: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get stage by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BatchStageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchStageResponse>> GetStageById(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stage = await _stageService.GetStageByIdAsync(id, siteId, cancellationToken);
            return Ok(stage);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get all batch stages
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BatchStageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchStageResponse>>> GetAllStages(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var stages = await _stageService.GetAllStagesAsync(siteId, cancellationToken);
        return Ok(stages);
    }

    /// <summary>
    /// Get active batch stages
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchStageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BatchStageResponse>>> GetActiveStages(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var stages = await _stageService.GetActiveStagesAsync(siteId, cancellationToken);
        return Ok(stages);
    }

    /// <summary>
    /// Update stage definition
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BatchStageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchStageResponse>> UpdateStage(
        Guid id,
        [FromBody] UpdateBatchStageRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var stage = await _stageService.UpdateStageAsync(id, request, siteId, userId, cancellationToken);
            return Ok(stage);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete stage definition
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteStage(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _stageService.DeleteStageAsync(id, siteId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to delete stage {StageId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reorder stages
    /// </summary>
    [HttpPost("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderStages(
        [FromBody] Dictionary<Guid, int> stageOrders,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        if (stageOrders == null || stageOrders.Count == 0)
        {
            return BadRequest("stageOrders must be provided and contain at least one entry");
        }

        await _stageService.ReorderStagesAsync(stageOrders, siteId, userId, cancellationToken);
        return NoContent();
    }

    // ===== Stage Transition Operations =====

    /// <summary>
    /// Create a stage transition rule
    /// </summary>
    [HttpPost("transitions")]
    [ProducesResponseType(typeof(StageTransitionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StageTransitionResponse>> CreateTransition(
        [FromBody] CreateStageTransitionRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var transition = await _stageService.CreateTransitionAsync(request, siteId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetTransitionById), new { id = transition.Id }, transition);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create transition: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get transition by ID
    /// </summary>
    [HttpGet("transitions/{id}")]
    [ProducesResponseType(typeof(StageTransitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StageTransitionResponse>> GetTransitionById(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var transition = await _stageService.GetTransitionByIdAsync(id, siteId, cancellationToken);
            return Ok(transition);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get all stage transitions
    /// </summary>
    [HttpGet("transitions")]
    [ProducesResponseType(typeof(IReadOnlyList<StageTransitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StageTransitionResponse>>> GetAllTransitions(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var transitions = await _stageService.GetAllTransitionsAsync(siteId, cancellationToken);
        return Ok(transitions);
    }

    /// <summary>
    /// Get transitions from a specific stage
    /// </summary>
    [HttpGet("transitions/from/{fromStageId}")]
    [ProducesResponseType(typeof(IReadOnlyList<StageTransitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StageTransitionResponse>>> GetTransitionsFromStage(
        Guid fromStageId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var transitions = await _stageService.GetTransitionsFromStageAsync(fromStageId, siteId, cancellationToken);
        return Ok(transitions);
    }

    /// <summary>
    /// Get transitions to a specific stage
    /// </summary>
    [HttpGet("transitions/to/{toStageId}")]
    [ProducesResponseType(typeof(IReadOnlyList<StageTransitionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StageTransitionResponse>>> GetTransitionsToStage(
        Guid toStageId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var transitions = await _stageService.GetTransitionsToStageAsync(toStageId, siteId, cancellationToken);
        return Ok(transitions);
    }

    /// <summary>
    /// Update transition rule
    /// </summary>
    [HttpPut("transitions/{id}")]
    [ProducesResponseType(typeof(StageTransitionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StageTransitionResponse>> UpdateTransition(
        Guid id,
        [FromBody] UpdateStageTransitionRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var transition = await _stageService.UpdateTransitionAsync(id, request, siteId, userId, cancellationToken);
            return Ok(transition);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Delete transition rule
    /// </summary>
    [HttpDelete("transitions/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransition(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _stageService.DeleteTransitionAsync(id, siteId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Check if transition is allowed
    /// </summary>
    [HttpGet("transitions/can-transition")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> CanTransition(
        [FromQuery] Guid fromStageId,
        [FromQuery] Guid toStageId,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (fromStageId == Guid.Empty)
        {
            return BadRequest("fromStageId cannot be empty");
        }

        if (toStageId == Guid.Empty)
        {
            return BadRequest("toStageId cannot be empty");
        }

        if (fromStageId == toStageId)
        {
            return BadRequest("fromStageId and toStageId cannot be the same");
        }

        var canTransition = await _stageService.CanTransitionAsync(fromStageId, toStageId, siteId, cancellationToken);
        return Ok(canTransition);
    }
}
