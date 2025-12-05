using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Repository interface for Phenotype entity
/// </summary>
public interface IPhenotypeRepository
{
    /// <summary>
    /// Add new phenotype to the database
    /// </summary>
    Task AddAsync(Phenotype phenotype, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing phenotype
    /// </summary>
    Task UpdateAsync(Phenotype phenotype, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete phenotype by ID
    /// </summary>
    Task DeleteAsync(Guid phenotypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get phenotype by ID
    /// </summary>
    Task<Phenotype?> GetByIdAsync(Guid phenotypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get phenotype by name, genetics ID, and site
    /// </summary>
    Task<Phenotype?> GetByNameAsync(
        Guid siteId, 
        Guid geneticsId, 
        string name, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all phenotypes for a genetics
    /// </summary>
    Task<IReadOnlyList<Phenotype>> GetByGeneticsAsync(
        Guid geneticsId, 
        CancellationToken cancellationToken = default);
}

