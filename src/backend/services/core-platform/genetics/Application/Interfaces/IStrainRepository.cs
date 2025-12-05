using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for Strain entity
/// </summary>
public interface IStrainRepository
{
    /// <summary>
    /// Add new strain to the database
    /// </summary>
    Task AddAsync(Strain strain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing strain
    /// </summary>
    Task UpdateAsync(Strain strain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete strain by ID
    /// </summary>
    Task DeleteAsync(Guid strainId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strain by ID
    /// </summary>
    Task<Strain?> GetByIdAsync(Guid strainId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strain by name and site
    /// </summary>
    Task<Strain?> GetByNameAsync(
        Guid siteId, 
        string name, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strains for a site
    /// </summary>
    Task<IReadOnlyList<Strain>> GetBySiteAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strains for a genetics
    /// </summary>
    Task<IReadOnlyList<Strain>> GetByGeneticsAsync(
        Guid geneticsId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if strain has dependent batches
    /// </summary>
    Task<bool> HasDependentBatchesAsync(Guid strainId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determine whether a strain exists for the specified site
    /// </summary>
    Task<bool> ExistsAsync(Guid strainId, Guid siteId, CancellationToken cancellationToken = default);
}
