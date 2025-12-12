using System.Text.Json;
using Harvestry.Compliance.Metrc.Application.Interfaces;
using Harvestry.Compliance.Metrc.Domain.Entities;
using Harvestry.Compliance.Metrc.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Compliance.Metrc.Application.Services;

/// <summary>
/// Service for managing the METRC sync queue (outbox pattern)
/// </summary>
public sealed class MetrcQueueService : IMetrcQueueService
{
    private readonly IMetrcQueueItemRepository _queueItemRepository;
    private readonly ILogger<MetrcQueueService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MetrcQueueService(
        IMetrcQueueItemRepository queueItemRepository,
        ILogger<MetrcQueueService> logger)
    {
        _queueItemRepository = queueItemRepository ?? throw new ArgumentNullException(nameof(queueItemRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<Guid> EnqueueAsync(
        Guid syncJobId,
        Guid siteId,
        string licenseNumber,
        MetrcEntityType entityType,
        MetrcOperationType operationType,
        Guid harvestryEntityId,
        object payload,
        int priority = 100,
        long? metrcId = null,
        string? metrcLabel = null,
        Guid? dependsOnItemId = null,
        CancellationToken cancellationToken = default)
    {
        var payloadJson = JsonSerializer.Serialize(payload, _jsonOptions);
        var idempotencyKey = GenerateIdempotencyKey(harvestryEntityId, operationType);

        // Check for duplicate by idempotency key
        var existing = await _queueItemRepository.GetByIdempotencyKeyAsync(
            idempotencyKey, cancellationToken);

        if (existing != null && !existing.IsTerminal)
        {
            _logger.LogDebug(
                "Queue item already exists for {EntityType} {EntityId} ({Operation}): {ItemId}",
                entityType, harvestryEntityId, operationType, existing.Id);
            return existing.Id;
        }

        var item = MetrcQueueItem.Create(
            syncJobId,
            siteId,
            licenseNumber,
            entityType,
            operationType,
            harvestryEntityId,
            payloadJson,
            priority,
            metrcId,
            metrcLabel,
            idempotencyKey,
            dependsOnItemId);

        await _queueItemRepository.CreateAsync(item, cancellationToken);

        _logger.LogDebug(
            "Enqueued {EntityType} {EntityId} for {Operation} to license {LicenseNumber}: {ItemId}",
            entityType, harvestryEntityId, operationType, licenseNumber, item.Id);

        return item.Id;
    }

    public async Task<IReadOnlyList<MetrcQueueItem>> GetNextBatchAsync(
        string licenseNumber,
        int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        var items = await _queueItemRepository.GetReadyForProcessingAsync(
            licenseNumber, batchSize, cancellationToken);

        if (items.Count > 0)
        {
            _logger.LogDebug(
                "Retrieved {Count} queue items ready for processing for license {LicenseNumber}",
                items.Count, licenseNumber);
        }

        return items;
    }

    public async Task CompleteItemAsync(
        Guid itemId,
        long? metrcId = null,
        string? metrcLabel = null,
        string? responseJson = null,
        CancellationToken cancellationToken = default)
    {
        var item = await _queueItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            _logger.LogWarning("Queue item {ItemId} not found for completion", itemId);
            return;
        }

        item.Complete(metrcId, metrcLabel, responseJson);
        await _queueItemRepository.UpdateAsync(item, cancellationToken);

        _logger.LogDebug(
            "Completed queue item {ItemId} for {EntityType} (MetrcId: {MetrcId})",
            itemId, item.EntityType, metrcId);
    }

    public async Task FailItemAsync(
        Guid itemId,
        string errorMessage,
        string? errorCode = null,
        string? responseJson = null,
        CancellationToken cancellationToken = default)
    {
        var item = await _queueItemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            _logger.LogWarning("Queue item {ItemId} not found for failure recording", itemId);
            return;
        }

        item.Fail(errorMessage, errorCode, responseJson);
        await _queueItemRepository.UpdateAsync(item, cancellationToken);

        if (item.CanRetry)
        {
            _logger.LogWarning(
                "Queue item {ItemId} failed (attempt {RetryCount}/{MaxRetries}), will retry: {Error}",
                itemId, item.RetryCount, item.MaxRetries, errorMessage);
        }
        else
        {
            _logger.LogError(
                "Queue item {ItemId} failed permanently after {RetryCount} attempts: {Error}",
                itemId, item.RetryCount, errorMessage);
        }
    }

    public async Task<int> GetPendingCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _queueItemRepository.GetPendingCountAsync(licenseNumber, cancellationToken);
    }

    public async Task<int> GetFailedCountAsync(
        string licenseNumber,
        CancellationToken cancellationToken = default)
    {
        return await _queueItemRepository.GetFailedCountAsync(licenseNumber, cancellationToken);
    }

    private static string GenerateIdempotencyKey(Guid entityId, MetrcOperationType operationType)
    {
        // Include date to allow re-sync of the same entity on different days
        var date = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
        return $"{entityId}:{operationType}:{date}";
    }
}
