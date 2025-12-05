namespace Harvestry.Compliance.Metrc.Domain.JurisdictionRules;

/// <summary>
/// Illinois-specific METRC compliance rules
/// </summary>
public sealed class ILMetrcRules : IJurisdictionRules
{
    public string StateCode => "IL";
    public string StateName => "Illinois";
    public bool RequiresPatientLicenseNumber => true;
    public bool SupportsMedical => true;
    public bool SupportsRecreational => true;
    public int? MaxPlantsPerBatch => null; // No specific limit
    public bool RequiresTwoPersonDestructionSignoff => false;
    public bool RequiresSublocationTracking => false;
    public int ApiRateLimitPerMinute => 60;

    private static readonly List<string> ValidCategories = new()
    {
        "Buds", "Shake/Trim", "Pre-Roll", "Pre-Roll Infused", "Vape Cartridge",
        "Concentrate", "Concentrate for Infusion", "Infused Edible", "Infused Non-Edible",
        "Infused Pre-Roll", "Tincture", "Topical", "Capsule", "Seeds", "Immature Plants",
        "Vegetative Plants", "Clone", "Tissue Culture"
    };

    private static readonly List<string> ValidWasteTypes = new()
    {
        "Plant Material", "Fibrous Material", "Root Ball"
    };

    private static readonly List<string> ValidWasteMethods = new()
    {
        "Grinder", "Compost", "Mixed Waste"
    };

    private static readonly List<string> RequiredTests = new()
    {
        "Potency", "Microbial", "Mycotoxins", "Pesticides", "Heavy Metals",
        "Residual Solvents", "Foreign Material"
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

        // Illinois METRC tags are typically 24 characters
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
            ? "https://sandbox-api-il.metrc.com/"
            : "https://api-il.metrc.com/";
    }
}




