using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harvestry.Shared.Kernel.Serialization;

/// <summary>
/// Provides canonical JSON serialization for deterministic hashing.
/// 
/// Canonical JSON ensures that identical data always produces identical JSON output,
/// regardless of key insertion order or other non-semantic variations.
/// 
/// Rules:
/// 1. All object keys are sorted alphabetically (case-sensitive, ordinal)
/// 2. No whitespace (compact format)
/// 3. Strict number handling (no precision loss)
/// 4. UTF-8 encoding
/// 5. Recursive sorting at all nesting levels
/// 
/// This is critical for audit hash chains where different services or refactorings
/// might produce the same data in different orders.
/// </summary>
public static class CanonicalJsonSerializer
{
    private static readonly JsonSerializerOptions CanonicalOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.Strict,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new SortedDictionaryConverter()
        }
    };

    /// <summary>
    /// Serializes an object to canonical JSON string.
    /// Keys are sorted alphabetically at all nesting levels.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <returns>Canonical JSON string</returns>
    public static string Serialize(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        return JsonSerializer.Serialize(value, CanonicalOptions);
    }

    /// <summary>
    /// Serializes an object to canonical JSON with custom options.
    /// </summary>
    /// <param name="value">The object to serialize</param>
    /// <param name="options">Custom options (key sorting always enabled)</param>
    /// <returns>Canonical JSON string</returns>
    public static string Serialize(object? value, CanonicalJsonOptions options)
    {
        if (value is null)
        {
            return "null";
        }

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = options.WriteIndented,
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            NumberHandling = options.NumberHandling,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
            {
                new SortedDictionaryConverter()
            }
        };

        return JsonSerializer.Serialize(value, jsonOptions);
    }

    /// <summary>
    /// Computes SHA256 hash of the canonical JSON representation.
    /// This is useful for audit trails where identical data must produce identical hashes.
    /// </summary>
    /// <param name="value">The object to hash</param>
    /// <returns>Hex-encoded SHA256 hash</returns>
    public static string ComputeHash(object? value)
    {
        var json = Serialize(value);
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Normalizes a dictionary by sorting its keys, then serializes to canonical JSON.
    /// This is useful when working with dictionaries that might have keys in different orders.
    /// </summary>
    /// <param name="dictionary">The dictionary to normalize and serialize</param>
    /// <returns>Canonical JSON string with sorted keys</returns>
    public static string SerializeDictionary(IDictionary<string, object>? dictionary)
    {
        if (dictionary is null || dictionary.Count == 0)
        {
            return "{}";
        }

        // Sort by key and create new sorted dictionary
        var sorted = new SortedDictionary<string, object>(dictionary, StringComparer.Ordinal);
        
        return JsonSerializer.Serialize(sorted, CanonicalOptions);
    }
}

/// <summary>
/// Options for canonical JSON serialization.
/// </summary>
public record CanonicalJsonOptions
{
    /// <summary>
    /// Whether to write indented (pretty-printed) JSON.
    /// Default: false (compact)
    /// </summary>
    public bool WriteIndented { get; init; } = false;

    /// <summary>
    /// How to handle JSON numbers.
    /// Default: Strict (no precision loss)
    /// </summary>
    public JsonNumberHandling NumberHandling { get; init; } = JsonNumberHandling.Strict;
}

/// <summary>
/// Custom JSON converter that ensures dictionary keys are always written in sorted order.
/// This converter handles Dictionary&lt;string, object&gt; and recursively sorts nested dictionaries.
/// </summary>
internal sealed class SortedDictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Reading is not the focus - we mainly care about deterministic writing
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var key = reader.GetString() ?? throw new JsonException("Property name cannot be null");

            reader.Read();
            var value = ReadValue(ref reader, options);

            dictionary[key] = value;
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Sort keys alphabetically (case-sensitive, ordinal comparison)
        var sortedKeys = value.Keys.OrderBy(k => k, StringComparer.Ordinal);

        foreach (var key in sortedKeys)
        {
            writer.WritePropertyName(key);
            WriteValue(writer, value[key], options);
        }

        writer.WriteEndObject();
    }

    private static object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.Number => reader.TryGetInt64(out var l) ? l : reader.GetDouble(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null!,
            JsonTokenType.StartObject => JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options)!,
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<object>>(ref reader, options)!,
            _ => throw new JsonException($"Unsupported token type: {reader.TokenType}")
        };
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else if (value is string str)
        {
            writer.WriteStringValue(str);
        }
        else if (value is bool b)
        {
            writer.WriteBooleanValue(b);
        }
        else if (value is int i)
        {
            writer.WriteNumberValue(i);
        }
        else if (value is long l)
        {
            writer.WriteNumberValue(l);
        }
        else if (value is double d)
        {
            writer.WriteNumberValue(d);
        }
        else if (value is decimal dec)
        {
            writer.WriteNumberValue(dec);
        }
        else if (value is Dictionary<string, object> dict)
        {
            // Recursively write nested dictionaries with sorted keys
            JsonSerializer.Serialize(writer, dict, options);
        }
        else if (value is IEnumerable<object> enumerable)
        {
            // Arrays preserve order (don't sort)
            writer.WriteStartArray();
            foreach (var item in enumerable)
            {
                WriteValue(writer, item, options);
            }
            writer.WriteEndArray();
        }
        else
        {
            // Fallback: use default serialization
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
