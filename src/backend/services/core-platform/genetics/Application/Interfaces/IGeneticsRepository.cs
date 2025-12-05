using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for Genetics aggregate
/// </summary>
public interface IGeneticsRepository
{
    /// <summary>
    /// Add new genetics to the database
    /// </summary>
    Task AddAsync(Domain.Entities.Genetics genetics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing genetics
    /// </summary>
    Task UpdateAsync(Domain.Entities.Genetics genetics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete genetics by ID
    /// </summary>
    Task DeleteAsync(Guid geneticsId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get genetics by ID
    /// </summary>
    Task<Domain.Entities.Genetics?> GetByIdAsync(Guid geneticsId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get genetics by name and site
    /// </summary>
    Task<Domain.Entities.Genetics?> GetByNameAsync(
        Guid siteId, 
        string name, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all genetics for a site
    /// </summary>
    Task<IReadOnlyList<Domain.Entities.Genetics>> GetBySiteAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if genetics has dependent strains
    /// </summary>
    Task<bool> HasDependentStrainsAsync(Guid geneticsId, CancellationToken cancellationToken = default);
}

