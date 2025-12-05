using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// DTO for alert rule.
/// </summary>
public record AlertRuleDto(
    Guid Id,
    Guid SiteId,
    string RuleName,
    AlertRuleType RuleType,
    List<Guid> StreamIds,
    ThresholdConfig ThresholdConfig,
    int EvaluationWindowMinutes,
    int CooldownMinutes,
    AlertSeverity Severity,
    bool IsActive,
    List<string> NotifyChannels,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid CreatedBy,
    Guid UpdatedBy
);

/// <summary>
/// Request to create alert rule.
/// </summary>
public record CreateAlertRuleRequestDto(
    string RuleName,
    AlertRuleType RuleType,
    List<Guid> StreamIds,
    ThresholdConfig ThresholdConfig,
    int EvaluationWindowMinutes = 5,
    int CooldownMinutes = 15,
    AlertSeverity Severity = AlertSeverity.Warning,
    List<string>? NotifyChannels = null
);

/// <summary>
/// Request to update alert rule.
/// </summary>
public record UpdateAlertRuleRequestDto(
    string? RuleName = null,
    List<Guid>? StreamIds = null,
    ThresholdConfig? ThresholdConfig = null,
    int? EvaluationWindowMinutes = null,
    int? CooldownMinutes = null,
    AlertSeverity? Severity = null,
    List<string>? NotifyChannels = null
);

