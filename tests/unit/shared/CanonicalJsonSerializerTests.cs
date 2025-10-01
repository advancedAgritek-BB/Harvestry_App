using System;
using System.Collections.Generic;
using Harvestry.Shared.Kernel.Serialization;
using Xunit;

namespace Harvestry.Tests.Unit.Shared;

/// <summary>
/// Unit tests for CanonicalJsonSerializer to ensure deterministic JSON serialization.
/// </summary>
public sealed class CanonicalJsonSerializerTests
{
    [Fact]
    public void Serialize_SortsKeys_Alphabetically()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "zebra", "z" },
            { "apple", "a" },
            { "mango", "m" }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Equal(@"{""apple"":""a"",""mango"":""m"",""zebra"":""z""}", result);
    }

    [Fact]
    public void Serialize_DifferentInsertionOrder_ProducesSameOutput()
    {
        // Arrange
        var obj1 = new Dictionary<string, object>
        {
            { "b", 2 },
            { "a", 1 },
            { "c", 3 }
        };

        var obj2 = new Dictionary<string, object>
        {
            { "a", 1 },
            { "c", 3 },
            { "b", 2 }
        };

        // Act
        var result1 = CanonicalJsonSerializer.Serialize(obj1);
        var result2 = CanonicalJsonSerializer.Serialize(obj2);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(@"{""a"":1,""b"":2,""c"":3}", result1);
    }

    [Fact]
    public void Serialize_NestedObjects_SortsAllLevels()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "outer", new Dictionary<string, object>
                {
                    { "zebra", "z" },
                    { "apple", "a" }
                }
            },
            { "first", "1" }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"""first"":""1""", result);
        Assert.Contains(@"""outer"":{""apple"":""a"",""zebra"":""z""}", result);
        // Verify outer comes after first
        var firstPos = result.IndexOf("first", StringComparison.Ordinal);
        var outerPos = result.IndexOf("outer", StringComparison.Ordinal);
        Assert.True(firstPos < outerPos, "Keys should be sorted: 'first' before 'outer'");
    }

    [Fact]
    public void ComputeHash_IdenticalContent_ProducesIdenticalHash()
    {
        // Arrange
        var obj1 = new Dictionary<string, object>
        {
            { "userId", "user-123" },
            { "action", "login" },
            { "timestamp", "2025-09-29T10:00:00Z" }
        };

        var obj2 = new Dictionary<string, object>
        {
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "action", "login" },
            { "userId", "user-123" }
        };

        // Act
        var hash1 = CanonicalJsonSerializer.ComputeHash(obj1);
        var hash2 = CanonicalJsonSerializer.ComputeHash(obj2);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
        Assert.Equal(64, hash1.Length); // SHA256 produces 64 hex characters
    }

    [Fact]
    public void ComputeHash_DifferentContent_ProducesDifferentHash()
    {
        // Arrange
        var obj1 = new Dictionary<string, object> { { "a", 1 } };
        var obj2 = new Dictionary<string, object> { { "a", 2 } };

        // Act
        var hash1 = CanonicalJsonSerializer.ComputeHash(obj1);
        var hash2 = CanonicalJsonSerializer.ComputeHash(obj2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Serialize_Numbers_UsesStrictHandling()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "int", 42 },
            { "long", 9999999999L },
            { "double", 3.14159 }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"""double"":3.14159", result);
        Assert.Contains(@"""int"":42", result);
        Assert.Contains(@"""long"":9999999999", result);
    }

    [Fact]
    public void Serialize_EmptyDictionary_ReturnsEmptyObject()
    {
        // Arrange
        var obj = new Dictionary<string, object>();

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Equal("{}", result);
    }

    [Fact]
    public void Serialize_NullValue_ReturnsNull()
    {
        // Act
        var result = CanonicalJsonSerializer.Serialize(null);

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void Serialize_ArrayValues_PreservesOrder()
    {
        // Arrange: Arrays should preserve order (not sort)
        var obj = new Dictionary<string, object>
        {
            { "items", new List<object> { "zebra", "apple", "mango" } }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"""items"":[""zebra"",""apple"",""mango""]", result);
    }

    [Fact]
    public void Serialize_SpecialCharacters_EscapedCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "quote", "He said \"hello\"" },
            { "newline", "line1\nline2" }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"\""", result); // Escaped quote
        Assert.Contains(@"\n", result);  // Escaped newline
    }

    [Fact]
    public void Serialize_BooleanValues_SerializedCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "isActive", true },
            { "isDeleted", false }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"""isActive"":true", result);
        Assert.Contains(@"""isDeleted"":false", result);
    }

    [Fact]
    public void SerializeDictionary_NullOrEmpty_ReturnsEmptyObject()
    {
        // Act
        var resultNull = CanonicalJsonSerializer.SerializeDictionary(null);
        var resultEmpty = CanonicalJsonSerializer.SerializeDictionary(new Dictionary<string, object>());

        // Assert
        Assert.Equal("{}", resultNull);
        Assert.Equal("{}", resultEmpty);
    }

    [Fact]
    public void SerializeDictionary_SortsByKey()
    {
        // Arrange
        var dict = new Dictionary<string, object>
        {
            { "z", "last" },
            { "a", "first" },
            { "m", "middle" }
        };

        // Act
        var result = CanonicalJsonSerializer.SerializeDictionary(dict);

        // Assert
        Assert.Equal(@"{""a"":""first"",""m"":""middle"",""z"":""last""}", result);
    }

    [Fact]
    public void Serialize_CaseSensitiveSorting_WorksCorrectly()
    {
        // Arrange: Ordinal comparison is case-sensitive
        // Capital letters come before lowercase in ordinal sort
        var obj = new Dictionary<string, object>
        {
            { "zebra", "z" },
            { "Zebra", "Z" },
            { "apple", "a" },
            { "Apple", "A" }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        // Ordinal sort: 'A' (65) < 'Z' (90) < 'a' (97) < 'z' (122)
        Assert.Equal(@"{""Apple"":""A"",""Zebra"":""Z"",""apple"":""a"",""zebra"":""z""}", result);
    }

    [Fact]
    public void ComputeHash_ComplexNestedObject_IsDeterministic()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "user", new Dictionary<string, object>
                {
                    { "name", "Alice" },
                    { "id", "123" },
                    { "roles", new List<object> { "admin", "user" } }
                }
            },
            { "action", "update" },
            { "timestamp", "2025-09-29T10:00:00Z" }
        };

        // Act: Serialize multiple times
        var hash1 = CanonicalJsonSerializer.ComputeHash(obj);
        var hash2 = CanonicalJsonSerializer.ComputeHash(obj);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Serialize_CompactFormat_NoWhitespace()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.DoesNotContain(" ", result);
        Assert.DoesNotContain("\n", result);
        Assert.DoesNotContain("\r", result);
        Assert.DoesNotContain("\t", result);
    }

    [Fact]
    public void Serialize_WithOptions_IndentedFormat()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "key1", "value1" }
        };
        var options = new CanonicalJsonOptions { WriteIndented = true };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj, options);

        // Assert
        Assert.Contains("\n", result); // Should have newlines when indented
    }

    [Fact]
    public void ComputeHash_RealAuditScenario_ProducesSameHash()
    {
        // Arrange: Simulate audit chain scenario
        var payload1 = new Dictionary<string, object>
        {
            { "payload", @"{""userId"":""123"",""action"":""login""}" },
            { "prevHash", "ABCD1234" }
        };

        var payload2 = new Dictionary<string, object>
        {
            { "prevHash", "ABCD1234" },
            { "payload", @"{""userId"":""123"",""action"":""login""}" }
        };

        // Act
        var hash1 = CanonicalJsonSerializer.ComputeHash(payload1);
        var hash2 = CanonicalJsonSerializer.ComputeHash(payload2);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Serialize_NullValueInDictionary_SerializedCorrectly()
    {
        // Arrange
        var obj = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", null! }
        };

        // Act
        var result = CanonicalJsonSerializer.Serialize(obj);

        // Assert
        Assert.Contains(@"""key2"":null", result);
    }
}
