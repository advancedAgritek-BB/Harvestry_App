using System;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Xunit;

namespace Harvestry.Identity.Tests.Unit.Domain;

public sealed class BadgeTests
{
    [Fact]
    public void Create_ValidData_ReturnsBadge()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));

        Assert.True(badge.IsActive);
        Assert.Equal(BadgeStatus.Active, badge.Status);
        Assert.NotNull(badge.BadgeCode);
    }

    [Fact]
    public void RecordUsage_ActiveBadge_UpdatesLastUsedAt()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));

        badge.RecordUsage();

        Assert.NotNull(badge.LastUsedAt);
    }

    [Fact]
    public void RecordUsage_InactiveBadge_Throws()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        badge.Deactivate();

        Assert.Throws<InvalidOperationException>(() => badge.RecordUsage());
    }

    [Fact]
    public void Revoke_ActiveBadge_UpdatesStatusAndReason()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        var revokedBy = Guid.NewGuid();
        badge.Revoke(revokedBy, "Lost badge");

        Assert.Equal(BadgeStatus.Revoked, badge.Status);
        Assert.Equal("Lost badge", badge.RevokeReason);
        Assert.Equal(revokedBy, badge.RevokedBy);
    }

    [Fact]
    public void IsActive_ExpiredBadge_ReturnsFalse()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"), expiresAt: DateTime.UtcNow.AddMinutes(-5));

        Assert.False(badge.IsActive);
    }
}
