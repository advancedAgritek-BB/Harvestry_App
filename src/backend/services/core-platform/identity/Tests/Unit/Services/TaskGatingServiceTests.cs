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

public sealed class TaskGatingServiceTests
{
    private readonly Mock<IDatabaseRepository> _databaseRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ILogger<TaskGatingService>> _logger = new();

    private TaskGatingService CreateService()
    {
        return new TaskGatingService(
            _databaseRepository.Object,
            _userRepository.Object,
            _logger.Object);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_UserNotFound_ReturnsBlock()
    {
        _userRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(Guid.NewGuid(), "harvest", Guid.NewGuid());

        Assert.False(result.IsAllowed);
        var requirement = Assert.Single(result.MissingRequirements);
        Assert.Equal("user", requirement.RequirementType);
        Assert.Equal("User not found", requirement.Reason);
        _databaseRepository.Verify(repo => repo.CheckTaskGatingAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_Allowed_ReturnsAllow()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _databaseRepository.Setup(repo => repo.CheckTaskGatingAsync(
                user.Id,
                "harvest",
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new List<TaskGatingRequirement>()));

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(user.Id, "harvest", Guid.NewGuid());

        Assert.True(result.IsAllowed);
        Assert.Empty(result.MissingRequirements);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_Blocked_ReturnsMissingRequirements()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var missing = new List<TaskGatingRequirement>
        {
            new("sop", Guid.NewGuid(), "SOP signoff required"),
            new("training", Guid.NewGuid(), "Training completion required")
        };

        _databaseRepository.Setup(repo => repo.CheckTaskGatingAsync(
                user.Id,
                "harvest",
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, missing));

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(user.Id, "harvest", Guid.NewGuid());

        Assert.False(result.IsAllowed);
        Assert.Equal(2, result.MissingRequirements.Count);
        Assert.Contains(result.MissingRequirements, r => r.RequirementType == "sop");
        Assert.Contains(result.MissingRequirements, r => r.RequirementType == "training");
    }

    [Fact]
    public async Task CheckTaskGatingAsync_MissingSopSignoff_ReturnsBlockWithReason()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var missing = new List<TaskGatingRequirement>
        {
            new("sop", Guid.NewGuid(), "SOP signoff required")
        };

        _databaseRepository.Setup(repo => repo.CheckTaskGatingAsync(
                user.Id,
                "harvest",
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, missing));

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(user.Id, "harvest", Guid.NewGuid());

        Assert.False(result.IsAllowed);
        var requirement = Assert.Single(result.MissingRequirements);
        Assert.Equal("sop", requirement.RequirementType);
        Assert.Equal("SOP signoff required", requirement.Reason);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_MissingTraining_ReturnsBlockWithReason()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var missing = new List<TaskGatingRequirement>
        {
            new("training", Guid.NewGuid(), "Training completion required")
        };

        _databaseRepository.Setup(repo => repo.CheckTaskGatingAsync(
                user.Id,
                "harvest",
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, missing));

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(user.Id, "harvest", Guid.NewGuid());

        Assert.False(result.IsAllowed);
        var requirement = Assert.Single(result.MissingRequirements);
        Assert.Equal("training", requirement.RequirementType);
        Assert.Equal("Training completion required", requirement.Reason);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_MissingPermissionRequirement_ReturnsPermissionReason()
    {
        var user = User.Create(Email.Create("user@example.com"), "Test", "User");
        _userRepository.Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var missing = new List<TaskGatingRequirement>
        {
            new("permission", null, "Permission 'irrigation:override' not granted")
        };

        _databaseRepository.Setup(repo => repo.CheckTaskGatingAsync(
                user.Id,
                "irrigation_override",
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, missing));

        var service = CreateService();
        var result = await service.CheckTaskGatingAsync(user.Id, "irrigation_override", Guid.NewGuid());

        Assert.False(result.IsAllowed);
        var requirement = Assert.Single(result.MissingRequirements);
        Assert.Equal("permission", requirement.RequirementType);
        Assert.Contains("irrigation:override", requirement.Reason);
    }

    [Fact]
    public async Task CheckTaskGatingAsync_Throws_WhenTaskTypeMissing()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CheckTaskGatingAsync(Guid.NewGuid(), string.Empty, Guid.NewGuid()));
    }
}
