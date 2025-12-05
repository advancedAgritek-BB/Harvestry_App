using FluentAssertions;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Xunit;

namespace Harvestry.Telemetry.Tests.Domain;

/// <summary>
/// Unit tests for SensorReading entity.
/// Tests creation, validation, and quality assessment.
/// </summary>
public class SensorReadingTests
{
    [Fact]
    public void FromIngestion_WithValidData_ShouldCreateReading()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var value = 75.5;
        var qualityCode = QualityCode.Good;
        var timestamp = DateTimeOffset.UtcNow;
        var messageId = "msg-12345";

        // Act
        var reading = SensorReading.FromIngestion(
            streamId,
            value,
            qualityCode,
            timestamp,
            messageId
        );

        // Assert
        reading.Should().NotBeNull();
        reading.StreamId.Should().Be(streamId);
        reading.Value.Should().Be(value);
        reading.QualityCode.Should().Be(qualityCode);
        reading.SourceTimestamp.Should().Be(timestamp);
        reading.MessageId.Should().Be(messageId);
        reading.IngestionTimestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FromIngestion_WithFutureTimestamp_ShouldSetBadFutureTimestampQuality()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var futureTimestamp = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        var reading = SensorReading.FromIngestion(
            streamId,
            75.0,
            QualityCode.Good,
            futureTimestamp,
            null
        );

        // Assert
        reading.QualityCode.Should().Be(QualityCode.BadFutureTimestamp);
    }

    [Fact]
    public void FromIngestion_WithNullTimestamp_ShouldUseCurrentTime()
    {
        // Arrange
        var streamId = Guid.NewGuid();

        // Act
        var reading = SensorReading.FromIngestion(
            streamId,
            75.0,
            QualityCode.Good,
            null,
            null
        );

        // Assert
        reading.Time.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        reading.SourceTimestamp.Should().BeNull();
    }

    [Fact]
    public void FromIngestion_WithMetadata_ShouldStoreMetadata()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var metadata = new Dictionary<string, object>
        {
            { "sensorType", "RTD" },
            { "accuracy", "±0.1°C" }
        };

        // Act
        var reading = SensorReading.FromIngestion(
            streamId,
            75.0,
            QualityCode.Good,
            null,
            null,
            metadata
        );

        // Assert
        reading.Metadata.Should().NotBeNull();
        reading.Metadata.Should().ContainKey("sensorType");
        reading.Metadata.Should().ContainKey("accuracy");
    }

    [Fact]
    public void IsGoodQuality_WithGoodQuality_ShouldReturnTrue()
    {
        // Arrange
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            75.0,
            QualityCode.Good,
            null,
            null
        );

        // Act
        var result = reading.IsGoodQuality();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsGoodQuality_WithBadQuality_ShouldReturnFalse()
    {
        // Arrange
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            75.0,
            QualityCode.Bad,
            null,
            null
        );

        // Act
        var result = reading.IsGoodQuality();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinExpectedRange_ValueInRange_ShouldReturnTrue()
    {
        // Arrange
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            75.0,
            QualityCode.Good,
            null,
            null
        );

        // Act
        var result = reading.IsWithinExpectedRange(50.0, 100.0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinExpectedRange_ValueOutOfRange_ShouldReturnFalse()
    {
        // Arrange
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            150.0,
            QualityCode.Good,
            null,
            null
        );

        // Act
        var result = reading.IsWithinExpectedRange(50.0, 100.0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetIngestionLatency_WithSourceTimestamp_ShouldCalculateLatency()
    {
        // Arrange
        var sourceTimestamp = DateTimeOffset.UtcNow.AddSeconds(-5);
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            75.0,
            QualityCode.Good,
            sourceTimestamp,
            null
        );

        // Act
        var latency = reading.GetIngestionLatency();

        // Assert
        latency.Should().NotBeNull();
        var latencyValue = latency!.Value;
        latencyValue.TotalSeconds.Should().BeGreaterThan(4);
        latencyValue.TotalSeconds.Should().BeLessThan(6);
    }

    [Fact]
    public void GetIngestionLatency_WithoutSourceTimestamp_ShouldReturnNull()
    {
        // Arrange
        var reading = SensorReading.FromIngestion(
            Guid.NewGuid(),
            75.0,
            QualityCode.Good,
            null,
            null
        );

        // Act
        var latency = reading.GetIngestionLatency();

        // Assert
        latency.Should().BeNull();
    }

    [Fact]
    public void FromPersistence_ShouldRehydrateCorrectly()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow.AddHours(-1);
        var streamId = Guid.NewGuid();
        var value = 72.5;
        var qualityCode = QualityCode.Good;
        var sourceTimestamp = time.AddMilliseconds(-100);
        var ingestionTimestamp = time;
        var messageId = "msg-67890";
        var metadata = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var reading = SensorReading.FromPersistence(
            time,
            streamId,
            value,
            qualityCode,
            sourceTimestamp,
            ingestionTimestamp,
            messageId,
            metadata
        );

        // Assert
        reading.Time.Should().Be(time);
        reading.StreamId.Should().Be(streamId);
        reading.Value.Should().Be(value);
        reading.QualityCode.Should().Be(qualityCode);
        reading.SourceTimestamp.Should().Be(sourceTimestamp);
        reading.IngestionTimestamp.Should().Be(ingestionTimestamp);
        reading.MessageId.Should().Be(messageId);
        reading.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void CompositeKey_ShouldBeSetCorrectly()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow;
        var streamId = Guid.NewGuid();

        // Act
        var reading = SensorReading.FromIngestion(streamId, 75.0, QualityCode.Good, time, null);

        // Assert
        reading.Id.Time.Should().BeCloseTo(time, TimeSpan.FromMilliseconds(1));
        reading.Id.StreamId.Should().Be(streamId);
    }
}
