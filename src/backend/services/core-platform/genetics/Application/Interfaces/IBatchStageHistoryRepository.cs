using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchStageHistory operations
/// </summary>
public interface IBatchStageHistoryRepository
{
    Task<BatchStageHistory> CreateAsync(BatchStageHistory history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchStageHistory>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchStageHistory?> GetMostRecentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
}

