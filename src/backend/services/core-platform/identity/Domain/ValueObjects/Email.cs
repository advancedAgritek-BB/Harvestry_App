using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.ValueObjects;

/// <summary>
/// Email address value object with validation
/// </summary>
public sealed class Email : ValueObject
{
    // Stricter email regex that allows plus addressing, disallows consecutive dots, leading/trailing dots, and requires 2+ char TLD
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9]([a-zA-Z0-9.+_-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    /// <summary>
    /// Create an Email from a string value
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > 255)
            throw new ArgumentException("Email cannot exceed 255 characters", nameof(email));

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
