using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Service for managing batch lifecycle operations
/// </summary>
public interface IBatchLifecycleService
{
    // Batch CRUD Operations
    Task<BatchResponse> CreateBatchAsync(CreateBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchResponse> GetBatchByIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponse>> GetBatchesByStrainAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponse>> GetBatchesByStageAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponse>> GetBatchesByStatusAsync(BatchStatus status, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchResponse>> GetActiveBatchesAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchResponse> UpdateBatchAsync(Guid batchId, UpdateBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteBatchAsync(Guid batchId, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Lifecycle Operations
    Task<BatchResponse> TransitionBatchStageAsync(Guid batchId, TransitionBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchResponse> UpdatePlantCountAsync(Guid batchId, UpdatePlantCountRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchResponse> CompleteBatchAsync(Guid batchId, DateOnly? actualHarvestDate, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchResponse> TerminateBatchAsync(Guid batchId, string reason, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Split/Merge Operations
    Task<(BatchResponse OriginalBatch, BatchResponse SplitBatch)> SplitBatchAsync(Guid batchId, SplitBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);
    Task<BatchResponse> MergeBatchesAsync(MergeBatchesRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default);

    // Relationship & Event Queries
    Task<IReadOnlyList<BatchRelationshipResponse>> GetBatchRelationshipsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchEventResponse>> GetBatchEventsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StageHistoryResponse>> GetBatchStageHistoryAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);

    // Genealogy Queries
    Task<IReadOnlyList<BatchResponse>> GetBatchDescendantsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<BatchResponse?> GetBatchParentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
}

