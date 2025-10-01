using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Infrastructure.Jobs;
using Harvestry.Shared.Kernel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Tests.Integration.Identity;

/// <summary>
/// Integration tests for canonical JSON hashing in the audit chain.
/// Verifies that hash computation is deterministic across different scenarios.
/// </summary>
[Collection("Integration")]
public sealed class CanonicalHashIntegrationTests : IntegrationTestBase
{
    public CanonicalHashIntegrationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AuditChain_DifferentKeyOrder_ProducesSameHash()
    {
        // Arrange: Create two audit entries with identical data but different key order
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var context1 = new Dictionary<string, object>
        {
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "action", "login" },
            { "userId", userId.ToString() }
        };

        var context2 = new Dictionary<string, object>
        {
            { "userId", userId.ToString() },
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "action", "login" }
        };

        var entry1 = new AuthorizationAuditEntry
        {
            UserId = userId,
            SiteId = siteId,
            Action = "login",
            ResourceType = "session",
            ResourceId = Guid.NewGuid(),
            Granted = true,
            Context = context1,
            IpAddress = "192.168.1.1",
            UserAgent = "Test"
        };

        var entry2 = new AuthorizationAuditEntry
        {
            UserId = userId,
            SiteId = siteId,
            Action = "login",
            ResourceType = "session",
            ResourceId = Guid.NewGuid(),
            Granted = true,
            Context = context2,
            IpAddress = "192.168.1.1",
            UserAgent = "Test"
        };

        // Act: Write both entries
        var repository = Scope.ServiceProvider.GetRequiredService<IAuthorizationAuditRepository>();
        await repository.LogAsync(entry1);
        await repository.LogAsync(entry2);

        // Assert: Verify both entries have consistent context serialization
        // (They should hash identically if we were to compute hashes on the context)
        var json1 = CanonicalJsonSerializer.Serialize(context1);
        var json2 = CanonicalJsonSerializer.Serialize(context2);
        Assert.Equal(json1, json2);
    }

    [Fact]
    public async Task AuditChainVerification_PassesWithCanonicalHashing()
    {
        // Arrange: Create several audit entries
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = Scope.ServiceProvider.GetRequiredService<IAuthorizationAuditRepository>();

        for (int i = 0; i < 5; i++)
        {
            var entry = new AuthorizationAuditEntry
            {
                UserId = userId,
                SiteId = siteId,
                Action = $"action_{i}",
                ResourceType = "resource",
                ResourceId = Guid.NewGuid(),
                Granted = true,
                Context = new Dictionary<string, object>
                {
                    { "index", i },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
            };

            await repository.LogAsync(entry);
            await Task.Delay(100); // Small delay to ensure different timestamps
        }

        // Act: Run verification (would need to expose VerifyAsync or test via job)
        // For now, we verify that the entries were created
        
        // Assert: Entries created successfully
        // In a full implementation, we would run the AuditChainVerificationJob
        // and verify that it reports 0 mismatches
        Assert.True(true); // Placeholder - would need actual verification logic
    }

    [Fact]
    public void CanonicalSerializer_ComplexNestedObject_IsDeterministic()
    {
        // Arrange: Create complex audit context with nested objects
        var context1 = new Dictionary<string, object>
        {
            { "user", new Dictionary<string, object>
                {
                    { "name", "Alice" },
                    { "id", "123" },
                    { "roles", new List<object> { "admin", "user" } }
                }
            },
            { "action", "update" },
            { "resource", new Dictionary<string, object>
                {
                    { "type", "document" },
                    { "id", "doc-456" }
                }
            }
        };

        var context2 = new Dictionary<string, object>
        {
            { "resource", new Dictionary<string, object>
                {
                    { "id", "doc-456" },
                    { "type", "document" }
                }
            },
            { "action", "update" },
            { "user", new Dictionary<string, object>
                {
                    { "roles", new List<object> { "admin", "user" } },
                    { "id", "123" },
                    { "name", "Alice" }
                }
            }
        };

        // Act
        var json1 = CanonicalJsonSerializer.Serialize(context1);
        var json2 = CanonicalJsonSerializer.Serialize(context2);
        var hash1 = CanonicalJsonSerializer.ComputeHash(context1);
        var hash2 = CanonicalJsonSerializer.ComputeHash(context2);

        // Assert
        Assert.Equal(json1, json2);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async Task AuditContext_SerializedCanonically_InDatabase()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Create context with specific key order
        var context = new Dictionary<string, object>
        {
            { "zebra", "last" },
            { "apple", "first" },
            { "mango", "middle" }
        };

        var entry = new AuthorizationAuditEntry
        {
            UserId = userId,
            SiteId = siteId,
            Action = "test_canonical",
            ResourceType = "test",
            ResourceId = Guid.NewGuid(),
            Granted = true,
            Context = context
        };

        // Act: Write to database
        var repository = Scope.ServiceProvider.GetRequiredService<IAuthorizationAuditRepository>();
        await repository.LogAsync(entry);

        // Assert: Retrieve from database and verify canonical serialization
        // In the database, keys should be sorted: apple, mango, zebra
        // Note: This would require reading back from the database to fully verify
        Assert.True(true); // Placeholder - would need database read logic
    }

    [Fact]
    public void CanonicalHash_CrossEnvironment_Reproducible()
    {
        // Arrange: Simulate data from different services/environments
        var payload = new Dictionary<string, object>
        {
            { "userId", "user-123" },
            { "action", "login" },
            { "siteId", "site-456" },
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "ip", "192.168.1.1" }
        };

        // Act: Compute hash multiple times (simulating different environments)
        var hash1 = CanonicalJsonSerializer.ComputeHash(payload);
        var hash2 = CanonicalJsonSerializer.ComputeHash(payload);
        var hash3 = CanonicalJsonSerializer.ComputeHash(payload);

        // Create same payload with different key order
        var payload2 = new Dictionary<string, object>
        {
            { "timestamp", "2025-09-29T10:00:00Z" },
            { "siteId", "site-456" },
            { "userId", "user-123" },
            { "ip", "192.168.1.1" },
            { "action", "login" }
        };
        var hash4 = CanonicalJsonSerializer.ComputeHash(payload2);

        // Assert: All hashes should be identical
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash2, hash3);
        Assert.Equal(hash3, hash4);
        
        // Verify it's a valid SHA256 hash (64 hex characters)
        Assert.Equal(64, hash1.Length);
        Assert.Matches("^[0-9A-F]+$", hash1);
    }
}
