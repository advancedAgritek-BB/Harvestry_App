using Harvestry.Genetics.Domain.ValueObjects;

namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create new strain
/// </summary>
public record CreateStrainRequest(
    Guid GeneticsId,
    Guid? PhenotypeId,
    string Name,
    string Description,
    string? Breeder,
    string? SeedBank,
    string? CultivationNotes,
    int? ExpectedHarvestWindowDays,
    TargetEnvironment TargetEnvironment,
    ComplianceRequirements ComplianceRequirements
);

/// <summary>
/// Request to update existing strain
/// </summary>
public record UpdateStrainRequest(
    string Description,
    string? Breeder,
    string? SeedBank,
    string? CultivationNotes,
    int? ExpectedHarvestWindowDays,
    TargetEnvironment TargetEnvironment,
    ComplianceRequirements ComplianceRequirements
);

/// <summary>
/// Strain response DTO
/// </summary>
public record StrainResponse(
    Guid Id,
    Guid SiteId,
    Guid GeneticsId,
    Guid? PhenotypeId,
    string Name,
    string Description,
    string? Breeder,
    string? SeedBank,
    string? CultivationNotes,
    int? ExpectedHarvestWindowDays,
    TargetEnvironment TargetEnvironment,
    ComplianceRequirements ComplianceRequirements,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId
);

