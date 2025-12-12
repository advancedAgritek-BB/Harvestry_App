using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Harvestry.Shared.Utilities.Llm;

/// <summary>
/// Performs lightweight output scanning to catch obvious policy violations before surfacing to users.
/// This is intentionally simple and should be augmented with provider-native safety endpoints if available.
/// </summary>
public sealed class ContentSafetyEvaluator
{
    private static readonly Regex PiiPattern = new(@"ssn|social security|credit card|passport", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UnsafeActionPattern = new(@"delete\s+all|shutdown|truncate\s+table|drop\s+table", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public ContentSafetyResult Evaluate(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ContentSafetyResult.Safe();
        }

        var flagged = PiiPattern.IsMatch(content) || UnsafeActionPattern.IsMatch(content);
        var reasons = new List<string>();

        if (PiiPattern.IsMatch(content))
        {
            reasons.Add("Detected possible PII disclosure.");
        }

        if (UnsafeActionPattern.IsMatch(content))
        {
            reasons.Add("Detected unsafe action language.");
        }

        return flagged ? ContentSafetyResult.Flagged(reasons) : ContentSafetyResult.Safe();
    }
}

public sealed class ContentSafetyResult
{
    private ContentSafetyResult(bool flagged, IReadOnlyCollection<string> reasons)
    {
        IsFlagged = flagged;
        Reasons = reasons;
    }

    public bool IsFlagged { get; }

    public IReadOnlyCollection<string> Reasons { get; }

    public static ContentSafetyResult Safe() => new(false, Array.Empty<string>());

    public static ContentSafetyResult Flagged(IEnumerable<string> reasons) =>
        new(true, reasons.ToArray());
}




