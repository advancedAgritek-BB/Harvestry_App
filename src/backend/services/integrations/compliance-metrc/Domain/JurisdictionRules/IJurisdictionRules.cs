namespace Harvestry.Compliance.Metrc.Domain.JurisdictionRules;

/// <summary>
/// Interface for jurisdiction-specific METRC rules
/// </summary>
public interface IJurisdictionRules
{
    /// <summary>
    /// Two-letter state code
    /// </summary>
    string StateCode { get; }

    /// <summary>
    /// State name
    /// </summary>
    string StateName { get; }

    /// <summary>
    /// Whether the state requires patient license numbers for medical batches
    /// </summary>
    bool RequiresPatientLicenseNumber { get; }

    /// <summary>
    /// Whether the state supports medical sales
    /// </summary>
    bool SupportsMedical { get; }

    /// <summary>
    /// Whether the state supports recreational/adult-use sales
    /// </summary>
    bool SupportsRecreational { get; }

    /// <summary>
    /// Maximum number of plants per batch (if regulated)
    /// </summary>
    int? MaxPlantsPerBatch { get; }

    /// <summary>
    /// Whether two-person signoff is required for destruction
    /// </summary>
    bool RequiresTwoPersonDestructionSignoff { get; }

    /// <summary>
    /// Whether the state requires sublocation tracking
    /// </summary>
    bool RequiresSublocationTracking { get; }

    /// <summary>
    /// API rate limit (requests per minute)
    /// </summary>
    int ApiRateLimitPerMinute { get; }

    /// <summary>
    /// Get valid item categories for this jurisdiction
    /// </summary>
    IReadOnlyList<string> GetValidItemCategories();

    /// <summary>
    /// Get valid waste types for this jurisdiction
    /// </summary>
    IReadOnlyList<string> GetValidWasteTypes();

    /// <summary>
    /// Get valid waste methods for this jurisdiction
    /// </summary>
    IReadOnlyList<string> GetValidWasteMethods();

    /// <summary>
    /// Get valid lab test types for this jurisdiction
    /// </summary>
    IReadOnlyList<string> GetRequiredLabTests();

    /// <summary>
    /// Validate a plant batch name for this jurisdiction
    /// </summary>
    bool ValidateBatchName(string batchName, out string? errorMessage);

    /// <summary>
    /// Validate a package label for this jurisdiction
    /// </summary>
    bool ValidatePackageLabel(string label, out string? errorMessage);

    /// <summary>
    /// Get the base METRC API URL for this jurisdiction
    /// </summary>
    string GetApiBaseUrl(bool useSandbox = false);
}








