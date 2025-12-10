using FluentAssertions;
using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Application.Interfaces;
using Harvestry.Harvests.Application.Services;
using Harvestry.Harvests.Domain.Entities;
using Harvestry.Harvests.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Harvests.Tests.Unit.Services;

/// <summary>
/// Unit tests for HarvestWorkflowService
/// </summary>
public class HarvestWorkflowServiceTests
{
    private readonly Mock<IHarvestRepository> _harvestRepositoryMock;
    private readonly Mock<IHarvestPlantRepository> _harvestPlantRepositoryMock;
    private readonly Mock<IWeightAdjustmentRepository> _weightAdjustmentRepositoryMock;
    private readonly Mock<IScaleDeviceRepository> _scaleDeviceRepositoryMock;
    private readonly Mock<IScaleCalibrationRepository> _scaleCalibrationRepositoryMock;
    private readonly Mock<IScaleReadingRepository> _scaleReadingRepositoryMock;
    private readonly Mock<ILogger<HarvestWorkflowService>> _loggerMock;
    private readonly HarvestWorkflowService _sut;

    public HarvestWorkflowServiceTests()
    {
        _harvestRepositoryMock = new Mock<IHarvestRepository>();
        _harvestPlantRepositoryMock = new Mock<IHarvestPlantRepository>();
        _weightAdjustmentRepositoryMock = new Mock<IWeightAdjustmentRepository>();
        _scaleDeviceRepositoryMock = new Mock<IScaleDeviceRepository>();
        _scaleCalibrationRepositoryMock = new Mock<IScaleCalibrationRepository>();
        _scaleReadingRepositoryMock = new Mock<IScaleReadingRepository>();
        _loggerMock = new Mock<ILogger<HarvestWorkflowService>>();

        _sut = new HarvestWorkflowService(
            _harvestRepositoryMock.Object,
            _harvestPlantRepositoryMock.Object,
            _weightAdjustmentRepositoryMock.Object,
            _scaleDeviceRepositoryMock.Object,
            _scaleCalibrationRepositoryMock.Object,
            _scaleReadingRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region GetHarvestWorkflow Tests

    [Fact]
    public async Task GetHarvestWorkflowAsync_WhenHarvestExists_ReturnsWorkflowResponse()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var harvest = CreateTestHarvest(harvestId, siteId);

        _harvestRepositoryMock
            .Setup(x => x.GetByIdWithPlantsAsync(harvestId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(harvest);

        // Act
        var result = await _sut.GetHarvestWorkflowAsync(harvestId, siteId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(harvestId);
        result.HarvestName.Should().Be(harvest.HarvestName);
    }

    [Fact]
    public async Task GetHarvestWorkflowAsync_WhenHarvestNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var siteId = Guid.NewGuid();

        _harvestRepositoryMock
            .Setup(x => x.GetByIdWithPlantsAsync(harvestId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Harvest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetHarvestWorkflowAsync(harvestId, siteId));
    }

    #endregion

    #region PIN Validation Tests

    [Theory]
    [InlineData("1234", true)]
    [InlineData("123456", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData("abcd", false)]
    [InlineData("12ab", false)]
    public async Task ValidatePinAsync_ValidatesCorrectly(string pin, bool expectedResult)
    {
        // Act
        var result = await _sut.ValidatePinAsync(pin, Guid.NewGuid());

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Drying Phase Tests

    [Fact]
    public async Task StartDryingAsync_WhenInWetHarvestPhase_TransitionsToDrying()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var harvest = CreateTestHarvest(harvestId, siteId);

        _harvestRepositoryMock
            .Setup(x => x.GetByIdWithPlantsAsync(harvestId, siteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(harvest);

        _harvestRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Harvest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(harvest);

        var request = new StartDryingRequest
        {
            DryingLocationId = Guid.NewGuid(),
            DryingLocationName = "Drying Room A"
        };

        // Act
        var result = await _sut.StartDryingAsync(harvestId, request, siteId, userId);

        // Assert
        result.Should().NotBeNull();
        _harvestRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Harvest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Weight Adjustment Tests

    [Fact]
    public async Task AdjustWeightAsync_WithInvalidPin_ThrowsUnauthorizedException()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new AdjustWeightRequest
        {
            WeightType = WeightType.WetPlant,
            NewWeight = 500,
            ReasonCode = "RECOUNTED",
            Pin = "12" // Invalid - too short
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.AdjustWeightAsync(harvestId, request, siteId, userId));
    }

    #endregion

    #region Helper Methods

    private static Harvest CreateTestHarvest(Guid harvestId, Guid siteId)
    {
        // Use reflection or a factory to create test harvest since constructor is private
        // In real tests, you'd use a test builder or factory pattern
        
        // For this example, we'll use the public factory method
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        // Note: In actual implementation, you'd need to expose a Create method
        // or use reflection to create test instances
        return null!; // Placeholder - real implementation would create valid test data
    }

    #endregion
}




