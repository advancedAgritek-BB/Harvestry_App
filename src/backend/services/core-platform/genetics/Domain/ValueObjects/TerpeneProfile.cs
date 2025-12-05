namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Terpene profile for aroma and flavor characteristics
/// Stored as JSONB in database
/// </summary>
public readonly record struct TerpeneProfile(
    IReadOnlyDictionary<string, decimal>? DominantTerpenes,
    string[]? AromaDescriptors,
    string[]? FlavorDescriptors,
    string? OverallProfile,
    Dictionary<string, object>? AdditionalData
)
{
    /// <summary>
    /// Create a default empty profile
    /// </summary>
    public static TerpeneProfile Empty => new(
        DominantTerpenes: null,
        AromaDescriptors: null,
        FlavorDescriptors: null,
        OverallProfile: null,
        AdditionalData: null
    );
}

