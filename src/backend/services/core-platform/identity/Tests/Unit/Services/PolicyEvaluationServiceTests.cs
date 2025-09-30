using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.DTOs;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Application.Services;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Identity.Tests.Unit.Services;

public sealed class PolicyEvaluationServiceTests
{
    private readonly Mock<IDatabaseRepository> _databaseRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ISiteRepository> _siteRepository = new();
    private readonly Mock<ITwoPersonApprovalRepository> _twoPersonApprovalRepository = new();
    private readonly Mock<IAuthorizationAuditRepository> _authorizationAuditRepository = new();
    private readonly Mock<ILogger<PolicyEvaluationService>> _logger = new();

    private PolicyEvaluationService CreateService()
    {
        return new PolicyEvaluationService(
            _databaseRepository.Object,
            _userRepository.Object,
            _siteRepository.Object,
            _twoPersonApprovalRepository.Object,
            _authorizationAuditRepository.Object,
            _logger.Object);
    }

    private static void VerifyLog(Mock<ILogger<PolicyEvaluationService>> logger, LogLevel level, string contains)
    {
        logger.Verify(log => log.Log(
            It.Is<LogLevel>(l => l == level),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() != null && state.ToString()!.Contains(contains)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task EvaluatePermissionAsync_ValidUser_CallsDatabaseFunction()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);

        Dictionary<string, object>? capturedContext = null;
        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:start",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, Guid, Dictionary<string, object>?, CancellationToken>((_, _, _, _, ctx, _) =>
            {
                capturedContext = ctx;
            })
            .ReturnsAsync((true, false, (string?)null));

        var context = new Dictionary<string, object>
        {
            { "resource_id", Guid.NewGuid() },
            { "notes", "should-be-dropped" },
            { "location", "   west wing   " },
            { "status", new string('x', 600) }
        };

        var service = CreateService();
        var result = await service.EvaluatePermissionAsync(
            user.Id,
            "tasks:start",
            "task",
            site.Id,
            context);

        Assert.True(result.IsGranted);
        Assert.False(result.RequiresTwoPersonApproval);
        _databaseRepository.Verify(repo => repo.CheckAbacPermissionAsync(
            user.Id,
            "tasks:start",
            "task",
            site.Id,
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(capturedContext);
        Assert.False(capturedContext!.ContainsKey("notes"));
        Assert.Equal("west wing", capturedContext!["location"]);
        var status = Assert.IsType<string>(capturedContext!["status"]);
        Assert.Equal(500, status.Length);

        VerifyLog(_logger, LogLevel.Information, "Permission granted");
    }

    [Fact]
    public async Task EvaluatePermissionAsync_UserNotFound_ReturnsDeny()
    {
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);

        var service = CreateService();
        var result = await service.EvaluatePermissionAsync(
            Guid.NewGuid(),
            "tasks:start",
            "task",
            site.Id);

        Assert.False(result.IsGranted);
        Assert.Equal("User not found", result.DenyReason);
        _databaseRepository.Verify(repo => repo.CheckAbacPermissionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        VerifyLog(_logger, LogLevel.Warning, "Permission denied: User");
    }

    [Fact]
    public async Task EvaluatePermissionAsync_SiteNotFound_ReturnsDeny()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Site?)null);

        var service = CreateService();
        var result = await service.EvaluatePermissionAsync(
            user.Id,
            "tasks:start",
            "task",
            Guid.NewGuid());

        Assert.False(result.IsGranted);
        Assert.Equal("Site not found", result.DenyReason);
        _databaseRepository.Verify(repo => repo.CheckAbacPermissionAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        VerifyLog(_logger, LogLevel.Warning, "Permission denied: Site");
    }

    [Fact]
    public async Task EvaluatePermissionAsync_PermissionDenied_ReturnsReason()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);

        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:start",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false, "Policy rule blocked"));

        var service = CreateService();
        var result = await service.EvaluatePermissionAsync(
            user.Id,
            "tasks:start",
            "task",
            site.Id);

        Assert.False(result.IsGranted);
        Assert.Equal("Policy rule blocked", result.DenyReason);

        VerifyLog(_logger, LogLevel.Warning, "Permission denied");
    }

    [Fact]
    public async Task EvaluatePermissionAsync_RequiresTwoPersonApproval_ReturnsGrantWithFlag()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);

        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:destroy",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, true, (string?)null));

        _twoPersonApprovalRepository.Setup(repo => repo.CreateAsync(It.IsAny<TwoPersonApprovalRequest>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TwoPersonApprovalResponse(Guid.NewGuid(), "pending", DateTime.UtcNow.AddHours(24)));

        var service = CreateService();
        var result = await service.EvaluatePermissionAsync(
            user.Id,
            "tasks:destroy",
            "task",
            site.Id);

        Assert.True(result.IsGranted);
        Assert.True(result.RequiresTwoPersonApproval);

        VerifyLog(_logger, LogLevel.Information, "Permission granted");
    }

    [Fact]
    public async Task InitiateTwoPersonApprovalAsync_UserWithoutPermission_Throws()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);
        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:destroy",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false, "Denied"));

        _twoPersonApprovalRepository.Setup(repo => repo.CreateAsync(It.IsAny<TwoPersonApprovalRequest>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TwoPersonApprovalResponse(Guid.NewGuid(), "pending", DateTime.UtcNow.AddHours(24)));

        var service = CreateService();
        var request = new TwoPersonApprovalRequest(
            "tasks:destroy",
            "task",
            Guid.NewGuid(),
            site.Id,
            user.Id,
            "Testing");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InitiateTwoPersonApprovalAsync(request));

        Assert.Contains("Initiator does not have permission", ex.Message);
    }

    [Fact]
    public async Task InitiateTwoPersonApprovalAsync_ActionDoesNotRequireTwoPersonApproval_Throws()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);
        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:update",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false, (string?)null));

        _twoPersonApprovalRepository.Setup(repo => repo.CreateAsync(It.IsAny<TwoPersonApprovalRequest>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TwoPersonApprovalResponse(Guid.NewGuid(), "pending", DateTime.UtcNow.AddHours(24)));

        var service = CreateService();
        var request = new TwoPersonApprovalRequest(
            "tasks:update",
            "task",
            Guid.NewGuid(),
            site.Id,
            user.Id,
            "Testing");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InitiateTwoPersonApprovalAsync(request));

        Assert.Contains("does not require two-person approval", ex.Message);
    }

    [Fact]
    public async Task EvaluatePermissionAsync_LogsWarningOnPermissionDenied()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);

        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:start",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, false, "Denied"));

        var service = CreateService();
        await service.EvaluatePermissionAsync(user.Id, "tasks:start", "task", site.Id);

        VerifyLog(_logger, LogLevel.Warning, "Permission denied");
    }

    [Fact]
    public async Task InitiateTwoPersonApprovalAsync_CreatesApprovalRecord()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        var site = Site.Create(Guid.NewGuid(), "Site", "SITE-001");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(site.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(site);
        _databaseRepository
            .Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                "tasks:destroy",
                "task",
                site.Id,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, true, (string?)null));

        var approvalResponse = new TwoPersonApprovalResponse(Guid.NewGuid(), "pending", DateTime.UtcNow.AddHours(24));
        _twoPersonApprovalRepository.Setup(repo => repo.CreateAsync(It.IsAny<TwoPersonApprovalRequest>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvalResponse);

        var service = CreateService();
        var request = new TwoPersonApprovalRequest("tasks:destroy", "task", Guid.NewGuid(), site.Id, user.Id, "Reason");

        var response = await service.InitiateTwoPersonApprovalAsync(request);

        Assert.Equal(approvalResponse.ApprovalId, response.ApprovalId);
        _twoPersonApprovalRepository.Verify(repo => repo.CreateAsync(It.IsAny<TwoPersonApprovalRequest>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveTwoPersonRequestAsync_Succeeds_WhenValid()
    {
        var record = new TwoPersonApprovalRecord(
            Guid.NewGuid(),
            "tasks:destroy",
            "task",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason",
            null,
            DateTime.UtcNow.AddMinutes(-5),
            "pending",
            DateTime.UtcNow.AddHours(1),
            null,
            null);

        _twoPersonApprovalRepository.Setup(repo => repo.GetByIdAsync(record.ApprovalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _twoPersonApprovalRepository.Setup(repo => repo.ApproveAsync(record.ApprovalId, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var user = User.Create(Email.Create("approver@example.com"), "Approver", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _siteRepository.Setup(repo => repo.GetByIdAsync(record.SiteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Site.Create(Guid.NewGuid(), "Site", "SITE-001"));
        _databaseRepository.Setup(repo => repo.CheckAbacPermissionAsync(
                user.Id,
                record.Action,
                record.ResourceType,
                record.SiteId,
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false, (string?)null));

        var service = CreateService();
        var success = await service.ApproveTwoPersonRequestAsync(record.ApprovalId, user.Id, "Looks good");

        Assert.True(success);
        _twoPersonApprovalRepository.Verify(repo => repo.ApproveAsync(record.ApprovalId, user.Id, "Looks good", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveTwoPersonRequestAsync_ReturnsFalseAndLogsAudit_WhenExpired()
    {
        var expiredRecord = new TwoPersonApprovalRecord(
            Guid.NewGuid(),
            "inventory:destroy",
            "lot",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Destroy expired lot",
            null,
            DateTime.UtcNow.AddHours(-2),
            "pending",
            DateTime.UtcNow.AddMinutes(-5),
            null,
            null);

        _twoPersonApprovalRepository.Setup(repo => repo.GetByIdAsync(expiredRecord.ApprovalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredRecord);

        var service = CreateService();
        var approverId = Guid.NewGuid();
        var result = await service.ApproveTwoPersonRequestAsync(expiredRecord.ApprovalId, approverId, "Looks good");

        Assert.False(result);
        _authorizationAuditRepository.Verify(repo => repo.LogAsync(
            It.Is<AuthorizationAuditEntry>(entry =>
                entry.UserId == approverId &&
                entry.SiteId == expiredRecord.SiteId &&
                entry.Action == expiredRecord.Action &&
                entry.ResourceType == expiredRecord.ResourceType &&
                entry.ResourceId == expiredRecord.ResourceId &&
                entry.DenyReason == "Approval expired" &&
                entry.Granted == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectTwoPersonRequestAsync_Succeeds_WhenValid()
    {
        var record = new TwoPersonApprovalRecord(
            Guid.NewGuid(),
            "tasks:destroy",
            "task",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason",
            null,
            DateTime.UtcNow.AddMinutes(-5),
            "pending",
            DateTime.UtcNow.AddHours(1),
            null,
            null);

        _twoPersonApprovalRepository.Setup(repo => repo.GetByIdAsync(record.ApprovalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);
        _twoPersonApprovalRepository.Setup(repo => repo.RejectAsync(record.ApprovalId, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();
        var success = await service.RejectTwoPersonRequestAsync(record.ApprovalId, Guid.NewGuid(), "Not valid");

        Assert.True(success);
        _twoPersonApprovalRepository.Verify(repo => repo.RejectAsync(record.ApprovalId, It.IsAny<Guid>(), "Not valid", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectTwoPersonRequestAsync_ReturnsFalse_WhenApprovalMissing()
    {
        _twoPersonApprovalRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TwoPersonApprovalRecord?)null);

        var service = CreateService();
        var result = await service.RejectTwoPersonRequestAsync(Guid.NewGuid(), Guid.NewGuid(), "Reason");

        Assert.False(result);
    }
}
