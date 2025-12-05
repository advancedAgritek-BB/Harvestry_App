using FluentAssertions;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Interfaces;
using Harvestry.Telemetry.Domain.Enums;
using Xunit;

namespace Harvestry.Telemetry.Tests.Domain;

/// <summary>
/// Unit tests for SensorStream entity.
/// Tests creation, validation, and business logic.
/// </summary>
public class SensorStreamTests
{
    private class FakeTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }
    [Fact]
    public void Create_WithValidData_ShouldCreateStream()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var streamType = StreamType.Temperature;
        var unit = Unit.DegreesFahrenheit;
        var displayName = "Room 1 - Temperature";

        // Act
        var stream = SensorStream.Create(siteId, equipmentId, streamType, unit, displayName);

        // Assert
        stream.Should().NotBeNull();
        stream.Id.Should().NotBeEmpty();
        stream.SiteId.Should().Be(siteId);
        stream.EquipmentId.Should().Be(equipmentId);
        stream.StreamType.Should().Be(streamType);
        stream.Unit.Should().Be(unit);
        stream.DisplayName.Should().Be(displayName);
        stream.IsActive.Should().BeTrue();
        stream.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        stream.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyDisplayName_ShouldThrow()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();

        // Act & Assert
        var act = () => SensorStream.Create(
            siteId,
            equipmentId,
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            ""
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Display name is required*");
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldSetFields()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();
        var channelId = Guid.NewGuid();

        // Act
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Humidity,
            Unit.Percent,
            "Test Stream",
            equipmentChannelId: channelId,
            locationId: locationId,
            roomId: roomId,
            zoneId: zoneId
        );

        // Assert
        stream.EquipmentChannelId.Should().Be(channelId);
        stream.LocationId.Should().Be(locationId);
        stream.RoomId.Should().Be(roomId);
        stream.ZoneId.Should().Be(zoneId);
    }

    [Fact]
    public void UpdateDisplayName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        SensorStream.SetTimeProvider(fakeTimeProvider);

        try
        {
            var stream = SensorStream.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                StreamType.Temperature,
                Unit.DegreesFahrenheit,
                "Original Name"
            );
            var originalUpdatedAt = stream.UpdatedAt;

            // Advance the fake clock to ensure timestamp changes
            fakeTimeProvider.UtcNow = fakeTimeProvider.UtcNow.AddMilliseconds(10);

            // Act
            stream.UpdateDisplayName("New Name");

            // Assert
            stream.DisplayName.Should().Be("New Name");
            stream.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }
        finally
        {
            SensorStream.ResetTimeProvider();
        }
    }

    [Fact]
    public void UpdateDisplayName_WithEmptyName_ShouldThrow()
    {
        // Arrange
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            "Original Name"
        );

        // Act & Assert
        var act = () => stream.UpdateDisplayName("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Display name is required*");
    }

    [Fact]
    public void UpdateLocation_ShouldUpdateLocationFields()
    {
        // Arrange
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            "Test Stream"
        );

        var locationId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var zoneId = Guid.NewGuid();

        // Act
        stream.UpdateLocation(locationId, roomId, zoneId);

        // Assert
        stream.LocationId.Should().Be(locationId);
        stream.RoomId.Should().Be(roomId);
        stream.ZoneId.Should().Be(zoneId);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            "Test Stream"
        );

        // Act
        stream.Deactivate();

        // Assert
        stream.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            "Test Stream"
        );
        stream.Deactivate();

        // Act
        stream.Activate();

        // Assert
        stream.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SetMetadata_ShouldStoreMetadata()
    {
        // Arrange
        var stream = SensorStream.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            StreamType.Temperature,
            Unit.DegreesFahrenheit,
            "Test Stream"
        );

        var metadata = new Dictionary<string, object>
        {
            { "calibrationDate", "2025-01-01" },
            { "manufacturer", "Acme Sensors" },
            { "model", "TS-1000" }
        };

        // Act
        stream.SetMetadata(metadata);

        // Assert
        stream.Metadata.Should().NotBeNull();
        stream.Metadata.Should().ContainKey("calibrationDate");
        stream.Metadata.Should().ContainKey("manufacturer");
    }

    [Fact]
    public void FromPersistence_ShouldRehydrateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-7);
        var updatedAt = DateTimeOffset.UtcNow;
        var metadata = new Dictionary<string, object> { { "test", "value" } };

        // Act
        var stream = SensorStream.FromPersistence(
            id,
            siteId,
            equipmentId,
            null,
            StreamType.Humidity,
            Unit.Percent,
            "Rehydrated Stream",
            null,
            null,
            null,
            true,
            metadata,
            createdAt,
            updatedAt
        );

        // Assert
        stream.Id.Should().Be(id);
        stream.SiteId.Should().Be(siteId);
        stream.CreatedAt.Should().Be(createdAt);
        stream.UpdatedAt.Should().Be(updatedAt);
        stream.Metadata.Should().ContainKey("test");
    }
}

