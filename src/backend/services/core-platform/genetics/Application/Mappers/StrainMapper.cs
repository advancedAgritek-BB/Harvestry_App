using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for Strain entity and DTOs
/// </summary>
public static class StrainMapper
{
    /// <summary>
    /// Map Strain entity to response DTO
    /// </summary>
    public static StrainResponse ToResponse(Strain strain)
    {
        return new StrainResponse(
            Id: strain.Id,
            SiteId: strain.SiteId,
            GeneticsId: strain.GeneticsId,
            PhenotypeId: strain.PhenotypeId,
            Name: strain.Name,
            Description: strain.Description,
            Breeder: strain.Breeder,
            SeedBank: strain.SeedBank,
            CultivationNotes: strain.CultivationNotes,
            ExpectedHarvestWindowDays: strain.ExpectedHarvestWindowDays,
            TargetEnvironment: strain.TargetEnvironment,
            ComplianceRequirements: strain.ComplianceRequirements,
            CreatedAt: strain.CreatedAt,
            UpdatedAt: strain.UpdatedAt,
            CreatedByUserId: strain.CreatedByUserId,
            UpdatedByUserId: strain.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of Strain entities to response DTOs
    /// </summary>
    public static IReadOnlyList<StrainResponse> ToResponseList(IEnumerable<Strain> strains)
    {
        return strains.Select(ToResponse).ToList();
    }
}

