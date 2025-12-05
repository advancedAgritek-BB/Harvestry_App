using System;
using System.Linq;
using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapper for Genetics entity and DTOs
/// </summary>
public static class GeneticsMapper
{
    /// <summary>
    /// Map Genetics entity to response DTO
    /// </summary>
    public static GeneticsResponse ToResponse(Domain.Entities.Genetics genetics)
    {
        if (genetics == null)
            throw new ArgumentNullException(nameof(genetics));

        return new GeneticsResponse(
            Id: genetics.Id,
            SiteId: genetics.SiteId,
            Name: genetics.Name,
            Description: genetics.Description,
            GeneticType: genetics.GeneticType,
            ThcMin: genetics.ThcMinPercentage,
            ThcMax: genetics.ThcMaxPercentage,
            CbdMin: genetics.CbdMinPercentage,
            CbdMax: genetics.CbdMaxPercentage,
            FloweringTimeDays: genetics.FloweringTimeDays,
            YieldPotential: genetics.YieldPotential,
            GrowthCharacteristics: genetics.GrowthCharacteristics,
            TerpeneProfile: genetics.TerpeneProfile,
            BreedingNotes: genetics.BreedingNotes,
            CreatedAt: genetics.CreatedAt,
            UpdatedAt: genetics.UpdatedAt,
            CreatedByUserId: genetics.CreatedByUserId,
            UpdatedByUserId: genetics.UpdatedByUserId
        );
    }

    /// <summary>
    /// Map list of Genetics entities to response DTOs
    /// </summary>
    public static IReadOnlyList<GeneticsResponse> ToResponseList(IEnumerable<Domain.Entities.Genetics> geneticsList)
    {
        if (geneticsList == null)
            throw new ArgumentNullException(nameof(geneticsList));

        return geneticsList.Where(g => g != null).Select(ToResponse).ToArray();
    }
}

