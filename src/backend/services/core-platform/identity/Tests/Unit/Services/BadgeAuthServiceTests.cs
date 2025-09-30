using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Application.Services;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Identity.Tests.Unit.Services;

public sealed class BadgeAuthServiceTests
{
    private readonly Mock<IBadgeRepository> _badgeRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ISessionRepository> _sessionRepository = new();
    private readonly Mock<ILogger<BadgeAuthService>> _logger = new();

    private BadgeAuthService CreateService()
    {
        return new BadgeAuthService(
            _badgeRepository.Object,
            _userRepository.Object,
            _sessionRepository.Object,
            _logger.Object);
    }

    private static void VerifyLog(Mock<ILogger<BadgeAuthService>> logger, LogLevel level, string contains)
    {
        logger.Verify(log => log.Log(
            It.Is<LogLevel>(l => l == level),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() != null && state.ToString()!.Contains(contains)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_InvalidBadgeFormat_ReturnsFailure()
    {
        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("abc", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Invalid badge code format", result.ErrorMessage);
        _badgeRepository.Verify(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_BadgeNotFound_ReturnsGenericError()
    {
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Badge?)null);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Invalid badge or credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_InactiveBadge_ReturnsGenericError()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        badge.Deactivate();
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", badge.SiteId);

        Assert.False(result.Success);
        Assert.Equal("Invalid badge or credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_RevokedBadge_ReturnsGenericError()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        badge.Revoke(Guid.NewGuid(), "Lost badge");
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", badge.SiteId);

        Assert.False(result.Success);
        Assert.Equal("Invalid badge or credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_WrongSite_ReturnsGenericError()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Invalid badge or credentials", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_UserNotFound_ReturnsFailure()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _userRepository.Setup(repo => repo.GetByIdAsync(badge.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", badge.SiteId);

        Assert.False(result.Success);
        Assert.Equal("User account not found", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_UserSuspended_ReturnsFailure()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        user.Suspend(Guid.NewGuid(), "Policy");

        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _userRepository.Setup(repo => repo.GetByIdAsync(badge.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", badge.SiteId);

        Assert.False(result.Success);
        Assert.Equal("User account is suspended", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_UserLocked_ReturnsFailureWithLockoutTime()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt();
        }

        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _userRepository.Setup(repo => repo.GetByIdAsync(badge.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", badge.SiteId);

        Assert.False(result.Success);
        Assert.Contains("Account is locked", result.ErrorMessage);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_ValidBadge_ReturnsSuccessWithToken()
    {
        var siteId = Guid.NewGuid();
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var badge = Badge.Create(user.Id, siteId, BadgeCode.Create("ABCD1234"));

        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _badgeRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Badge>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sessionRepository.Setup(repo => repo.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session s, CancellationToken _) => s);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", siteId, "127.0.0.1", "TestAgent/1.0");

        Assert.True(result.Success);
        Assert.NotNull(result.SessionId);
        Assert.NotNull(result.SessionToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.NotNull(result.ExpiresAt);
        Assert.NotNull(badge.LastUsedAt);
        Assert.NotNull(user.LastLoginAt);

        _badgeRepository.Verify(repo => repo.UpdateAsync(It.Is<Badge>(b => b.LastUsedAt.HasValue), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(repo => repo.UpdateAsync(It.Is<User>(u => u.LastLoginAt.HasValue), It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepository.Verify(repo => repo.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginWithBadgeAsync_TokenGeneration_Is256Bits()
    {
        var siteId = Guid.NewGuid();
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var badge = Badge.Create(user.Id, siteId, BadgeCode.Create("ABCD1234"));

        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _sessionRepository.Setup(repo => repo.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session s, CancellationToken _) => s);

        var service = CreateService();
        var result = await service.LoginWithBadgeAsync("ABCD1234", siteId);

        var token = result.SessionToken ?? throw new Xunit.Sdk.XunitException("Token not generated");
        var padded = token.Replace('-', '+').Replace('_', '/');
        while (padded.Length % 4 != 0)
        {
            padded += "=";
        }

        var bytes = Convert.FromBase64String(padded);
        Assert.Equal(32, bytes.Length); // 256 bits
    }

    [Fact]
    public async Task LoginWithBadgeAsync_LogsMaskedBadgeCode()
    {
        var siteId = Guid.NewGuid();
        _badgeRepository.Setup(repo => repo.GetByCodeAsync(It.IsAny<BadgeCode>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Badge?)null);

        var service = CreateService();
        await service.LoginWithBadgeAsync("ABCD1234WXYZ", siteId);

        VerifyLog(_logger, LogLevel.Information, "ABCD****WXYZ");
    }

    [Fact]
    public async Task RevokeBadgeAsync_ValidBadge_RevokesAllSessions()
    {
        var badge = Badge.Create(Guid.NewGuid(), Guid.NewGuid(), BadgeCode.Create("ABCD1234"));
        _badgeRepository.Setup(repo => repo.GetByIdAsync(badge.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _badgeRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Badge>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sessionRepository.Setup(repo => repo.RevokeAllByUserIdAsync(badge.UserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();
        var result = await service.RevokeBadgeAsync(badge.Id, Guid.NewGuid(), "Lost badge");

        Assert.True(result);
        _sessionRepository.Verify(repo => repo.RevokeAllByUserIdAsync(badge.UserId,
            It.Is<string>(reason => reason.Contains("Badge revoked")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveSessionsAsync_FiltersExpiredSessions()
    {
        var userId = Guid.NewGuid();
        var activeSession = Session.Create(userId, "token", LoginMethod.Badge, TimeSpan.FromHours(1));
        var expiredSession = Session.Create(userId, "token2", LoginMethod.Badge, TimeSpan.FromHours(1));
        SetPrivateProperty(expiredSession, nameof(Session.ExpiresAt), DateTime.UtcNow.AddMinutes(-1));

        _sessionRepository.Setup(repo => repo.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { activeSession, expiredSession });

        var service = CreateService();
        var sessions = await service.GetActiveSessionsAsync(userId);

        var sessionInfos = sessions.ToList();
        Assert.Single(sessionInfos);
        Assert.Equal(activeSession.Id, sessionInfos[0].SessionId);
        Assert.Equal("Badge", sessionInfos[0].LoginMethod);
    }

    private static void SetPrivateProperty<T>(T target, string propertyName, object value)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property?.SetValue(target, value);
    }
}
