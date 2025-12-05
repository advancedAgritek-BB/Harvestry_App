using System.Text.RegularExpressions;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Plant identifier value object for mother plants
/// </summary>
public sealed class PlantId : ValueObject
{
    // Alphanumeric, hyphens, underscores for plant tags/IDs
    private static readonly Regex PlantIdRegex = new(
        @"^[A-Z0-9][A-Z0-9\-_]{0,48}[A-Z0-9]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private PlantId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create a PlantId from a string value
    /// </summary>
    public static PlantId Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Plant ID cannot be empty", nameof(id));

        var normalized = id.Trim().ToUpperInvariant();

        if (normalized.Length < 2)
            throw new ArgumentException("Plant ID must be at least 2 characters", nameof(id));

        if (normalized.Length > 50)
            throw new ArgumentException("Plant ID cannot exceed 50 characters", nameof(id));

        if (!PlantIdRegex.IsMatch(normalized))
            throw new ArgumentException(
                "Plant ID must contain only alphanumeric characters, hyphens, and underscores", 
                nameof(id));

        return new PlantId(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PlantId id) => id.Value;
}

