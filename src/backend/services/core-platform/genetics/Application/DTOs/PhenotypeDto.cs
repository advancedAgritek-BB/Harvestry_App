namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request to create new phenotype
/// </summary>
public record CreatePhenotypeRequest(
    Guid GeneticsId,
    string Name,
    string Description,
    string? ExpressionNotes,
    Dictionary<string, object>? VisualCharacteristics,
    Dictionary<string, object>? AromaProfile,
    Dictionary<string, object>? GrowthPattern
);

/// <summary>
/// Request to update existing phenotype
/// </summary>
public record UpdatePhenotypeRequest(
    string Description,
    string? ExpressionNotes,
    Dictionary<string, object>? VisualCharacteristics,
    Dictionary<string, object>? AromaProfile,
    Dictionary<string, object>? GrowthPattern
);

/// <summary>
/// Phenotype response DTO
/// </summary>
public record PhenotypeResponse(
    Guid Id,
    Guid SiteId,
    Guid GeneticsId,
    string Name,
    string Description,
    string? ExpressionNotes,
    Dictionary<string, object> VisualCharacteristics,
    Dictionary<string, object> AromaProfile,
    Dictionary<string, object> GrowthPattern,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid CreatedByUserId,
    Guid UpdatedByUserId
);

