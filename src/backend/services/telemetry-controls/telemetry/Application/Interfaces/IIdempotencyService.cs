using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Service for enforcing message idempotency (deduplication).
/// Prevents duplicate sensor readings from being stored.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Checks if a message is a duplicate.
    /// </summary>
    Task<bool> IsDuplicateAsync(
        Guid streamId,
        string messageId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Filters duplicate readings from a batch.
    /// Returns count of duplicates removed.
    /// </summary>
    Task<int> DeduplicateBatchAsync(
        ICollection<SensorReading> readings,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks multiple message IDs for duplicates in bulk.
    /// Returns set of message IDs that are duplicates.
    /// </summary>
    Task<HashSet<string>> GetDuplicatesAsync(
        Guid streamId,
        IEnumerable<string> messageIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves duplicates for multiple streams in a single query.
    /// </summary>
    Task<HashSet<(Guid StreamId, string MessageId)>> GetDuplicatesByStreamAsync(
        IReadOnlyDictionary<Guid, string[]> messageIdsByStream,
        CancellationToken cancellationToken = default);
}
