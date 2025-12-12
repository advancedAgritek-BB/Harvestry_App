namespace Harvestry.Sales.Application.DTOs;

/// <summary>
/// Compliance event for display.
/// </summary>
public sealed record ComplianceEventDto(
    Guid Id,
    Guid SiteId,
    string EntityType,
    Guid EntityId,
    string EventType,
    string? PayloadJson,
    DateTime CreatedAt,
    Guid CreatedByUserId
);

/// <summary>
/// Paginated list of compliance events.
/// </summary>
public sealed record ComplianceEventListResponse(
    IReadOnlyList<ComplianceEventDto> Events,
    int TotalCount,
    int Page,
    int PageSize
);

/// <summary>
/// Request to log a compliance event.
/// </summary>
public sealed record LogComplianceEventRequest(
    string EntityType,
    Guid EntityId,
    string EventType,
    string? PayloadJson
);
