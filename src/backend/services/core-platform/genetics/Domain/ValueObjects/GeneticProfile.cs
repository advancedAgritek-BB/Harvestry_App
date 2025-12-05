namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Growth characteristics and environmental preferences for genetics
/// Stored as JSONB in database
/// </summary>
public readonly record struct GeneticProfile(
    string? StretchTendency,
    string? BranchingPattern,
    string? LeafMorphology,
    string? InternodeSpacing,
    string? RootVigour,
    int? OptimalTemperatureMin,
    int? OptimalTemperatureMax,
    int? OptimalHumidityMin,
    int? OptimalHumidityMax,
    string? LightIntensityPreference,
    string? NutrientSensitivity,
    Dictionary<string, object>? AdditionalCharacteristics
)
{
    /// <summary>
    /// Create a default empty profile
    /// </summary>
    public static GeneticProfile Empty => new(
        StretchTendency: null,
        BranchingPattern: null,
        LeafMorphology: null,
        InternodeSpacing: null,
        RootVigour: null,
        OptimalTemperatureMin: null,
        OptimalTemperatureMax: null,
        OptimalHumidityMin: null,
        OptimalHumidityMax: null,
        LightIntensityPreference: null,
        NutrientSensitivity: null,
        AdditionalCharacteristics: null
    );
}

