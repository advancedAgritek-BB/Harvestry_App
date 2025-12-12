using Harvestry.Sales.Application.DTOs;

namespace Harvestry.Sales.Application.Interfaces;

/// <summary>
/// Application service for compliance event operations.
/// </summary>
public interface IComplianceEventService
{
    Task<ComplianceEventListResponse> ListAsync(
        Guid siteId,
        string? entityType,
        Guid? entityId,
        string? eventType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<ComplianceEventDto>> GetByEntityAsync(
        Guid siteId,
        string entityType,
        Guid entityId,
        CancellationToken ct = default);

    Task<ComplianceEventDto> LogEventAsync(
        Guid siteId,
        LogComplianceEventRequest request,
        Guid userId,
        CancellationToken ct = default);
}
