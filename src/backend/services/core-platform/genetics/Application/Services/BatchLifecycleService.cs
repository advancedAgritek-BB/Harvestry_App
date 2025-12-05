using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Genetics.Application.Services;

/// <summary>
/// Service for managing batch lifecycle operations
/// </summary>
public class BatchLifecycleService : IBatchLifecycleService
{
    private readonly IBatchRepository _batchRepository;
    private readonly IBatchEventRepository _batchEventRepository;
    private readonly IBatchRelationshipRepository _batchRelationshipRepository;
    private readonly IBatchStageHistoryRepository _batchStageHistoryRepository;
    private readonly IStrainRepository _strainRepository;
    private readonly IBatchStageDefinitionRepository _stageRepository;
    private readonly ILogger<BatchLifecycleService> _logger;

    public BatchLifecycleService(
        IBatchRepository batchRepository,
        IBatchEventRepository batchEventRepository,
        IBatchRelationshipRepository batchRelationshipRepository,
        IBatchStageHistoryRepository batchStageHistoryRepository,
        IStrainRepository strainRepository,
        IBatchStageDefinitionRepository stageRepository,
        ILogger<BatchLifecycleService> logger)
    {
        _batchRepository = batchRepository;
        _batchEventRepository = batchEventRepository;
        _batchRelationshipRepository = batchRelationshipRepository;
        _batchStageHistoryRepository = batchStageHistoryRepository;
        _strainRepository = strainRepository;
        _stageRepository = stageRepository;
        _logger = logger;
    }

    // ===== CRUD Operations =====

    public async Task<BatchResponse> CreateBatchAsync(CreateBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating batch {BatchCode} for site {SiteId}", request.BatchCode, siteId);

        // Validate dependencies
        var strainExists = await _strainRepository.ExistsAsync(request.StrainId, siteId, cancellationToken);
        if (!strainExists)
            throw new InvalidOperationException($"Strain {request.StrainId} not found");

        var stageExists = await _stageRepository.ExistsAsync(request.CurrentStageId, siteId, cancellationToken);
        if (!stageExists)
            throw new InvalidOperationException($"Stage {request.CurrentStageId} not found");

        // Check for duplicate batch code
        var batchCodeExists = await _batchRepository.BatchCodeExistsAsync(request.BatchCode, siteId, cancellationToken);
        if (batchCodeExists)
            throw new InvalidOperationException($"Batch code {request.BatchCode} already exists");

        // Validate parent batch if specified
        if (request.ParentBatchId.HasValue)
        {
            var parentExists = await _batchRepository.ExistsAsync(request.ParentBatchId.Value, siteId, cancellationToken);
            if (!parentExists)
                throw new InvalidOperationException($"Parent batch {request.ParentBatchId} not found");
        }

        // Create batch entity
        var batchCode = BatchCode.Create(request.BatchCode);
        var batch = Batch.Create(
            siteId: siteId,
            strainId: request.StrainId,
            batchCode: batchCode,
            batchName: request.BatchName,
            batchType: request.BatchType,
            sourceType: request.SourceType,
            plantCount: request.PlantCount,
            currentStageId: request.CurrentStageId,
            createdByUserId: userId,
            parentBatchId: request.ParentBatchId,
            generation: request.Generation,
            targetPlantCount: request.TargetPlantCount);

        // Set optional fields
        if (request.LocationId.HasValue || request.RoomId.HasValue || request.ZoneId.HasValue)
            batch.UpdateLocation(request.LocationId, request.RoomId, request.ZoneId, userId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            batch.AddNotes(request.Notes, userId);

        if (request.Metadata != null && request.Metadata.Count > 0)
            batch.UpdateMetadata(request.Metadata, userId);

        // Persist batch
        var createdBatch = await _batchRepository.CreateAsync(batch, cancellationToken);

        // Persist events (batch creation event is added in entity constructor)
        await PersistNewEventsAsync(batch, cancellationToken);

        _logger.LogInformation("Created batch {BatchId} with code {BatchCode}", createdBatch.Id, createdBatch.BatchCode.Value);
        return BatchMapper.ToResponse(createdBatch);
    }

    public async Task<BatchResponse> GetBatchByIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        return BatchMapper.ToResponse(batch);
    }

    public async Task<IReadOnlyList<BatchResponse>> GetBatchesByStrainAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var batches = await _batchRepository.GetByStrainIdAsync(strainId, siteId, cancellationToken);
        return BatchMapper.ToResponseList(batches);
    }

    public async Task<IReadOnlyList<BatchResponse>> GetBatchesByStageAsync(Guid stageId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var batches = await _batchRepository.GetByStageIdAsync(stageId, siteId, cancellationToken);
        return BatchMapper.ToResponseList(batches);
    }

    public async Task<IReadOnlyList<BatchResponse>> GetBatchesByStatusAsync(BatchStatus status, Guid siteId, CancellationToken cancellationToken = default)
    {
        var batches = await _batchRepository.GetByStatusAsync(status, siteId, cancellationToken);
        return BatchMapper.ToResponseList(batches);
    }

    public async Task<IReadOnlyList<BatchResponse>> GetActiveBatchesAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var batches = await _batchRepository.GetActiveAsync(siteId, cancellationToken);
        return BatchMapper.ToResponseList(batches);
    }

    public async Task<BatchResponse> UpdateBatchAsync(Guid batchId, UpdateBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        // Update properties
        batch.UpdateName(request.BatchName, userId);
        batch.UpdateTargetPlantCount(request.TargetPlantCount ?? batch.TargetPlantCount, userId);
        batch.UpdateLocation(request.LocationId, request.RoomId, request.ZoneId, userId);

        if (!string.IsNullOrWhiteSpace(request.Notes))
            batch.AddNotes(request.Notes, userId);

        if (request.Metadata != null && request.Metadata.Count > 0)
            batch.UpdateMetadata(request.Metadata, userId);

        var updatedBatch = await _batchRepository.UpdateAsync(batch, cancellationToken);

        // Persist any new events
        await PersistNewEventsAsync(batch, cancellationToken);

        _logger.LogInformation("Updated batch {BatchId}", batchId);
        return BatchMapper.ToResponse(updatedBatch);
    }

    public async Task DeleteBatchAsync(Guid batchId, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        // Check if batch has descendants
        var descendants = await _batchRepository.GetDescendantsAsync(batchId, siteId, cancellationToken);
        if (descendants.Count > 0)
            throw new InvalidOperationException($"Cannot delete batch {batchId} because it has {descendants.Count} descendant batches");

        await _batchRepository.DeleteAsync(batchId, siteId, cancellationToken);
        _logger.LogInformation("Deleted batch {BatchId}", batchId);
    }

    // ===== Lifecycle Operations =====

    public async Task<BatchResponse> TransitionBatchStageAsync(Guid batchId, TransitionBatchStageRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transitioning batch {BatchId} to stage {NewStageId}", batchId, request.NewStageId);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        var stageExists = await _stageRepository.ExistsAsync(request.NewStageId, siteId, cancellationToken);
        if (!stageExists)
            throw new InvalidOperationException($"Stage {request.NewStageId} not found");

        // Calculate days in previous stage for history
        var daysInPreviousStage = (int)Math.Floor(batch.GetStageDuration().TotalDays);

        // Change stage
        var oldStageId = batch.CurrentStageId;
        batch.ChangeStage(request.NewStageId, userId, request.TransitionNotes);

        var updatedBatch = await _batchRepository.UpdateAsync(batch, cancellationToken);

        // Persist events
        await PersistNewEventsAsync(batch, cancellationToken);

        // Create stage history entry
        var stageHistory = BatchStageHistory.Create(
            batchId: batchId,
            fromStageId: oldStageId,
            toStageId: request.NewStageId,
            changedByUserId: userId,
            changedAt: DateTime.UtcNow,
            notes: request.TransitionNotes);

        await _batchStageHistoryRepository.CreateAsync(stageHistory, cancellationToken);

        _logger.LogInformation("Transitioned batch {BatchId} from stage {OldStageId} to {NewStageId}", batchId, oldStageId, request.NewStageId);
        return BatchMapper.ToResponse(updatedBatch);
    }

    public async Task<BatchResponse> UpdatePlantCountAsync(Guid batchId, UpdatePlantCountRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating plant count for batch {BatchId} to {NewCount}", batchId, request.NewPlantCount);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        batch.UpdatePlantCount(request.NewPlantCount, request.Reason, userId);

        var updatedBatch = await _batchRepository.UpdateAsync(batch, cancellationToken);

        // Persist events
        await PersistNewEventsAsync(batch, cancellationToken);

        _logger.LogInformation("Updated plant count for batch {BatchId} to {NewCount}", batchId, request.NewPlantCount);
        return BatchMapper.ToResponse(updatedBatch);
    }

    public async Task<BatchResponse> CompleteBatchAsync(Guid batchId, DateOnly? actualHarvestDate, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        if (actualHarvestDate.HasValue)
            batch.Harvest(actualHarvestDate.Value, userId);

        batch.Complete(userId);

        var updatedBatch = await _batchRepository.UpdateAsync(batch, cancellationToken);

        // Persist events
        foreach (var batchEvent in batch.Events)
        {
            await _batchEventRepository.CreateAsync(batchEvent, cancellationToken);
        }

        _logger.LogInformation("Completed batch {BatchId}", batchId);
        return BatchMapper.ToResponse(updatedBatch);
    }

    public async Task<BatchResponse> TerminateBatchAsync(Guid batchId, string reason, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Terminating batch {BatchId}", batchId);

        var batch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (batch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        batch.Destroy(reason, userId);

        var updatedBatch = await _batchRepository.UpdateAsync(batch, cancellationToken);

        // Persist events
        await PersistNewEventsAsync(batch, cancellationToken);

        _logger.LogInformation("Terminated batch {BatchId}", batchId);
        return BatchMapper.ToResponse(updatedBatch);
    }

    // ===== Split/Merge Operations =====

    public async Task<(BatchResponse OriginalBatch, BatchResponse SplitBatch)> SplitBatchAsync(Guid batchId, SplitBatchRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Splitting batch {BatchId}: {PlantCount} plants to new batch", batchId, request.PlantCountToSplit);

        var originalBatch = await _batchRepository.GetByIdAsync(batchId, siteId, cancellationToken);
        if (originalBatch == null)
            throw new KeyNotFoundException($"Batch {batchId} not found");

        // Validate split is possible
        if (!originalBatch.CanSplit(request.PlantCountToSplit))
            throw new InvalidOperationException($"Cannot split batch {batchId}: insufficient plants or invalid status");

        // Generate batch code for split batch (append -S01, -S02, etc.) with retry logic
        var splitBatchCode = await GenerateUniqueBatchCodeAsync(
            $"{originalBatch.BatchCode.Value}-S", siteId, cancellationToken);

        // Update original batch plant count
        originalBatch.UpdatePlantCount(
            originalBatch.PlantCount - request.PlantCountToSplit,
            $"Split: {request.PlantCountToSplit} plants moved to {splitBatchCode.Value}",
            userId);

        // Create new split batch
        var splitBatch = Batch.Create(
            siteId: siteId,
            strainId: originalBatch.StrainId,
            batchCode: splitBatchCode,
            batchName: request.NewBatchName,
            batchType: originalBatch.BatchType,
            sourceType: BatchSourceType.Split,
            plantCount: request.PlantCountToSplit,
            currentStageId: originalBatch.CurrentStageId,
            createdByUserId: userId,
            parentBatchId: originalBatch.Id,
            generation: originalBatch.Generation);

        // Copy location from original batch
        splitBatch.UpdateLocation(originalBatch.LocationId, originalBatch.RoomId, originalBatch.ZoneId, userId);

        // Persist batches
        var updatedOriginal = await _batchRepository.UpdateAsync(originalBatch, cancellationToken);
        var createdSplit = await _batchRepository.CreateAsync(splitBatch, cancellationToken);

        // Create relationship
        var relationship = BatchRelationship.CreateSplit(
            siteId: siteId,
            parentBatchId: originalBatch.Id,
            childBatchId: splitBatch.Id,
            plantCountTransferred: request.PlantCountToSplit,
            userId: userId,
            notes: request.SplitReason);

        await _batchRelationshipRepository.CreateAsync(relationship, cancellationToken);

        // Persist events
        foreach (var evt in originalBatch.Events)
            await _batchEventRepository.CreateAsync(evt, cancellationToken);
        foreach (var evt in splitBatch.Events)
            await _batchEventRepository.CreateAsync(evt, cancellationToken);

        _logger.LogInformation("Split batch {OriginalBatchId} into {SplitBatchId}", batchId, splitBatch.Id);
        return (BatchMapper.ToResponse(updatedOriginal), BatchMapper.ToResponse(createdSplit));
    }

    public async Task<BatchResponse> MergeBatchesAsync(MergeBatchesRequest request, Guid siteId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Merging {Count} batches into new batch", request.SourceBatchIds.Length);

        if (request.SourceBatchIds.Length < 2)
            throw new InvalidOperationException("At least 2 batches are required for merge");

        // Load all source batches
        var sourceBatches = new List<Batch>();
        foreach (var sourceBatchId in request.SourceBatchIds)
        {
            var batch = await _batchRepository.GetByIdAsync(sourceBatchId, siteId, cancellationToken);
            if (batch == null)
                throw new KeyNotFoundException($"Batch {sourceBatchId} not found");

            if (batch.Status != BatchStatus.Active)
                throw new InvalidOperationException($"Cannot merge batch {sourceBatchId}: status is {batch.Status}");

            sourceBatches.Add(batch);
        }

        // Validate all batches have same strain and stage
        var firstBatch = sourceBatches[0];
        if (sourceBatches.Any(b => b.StrainId != firstBatch.StrainId))
            throw new InvalidOperationException("All batches must have the same strain to merge");

        if (sourceBatches.Any(b => b.CurrentStageId != firstBatch.CurrentStageId))
            throw new InvalidOperationException("All batches must be in the same stage to merge");

        // Calculate total plant count
        var totalPlantCount = sourceBatches.Sum(b => b.PlantCount);

        // Generate batch code for merged batch with retry logic
        var mergedBatchCode = await GenerateUniqueBatchCodeAsync(
            $"{firstBatch.BatchCode.Value}-M", siteId, cancellationToken);

        // Create merged batch
        var mergedBatch = Batch.Create(
            siteId: siteId,
            strainId: firstBatch.StrainId,
            batchCode: mergedBatchCode,
            batchName: request.MergedBatchName,
            batchType: firstBatch.BatchType,
            sourceType: BatchSourceType.Merge,
            plantCount: totalPlantCount,
            currentStageId: firstBatch.CurrentStageId,
            createdByUserId: userId,
            parentBatchId: firstBatch.Id,
            generation: firstBatch.Generation);

        // Copy location from first batch
        mergedBatch.UpdateLocation(firstBatch.LocationId, firstBatch.RoomId, firstBatch.ZoneId, userId);

        var createdMerged = await _batchRepository.CreateAsync(mergedBatch, cancellationToken);

        // Complete source batches and create relationships
        foreach (var sourceBatch in sourceBatches)
        {
            sourceBatch.Complete(userId);
            await _batchRepository.UpdateAsync(sourceBatch, cancellationToken);

            var relationship = BatchRelationship.CreateMerge(
                siteId: siteId,
                parentBatchId: sourceBatch.Id,
                childBatchId: mergedBatch.Id,
                plantCountTransferred: sourceBatch.PlantCount,
                userId: userId,
                notes: request.MergeReason);

            await _batchRelationshipRepository.CreateAsync(relationship, cancellationToken);

            // Persist source batch events
            await PersistNewEventsAsync(sourceBatch, cancellationToken);
        }

        // Persist merged batch events
        await PersistNewEventsAsync(mergedBatch, cancellationToken);

        _logger.LogInformation("Merged {Count} batches into {MergedBatchId}", request.SourceBatchIds.Length, mergedBatch.Id);
        return BatchMapper.ToResponse(createdMerged);
    }

    private async Task<BatchCode> GenerateUniqueBatchCodeAsync(string prefix, Guid siteId, CancellationToken cancellationToken)
    {
        const int maxSuffix = 99;

        for (var suffix = 1; suffix <= maxSuffix; suffix++)
        {
            var candidateCode = BatchCode.Create($"{prefix}{suffix:D2}");
            var exists = await _batchRepository.BatchCodeExistsAsync(candidateCode.Value, siteId, cancellationToken);

            if (!exists)
            {
                return candidateCode;
            }
        }

        throw new InvalidOperationException($"Could not generate unique batch code after exhausting {maxSuffix} suffix values");
    }

    private async Task PersistNewEventsAsync(Batch batch, CancellationToken cancellationToken)
    {
        var eventsToPersist = batch.Events.ToList();
        foreach (var batchEvent in eventsToPersist)
        {
            await _batchEventRepository.CreateAsync(batchEvent, cancellationToken);
        }

        if (eventsToPersist.Count > 0)
        {
            _logger.LogDebug("Persisted {Count} new events for batch {BatchId}",
                eventsToPersist.Count, batch.Id);
            batch.ClearEvents();
        }
    }

    // ===== Relationship & Event Queries =====

    public async Task<IReadOnlyList<BatchRelationshipResponse>> GetBatchRelationshipsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var relationships = await _batchRelationshipRepository.GetByBatchIdAsync(batchId, siteId, cancellationToken);
        return BatchMapper.ToRelationshipResponseList(relationships);
    }

    public async Task<IReadOnlyList<BatchEventResponse>> GetBatchEventsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var events = await _batchEventRepository.GetByBatchIdAsync(batchId, siteId, cancellationToken);
        return BatchMapper.ToEventResponseList(events);
    }

    public async Task<IReadOnlyList<StageHistoryResponse>> GetBatchStageHistoryAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var history = await _batchStageHistoryRepository.GetByBatchIdAsync(batchId, siteId, cancellationToken);
        return BatchStageMapper.ToHistoryResponseList(history);
    }

    // ===== Genealogy Queries =====

    public async Task<IReadOnlyList<BatchResponse>> GetBatchDescendantsAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var descendants = await _batchRepository.GetDescendantsAsync(batchId, siteId, cancellationToken);
        return BatchMapper.ToResponseList(descendants);
    }

    public async Task<BatchResponse?> GetBatchParentAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default)
    {
        var parent = await _batchRepository.GetParentAsync(batchId, siteId, cancellationToken);
        return parent != null ? BatchMapper.ToResponse(parent) : null;
    }
}
