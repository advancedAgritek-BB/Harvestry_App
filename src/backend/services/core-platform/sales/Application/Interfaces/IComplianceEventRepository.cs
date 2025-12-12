using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Interfaces;

/// <summary>
/// Repository for ComplianceEvent entities.
/// </summary>
public interface IComplianceEventRepository
{
    Task<(IReadOnlyList<ComplianceEvent> Items, int TotalCount)> ListAsync(
        Guid siteId,
        string? entityType,
        Guid? entityId,
        string? eventType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<ComplianceEvent>> GetByEntityAsync(
        Guid siteId,
        string entityType,
        Guid entityId,
        CancellationToken ct = default);

    Task AddAsync(ComplianceEvent evt, CancellationToken ct = default);
}
