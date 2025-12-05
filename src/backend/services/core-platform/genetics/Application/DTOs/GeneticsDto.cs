using Harvestry.Genetics.Domain.Enums;
using Harvestry.Genetics.Domain.ValueObjects;

namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create new genetics
/// </summary>
public record CreateGeneticsRequest(
    string Name,
    string Description,
    GeneticType GeneticType,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes
);

/// <summary>
/// Request to update existing genetics
/// </summary>
public record UpdateGeneticsRequest(
    string Description,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes
);

/// <summary>
/// Genetics response DTO
/// </summary>
public record GeneticsResponse(
    Guid Id,
    Guid SiteId,
    string Name,
    string Description,
    GeneticType GeneticType,
    decimal ThcMin,
    decimal ThcMax,
    decimal CbdMin,
    decimal CbdMax,
    int? FloweringTimeDays,
    YieldPotential YieldPotential,
    GeneticProfile GrowthCharacteristics,
    TerpeneProfile TerpeneProfile,
    string? BreedingNotes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId
);

