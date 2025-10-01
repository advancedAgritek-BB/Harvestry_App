using System;
using System.Collections.Generic;
using System.Reflection;
using Harvestry.Identity.Infrastructure.Jobs;
using Xunit;

namespace Harvestry.Tests.Unit.Infrastructure;

/// <summary>
/// Unit tests for audit chain verification logic, specifically focusing on
/// canonical hash computation to ensure deterministic results.
/// </summary>
public sealed class AuditChainVerificationTests
{
    [Fact]
    public void ComputeRowHash_SameData_ProducesSameHash()
    {
        // Arrange
        var prevHash = "ABCD1234";
        var payload = @"{""userId"":""123"",""action"":""login""}";

        // Act: Compute hash twice with same data
        var hash1 = InvokeComputeRowHash(prevHash, payload);
        var hash2 = InvokeComputeRowHash(prevHash, payload);

        // Assert: Should be identical
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeRowHash_NullPrevHash_TreatsAsEmptyString()
    {
        // Arrange
        var payload = @"{""userId"":""123""}";

        // Act
        var hashWithNull = InvokeComputeRowHash(null, payload);
        var hashWithEmpty = InvokeComputeRowHash(string.Empty, payload);

        // Assert: null and empty string should produce same hash
        Assert.Equal(hashWithNull, hashWithEmpty);
    }

    [Fact]
    public void ComputeRowHash_DifferentPayload_ProducesDifferentHash()
    {
        // Arrange
        var prevHash = "ABCD1234";
        var payload1 = @"{""userId"":""123""}";
        var payload2 = @"{""userId"":""456""}";

        // Act
        var hash1 = InvokeComputeRowHash(prevHash, payload1);
        var hash2 = InvokeComputeRowHash(prevHash, payload2);

        // Assert: Different payloads should produce different hashes
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeRowHash_DifferentPrevHash_ProducesDifferentHash()
    {
        // Arrange
        var payload = @"{""userId"":""123""}";
        var prevHash1 = "ABCD1234";
        var prevHash2 = "EFGH5678";

        // Act
        var hash1 = InvokeComputeRowHash(prevHash1, payload);
        var hash2 = InvokeComputeRowHash(prevHash2, payload);

        // Assert: Different prevHash should produce different hashes
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeRowHash_ReturnsHexString()
    {
        // Arrange
        var prevHash = "ABCD1234";
        var payload = @"{""userId"":""123""}";

        // Act
        var hash = InvokeComputeRowHash(prevHash, payload);

        // Assert: Should be hex string (SHA256 = 64 hex characters)
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9A-F]+$", hash); // Only hex characters
    }

    [Fact]
    public void ComputeRowHash_ComplexPayload_ProducesDeterministicHash()
    {
        // Arrange: Complex nested payload
        var prevHash = "ABCD1234";
        var payload = @"{
            ""userId"":""user-123"",
            ""action"":""update"",
            ""changes"":{
                ""email"":""new@example.com"",
                ""phone"":""+1234567890""
            },
            ""timestamp"":""2025-09-29T10:00:00Z""
        }";

        // Act: Compute multiple times
        var hash1 = InvokeComputeRowHash(prevHash, payload);
        var hash2 = InvokeComputeRowHash(prevHash, payload);
        var hash3 = InvokeComputeRowHash(prevHash, payload);

        // Assert: All should be identical
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
    }

    [Fact]
    public void ComputeRowHash_EmptyPayload_ProducesValidHash()
    {
        // Arrange
        var prevHash = "ABCD1234";
        var payload = "{}";

        // Act
        var hash = InvokeComputeRowHash(prevHash, payload);

        // Assert: Should still produce valid hash
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void ComputeRowHash_SpecialCharactersInPayload_HandledCorrectly()
    {
        // Arrange
        var prevHash = "ABCD1234";
        var payload = @"{""message"":""Hello\nWorld\t\"" with quotes \""""}";

        // Act
        var hash = InvokeComputeRowHash(prevHash, payload);

        // Assert: Should handle special characters
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length);
    }

    /// <summary>
    /// Helper method to invoke the private ComputeRowHash method via reflection.
    /// This allows us to unit test the hash computation logic without requiring
    /// a full integration test setup.
    /// </summary>
    private static string InvokeComputeRowHash(string? prevHash, string payload)
    {
        var type = typeof(AuditChainVerificationJob);
        var method = type.GetMethod("ComputeRowHash", BindingFlags.NonPublic | BindingFlags.Static);
        
        if (method == null)
        {
            throw new InvalidOperationException("ComputeRowHash method not found");
        }

        var result = method.Invoke(null, new object?[] { prevHash, payload });
        return result as string ?? throw new InvalidOperationException("ComputeRowHash returned null");
    }
}
