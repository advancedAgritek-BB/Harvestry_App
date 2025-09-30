using System;
using System.Linq;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Xunit;

namespace Harvestry.Identity.Tests.Unit.Domain;

public sealed class UserTests
{
    [Fact]
    public void Create_ValidData_ReturnsUser()
    {
        var email = Email.Create("user@example.com");
        var user = User.Create(email, "Test", "User");

        Assert.Equal(email, user.Email);
        Assert.Equal("Test", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void SetPassword_ValidData_UpdatesHashAndSalt()
    {
        var user = CreateUser();
        user.SetPassword("hash", "salt");

        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal("salt", user.PasswordSalt);
        Assert.False(user.IsLocked);
    }

    [Fact]
    public void RecordFailedLoginAttempt_ExceedsMax_LocksAccount()
    {
        var user = CreateUser();
        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt();
        }

        Assert.True(user.IsLocked);
        Assert.NotNull(user.LockedUntil);
    }

    [Fact]
    public void RecordSuccessfulLogin_ResetsFailedAttempts()
    {
        var user = CreateUser();
        for (var i = 0; i < 3; i++)
        {
            user.RecordFailedLoginAttempt();
        }

        user.RecordSuccessfulLogin();

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.False(user.IsLocked);
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public void Unlock_LockedAccount_ClearsLockout()
    {
        var user = CreateUser();
        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt();
        }

        user.Unlock(Guid.NewGuid());

        Assert.False(user.IsLocked);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public void AssignToSite_NewSite_AddsUserSite()
    {
        var user = CreateUser();
        var siteId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        user.AssignToSite(siteId, roleId, true, Guid.NewGuid());

        Assert.Single(user.UserSites);
        Assert.Equal(siteId, user.UserSites.First().SiteId);
    }

    [Fact]
    public void AssignToSite_DuplicateSite_Throws()
    {
        var user = CreateUser();
        var siteId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        user.AssignToSite(siteId, roleId, false, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            user.AssignToSite(siteId, roleId, false, Guid.NewGuid()));
    }

    [Fact]
    public void Suspend_ActiveUser_ChangesSuspendedStatus()
    {
        var user = CreateUser();
        user.Suspend(Guid.NewGuid(), "Policy violation");

        Assert.Equal(UserStatus.Suspended, user.Status);
    }

    private static User CreateUser()
    {
        return User.Create(Email.Create("user@example.com"), "Test", "User");
    }
}
