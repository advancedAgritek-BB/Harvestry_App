using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// DTO for alert instance (fired alert).
/// </summary>
public record AlertInstanceDto(
    Guid Id,
    Guid SiteId,
    Guid RuleId,
    Guid StreamId,
    DateTimeOffset FiredAt,
    DateTimeOffset? ClearedAt,
    AlertSeverity Severity,
    double? CurrentValue,
    double? ThresholdValue,
    string Message,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedBy,
    string? AcknowledgmentNotes,
    bool IsActive,
    TimeSpan Duration
);

/// <summary>
/// Request to acknowledge an alert.
/// </summary>
public record AcknowledgeAlertRequestDto(
    string? Notes = null
);

