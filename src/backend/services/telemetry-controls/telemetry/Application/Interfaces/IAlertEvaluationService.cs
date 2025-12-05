using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.ValueObjects;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Service for evaluating alert rules against sensor data.
/// </summary>
public interface IAlertEvaluationService
{
    /// <summary>
    /// Evaluates all active alert rules for a site.
    /// </summary>
    Task EvaluateRulesAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Evaluates a single alert rule.
    /// </summary>
    Task<AlertRuleResult> EvaluateRuleAsync(
        AlertRule rule,
        DateTimeOffset evaluationTime,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Fires an alert (creates alert instance).
    /// </summary>
    Task<AlertInstance?> FireAlertAsync(
        Guid ruleId,
        Guid streamId,
        double currentValue,
        double thresholdValue,
        string message,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clears an alert (condition no longer met).
    /// </summary>
    Task ClearAlertAsync(
        Guid alertInstanceId,
        DateTimeOffset clearedAt,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active alerts for a site.
    /// </summary>
    Task<List<AlertInstanceDto>> GetActiveAlertsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Acknowledges an alert (user has seen and noted it).
    /// </summary>
    Task<bool> AcknowledgeAlertAsync(
        Guid siteId,
        Guid alertInstanceId,
        Guid userId,
        string? notes,
        CancellationToken cancellationToken = default);
}
