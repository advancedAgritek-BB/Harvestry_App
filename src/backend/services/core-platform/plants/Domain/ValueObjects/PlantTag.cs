namespace Harvestry.Plants.Domain.ValueObjects;

/// <summary>
/// Represents a METRC plant tag (RFID label) - immutable value object
/// </summary>
public sealed class PlantTag : IEquatable<PlantTag>
{
    /// <summary>
    /// The tag number/identifier
    /// </summary>
    public string Value { get; }

    private PlantTag(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PlantTag from a string value
    /// </summary>
    public static PlantTag Create(string tagNumber)
    {
        if (string.IsNullOrWhiteSpace(tagNumber))
            throw new ArgumentException("Plant tag number cannot be empty", nameof(tagNumber));

        var trimmed = tagNumber.Trim().ToUpperInvariant();
        
        // METRC tags are typically 24 characters, but this can vary by state
        if (trimmed.Length < 10 || trimmed.Length > 30)
            throw new ArgumentException("Plant tag must be between 10 and 30 characters", nameof(tagNumber));

        return new PlantTag(trimmed);
    }

    /// <summary>
    /// Attempts to create a PlantTag, returning null if invalid
    /// </summary>
    public static PlantTag? TryCreate(string? tagNumber)
    {
        if (string.IsNullOrWhiteSpace(tagNumber))
            return null;

        var trimmed = tagNumber.Trim().ToUpperInvariant();
        
        if (trimmed.Length < 10 || trimmed.Length > 30)
            return null;

        return new PlantTag(trimmed);
    }

    public bool Equals(PlantTag? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as PlantTag);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(PlantTag? left, PlantTag? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PlantTag? left, PlantTag? right) => !(left == right);

    public static implicit operator string(PlantTag tag) => tag.Value;
}



