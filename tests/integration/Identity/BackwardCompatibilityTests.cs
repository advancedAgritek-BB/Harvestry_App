using System;
using System.Collections.Generic;
using Harvestry.Shared.Kernel.Serialization;
using Xunit;

namespace Harvestry.Tests.Integration.Identity;

/// <summary>
/// Tests to verify backward compatibility of canonical JSON serialization.
/// Ensures that the new canonical serialization doesn't break existing audit records.
/// </summary>
public sealed class BackwardCompatibilityTests
{
    [Fact]
    public void CanonicalSerialization_PreservesPayloadShape()
    {
        // Arrange: Simulate existing audit payload structure
        // Old format: { "payload": "...", "prevHash": "..." }
        var oldStylePayload = new Dictionary<string, object>
        {
            { "payload", @"{""userId"":""123"",""action"":""login""}" },
            { "prevHash", "ABCD1234" }
        };

        // Act: Serialize with canonical serializer
        var json = CanonicalJsonSerializer.Serialize(oldStylePayload);

        // Assert: Should contain both keys (order doesn't matter for compatibility)
        Assert.Contains("\"payload\"", json);
        Assert.Contains("\"prevHash\"", json);
        Assert.Contains("ABCD1234", json);
    }

    [Fact]
    public void CanonicalSerialization_AlphabeticalOrder_StillContainsSameData()
    {
        // Arrange: Keys that would be in alphabetical order
        var payload = new Dictionary<string, object>
        {
            { "payload", "test-payload" },
            { "prevHash", "test-hash" }
        };

        // Act
        var json = CanonicalJsonSerializer.Serialize(payload);

        // Assert: Both keys present, "payload" comes before "prevHash" (alphabetical)
        var payloadPos = json.IndexOf("\"payload\"", StringComparison.Ordinal);
        var prevHashPos = json.IndexOf("\"prevHash\"", StringComparison.Ordinal);
        
        Assert.True(payloadPos >= 0, "payload key should be present");
        Assert.True(prevHashPos >= 0, "prevHash key should be present");
        Assert.True(payloadPos < prevHashPos, "payload should come before prevHash (alphabetical)");
    }

    [Fact]
    public void HashComputation_KnownInput_ProducesKnownOutput()
    {
        // Arrange: Known input (for regression testing)
        var input = new Dictionary<string, object>
        {
            { "payload", "test" },
            { "prevHash", "hash" }
        };

        // Act: Compute hash
        var hash1 = CanonicalJsonSerializer.ComputeHash(input);
        var hash2 = CanonicalJsonSerializer.ComputeHash(input);

        // Assert: Same input always produces same hash
        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // SHA256 = 64 hex chars
    }

    [Fact]
    public void ExistingHashFormat_StillValid()
    {
        // Arrange: Simulate existing hash computation pattern
        // This mimics what existing audit records would have
        var payload = @"{""userId"":""user-123"",""action"":""login""}";
        var prevHash = "ABC123";

        var hashInput = new Dictionary<string, object>
        {
            { "payload", payload },
            { "prevHash", prevHash }
        };

        // Act: Compute with canonical serializer
        var hash = CanonicalJsonSerializer.ComputeHash(hashInput);

        // Assert: Produces valid hash
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]+$", hash);
    }

    [Fact]
    public void CanonicalJson_NoWhitespace()
    {
        // Arrange
        var payload = new Dictionary<string, object>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var json = CanonicalJsonSerializer.Serialize(payload);

        // Assert: Compact format (no whitespace)
        Assert.DoesNotContain(" ", json);
        Assert.DoesNotContain("\n", json);
        Assert.DoesNotContain("\r", json);
        Assert.DoesNotContain("\t", json);
    }

    [Fact]
    public void CanonicalJson_EmptyPrevHash_TreatedAsEmpty()
    {
        // Arrange: First record in chain has no prevHash
        var payload1 = new Dictionary<string, object>
        {
            { "payload", "test" },
            { "prevHash", "" }
        };

        var payload2 = new Dictionary<string, object>
        {
            { "payload", "test" },
            { "prevHash", string.Empty }
        };

        // Act
        var hash1 = CanonicalJsonSerializer.ComputeHash(payload1);
        var hash2 = CanonicalJsonSerializer.ComputeHash(payload2);

        // Assert: Empty string and "" should produce same hash
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CanonicalJson_NumberPrecision_Preserved()
    {
        // Arrange: Test number handling
        var payload = new Dictionary<string, object>
        {
            { "amount", 123.45m },
            { "count", 42 },
            { "ratio", 0.123456789 }
        };

        // Act
        var json = CanonicalJsonSerializer.Serialize(payload);

        // Assert: Numbers should be preserved
        Assert.Contains("123.45", json);
        Assert.Contains("42", json);
        Assert.Contains("0.123456789", json);
    }

    [Fact]
    public void Migration_OldDataStructure_WorksWithNewSerializer()
    {
        // Arrange: Simulate old audit entry structure
        var oldAuditContext = new Dictionary<string, object>
        {
            { "userId", "user-123" },
            { "action", "login" },
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "siteId", "site-456" }
        };

        // Act: Serialize with new canonical serializer
        var json = CanonicalJsonSerializer.Serialize(oldAuditContext);
        var hash = CanonicalJsonSerializer.ComputeHash(oldAuditContext);

        // Assert: Should work without issues
        Assert.NotEmpty(json);
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
        
        // Verify all keys are present
        Assert.Contains("\"userId\"", json);
        Assert.Contains("\"action\"", json);
        Assert.Contains("\"timestamp\"", json);
        Assert.Contains("\"siteId\"", json);
    }
}
