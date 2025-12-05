using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Evaluates alert rules against telemetry readings and manages alert instances.
/// </summary>
public sealed class AlertEvaluationService : IAlertEvaluationService
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IAlertInstanceRepository _alertInstanceRepository;
    private readonly ITelemetryQueryRepository _telemetryQueryRepository;
    private readonly ITelemetryRlsContextAccessor _rlsContextAccessor;
    private readonly IMapper _mapper;
    private readonly ILogger<AlertEvaluationService> _logger;

    public AlertEvaluationService(
        IAlertRuleRepository alertRuleRepository,
        IAlertInstanceRepository alertInstanceRepository,
        ITelemetryQueryRepository telemetryQueryRepository,
        ITelemetryRlsContextAccessor rlsContextAccessor,
        IMapper mapper,
        ILogger<AlertEvaluationService> logger)
    {
        _alertRuleRepository = alertRuleRepository ?? throw new ArgumentNullException(nameof(alertRuleRepository));
        _alertInstanceRepository = alertInstanceRepository ?? throw new ArgumentNullException(nameof(alertInstanceRepository));
        _telemetryQueryRepository = telemetryQueryRepository ?? throw new ArgumentNullException(nameof(telemetryQueryRepository));
        _rlsContextAccessor = rlsContextAccessor ?? throw new ArgumentNullException(nameof(rlsContextAccessor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EvaluateRulesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            var rules = await _alertRuleRepository.GetActiveBySiteIdAsync(siteId, cancellationToken).ConfigureAwait(false);
            if (rules.Count == 0)
            {
                return;
            }

            var evaluationTime = DateTimeOffset.UtcNow;

            foreach (var rule in rules)
            {
                try
                {
                    await EvaluateRuleAsync(rule, evaluationTime, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to evaluate alert rule {RuleId} for site {SiteId}", rule.Id, siteId);
                }
            }
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }

    public async Task<AlertRuleResult> EvaluateRuleAsync(
        AlertRule rule,
        DateTimeOffset evaluationTime,
        CancellationToken cancellationToken = default)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));

        var contextAdjusted = EnsureSiteContext(rule.SiteId, out var originalContext);
        AlertRuleResult lastResult = AlertRuleResult.NoData();

        try
        {
            foreach (var streamId in rule.StreamIds)
            {
                var start = evaluationTime - TimeSpan.FromMinutes(rule.EvaluationWindowMinutes);
                var readings = await _telemetryQueryRepository
                    .GetReadingsAsync(streamId, start, evaluationTime, limit: null, cancellationToken)
                    .ConfigureAwait(false);

                var result = rule.Evaluate(readings, evaluationTime);
                await HandleEvaluationResultAsync(rule, streamId, result, evaluationTime, cancellationToken).ConfigureAwait(false);
                lastResult = result;
            }
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }

        return lastResult;
    }

    public async Task<AlertInstance?> FireAlertAsync(
        Guid ruleId,
        Guid streamId,
        double currentValue,
        double thresholdValue,
        string message,
        CancellationToken cancellationToken = default)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(ruleId, cancellationToken).ConfigureAwait(false);
        if (rule == null)
        {
            _logger.LogWarning("Attempted to fire alert for unknown rule {RuleId}", ruleId);
            return null;
        }

        var previousContext = EnsureSiteContext(rule.SiteId);
        try
        {
            var alert = AlertInstance.Fire(
                rule.SiteId,
                rule.Id,
                streamId,
                rule.Severity,
                message,
                currentValue,
                thresholdValue);

            await _alertInstanceRepository.CreateAsync(alert, cancellationToken).ConfigureAwait(false);
            return alert;
        }
        finally
        {
            RestoreSiteContext(previousContext);
        }
    }

    public async Task ClearAlertAsync(
        Guid alertInstanceId,
        DateTimeOffset clearedAt,
        CancellationToken cancellationToken = default)
    {
        var alert = await _alertInstanceRepository.GetByIdAsync(alertInstanceId, cancellationToken).ConfigureAwait(false);
        if (alert == null)
        {
            _logger.LogWarning("Attempted to clear unknown alert {AlertId}", alertInstanceId);
            return;
        }

        if (!alert.IsActive())
        {
            return;
        }

        alert.Clear(clearedAt);
        await _alertInstanceRepository.UpdateAsync(alert, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AlertInstanceDto>> GetActiveAlertsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            var alerts = await _alertInstanceRepository.GetActiveBySiteAsync(siteId, cancellationToken).ConfigureAwait(false);
            return _mapper.Map<List<AlertInstanceDto>>(alerts);
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }

    public async Task<bool> AcknowledgeAlertAsync(
        Guid siteId,
        Guid alertInstanceId,
        Guid userId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var contextAdjusted = EnsureSiteContext(siteId, out var originalContext);

        try
        {
            var alert = await _alertInstanceRepository.GetByIdAsync(alertInstanceId, cancellationToken).ConfigureAwait(false);
            if (alert == null || alert.SiteId != siteId)
            {
                _logger.LogWarning("Attempted to acknowledge unknown alert {AlertId}", alertInstanceId);
                return false;
            }

            if (alert.IsAcknowledged())
            {
                return true;
            }

            alert.Acknowledge(userId, notes, DateTimeOffset.UtcNow);
            await _alertInstanceRepository.UpdateAsync(alert, cancellationToken).ConfigureAwait(false);
            return true;
        }
        finally
        {
            RestoreSiteContext(originalContext, contextAdjusted);
        }
    }

    private async Task HandleEvaluationResultAsync(
        AlertRule rule,
        Guid streamId,
        AlertRuleResult result,
        DateTimeOffset evaluationTime,
        CancellationToken cancellationToken)
    {
        var activeAlert = await _alertInstanceRepository
            .GetActiveByRuleAndStreamAsync(rule.Id, streamId, cancellationToken)
            .ConfigureAwait(false);

        if (result.ShouldFireAlert)
        {
            if (activeAlert != null)
            {
                activeAlert.Refresh(result.CurrentValue, result.ThresholdValue, result.Message ?? rule.RuleName);
                await _alertInstanceRepository.UpdateAsync(activeAlert, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var fired = AlertInstance.Fire(
                    rule.SiteId,
                    rule.Id,
                    streamId,
                    rule.Severity,
                    result.Message ?? rule.RuleName,
                    result.CurrentValue,
                    result.ThresholdValue);
                await _alertInstanceRepository.CreateAsync(fired, cancellationToken).ConfigureAwait(false);
            }
        }
        else if (result.ShouldClearAlert && activeAlert != null)
        {
            try
            {
                activeAlert.Clear(evaluationTime);
                await _alertInstanceRepository.UpdateAsync(activeAlert, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Alert {AlertId} already cleared", activeAlert.Id);
            }
        }
    }

    
    private TelemetryRlsContext EnsureSiteContext(Guid siteId)
    {
        var originalContext = _rlsContextAccessor.Current;
        if (originalContext.SiteId == siteId)
        {
            return originalContext;
        }

        _rlsContextAccessor.Set(originalContext with { SiteId = siteId });
        return originalContext;
    }

    private void RestoreSiteContext(TelemetryRlsContext originalContext)
    {
        _rlsContextAccessor.Set(originalContext);
    }
private bool EnsureSiteContext(Guid siteId, out TelemetryRlsContext originalContext)
    {
        originalContext = _rlsContextAccessor.Current;
        if (originalContext.SiteId == siteId)
        {
            return false;
        }

        _rlsContextAccessor.Set(originalContext with { SiteId = siteId });
        return true;
    }

    private void RestoreSiteContext(TelemetryRlsContext originalContext, bool contextAdjusted)
    {
        if (contextAdjusted)
        {
            _rlsContextAccessor.Set(originalContext);
        }
    }
}
