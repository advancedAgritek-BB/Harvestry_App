using System;
using System.Collections.Generic;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.ValueObjects;

/// <summary>
/// Badge code value object (barcode or RFID identifier)
/// </summary>
public sealed class BadgeCode : ValueObject
{
    private BadgeCode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create a BadgeCode from a string value
    /// </summary>
    public static BadgeCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Badge code cannot be empty", nameof(code));

        var trimmed = code.Trim();

        if (trimmed.Length < 4)
            throw new ArgumentException("Badge code must be at least 4 characters", nameof(code));

        if (trimmed.Length > 100)
            throw new ArgumentException("Badge code cannot exceed 100 characters", nameof(code));

        return new BadgeCode(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BadgeCode code) => code.Value;
}
