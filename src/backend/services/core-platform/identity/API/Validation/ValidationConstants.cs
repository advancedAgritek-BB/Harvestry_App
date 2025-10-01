using System.Text.RegularExpressions;

namespace Harvestry.Identity.API.Validation;

/// <summary>
/// Centralized validation constants and patterns used across all validators.
/// </summary>
public static class ValidationConstants
{
    /// <summary>
    /// E.164 phone number pattern: optional +, first digit 1-9, then 0-14 more digits (max 15 total).
    /// Examples: +14155552671, 14155552671, 9155552671
    /// </summary>
    public const string E164Pattern = @"^\+?[1-9]\d{0,14}$";

    /// <summary>
    /// Compiled regex for E.164 phone validation (better performance).
    /// </summary>
    public static readonly Regex PhoneRegex = new(E164Pattern, RegexOptions.Compiled);

    /// <summary>
    /// Common string length limits
    /// </summary>
    public static class StringLimits
    {
        public const int ShortString = 100;
        public const int MediumString = 512;
        public const int LongString = 2000;
        public const int Email = 320; // RFC 5321
        public const int Url = 2048;
        public const int Phone = 15; // E.164 max
    }

    /// <summary>
    /// Validation error messages
    /// </summary>
    public static class ErrorMessages
    {
        public const string InvalidPhoneFormat = "Phone number must be in E.164 format (e.g., +14155552671 or 9155552671).";
        public const string PhoneRequired = "Phone number is required.";
        public const string PhoneTooLong = "Phone number cannot exceed 15 digits.";
    }
}
