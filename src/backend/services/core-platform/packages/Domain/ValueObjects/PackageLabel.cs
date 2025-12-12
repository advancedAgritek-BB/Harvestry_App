namespace Harvestry.Packages.Domain.ValueObjects;

/// <summary>
/// Represents a METRC package tag/label (RFID) - immutable value object
/// </summary>
public sealed class PackageLabel : IEquatable<PackageLabel>
{
    /// <summary>
    /// The tag/label value
    /// </summary>
    public string Value { get; }

    private PackageLabel(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a PackageLabel from a string value
    /// </summary>
    public static PackageLabel Create(string labelValue)
    {
        if (string.IsNullOrWhiteSpace(labelValue))
            throw new ArgumentException("Package label cannot be empty", nameof(labelValue));

        var trimmed = labelValue.Trim().ToUpperInvariant();
        
        // METRC tags are typically 24 characters, but this can vary by state
        if (trimmed.Length < 10 || trimmed.Length > 30)
            throw new ArgumentException("Package label must be between 10 and 30 characters", nameof(labelValue));

        return new PackageLabel(trimmed);
    }

    /// <summary>
    /// Attempts to create a PackageLabel, returning null if invalid
    /// </summary>
    public static PackageLabel? TryCreate(string? labelValue)
    {
        if (string.IsNullOrWhiteSpace(labelValue))
            return null;

        var trimmed = labelValue.Trim().ToUpperInvariant();
        
        if (trimmed.Length < 10 || trimmed.Length > 30)
            return null;

        return new PackageLabel(trimmed);
    }

    public bool Equals(PackageLabel? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as PackageLabel);

    public override int GetHashCode() => Value.ToUpperInvariant().GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(PackageLabel? left, PackageLabel? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(PackageLabel? left, PackageLabel? right) => !(left == right);

    public static implicit operator string(PackageLabel label) => label.Value;
}









