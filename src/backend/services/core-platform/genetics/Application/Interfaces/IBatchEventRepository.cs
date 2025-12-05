using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchEvent operations
/// </summary>
public interface IBatchEventRepository
{
    Task<BatchEvent> CreateAsync(BatchEvent batchEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchEvent>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchEvent>> GetByEventTypeAsync(Guid batchId, EventType eventType, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchEvent>> GetRecentEventsAsync(Guid siteId, int limit = 100, CancellationToken cancellationToken = default);
}

