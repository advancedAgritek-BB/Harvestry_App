namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Compliance and regulatory requirements for strain cultivation
/// Stored as JSONB in database
/// </summary>
public readonly record struct ComplianceRequirements(
    string[]? RequiredLabTests,
    string[]? ProhibitedStageMethods,
    string[]? MandatoryTrackingEvents,
    int? MinimumTrackingDays,
    int? MaximumPlantCount,
    string[]? LabelingRequirements,
    bool RequiresMetrcTagging,
    bool RequiresBatchPhotography,
    Dictionary<string, object>? JurisdictionSpecificRules
)
{
    /// <summary>
    /// Create a default empty requirements set
    /// </summary>
    public static ComplianceRequirements Empty => new(
        RequiredLabTests: null,
        ProhibitedStageMethods: null,
        MandatoryTrackingEvents: null,
        MinimumTrackingDays: null,
        MaximumPlantCount: null,
        LabelingRequirements: null,
        RequiresMetrcTagging: false,
        RequiresBatchPhotography: false,
        JurisdictionSpecificRules: null
    );
}

