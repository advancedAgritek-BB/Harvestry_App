using System;
using System.Reflection;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Xunit;

namespace Harvestry.Identity.Tests.Unit.Domain;

public sealed class SessionTests
{
    [Fact]
    public void Create_ValidData_ReturnsSession()
    {
        var session = Session.Create(Guid.NewGuid(), "token", LoginMethod.Badge, TimeSpan.FromHours(1));

        Assert.True(session.IsActive);
        Assert.Equal(LoginMethod.Badge, session.LoginMethod);
        Assert.NotNull(session.SessionToken);
    }

    [Fact]
    public void Revoke_ActiveSession_EndsSession()
    {
        var session = Session.Create(Guid.NewGuid(), "token", LoginMethod.Badge, TimeSpan.FromHours(1));

        session.Revoke("Logout");

        Assert.True(session.IsRevoked);
        Assert.NotNull(session.SessionEnd);
        Assert.Equal("Logout", session.RevokeReason);
    }

    [Fact]
    public void IsActive_ExpiredSession_ReturnsFalse()
    {
        var session = Session.Create(Guid.NewGuid(), "token", LoginMethod.Badge, TimeSpan.FromHours(1));
        SetPrivateProperty(session, nameof(Session.ExpiresAt), DateTime.UtcNow.AddMinutes(-1));

        Assert.False(session.IsActive);
    }

    [Fact]
    public void ExtendExpiration_ActiveSession_UpdatesExpiresAt()
    {
        var session = Session.Create(Guid.NewGuid(), "token", LoginMethod.Badge, TimeSpan.FromHours(1));
        var originalExpiry = session.ExpiresAt;

        session.ExtendExpiration(TimeSpan.FromMinutes(30));

        Assert.True(session.ExpiresAt > originalExpiry);
    }

    private static void SetPrivateProperty<T>(T target, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property?.SetValue(target, value);
    }
}
