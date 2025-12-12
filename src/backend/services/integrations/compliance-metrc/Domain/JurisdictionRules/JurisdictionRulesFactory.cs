namespace Harvestry.Compliance.Metrc.Domain.JurisdictionRules;

/// <summary>
/// Factory for creating jurisdiction-specific rule sets
/// </summary>
public sealed class JurisdictionRulesFactory : IJurisdictionRulesFactory
{
    private static readonly Dictionary<string, Func<IJurisdictionRules>> RulesRegistry = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IL"] = () => new ILMetrcRules(),
        ["CO"] = () => new COMetrcRules()
    };

    /// <summary>
    /// Get jurisdiction rules for a specific state
    /// </summary>
    public IJurisdictionRules GetRules(string stateCode)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required", nameof(stateCode));

        var normalizedCode = stateCode.ToUpperInvariant();

        if (!RulesRegistry.TryGetValue(normalizedCode, out var factory))
            throw new NotSupportedException($"Jurisdiction rules not implemented for state: {stateCode}");

        return factory();
    }

    /// <summary>
    /// Check if rules are available for a state
    /// </summary>
    public bool HasRules(string stateCode)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
            return false;

        return RulesRegistry.ContainsKey(stateCode.ToUpperInvariant());
    }

    /// <summary>
    /// Get all supported state codes
    /// </summary>
    public IReadOnlyList<string> GetSupportedStates()
    {
        return RulesRegistry.Keys.ToList().AsReadOnly();
    }
}

/// <summary>
/// Interface for jurisdiction rules factory
/// </summary>
public interface IJurisdictionRulesFactory
{
    IJurisdictionRules GetRules(string stateCode);
    bool HasRules(string stateCode);
    IReadOnlyList<string> GetSupportedStates();
}









