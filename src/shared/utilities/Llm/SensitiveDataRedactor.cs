using System;
using System.Text.RegularExpressions;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Performs lightweight, deterministic redaction of common PII before sending to the model.
/// </summary>
public sealed class SensitiveDataRedactor
{
    // Simple patterns; avoid over-matching to reduce noise.
    private static readonly Regex EmailRegex = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\+?\d[\d\s().-]{6,}\d", RegexOptions.Compiled);
    private static readonly Regex AddressRegex = new(@"\d{1,5}\s+\w.+", RegexOptions.Compiled);

    public RedactionResult Redact(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new RedactionResult(input ?? string.Empty, false);
        }

        var redacted = EmailRegex.Replace(input, "***@redacted");
        redacted = PhoneRegex.Replace(redacted, "***-redacted-phone***");
        redacted = AddressRegex.Replace(redacted, "***-redacted-address***");

        var changed = !string.Equals(input, redacted, StringComparison.Ordinal);
        return new RedactionResult(redacted, changed);
    }
}

public readonly record struct RedactionResult(string Value, bool WasRedacted);



