using FluentAssertions;
using Harvestry.Harvests.Domain.Entities;
using Xunit;

namespace Harvestry.Harvests.Tests.Unit.Domain;

/// <summary>
/// Unit tests for HarvestPlant entity
/// </summary>
public class HarvestPlantTests
{
    [Fact]
    public void Create_WithValidInputs_CreatesHarvestPlant()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var plantId = Guid.NewGuid();
        var plantTag = "BD-001";
        var wetWeight = 234.5m;
        var unitOfWeight = "Grams";

        // Act
        var plant = HarvestPlant.Create(harvestId, plantId, plantTag, wetWeight, unitOfWeight);

        // Assert
        plant.Should().NotBeNull();
        plant.HarvestId.Should().Be(harvestId);
        plant.PlantId.Should().Be(plantId);
        plant.PlantTag.Should().Be(plantTag);
        plant.WetWeight.Should().Be(wetWeight);
        plant.UnitOfWeight.Should().Be(unitOfWeight);
        plant.IsWeightLocked.Should().BeFalse();
        plant.WeightSource.Should().Be("manual");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidPlantTag_ThrowsArgumentException(string? plantTag)
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var plantId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            HarvestPlant.Create(harvestId, plantId, plantTag!, 100, "Grams"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidWeight_ThrowsArgumentException(decimal weight)
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var plantId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            HarvestPlant.Create(harvestId, plantId, "BD-001", weight, "Grams"));
    }

    [Fact]
    public void CreateFromScaleReading_SetsScaleReadingIdAndSource()
    {
        // Arrange
        var harvestId = Guid.NewGuid();
        var plantId = Guid.NewGuid();
        var scaleReadingId = Guid.NewGuid();

        // Act
        var plant = HarvestPlant.CreateFromScaleReading(
            harvestId, plantId, "BD-002", 200, "Grams", scaleReadingId);

        // Assert
        plant.ScaleReadingId.Should().Be(scaleReadingId);
        plant.WeightSource.Should().Be("scale");
    }

    [Fact]
    public void LockWeight_WhenNotLocked_LocksWeight()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");
        var userId = Guid.NewGuid();

        // Act
        plant.LockWeight(userId);

        // Assert
        plant.IsWeightLocked.Should().BeTrue();
        plant.WeightLockedByUserId.Should().Be(userId);
        plant.WeightLockedAt.Should().NotBeNull();
    }

    [Fact]
    public void LockWeight_WhenAlreadyLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");
        var userId = Guid.NewGuid();
        plant.LockWeight(userId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => plant.LockWeight(userId));
    }

    [Fact]
    public void UpdateWetWeight_WhenLocked_ThrowsInvalidOperationException()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");
        plant.LockWeight(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            plant.UpdateWetWeight(150, null, "manual"));
    }

    [Fact]
    public void UpdateWetWeight_WhenNotLocked_UpdatesWeight()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");

        // Act
        plant.UpdateWetWeight(150, null, "manual");

        // Assert
        plant.WetWeight.Should().Be(150);
    }

    [Fact]
    public void AdjustWeightWithOverride_WhenLocked_UpdatesWeightWithoutUnlocking()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");
        plant.LockWeight(Guid.NewGuid());

        // Act
        plant.AdjustWeightWithOverride(200, null, "manual");

        // Assert
        plant.WetWeight.Should().Be(200);
        plant.IsWeightLocked.Should().BeTrue(); // Still locked
    }

    [Fact]
    public void UnlockWeight_ClearsLockStatus()
    {
        // Arrange
        var plant = HarvestPlant.Create(Guid.NewGuid(), Guid.NewGuid(), "BD-001", 100, "Grams");
        plant.LockWeight(Guid.NewGuid());

        // Act
        plant.UnlockWeight();

        // Assert
        plant.IsWeightLocked.Should().BeFalse();
        plant.WeightLockedAt.Should().BeNull();
        plant.WeightLockedByUserId.Should().BeNull();
    }
}





