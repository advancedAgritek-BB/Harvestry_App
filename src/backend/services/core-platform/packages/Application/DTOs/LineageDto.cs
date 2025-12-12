namespace Harvestry.Packages.Application.DTOs;

/// <summary>
/// Package lineage tree response
/// </summary>
public record PackageLineageDto
{
    public Guid PackageId { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public int GenerationDepth { get; init; }
    public List<LineageNodeDto> Ancestors { get; init; } = new();
    public List<LineageNodeDto> Descendants { get; init; } = new();
}

/// <summary>
/// Node in lineage tree
/// </summary>
public record LineageNodeDto
{
    public Guid Id { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly PackagedDate { get; init; }
    public int GenerationDepth { get; init; }
    public string? TransformationType { get; init; } // processing, split, merge
    public List<LineageNodeDto> Children { get; init; } = new();
}

/// <summary>
/// Expiring inventory DTO
/// </summary>
public record ExpiringPackageDto
{
    public Guid Id { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string ItemCategory { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public DateOnly ExpirationDate { get; init; }
    public int DaysUntilExpiry { get; init; }
    public decimal ValueAtRisk { get; init; }
    public string? LocationName { get; init; }
    public string? Grade { get; init; }
    public string ExpiryStatus { get; init; } = string.Empty; // expired, critical, warning, upcoming
}

/// <summary>
/// Hold summary DTO
/// </summary>
public record HoldSummaryDto
{
    public Guid Id { get; init; }
    public string PackageLabel { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string HoldReasonCode { get; init; } = string.Empty;
    public DateTime HoldPlacedAt { get; init; }
    public Guid HoldPlacedByUserId { get; init; }
    public bool RequiresTwoPersonRelease { get; init; }
    public bool HasFirstApproval { get; init; }
    public decimal ValueOnHold { get; init; }
    public string? LocationName { get; init; }
}




