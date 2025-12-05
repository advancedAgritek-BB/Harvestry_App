using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Genetics.API.Controllers;

/// <summary>
/// Controller for batch code rule management
/// </summary>
[ApiController]
[Route("api/v1/genetics/batch-code-rules")]
[Authorize]
public class BatchCodeRulesController : ControllerBase
{
    private readonly IBatchCodeRuleService _ruleService;
    private readonly ILogger<BatchCodeRulesController> _logger;

    public BatchCodeRulesController(IBatchCodeRuleService ruleService, ILogger<BatchCodeRulesController> logger)
    {
        _ruleService = ruleService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new batch code rule
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BatchCodeRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchCodeRuleResponse>> CreateRule(
        [FromBody] CreateBatchCodeRuleRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _ruleService.CreateRuleAsync(request, siteId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, rule);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create rule: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get rule by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BatchCodeRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchCodeRuleResponse>> GetRuleById(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _ruleService.GetRuleByIdAsync(id, siteId, cancellationToken);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Get all batch code rules
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BatchCodeRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<BatchCodeRuleResponse>>> GetAllRules(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rules = await _ruleService.GetAllRulesAsync(siteId, cancellationToken);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all rules for site {SiteId}", siteId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve rules");
        }
    }

    /// <summary>
    /// Get active batch code rules
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<BatchCodeRuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<BatchCodeRuleResponse>>> GetActiveRules(
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rules = await _ruleService.GetActiveRulesAsync(siteId, cancellationToken);
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active rules for site {SiteId}", siteId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve active rules");
        }
    }

    /// <summary>
    /// Update batch code rule
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BatchCodeRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchCodeRuleResponse>> UpdateRule(
        Guid id,
        [FromBody] UpdateBatchCodeRuleRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _ruleService.UpdateRuleAsync(id, request, siteId, userId, cancellationToken);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update rule {RuleId}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete batch code rule
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRule(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _ruleService.DeleteRuleAsync(id, siteId, userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Activate batch code rule
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(BatchCodeRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchCodeRuleResponse>> ActivateRule(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _ruleService.ActivateRuleAsync(id, siteId, userId, cancellationToken);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deactivate batch code rule
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(BatchCodeRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchCodeRuleResponse>> DeactivateRule(
        Guid id,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        [FromHeader(Name = "X-User-Id")] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var rule = await _ruleService.DeactivateRuleAsync(id, siteId, userId, cancellationToken);
            return Ok(rule);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Validate a batch code against rules
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(BatchCodeValidationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchCodeValidationResponse>> ValidateBatchCode(
        [FromBody] ValidateBatchCodeRequest request,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        var result = await _ruleService.ValidateBatchCodeAsync(request.BatchCode, siteId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Check if batch code is unique
    /// </summary>
    [HttpGet("check-unique")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> IsBatchCodeUnique(
        [FromQuery] string batchCode,
        [FromHeader(Name = "X-Site-Id")] Guid siteId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(batchCode))
        {
            return BadRequest("Batch code must be provided and cannot be empty or whitespace");
        }

        try
        {
            var isUnique = await _ruleService.IsBatchCodeUniqueAsync(batchCode, siteId, cancellationToken);
            return Ok(isUnique);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check uniqueness for batch code '{BatchCode}' in site {SiteId}", batchCode, siteId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to check batch code uniqueness");
        }
    }
}
