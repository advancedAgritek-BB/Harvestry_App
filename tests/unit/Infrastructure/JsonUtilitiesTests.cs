using System.Collections.Generic;
using Harvestry.Identity.Infrastructure.Persistence;
using Xunit;

namespace Harvestry.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for JsonUtilities to verify serialization behavior.
/// </summary>
public sealed class JsonUtilitiesTests
{
    [Fact]
    public void SerializeDictionary_EmptyDictionary_ReturnsEmptyObject()
    {
        // Arrange
        var dict = new Dictionary<string, object>();

        // Act
        var result = JsonUtilities.SerializeDictionary(dict);

        // Assert
        Assert.Equal("{}", result);
    }

    [Fact]
    public void SerializeDictionary_NullDictionary_ReturnsEmptyObject()
    {
        // Act
        var result = JsonUtilities.SerializeDictionary(null);

        // Assert
        Assert.Equal("{}", result);
    }

    [Fact]
    public void SerializeDictionaryCanonical_EmptyDictionary_ReturnsEmptyObject()
    {
        // Arrange
        var dict = new Dictionary<string, object>();

        // Act
        var result = JsonUtilities.SerializeDictionaryCanonical(dict);

        // Assert
        Assert.Equal("{}", result);
    }

    [Fact]
    public void SerializeDictionaryCanonical_NullDictionary_ReturnsEmptyObject()
    {
        // Act
        var result = JsonUtilities.SerializeDictionaryCanonical(null);

        // Assert
        Assert.Equal("{}", result);
    }

    [Fact]
    public void SerializeDictionaryCanonical_DifferentOrder_ProducesSameOutput()
    {
        // Arrange
        var dict1 = new Dictionary<string, object>
        {
            { "b", 2 },
            { "a", 1 },
            { "c", 3 }
        };

        var dict2 = new Dictionary<string, object>
        {
            { "c", 3 },
            { "a", 1 },
            { "b", 2 }
        };

        // Act
        var result1 = JsonUtilities.SerializeDictionaryCanonical(dict1);
        var result2 = JsonUtilities.SerializeDictionaryCanonical(dict2);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(@"{""a"":1,""b"":2,""c"":3}", result1);
    }

    [Fact]
    public void SerializeDictionary_PreservesInsertionOrder()
    {
        // Arrange: Regular serialization preserves order
        var dict = new Dictionary<string, object>
        {
            { "zebra", "z" },
            { "apple", "a" },
            { "mango", "m" }
        };

        // Act
        var result = JsonUtilities.SerializeDictionary(dict);

        // Assert: Should preserve insertion order (zebra, apple, mango)
        var zebraPos = result.IndexOf("zebra");
        var applePos = result.IndexOf("apple");
        var mangoPos = result.IndexOf("mango");
        
        Assert.True(zebraPos < applePos, "zebra should come before apple (insertion order)");
        Assert.True(applePos < mangoPos, "apple should come before mango (insertion order)");
    }

    [Fact]
    public void SerializeDictionaryCanonical_SortsKeys()
    {
        // Arrange: Canonical serialization sorts keys
        var dict = new Dictionary<string, object>
        {
            { "zebra", "z" },
            { "apple", "a" },
            { "mango", "m" }
        };

        // Act
        var result = JsonUtilities.SerializeDictionaryCanonical(dict);

        // Assert: Should be sorted alphabetically
        Assert.Equal(@"{""apple"":""a"",""mango"":""m"",""zebra"":""z""}", result);
        
        var applePos = result.IndexOf("apple");
        var mangoPos = result.IndexOf("mango");
        var zebraPos = result.IndexOf("zebra");
        
        Assert.True(applePos < mangoPos, "apple should come before mango (sorted)");
        Assert.True(mangoPos < zebraPos, "mango should come before zebra (sorted)");
    }

    [Fact]
    public void ToStringList_ValidArray_ReturnsStringList()
    {
        // Arrange
        var json = @"[""apple"",""banana"",""cherry""]";

        // Act
        var result = JsonUtilities.ToStringList(json);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("apple", result[0]);
        Assert.Equal("banana", result[1]);
        Assert.Equal("cherry", result[2]);
    }

    [Fact]
    public void ToStringList_NullInput_ReturnsEmptyList()
    {
        // Act
        var result = JsonUtilities.ToStringList(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToDictionary_ValidObject_ReturnsDictionary()
    {
        // Arrange
        var json = @"{""key1"":""value1"",""key2"":""value2""}";

        // Act
        var result = JsonUtilities.ToDictionary(json);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void ToDictionary_NullInput_ReturnsEmptyDictionary()
    {
        // Act
        var result = JsonUtilities.ToDictionary(null);

        // Assert
        Assert.Empty(result);
    }
}
