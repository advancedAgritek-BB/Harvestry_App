using Harvestry.Sales.Application.DTOs;
using Harvestry.Sales.Application.Interfaces;
using Harvestry.Sales.Domain.Entities;

namespace Harvestry.Sales.Application.Services;

/// <summary>
/// Application service for compliance event operations.
/// </summary>
public sealed class ComplianceEventService : IComplianceEventService
{
    private readonly IComplianceEventRepository _repository;

    public ComplianceEventService(IComplianceEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<ComplianceEventListResponse> ListAsync(
        Guid siteId,
        string? entityType,
        Guid? entityId,
        string? eventType,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _repository.ListAsync(
            siteId, entityType, entityId, eventType, fromDate, toDate, page, pageSize, ct);

        var dtos = items.Select(MapToDto).ToList();
        return new ComplianceEventListResponse(dtos, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyList<ComplianceEventDto>> GetByEntityAsync(
        Guid siteId,
        string entityType,
        Guid entityId,
        CancellationToken ct = default)
    {
        var items = await _repository.GetByEntityAsync(siteId, entityType, entityId, ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<ComplianceEventDto> LogEventAsync(
        Guid siteId,
        LogComplianceEventRequest request,
        Guid userId,
        CancellationToken ct = default)
    {
        var evt = ComplianceEvent.Create(
            siteId,
            request.EntityType,
            request.EntityId,
            request.EventType,
            request.PayloadJson,
            userId);

        await _repository.AddAsync(evt, ct);
        return MapToDto(evt);
    }

    private static ComplianceEventDto MapToDto(ComplianceEvent e) => new(
        e.Id,
        e.SiteId,
        e.EntityType,
        e.EntityId,
        e.EventType,
        e.PayloadJson,
        e.CreatedAt,
        e.CreatedByUserId
    );
}
