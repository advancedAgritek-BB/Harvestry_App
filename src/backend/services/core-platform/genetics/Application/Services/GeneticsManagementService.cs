using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Application.Interfaces;
using Harvestry.Genetics.Application.Mappers;
using Harvestry.Genetics.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Genetics.Application.Services;

/// <summary>
/// Service for managing genetics, phenotypes, and strains
/// </summary>
public class GeneticsManagementService : IGeneticsManagementService
{
    private readonly IGeneticsRepository _geneticsRepository;
    private readonly IPhenotypeRepository _phenotypeRepository;
    private readonly IStrainRepository _strainRepository;
    private readonly ILogger<GeneticsManagementService> _logger;

    public GeneticsManagementService(
        IGeneticsRepository geneticsRepository,
        IPhenotypeRepository phenotypeRepository,
        IStrainRepository strainRepository,
        ILogger<GeneticsManagementService> logger)
    {
        _geneticsRepository = geneticsRepository ?? throw new ArgumentNullException(nameof(geneticsRepository));
        _phenotypeRepository = phenotypeRepository ?? throw new ArgumentNullException(nameof(phenotypeRepository));
        _strainRepository = strainRepository ?? throw new ArgumentNullException(nameof(strainRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===== Genetics Operations =====

    public async Task<GeneticsResponse> CreateGeneticsAsync(
        Guid siteId,
        CreateGeneticsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating genetics {Name} for site {SiteId} by user {UserId}",
            request.Name, siteId, userId);

        // Create entity (database constraint will prevent duplicates)
        var genetics = Domain.Entities.Genetics.Create(
            siteId,
            request.Name,
            request.Description,
            request.GeneticType,
            (request.ThcMin, request.ThcMax),
            (request.CbdMin, request.CbdMax),
            request.FloweringTimeDays,
            request.YieldPotential,
            request.GrowthCharacteristics,
            request.TerpeneProfile,
            userId,
            request.BreedingNotes);

        // Save to database (catch unique constraint violation)
        await _geneticsRepository.AddAsync(genetics, cancellationToken);

        _logger.LogInformation(
            "Created genetics {GeneticsId} ({Name}) for site {SiteId}",
            genetics.Id, genetics.Name, siteId);

        return GeneticsMapper.ToResponse(genetics);
    }

    public async Task<GeneticsResponse?> GetGeneticsByIdAsync(
        Guid siteId,
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        var genetics = await _geneticsRepository.GetByIdAsync(geneticsId, cancellationToken);
        
        if (genetics == null || genetics.SiteId != siteId)
            return null;

        return GeneticsMapper.ToResponse(genetics);
    }

    public async Task<IReadOnlyList<GeneticsResponse>> GetGeneticsBySiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var geneticsList = await _geneticsRepository.GetBySiteAsync(siteId, cancellationToken);
        return GeneticsMapper.ToResponseList(geneticsList);
    }

    public async Task<GeneticsResponse> UpdateGeneticsAsync(
        Guid siteId,
        Guid geneticsId,
        UpdateGeneticsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating genetics {GeneticsId} for site {SiteId} by user {UserId}",
            geneticsId, siteId, userId);

        var genetics = await _geneticsRepository.GetByIdAsync(geneticsId, cancellationToken);
        
        if (genetics == null)
            throw new InvalidOperationException($"Genetics with ID {geneticsId} not found");

        if (genetics.SiteId != siteId)
            throw new InvalidOperationException("Cannot update genetics from a different site");

        // Update cannabinoid ranges
        genetics.UpdateCannabinoidRanges(
            (request.ThcMin, request.ThcMax),
            (request.CbdMin, request.CbdMax),
            userId);

        // Update flowering time
        if (request.FloweringTimeDays != genetics.FloweringTimeDays)
        {
            genetics.UpdateFloweringTime(request.FloweringTimeDays, userId);
        }

        // Update yield potential
        if (request.YieldPotential != genetics.YieldPotential)
        {
            genetics.UpdateYieldPotential(request.YieldPotential, userId);
        }

        // Update profile
        genetics.UpdateProfile(
            request.Description,
            request.GrowthCharacteristics,
            request.TerpeneProfile,
            userId,
            request.BreedingNotes);

        // Save to database
        await _geneticsRepository.UpdateAsync(genetics, cancellationToken);

        _logger.LogInformation(
            "Updated genetics {GeneticsId} for site {SiteId}",
            geneticsId, siteId);

        return GeneticsMapper.ToResponse(genetics);
    }

    public async Task DeleteGeneticsAsync(
        Guid siteId,
        Guid geneticsId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting genetics {GeneticsId} for site {SiteId} by user {UserId}",
            geneticsId, siteId, userId);

        var genetics = await _geneticsRepository.GetByIdAsync(geneticsId, cancellationToken);
        
        if (genetics == null)
            throw new InvalidOperationException($"Genetics with ID {geneticsId} not found");

        if (genetics.SiteId != siteId)
            throw new InvalidOperationException("Cannot delete genetics from a different site");

        // Check for dependent strains
        var hasDependentStrains = await _geneticsRepository.HasDependentStrainsAsync(geneticsId, cancellationToken);
        if (hasDependentStrains)
        {
            throw new InvalidOperationException("Cannot delete genetics that has dependent strains");
        }

        await _geneticsRepository.DeleteAsync(geneticsId, cancellationToken);

        _logger.LogInformation(
            "Deleted genetics {GeneticsId} for site {SiteId}",
            geneticsId, siteId);
    }

    public async Task<bool> CanDeleteGeneticsAsync(
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        return !await _geneticsRepository.HasDependentStrainsAsync(geneticsId, cancellationToken);
    }

    // ===== Phenotype Operations =====

    public async Task<PhenotypeResponse> CreatePhenotypeAsync(
        Guid siteId,
        CreatePhenotypeRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating phenotype {Name} for genetics {GeneticsId} by user {UserId}",
            request.Name, request.GeneticsId, userId);

        // Verify genetics exists and belongs to site
        var genetics = await _geneticsRepository.GetByIdAsync(request.GeneticsId, cancellationToken);
        if (genetics == null || genetics.SiteId != siteId)
        {
            throw new InvalidOperationException("Genetics not found or does not belong to this site");
        }

        // Create entity (database constraint will prevent duplicates)
        var phenotype = Phenotype.Create(
            siteId,
            request.GeneticsId,
            request.Name,
            request.Description,
            userId,
            request.ExpressionNotes,
            request.VisualCharacteristics,
            request.AromaProfile,
            request.GrowthPattern);

        // Save to database (catch unique constraint violation)
        await _phenotypeRepository.AddAsync(phenotype, cancellationToken);

        _logger.LogInformation(
            "Created phenotype {PhenotypeId} ({Name}) for genetics {GeneticsId}",
            phenotype.Id, phenotype.Name, request.GeneticsId);

        return PhenotypeMapper.ToResponse(phenotype);
    }

    public async Task<PhenotypeResponse?> GetPhenotypeByIdAsync(
        Guid siteId,
        Guid phenotypeId,
        CancellationToken cancellationToken = default)
    {
        var phenotype = await _phenotypeRepository.GetByIdAsync(phenotypeId, cancellationToken);
        
        if (phenotype == null || phenotype.SiteId != siteId)
            return null;

        return PhenotypeMapper.ToResponse(phenotype);
    }

    public async Task<IReadOnlyList<PhenotypeResponse>> GetPhenotypesByGeneticsAsync(
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        var phenotypes = await _phenotypeRepository.GetByGeneticsAsync(geneticsId, cancellationToken);
        return PhenotypeMapper.ToResponseList(phenotypes);
    }

    public async Task<PhenotypeResponse> UpdatePhenotypeAsync(
        Guid siteId,
        Guid phenotypeId,
        UpdatePhenotypeRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating phenotype {PhenotypeId} by user {UserId}",
            phenotypeId, userId);

        var phenotype = await _phenotypeRepository.GetByIdAsync(phenotypeId, cancellationToken);
        
        if (phenotype == null)
            throw new InvalidOperationException($"Phenotype with ID {phenotypeId} not found");

        if (phenotype.SiteId != siteId)
            throw new InvalidOperationException("Cannot update phenotype from a different site");

        // Update entity
        phenotype.Update(
            request.Description,
            request.ExpressionNotes,
            request.VisualCharacteristics,
            request.AromaProfile,
            request.GrowthPattern,
            userId);

        // Save to database
        await _phenotypeRepository.UpdateAsync(phenotype, cancellationToken);

        _logger.LogInformation("Updated phenotype {PhenotypeId}", phenotypeId);

        return PhenotypeMapper.ToResponse(phenotype);
    }

    public async Task DeletePhenotypeAsync(
        Guid siteId,
        Guid phenotypeId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting phenotype {PhenotypeId} by user {UserId}",
            phenotypeId, userId);

        var phenotype = await _phenotypeRepository.GetByIdAsync(phenotypeId, cancellationToken);
        
        if (phenotype == null)
            throw new InvalidOperationException($"Phenotype with ID {phenotypeId} not found");

        if (phenotype.SiteId != siteId)
            throw new InvalidOperationException("Cannot delete phenotype from a different site");

        await _phenotypeRepository.DeleteAsync(phenotypeId, cancellationToken);

        _logger.LogInformation("Deleted phenotype {PhenotypeId}", phenotypeId);
    }

    // ===== Strain Operations =====

    public async Task<StrainResponse> CreateStrainAsync(
        Guid siteId,
        CreateStrainRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating strain {Name} for site {SiteId} by user {UserId}",
            request.Name, siteId, userId);

        // Verify genetics exists and belongs to site
        var genetics = await _geneticsRepository.GetByIdAsync(request.GeneticsId, cancellationToken);
        if (genetics == null || genetics.SiteId != siteId)
        {
            throw new InvalidOperationException("Genetics not found or does not belong to this site");
        }

        // Verify phenotype if specified
        if (request.PhenotypeId.HasValue)
        {
            var phenotype = await _phenotypeRepository.GetByIdAsync(request.PhenotypeId.Value, cancellationToken);
            if (phenotype == null || phenotype.SiteId != siteId || phenotype.GeneticsId != request.GeneticsId)
            {
                throw new InvalidOperationException("Phenotype not found or does not belong to this genetics");
            }
        }

        // Create entity (database constraint will prevent duplicates)
        var strain = Strain.Create(
            siteId,
            request.GeneticsId,
            request.PhenotypeId,
            request.Name,
            request.Description,
            userId,
            request.Breeder,
            request.SeedBank,
            request.CultivationNotes,
            request.ExpectedHarvestWindowDays,
            request.TargetEnvironment,
            request.ComplianceRequirements);

        await _strainRepository.AddAsync(strain, cancellationToken);

        _logger.LogInformation(
            "Created strain {StrainId} ({Name}) for site {SiteId}",
            strain.Id, strain.Name, siteId);

        return StrainMapper.ToResponse(strain);
    }

    public async Task<StrainResponse?> GetStrainByIdAsync(
        Guid siteId,
        Guid strainId,
        CancellationToken cancellationToken = default)
    {
        var strain = await _strainRepository.GetByIdAsync(strainId, cancellationToken);
        
        if (strain == null || strain.SiteId != siteId)
            return null;

        return StrainMapper.ToResponse(strain);
    }

    public async Task<IReadOnlyList<StrainResponse>> GetStrainsBySiteAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var strains = await _strainRepository.GetBySiteAsync(siteId, cancellationToken);
        return StrainMapper.ToResponseList(strains);
    }

    public async Task<IReadOnlyList<StrainResponse>> GetStrainsByGeneticsAsync(
        Guid geneticsId,
        CancellationToken cancellationToken = default)
    {
        var strains = await _strainRepository.GetByGeneticsAsync(geneticsId, cancellationToken);
        return StrainMapper.ToResponseList(strains);
    }

    public async Task<StrainResponse> UpdateStrainAsync(
        Guid siteId,
        Guid strainId,
        UpdateStrainRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Updating strain {StrainId} by user {UserId}",
            strainId, userId);

        var strain = await _strainRepository.GetByIdAsync(strainId, cancellationToken);
        
        if (strain == null)
            throw new InvalidOperationException($"Strain with ID {strainId} not found");

        if (strain.SiteId != siteId)
            throw new InvalidOperationException("Cannot update strain from a different site");

        // Update entity
        strain.Update(
            request.Description,
            request.Breeder,
            request.SeedBank,
            request.CultivationNotes,
            request.ExpectedHarvestWindowDays,
            request.TargetEnvironment,
            request.ComplianceRequirements,
            userId);

        // Save to database
        await _strainRepository.UpdateAsync(strain, cancellationToken);

        _logger.LogInformation("Updated strain {StrainId}", strainId);

        return StrainMapper.ToResponse(strain);
    }

    public async Task DeleteStrainAsync(
        Guid siteId,
        Guid strainId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting strain {StrainId} by user {UserId}",
            strainId, userId);

        var strain = await _strainRepository.GetByIdAsync(strainId, cancellationToken);
        
        if (strain == null)
            throw new InvalidOperationException($"Strain with ID {strainId} not found");

        if (strain.SiteId != siteId)
            throw new InvalidOperationException("Cannot delete strain from a different site");

        // Check for dependent batches
        var hasDependentBatches = await _strainRepository.HasDependentBatchesAsync(strainId, cancellationToken);
        if (hasDependentBatches)
        {
            throw new InvalidOperationException("Cannot delete strain that has dependent batches");
        }

        await _strainRepository.DeleteAsync(strainId, cancellationToken);

        _logger.LogInformation("Deleted strain {StrainId}", strainId);
    }

    public async Task<bool> CanDeleteStrainAsync(
        Guid strainId,
        CancellationToken cancellationToken = default)
    {
        return !await _strainRepository.HasDependentBatchesAsync(strainId, cancellationToken);
    }
}
