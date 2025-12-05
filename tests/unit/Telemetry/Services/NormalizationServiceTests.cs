using FluentAssertions;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Services;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Xunit;

namespace Harvestry.Telemetry.Tests.Services;

/// <summary>
/// Unit tests for NormalizationService.
/// Tests unit conversion, range validation, and quality code determination.
/// </summary>
public class NormalizationServiceTests
{
    private readonly NormalizationService _sut;

    public NormalizationServiceTests()
    {
        _sut = new NormalizationService();
    }

    #region Unit Conversion Tests

    [Theory]
    [InlineData(32.0, Unit.DegreesFahrenheit, Unit.DegreesCelsius, 0.0)]
    [InlineData(212.0, Unit.DegreesFahrenheit, Unit.DegreesCelsius, 100.0)]
    [InlineData(0.0, Unit.DegreesCelsius, Unit.DegreesFahrenheit, 32.0)]
    [InlineData(100.0, Unit.DegreesCelsius, Unit.DegreesFahrenheit, 212.0)]
    [InlineData(273.15, Unit.Kelvin, Unit.DegreesCelsius, 0.0)]
    public void ConvertUnit_Temperature_ShouldConvertCorrectly(
        double sourceValue,
        Unit sourceUnit,
        Unit targetUnit,
        double expectedValue)
    {
        // Act
        var result = _sut.ConvertUnit(sourceValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedValue, 0.1);
    }

    [Theory]
    [InlineData(1.0, Unit.Kilopascals, Unit.Psi, 0.145)]
    [InlineData(14.5, Unit.Psi, Unit.Kilopascals, 100.0)]
    [InlineData(1.0, Unit.Bar, Unit.Kilopascals, 100.0)]
    [InlineData(100.0, Unit.Kilopascals, Unit.Bar, 1.0)]
    public void ConvertUnit_Pressure_ShouldConvertCorrectly(
        double sourceValue,
        Unit sourceUnit,
        Unit targetUnit,
        double expectedValue)
    {
        // Act
        var result = _sut.ConvertUnit(sourceValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedValue, 0.1);
    }

    [Theory]
    [InlineData(1000.0, Unit.Microsiemens, Unit.MillisiemensPerCm, 1.0)]
    [InlineData(1.0, Unit.MillisiemensPerCm, Unit.Microsiemens, 1000.0)]
    public void ConvertUnit_ElectricalConductivity_ShouldConvertCorrectly(
        double sourceValue,
        Unit sourceUnit,
        Unit targetUnit,
        double expectedValue)
    {
        // Act
        var result = _sut.ConvertUnit(sourceValue, sourceUnit, targetUnit);

        // Assert
        result.Should().BeApproximately(expectedValue, 0.01);
    }

    [Theory]
    [InlineData(100.0, Unit.Percent, Unit.Percent, 100.0)]
    [InlineData(7.0, Unit.Ph, Unit.Ph, 7.0)]
    public void ConvertUnit_SameUnit_ShouldReturnOriginalValue(
        double value,
        Unit unit,
        Unit targetUnit,
        double expectedValue)
    {
        // Act
        var result = _sut.ConvertUnit(value, unit, targetUnit);

        // Assert
        result.Should().Be(expectedValue);
    }

    #endregion

    #region Canonical Unit Tests

    [Theory]
    [InlineData(StreamType.Temperature, Unit.DegreesFahrenheit)]
    [InlineData(StreamType.Humidity, Unit.Percent)]
    [InlineData(StreamType.Co2, Unit.PartsPerMillion)]
    [InlineData(StreamType.Vpd, Unit.Kilopascals)]
    [InlineData(StreamType.LightPar, Unit.Micromoles)]
    [InlineData(StreamType.Ec, Unit.Microsiemens)]
    [InlineData(StreamType.Ph, Unit.Ph)]
    public void GetCanonicalUnit_ShouldReturnCorrectUnit(StreamType streamType, Unit expectedUnit)
    {
        // Act
        var result = _sut.GetCanonicalUnit(streamType);

        // Assert
        result.Should().Be(expectedUnit);
    }

    #endregion

    #region Range Validation Tests

    [Theory]
    [InlineData(75.0, Unit.DegreesFahrenheit, StreamType.Temperature, true)]
    [InlineData(-100.0, Unit.DegreesFahrenheit, StreamType.Temperature, false)]
    [InlineData(200.0, Unit.DegreesFahrenheit, StreamType.Temperature, false)]
    public void ValidateReading_Temperature_ShouldValidateCorrectly(
        double value,
        Unit unit,
        StreamType streamType,
        bool expectedValid)
    {
        // Act
        var result = _sut.ValidateReading(value, unit, streamType);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(50.0, Unit.Percent, StreamType.Humidity, true)]
    [InlineData(-5.0, Unit.Percent, StreamType.Humidity, false)]
    [InlineData(105.0, Unit.Percent, StreamType.Humidity, false)]
    public void ValidateReading_Humidity_ShouldValidateCorrectly(
        double value,
        Unit unit,
        StreamType streamType,
        bool expectedValid)
    {
        // Act
        var result = _sut.ValidateReading(value, unit, streamType);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(800.0, Unit.PartsPerMillion, StreamType.Co2, true)]
    [InlineData(-100.0, Unit.PartsPerMillion, StreamType.Co2, false)]
    [InlineData(6000.0, Unit.PartsPerMillion, StreamType.Co2, false)]
    public void ValidateReading_CO2_ShouldValidateCorrectly(
        double value,
        Unit unit,
        StreamType streamType,
        bool expectedValid)
    {
        // Act
        var result = _sut.ValidateReading(value, unit, streamType);

        // Assert
        result.Should().Be(expectedValid);
    }

    #endregion

    #region Quality Code Tests

    [Fact]
    public void DetermineQualityCode_ValidReading_ShouldReturnGood()
    {
        // Arrange
        var value = 75.0;
        var unit = Unit.DegreesFahrenheit;
        var streamType = StreamType.Temperature;
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-1);

        // Act
        var result = _sut.DetermineQualityCode(value, unit, streamType, timestamp);

        // Assert
        result.Should().Be(QualityCode.Good);
    }

    [Fact]
    public void DetermineQualityCode_OutOfRange_ShouldReturnBadOutOfRange()
    {
        // Arrange
        var value = 200.0; // Out of range for temperature
        var unit = Unit.DegreesFahrenheit;
        var streamType = StreamType.Temperature;
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var result = _sut.DetermineQualityCode(value, unit, streamType, timestamp);

        // Assert
        result.Should().Be(QualityCode.BadOutOfRange);
    }

    [Fact]
    public void DetermineQualityCode_FutureTimestamp_ShouldReturnBadFutureTimestamp()
    {
        // Arrange
        var value = 75.0;
        var unit = Unit.DegreesFahrenheit;
        var streamType = StreamType.Temperature;
        var timestamp = DateTimeOffset.UtcNow.AddHours(1); // Future timestamp

        // Act
        var result = _sut.DetermineQualityCode(value, unit, streamType, timestamp);

        // Assert
        result.Should().Be(QualityCode.BadFutureTimestamp);
    }

    [Fact]
    public void DetermineQualityCode_NullTimestamp_ShouldReturnGoodForValidValue()
    {
        // Arrange
        var value = 75.0;
        var unit = Unit.DegreesFahrenheit;
        var streamType = StreamType.Temperature;

        // Act
        var result = _sut.DetermineQualityCode(value, unit, streamType, null);

        // Assert
        result.Should().Be(QualityCode.Good);
    }

    #endregion

    #region Normalization Tests

    [Fact]
    public void NormalizeReading_TemperatureCelsiusToFahrenheit_ShouldConvert()
    {
        // Arrange
        var value = 25.0; // 25°C
        var sourceUnit = Unit.DegreesCelsius;
        var streamType = StreamType.Temperature;

        // Act
        var (normalizedValue, targetUnit) = _sut.NormalizeReading(value, sourceUnit, streamType);

        // Assert
        targetUnit.Should().Be(Unit.DegreesFahrenheit);
        normalizedValue.Should().BeApproximately(77.0, 0.1); // 25°C = 77°F
    }

    [Fact]
    public void NormalizeReading_AlreadyCanonicalUnit_ShouldReturnSameValue()
    {
        // Arrange
        var value = 75.0;
        var sourceUnit = Unit.DegreesFahrenheit;
        var streamType = StreamType.Temperature;

        // Act
        var (normalizedValue, targetUnit) = _sut.NormalizeReading(value, sourceUnit, streamType);

        // Assert
        targetUnit.Should().Be(Unit.DegreesFahrenheit);
        normalizedValue.Should().Be(75.0);
    }

    [Fact]
    public void NormalizeReading_HumidityPercent_ShouldNotConvert()
    {
        // Arrange
        var value = 65.0;
        var sourceUnit = Unit.Percent;
        var streamType = StreamType.Humidity;

        // Act
        var (normalizedValue, targetUnit) = _sut.NormalizeReading(value, sourceUnit, streamType);

        // Assert
        targetUnit.Should().Be(Unit.Percent);
        normalizedValue.Should().Be(65.0);
    }

    #endregion

    #region Expected Range Tests

    [Theory]
    [InlineData(StreamType.Temperature, -50.0, 150.0)]
    [InlineData(StreamType.Humidity, 0.0, 100.0)]
    [InlineData(StreamType.Co2, 0.0, 5000.0)]
    [InlineData(StreamType.Ph, 0.0, 14.0)]
    public void GetExpectedRange_ShouldReturnCorrectRange(
        StreamType streamType,
        double expectedMin,
        double expectedMax)
    {
        // Act
        var (min, max) = _sut.GetExpectedRange(streamType);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task NormalizeAsync_CompleteFlow_ShouldCreateValidSensorReading()
    {
        // Arrange
        var dto = new SensorReadingDto(
            StreamId: Guid.NewGuid(),
            Time: DateTimeOffset.UtcNow,
            Value: 25.0, // 25°C
            Unit: Unit.DegreesCelsius,
            SourceTimestamp: DateTimeOffset.UtcNow.AddSeconds(-1),
            MessageId: "test-msg-001",
            Metadata: null
        );

        var stream = SensorStream.Create(
            siteId: Guid.NewGuid(),
            equipmentId: Guid.NewGuid(),
            streamType: StreamType.Temperature,
            unit: Unit.DegreesFahrenheit, // Canonical unit
            displayName: "Test Temperature Sensor"
        );

        // Act
        var result = await _sut.NormalizeAsync(dto, stream, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StreamId.Should().Be(dto.StreamId);
        result.Value.Should().BeApproximately(77.0, 0.1); // Converted to °F
        result.QualityCode.Should().Be(QualityCode.Good);
        result.MessageId.Should().Be("test-msg-001");
    }

    [Fact]
    public async Task NormalizeAsync_OutOfRangeValue_ShouldSetBadQuality()
    {
        // Arrange
        var dto = new SensorReadingDto(
            StreamId: Guid.NewGuid(),
            Time: DateTimeOffset.UtcNow,
            Value: 200.0, // Out of range
            Unit: Unit.DegreesFahrenheit,
            SourceTimestamp: DateTimeOffset.UtcNow,
            MessageId: null,
            Metadata: null
        );

        var stream = SensorStream.Create(
            siteId: Guid.NewGuid(),
            equipmentId: Guid.NewGuid(),
            streamType: StreamType.Temperature,
            unit: Unit.DegreesFahrenheit,
            displayName: "Test Temperature Sensor"
        );

        // Act
        var result = await _sut.NormalizeAsync(dto, stream, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QualityCode.Should().Be(QualityCode.BadOutOfRange);
    }

    #endregion
}

