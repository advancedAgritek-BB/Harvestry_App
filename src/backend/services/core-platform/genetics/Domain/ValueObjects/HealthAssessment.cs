using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Structured health assessment for mother plants
/// </summary>
public readonly record struct HealthAssessment(
    HealthStatus Status,
    PressureLevel PestPressure,
    PressureLevel DiseasePressure,
    IReadOnlyCollection<string> NutrientDeficiencies,
    string? Observations,
    string? TreatmentsApplied,
    string? EnvironmentalNotes,
    IReadOnlyCollection<Uri> PhotoUrls
)
{
    /// <summary>
    /// Create a basic healthy assessment
    /// </summary>
    public static HealthAssessment CreateHealthy(string? observations = null) => new(
        Status: HealthStatus.Excellent,
        PestPressure: PressureLevel.None,
        DiseasePressure: PressureLevel.None,
        NutrientDeficiencies: Array.Empty<string>(),
        Observations: observations,
        TreatmentsApplied: null,
        EnvironmentalNotes: null,
        PhotoUrls: Array.Empty<Uri>()
    );
}

