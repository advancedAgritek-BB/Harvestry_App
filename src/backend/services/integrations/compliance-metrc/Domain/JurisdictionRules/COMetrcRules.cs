namespace Harvestry.Compliance.Metrc.Domain.JurisdictionRules;

/// <summary>
/// Colorado-specific METRC compliance rules
/// </summary>
public sealed class COMetrcRules : IJurisdictionRules
{
    public string StateCode => "CO";
    public string StateName => "Colorado";
    public bool RequiresPatientLicenseNumber => true;
    public bool SupportsMedical => true;
    public bool SupportsRecreational => true;
    public int? MaxPlantsPerBatch => null;
    public bool RequiresTwoPersonDestructionSignoff => true; // CO requires decontamination tracking
    public bool RequiresSublocationTracking => true;
    public int ApiRateLimitPerMinute => 60;

    private static readonly List<string> ValidCategories = new()
    {
        "Buds", "Shake/Trim", "Pre-Roll", "Vape Cartridge", "Wax", "Shatter",
        "Live Resin", "Rosin", "Distillate", "Oil", "Hash", "Kief",
        "Concentrate", "Concentrate (Each)", "Edibles", "Tincture", "Topical",
        "Capsule", "Suppository", "Transdermal Patch", "Seeds", "Immature Plants",
        "Vegetative Plants", "Clone", "Mature Plant"
    };

    private static readonly List<string> ValidWasteTypes = new()
    {
        "Plant Material", "Fibrous Material", "Root Ball", "Vegetative Waste",
        "Flower Waste", "Other Plant Material"
    };

    private static readonly List<string> ValidWasteMethods = new()
    {
        "Grinder", "Compost", "Incinerator", "Mixed Waste"
    };

    private static readonly List<string> RequiredTests = new()
    {
        "Potency", "Microbial", "Mycotoxins", "Pesticides", "Heavy Metals",
        "Residual Solvents", "Foreign Material", "Water Activity", "Moisture Content"
    };

    public IReadOnlyList<string> GetValidItemCategories() => ValidCategories.AsReadOnly();
    public IReadOnlyList<string> GetValidWasteTypes() => ValidWasteTypes.AsReadOnly();
    public IReadOnlyList<string> GetValidWasteMethods() => ValidWasteMethods.AsReadOnly();
    public IReadOnlyList<string> GetRequiredLabTests() => RequiredTests.AsReadOnly();

    public bool ValidateBatchName(string batchName, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(batchName))
        {
            errorMessage = "Batch name is required";
            return false;
        }

        if (batchName.Length > 150)
        {
            errorMessage = "Batch name cannot exceed 150 characters";
            return false;
        }

        return true;
    }

    public bool ValidatePackageLabel(string label, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(label))
        {
            errorMessage = "Package label is required";
            return false;
        }

        // Colorado METRC tags are typically 24 characters
        if (label.Length != 24)
        {
            errorMessage = "Package label must be exactly 24 characters";
            return false;
        }

        // Must be alphanumeric
        if (!label.All(c => char.IsLetterOrDigit(c)))
        {
            errorMessage = "Package label must be alphanumeric";
            return false;
        }

        return true;
    }

    public string GetApiBaseUrl(bool useSandbox = false)
    {
        return useSandbox
            ? "https://sandbox-api-co.metrc.com/"
            : "https://api-co.metrc.com/";
    }
}



