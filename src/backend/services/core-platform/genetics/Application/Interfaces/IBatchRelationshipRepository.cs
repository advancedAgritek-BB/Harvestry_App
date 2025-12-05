using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for BatchRelationship operations
/// </summary>
public interface IBatchRelationshipRepository
{
    Task<BatchRelationship> CreateAsync(BatchRelationship relationship, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchRelationship>> GetByBatchIdAsync(Guid batchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchRelationship>> GetByRelationshipTypeAsync(Guid batchId, RelationshipType relationshipType, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchRelationship>> GetSourceRelationshipsAsync(Guid sourceBatchId, Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BatchRelationship>> GetTargetRelationshipsAsync(Guid targetBatchId, Guid siteId, CancellationToken cancellationToken = default);
}

