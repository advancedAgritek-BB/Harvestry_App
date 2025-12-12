using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Shared.Utilities.Llm;

public sealed record DataQaSuggestion(string Title, string Detail, double Confidence, string SourceId, string Category);

public sealed class DataQaThresholds
{
    public double MinimumConfidence { get; init; } = 0.65;
    public int MaxSuggestions { get; init; } = 5;
}

/// <summary>
/// Applies thresholds and deduplication to LLM QA suggestions to reduce noise.
/// </summary>
public sealed class DataQaSuggestionFilter
{
    private readonly DataQaThresholds _thresholds;

    public DataQaSuggestionFilter(DataQaThresholds thresholds)
    {
        _thresholds = thresholds ?? throw new ArgumentNullException(nameof(thresholds));
    }

    public IReadOnlyCollection<DataQaSuggestion> Filter(IEnumerable<DataQaSuggestion> suggestions)
    {
        var filtered = suggestions
            .Where(s => s.Confidence >= _thresholds.MinimumConfidence)
            .OrderByDescending(s => s.Confidence)
            .ThenBy(s => s.Title)
            .Take(_thresholds.MaxSuggestions)
            .ToList();

        return Deduplicate(filtered);
    }

    private static IReadOnlyCollection<DataQaSuggestion> Deduplicate(IEnumerable<DataQaSuggestion> suggestions)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<DataQaSuggestion>();

        foreach (var suggestion in suggestions)
        {
            var key = $"{suggestion.Category}:{suggestion.Title}";
            if (seen.Add(key))
            {
                result.Add(suggestion);
            }
        }

        return result;
    }
}




