namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Target environmental conditions for optimal strain performance
/// Stored as JSONB in database
/// </summary>
public readonly record struct TargetEnvironment(
    string? GrowMedium,
    string? LightingType,
    int? PhotoperiodHours,
    int? TargetTemperatureDayF,
    int? TargetTemperatureNightF,
    int? TargetHumidityVeg,
    int? TargetHumidityFlower,
    string? Co2Supplementation,
    string? IrrigationMethod,
    string? NutrientRegime,
    Dictionary<string, object>? AdditionalParameters
)
{
    /// <summary>
    /// Create a default empty environment
    /// </summary>
    public static TargetEnvironment Empty => new(
        GrowMedium: null,
        LightingType: null,
        PhotoperiodHours: null,
        TargetTemperatureDayF: null,
        TargetTemperatureNightF: null,
        TargetHumidityVeg: null,
        TargetHumidityFlower: null,
        Co2Supplementation: null,
        IrrigationMethod: null,
        NutrientRegime: null,
        AdditionalParameters: null
    );
}

