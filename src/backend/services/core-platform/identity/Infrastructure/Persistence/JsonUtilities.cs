using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Harvestry.Shared.Kernel.Serialization;

namespace Harvestry.Identity.Infrastructure.Persistence;

internal static class JsonUtilities
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    internal static Dictionary<string, object> ToDictionary(object? value)
    {
        if (value is null)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        var json = value switch
        {
            string s when !string.IsNullOrWhiteSpace(s) => s,
            JsonDocument document => document.RootElement.GetRawText(),
            JsonElement element => element.GetRawText(),
            _ => value.ToString() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            return ParseObject(document.RootElement);
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal static IReadOnlyList<string> ToStringList(object? value)
    {
        if (value is null)
        {
            return Array.Empty<string>();
        }

        var json = value switch
        {
            string s when !string.IsNullOrWhiteSpace(s) => s,
            JsonDocument document => document.RootElement.GetRawText(),
            JsonElement element => element.GetRawText(),
            _ => value.ToString() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            var result = JsonSerializer.Deserialize<string[]>(json, SerializerOptions);
            return result ?? Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Serializes dictionary to JSON (preserves insertion order).
    /// For audit hashing, use SerializeDictionaryCanonical() instead.
    /// </summary>
    internal static string SerializeDictionary(IDictionary<string, object> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(dictionary, SerializerOptions);
    }

    /// <summary>
    /// Serializes dictionary to canonical JSON with sorted keys.
    /// Use this for audit hashing to ensure deterministic output.
    /// </summary>
    internal static string SerializeDictionaryCanonical(IDictionary<string, object> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return "{}";
        }

        return CanonicalJsonSerializer.SerializeDictionary(dictionary);
    }

    internal static string SerializeStringCollection(IEnumerable<string> values)
    {
        var list = values?.ToArray() ?? Array.Empty<string>();
        return JsonSerializer.Serialize(list, SerializerOptions);
    }

    private static Dictionary<string, object> ParseObject(JsonElement element)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in element.EnumerateObject())
        {
            var parsed = ParseElement(property.Value);
            if (parsed is not null)
            {
                result[property.Name] = parsed;
            }
        }

        return result;
    }

    private static object? ParseElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l)
                ? l
                : element.TryGetDouble(out var d)
                    ? d
                    : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ParseElement)
                .Where(v => v is not null)
                .Cast<object>()
                .ToList(),
            JsonValueKind.Object => ParseObject(element),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
    }
}
