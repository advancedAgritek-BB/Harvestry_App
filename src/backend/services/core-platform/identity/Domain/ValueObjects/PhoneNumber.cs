using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.ValueObjects;

/// <summary>
/// Phone number value object with validation
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$", // E.164 format
        RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create a PhoneNumber from a string value
    /// </summary>
    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        // Remove common formatting characters
        var cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)\.]+", "");

        // E.164 standard: max 15 digits
        if (cleaned.Length > 15)
            throw new ArgumentException("Phone number cannot exceed 15 digits", nameof(phoneNumber));

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

        return new PhoneNumber(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
