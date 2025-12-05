using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for Phenotype entity and DTOs
/// </summary>
public static class PhenotypeMapper
{
    /// <summary>
    /// Map Phenotype entity to response DTO
    /// </summary>
    public static PhenotypeResponse ToResponse(Phenotype phenotype)
    {
        return new PhenotypeResponse(
            Id: phenotype.Id,
            SiteId: phenotype.SiteId,
            GeneticsId: phenotype.GeneticsId,
            Name: phenotype.Name,
            Description: phenotype.Description,
            ExpressionNotes: phenotype.ExpressionNotes,
            VisualCharacteristics: phenotype.VisualCharacteristics,
            AromaProfile: phenotype.AromaProfile,
            GrowthPattern: phenotype.GrowthPattern,
            CreatedAt: phenotype.CreatedAt,
            UpdatedAt: phenotype.UpdatedAt,
            CreatedByUserId: phenotype.CreatedByUserId,
            UpdatedByUserId: phenotype.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of Phenotype entities to response DTOs
    /// </summary>
    public static IReadOnlyList<PhenotypeResponse> ToResponseList(IEnumerable<Phenotype> phenotypes)
    {
        return phenotypes.Select(ToResponse).ToList();
    }
}

