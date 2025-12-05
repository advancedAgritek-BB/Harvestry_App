using System.Text.RegularExpressions;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Stable identifier for batch stages (used for lookups and API contracts)
/// </summary>
public sealed class StageKey : ValueObject
{
    // Lowercase alphanumeric with underscores (snake_case convention)
    private static readonly Regex StageKeyRegex = new(
        @"^[a-z][a-z0-9_]{1,48}[a-z0-9]$",
        RegexOptions.Compiled);

    private StageKey(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create a StageKey from a string value
    /// </summary>
    public static StageKey Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Stage key cannot be empty", nameof(key));

        var normalized = key.Trim().ToLowerInvariant();

        if (normalized.Length < 3)
            throw new ArgumentException("Stage key must be at least 3 characters", nameof(key));

        if (normalized.Length > 50)
            throw new ArgumentException("Stage key cannot exceed 50 characters", nameof(key));

        if (!StageKeyRegex.IsMatch(normalized))
            throw new ArgumentException(
                "Stage key must be lowercase alphanumeric with underscores (snake_case)", 
                nameof(key));

        return new StageKey(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(StageKey? key) => key?.Value ?? string.Empty;
}

