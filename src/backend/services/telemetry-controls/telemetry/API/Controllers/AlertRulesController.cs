using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Telemetry.API.Controllers;

/// <summary>
/// Manages alert rule configuration for a site.
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/alert-rules")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly IMapper _mapper;
    private readonly ILogger<AlertRulesController> _logger;

    public AlertRulesController(
        IAlertRuleRepository alertRuleRepository,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        IMapper mapper,
        ILogger<AlertRulesController> logger)
    {
        _alertRuleRepository = alertRuleRepository ?? throw new ArgumentNullException(nameof(alertRuleRepository));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets alert rules for the site.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertRules(
        Guid siteId,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var rules = includeInactive
            ? await _alertRuleRepository.GetBySiteIdAsync(siteId, cancellationToken).ConfigureAwait(false)
            : await _alertRuleRepository.GetActiveBySiteIdAsync(siteId, cancellationToken).ConfigureAwait(false);

        var response = _mapper.Map<List<AlertRuleDto>>(rules);
        return Ok(response);
    }

    /// <summary>
    /// Gets a single alert rule.
    /// </summary>
    [HttpGet("{ruleId:guid}")]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAlertRule(Guid siteId, Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null || rule.SiteId != siteId)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AlertRuleDto>(rule));
    }

    /// <summary>
    /// Creates a new alert rule.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AlertRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAlertRule(
        Guid siteId,
        [FromBody] CreateAlertRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null");
        }

        if (request.StreamIds == null || request.StreamIds.Count == 0)
        {
            return BadRequest("At least one stream must be specified");
        }

        var context = _rlsContextAccessor.Current;
        var createdBy = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;

        try
        {
            var rule = AlertRule.Create(
                siteId,
                request.RuleName,
                request.RuleType,
                request.ThresholdConfig,
                request.StreamIds,
                createdBy,
                request.EvaluationWindowMinutes,
                request.CooldownMinutes,
                request.Severity,
                request.NotifyChannels ?? new List<string>());

            await _alertRuleRepository.CreateAsync(rule, cancellationToken).ConfigureAwait(false);

            var dto = _mapper.Map<AlertRuleDto>(rule);
            return CreatedAtAction(nameof(GetAlertRule), new { siteId, ruleId = rule.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid alert rule create request for site {SiteId}", siteId);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing alert rule.
    /// </summary>
    [HttpPatch("{ruleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAlertRule(
        Guid siteId,
        Guid ruleId,
        [FromBody] UpdateAlertRuleRequestDto request,
        CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null || rule.SiteId != siteId)
        {
            return NotFound();
        }

        var context = _rlsContextAccessor.Current;
        var updatedBy = context.UserId == Guid.Empty ? Guid.Empty : context.UserId;

        try
        {
            rule.UpdateDetails(
                request.RuleName,
                request.StreamIds,
                request.EvaluationWindowMinutes,
                request.CooldownMinutes,
                request.Severity,
                request.NotifyChannels,
                updatedBy);

            if (request.ThresholdConfig != null)
            {
                rule.UpdateThreshold(request.ThresholdConfig.Value, updatedBy);
            }

            await _alertRuleRepository.UpdateAsync(rule, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid alert rule update for rule {RuleId}", ruleId);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{ruleId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateAlertRule(Guid siteId, Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null || rule.SiteId != siteId)
        {
            return NotFound();
        }

        var updatedBy = _rlsContextAccessor.Current.UserId;
        rule.Deactivate(updatedBy);
        await _alertRuleRepository.UpdateAsync(rule, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{ruleId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateAlertRule(Guid siteId, Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null || rule.SiteId != siteId)
        {
            return NotFound();
        }

        var updatedBy = _rlsContextAccessor.Current.UserId;
        rule.Activate(updatedBy);
        await _alertRuleRepository.UpdateAsync(rule, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{ruleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAlertRule(Guid siteId, Guid ruleId, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null || rule.SiteId != siteId)
        {
            return NotFound();
        }

        await _alertRuleRepository.DeleteAsync(ruleId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
