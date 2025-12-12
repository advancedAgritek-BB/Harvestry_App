namespace Harvestry.Compliance.BioTrack.Domain.JurisdictionRules;

/// <summary>
/// Interface for BioTrack jurisdiction-specific rules
/// </summary>
public interface IBioTrackJurisdictionRules
{
    string StateCode { get; }
    string StateName { get; }
    bool SupportsMedical { get; }
    bool SupportsRecreational { get; }
    int ApiRateLimitPerMinute { get; }
    string GetApiBaseUrl(bool useSandbox = false);
    IReadOnlyList<string> GetValidInventoryTypes();
}

/// <summary>
/// Washington state BioTrack rules
/// </summary>
public sealed class WABioTrackRules : IBioTrackJurisdictionRules
{
    public string StateCode => "WA";
    public string StateName => "Washington";
    public bool SupportsMedical => true;
    public bool SupportsRecreational => true;
    public int ApiRateLimitPerMinute => 60;

    private static readonly List<string> ValidInventoryTypes = new()
    {
        "Usable Marijuana", "Marijuana Extract for Inhalation", "Marijuana Mix Infused",
        "Marijuana Mix Packaged", "Immature Plant", "Mature Plant", "Clone",
        "Marijuana Mix", "Concentrate", "Solid Marijuana Infused Edible"
    };

    public string GetApiBaseUrl(bool useSandbox = false) =>
        useSandbox 
            ? "https://wslcb-sandbox.biotrack.com/serverjson.asp"
            : "https://wslcb.biotrack.com/serverjson.asp";

    public IReadOnlyList<string> GetValidInventoryTypes() => ValidInventoryTypes.AsReadOnly();
}

/// <summary>
/// Florida state BioTrack rules
/// </summary>
public sealed class FLBioTrackRules : IBioTrackJurisdictionRules
{
    public string StateCode => "FL";
    public string StateName => "Florida";
    public bool SupportsMedical => true;
    public bool SupportsRecreational => false;
    public int ApiRateLimitPerMinute => 60;

    private static readonly List<string> ValidInventoryTypes = new()
    {
        "Flower", "Concentrate", "Edible", "Topical", "Suppository",
        "Tincture", "Oil", "Capsule", "Vape Cartridge", "Pre-Roll"
    };

    public string GetApiBaseUrl(bool useSandbox = false) =>
        useSandbox
            ? "https://fl-sandbox.biotrack.com/serverjson.asp"
            : "https://fl.biotrack.com/serverjson.asp";

    public IReadOnlyList<string> GetValidInventoryTypes() => ValidInventoryTypes.AsReadOnly();
}

/// <summary>
/// Factory for getting jurisdiction-specific rules
/// </summary>
public sealed class BioTrackJurisdictionRulesFactory
{
    private readonly Dictionary<string, IBioTrackJurisdictionRules> _rules;

    public BioTrackJurisdictionRulesFactory()
    {
        _rules = new Dictionary<string, IBioTrackJurisdictionRules>(StringComparer.OrdinalIgnoreCase)
        {
            ["WA"] = new WABioTrackRules(),
            ["FL"] = new FLBioTrackRules()
        };
    }

    public IBioTrackJurisdictionRules GetRules(string stateCode)
    {
        if (_rules.TryGetValue(stateCode, out var rules))
        {
            return rules;
        }
        throw new NotSupportedException($"BioTrack is not supported for state: {stateCode}");
    }

    public bool IsSupported(string stateCode) => _rules.ContainsKey(stateCode);
}
