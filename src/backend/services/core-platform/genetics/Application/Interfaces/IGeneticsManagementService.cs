using Harvestry.Genetics.Application.DTOs;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Service for managing genetics, phenotypes, and strains
/// </summary>
public interface IGeneticsManagementService
{
    // ===== Genetics Operations =====
    
    /// <summary>
    /// Create new genetics
    /// </summary>
    Task<GeneticsResponse> CreateGeneticsAsync(
        Guid siteId, 
        CreateGeneticsRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get genetics by ID
    /// </summary>
    Task<GeneticsResponse?> GetGeneticsByIdAsync(
        Guid siteId, 
        Guid geneticsId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all genetics for a site
    /// </summary>
    Task<IReadOnlyList<GeneticsResponse>> GetGeneticsBySiteAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update genetics
    /// </summary>
    Task<GeneticsResponse> UpdateGeneticsAsync(
        Guid siteId, 
        Guid geneticsId, 
        UpdateGeneticsRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete genetics (only if no dependent strains)
    /// </summary>
    Task DeleteGeneticsAsync(
        Guid siteId, 
        Guid geneticsId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if genetics can be deleted
    /// </summary>
    Task<bool> CanDeleteGeneticsAsync(
        Guid geneticsId, 
        CancellationToken cancellationToken = default);

    // ===== Phenotype Operations =====
    
    /// <summary>
    /// Create new phenotype
    /// </summary>
    Task<PhenotypeResponse> CreatePhenotypeAsync(
        Guid siteId, 
        CreatePhenotypeRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get phenotype by ID
    /// </summary>
    Task<PhenotypeResponse?> GetPhenotypeByIdAsync(
        Guid siteId, 
        Guid phenotypeId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all phenotypes for a genetics
    /// </summary>
    Task<IReadOnlyList<PhenotypeResponse>> GetPhenotypesByGeneticsAsync(
        Guid geneticsId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update phenotype
    /// </summary>
    Task<PhenotypeResponse> UpdatePhenotypeAsync(
        Guid siteId, 
        Guid phenotypeId, 
        UpdatePhenotypeRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete phenotype
    /// </summary>
    Task DeletePhenotypeAsync(
        Guid siteId, 
        Guid phenotypeId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    // ===== Strain Operations =====
    
    /// <summary>
    /// Create new strain
    /// </summary>
    Task<StrainResponse> CreateStrainAsync(
        Guid siteId, 
        CreateStrainRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get strain by ID
    /// </summary>
    Task<StrainResponse?> GetStrainByIdAsync(
        Guid siteId, 
        Guid strainId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strains for a site
    /// </summary>
    Task<IReadOnlyList<StrainResponse>> GetStrainsBySiteAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strains for a genetics
    /// </summary>
    Task<IReadOnlyList<StrainResponse>> GetStrainsByGeneticsAsync(
        Guid geneticsId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update strain
    /// </summary>
    Task<StrainResponse> UpdateStrainAsync(
        Guid siteId, 
        Guid strainId, 
        UpdateStrainRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete strain (only if no dependent batches)
    /// </summary>
    Task DeleteStrainAsync(
        Guid siteId, 
        Guid strainId, 
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if strain can be deleted
    /// </summary>
    Task<bool> CanDeleteStrainAsync(
        Guid strainId, 
        CancellationToken cancellationToken = default);
}

