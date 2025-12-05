using System.Text.RegularExpressions;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.ValueObjects;

/// <summary>
/// Batch code value object with validation
/// Supports flexible formats configured per site via batch code rules
/// </summary>
public sealed class BatchCode : ValueObject
{
    // Allows alphanumeric, hyphens, underscores - flexible for site-specific rules
    private static readonly Regex BatchCodeRegex = new(
        @"^[A-Z0-9][A-Z0-9\-_]{1,98}[A-Z0-9]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private BatchCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create a BatchCode from a string value
    /// </summary>
    public static BatchCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Batch code cannot be empty", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length < 3)
            throw new ArgumentException("Batch code must be at least 3 characters", nameof(code));

        if (normalized.Length > 100)
            throw new ArgumentException("Batch code cannot exceed 100 characters", nameof(code));

        if (!BatchCodeRegex.IsMatch(normalized))
            throw new ArgumentException(
                "Batch code must contain only alphanumeric characters, hyphens, and underscores", 
                nameof(code));

        return new BatchCode(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BatchCode code) => code.Value;
}

