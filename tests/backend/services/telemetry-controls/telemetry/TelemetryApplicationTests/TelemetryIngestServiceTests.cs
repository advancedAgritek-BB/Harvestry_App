using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Services;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harvestry.Telemetry.Application.Tests;

public sealed class TelemetryIngestServiceTests
{
    [Fact]
    public async Task IngestBatchAsync_FiltersExistingDuplicates()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        const string duplicateMessageId = "msg-100";
        var timestamp = DateTimeOffset.UtcNow;

        var rlsAccessor = new TestRlsContextAccessor();
        var connectionFactory = new Mock<ITelemetryConnectionFactory>(MockBehavior.Strict);
        var normalizationService = new Mock<INormalizationService>();
        var idempotencyService = new Mock<IIdempotencyService>();
        var readingRepository = new Mock<ISensorReadingRepository>();
        var sensorStreamRepository = new Mock<ISensorStreamRepository>();
        var realtimeDispatcher = new Mock<ITelemetryRealtimeDispatcher>();
        var sessionRepository = new Mock<IIngestionSessionRepository>();
        var errorRepository = new Mock<IIngestionErrorRepository>();
        var logger = new Mock<ILogger<TelemetryIngestService>>();

        var stream = SensorStream.FromPersistence(
            streamId,
            siteId,
            equipmentId,
            equipmentChannelId: null,
            streamType: StreamType.Temperature,
            unit: Unit.DegreesCelsius,
            displayName: "Canopy Temp",
            locationId: null,
            roomId: null,
            zoneId: null,
            isActive: true,
            metadata: null,
            createdAt: timestamp,
            updatedAt: timestamp);

        sensorStreamRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorStream> { stream });

        idempotencyService
            .Setup(service => service.GetDuplicatesByStreamAsync(
                It.IsAny<IReadOnlyDictionary<Guid, string[]>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid StreamId, string MessageId)>
            {
                (streamId, duplicateMessageId)
            });

        normalizationService
            .Setup(service => service.NormalizeAsync(
                It.IsAny<SensorReadingDto>(),
                stream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SensorReadingDto dto, SensorStream _, CancellationToken _) =>
                SensorReading.FromIngestion(
                    dto.StreamId,
                    dto.Value,
                    QualityCode.Good,
                    dto.SourceTimestamp,
                    dto.MessageId,
                    metadata: null));

        readingRepository
            .Setup(repo => repo.BulkInsertAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        realtimeDispatcher
            .Setup(dispatcher => dispatcher.PublishAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        errorRepository
            .Setup(repo => repo.LogAsync(It.IsAny<IEnumerable<IngestionError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TelemetryIngestService(
            connectionFactory.Object,
            normalizationService.Object,
            idempotencyService.Object,
            readingRepository.Object,
            sensorStreamRepository.Object,
            realtimeDispatcher.Object,
            sessionRepository.Object,
            errorRepository.Object,
            rlsAccessor,
            logger.Object);

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http,
            new List<SensorReadingDto>
            {
                new(streamId, timestamp, 70.2, Unit.DegreesCelsius, timestamp, duplicateMessageId),
                new(streamId, timestamp.AddSeconds(1), 71.5, Unit.DegreesCelsius, timestamp.AddSeconds(1), duplicateMessageId)
            });

        // Act
        var result = await service.IngestBatchAsync(siteId, request, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalReceived);
        Assert.Equal(2, result.Duplicates);
        Assert.Equal(0, result.Accepted);
        Assert.Equal(0, result.Rejected);
        readingRepository.Verify(repo => repo.BulkInsertAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()), Times.Never);
        realtimeDispatcher.Verify(dispatcher => dispatcher.PublishAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()), Times.Never);
        errorRepository.Verify(repo => repo.LogAsync(It.IsAny<IEnumerable<IngestionError>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IngestMqttMessageAsync_ParsesTopicAndPayload()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var rlsAccessor = new TestRlsContextAccessor();
        var connectionFactory = new Mock<ITelemetryConnectionFactory>(MockBehavior.Strict);
        var normalizationService = new Mock<INormalizationService>();
        var idempotencyService = new Mock<IIdempotencyService>();
        var readingRepository = new Mock<ISensorReadingRepository>();
        var sensorStreamRepository = new Mock<ISensorStreamRepository>();
        var realtimeDispatcher = new Mock<ITelemetryRealtimeDispatcher>();
        var sessionRepository = new Mock<IIngestionSessionRepository>();
        var errorRepository = new Mock<IIngestionErrorRepository>();
        var logger = new Mock<ILogger<TelemetryIngestService>>();

        var stream = SensorStream.FromPersistence(
            streamId,
            siteId,
            equipmentId,
            equipmentChannelId: null,
            streamType: StreamType.Temperature,
            unit: Unit.DegreesCelsius,
            displayName: "Canopy Temp",
            locationId: null,
            roomId: null,
            zoneId: null,
            isActive: true,
            metadata: null,
            createdAt: timestamp,
            updatedAt: timestamp);

        sensorStreamRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorStream> { stream });

        idempotencyService
            .Setup(service => service.GetDuplicatesByStreamAsync(
                It.IsAny<IReadOnlyDictionary<Guid, string[]>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid StreamId, string MessageId)>());

        normalizationService
            .Setup(service => service.NormalizeAsync(
                It.IsAny<SensorReadingDto>(),
                stream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SensorReadingDto dto, SensorStream _, CancellationToken _) =>
                SensorReading.FromIngestion(
                    dto.StreamId,
                    dto.Value,
                    QualityCode.Good,
                    dto.SourceTimestamp,
                    dto.MessageId,
                    metadata: null));

        IReadOnlyCollection<SensorReading>? persisted = null;
        readingRepository
            .Setup(repo => repo.BulkInsertAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<SensorReading>, CancellationToken>((readings, _) => persisted = readings.ToList())
            .Returns(Task.CompletedTask);

        realtimeDispatcher
            .Setup(dispatcher => dispatcher.PublishAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        errorRepository
            .Setup(repo => repo.LogAsync(It.IsAny<IEnumerable<IngestionError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TelemetryIngestService(
            connectionFactory.Object,
            normalizationService.Object,
            idempotencyService.Object,
            readingRepository.Object,
            sensorStreamRepository.Object,
            realtimeDispatcher.Object,
            sessionRepository.Object,
            errorRepository.Object,
            rlsAccessor,
            logger.Object);

        var payloadObject = new
        {
            timestamp,
            readings = new[]
            {
                new
                {
                    stream_id = streamId,
                    value = 70.5,
                    unit = "degC",
                    timestamp,
                    message_id = "msg-1"
                },
                new
                {
                    stream_id = streamId,
                    value = 71.3,
                    unit = "degC",
                    timestamp = timestamp.AddSeconds(1),
                    message_id = "msg-2"
                }
            }
        };

        var payloadJson = JsonSerializer.Serialize(payloadObject);
        var payload = Encoding.UTF8.GetBytes(payloadJson);
        var topic = $"site/{siteId}/equipment/{equipmentId}/telemetry";

        // Act
        var result = await service.IngestMqttMessageAsync(topic, payload, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalReceived);
        Assert.Equal(2, result.Accepted);
        Assert.Equal(0, result.Duplicates);
        Assert.Equal(0, result.Rejected);
        Assert.NotNull(persisted);
        Assert.Equal(2, persisted!.Count);
        Assert.All(persisted!, reading => Assert.Equal(streamId, reading.StreamId));
        realtimeDispatcher.Verify(dispatcher => dispatcher.PublishAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()), Times.Once);
        errorRepository.Verify(repo => repo.LogAsync(It.IsAny<IEnumerable<IngestionError>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IngestBatchAsync_LogsErrorsForBadQualityReadings()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var streamId = Guid.NewGuid();
        var timestamp = DateTimeOffset.UtcNow;

        var rlsAccessor = new TestRlsContextAccessor();
        var connectionFactory = new Mock<ITelemetryConnectionFactory>(MockBehavior.Strict);
        var normalizationService = new Mock<INormalizationService>();
        var idempotencyService = new Mock<IIdempotencyService>();
        var readingRepository = new Mock<ISensorReadingRepository>(MockBehavior.Strict);
        var sensorStreamRepository = new Mock<ISensorStreamRepository>();
        var realtimeDispatcher = new Mock<ITelemetryRealtimeDispatcher>(MockBehavior.Strict);
        var sessionRepository = new Mock<IIngestionSessionRepository>();
        var errorRepository = new Mock<IIngestionErrorRepository>();
        var logger = new Mock<ILogger<TelemetryIngestService>>();

        var stream = SensorStream.FromPersistence(
            streamId,
            siteId,
            equipmentId,
            equipmentChannelId: null,
            streamType: StreamType.Humidity,
            unit: Unit.Percent,
            displayName: "RH",
            locationId: null,
            roomId: null,
            zoneId: null,
            isActive: true,
            metadata: null,
            createdAt: timestamp,
            updatedAt: timestamp);

        sensorStreamRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SensorStream> { stream });

        idempotencyService
            .Setup(service => service.GetDuplicatesByStreamAsync(
                It.IsAny<IReadOnlyDictionary<Guid, string[]>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<(Guid StreamId, string MessageId)>());

        normalizationService
            .Setup(service => service.NormalizeAsync(
                It.IsAny<SensorReadingDto>(),
                stream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((SensorReadingDto dto, SensorStream _, CancellationToken _) =>
                SensorReading.FromIngestion(
                    dto.StreamId,
                    dto.Value,
                    QualityCode.BadOutOfRange,
                    dto.SourceTimestamp,
                    dto.MessageId,
                    metadata: null));

        errorRepository
            .Setup(repo => repo.LogAsync(It.IsAny<IEnumerable<IngestionError>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TelemetryIngestService(
            connectionFactory.Object,
            normalizationService.Object,
            idempotencyService.Object,
            readingRepository.Object,
            sensorStreamRepository.Object,
            realtimeDispatcher.Object,
            sessionRepository.Object,
            errorRepository.Object,
            rlsAccessor,
            logger.Object);

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http,
            new List<SensorReadingDto>
            {
                new(streamId, timestamp, 101.2, Unit.Percent, timestamp, "msg-bad")
            });

        // Act
        var result = await service.IngestBatchAsync(siteId, request, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalReceived);
        Assert.Equal(0, result.Accepted);
        Assert.Equal(1, result.Rejected);
        Assert.Equal(0, result.Duplicates);
        errorRepository.Verify(repo => repo.LogAsync(It.Is<IEnumerable<IngestionError>>(errors => errors.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
        readingRepository.Verify(repo => repo.BulkInsertAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()), Times.Never);
        realtimeDispatcher.Verify(dispatcher => dispatcher.PublishAsync(It.IsAny<IEnumerable<SensorReading>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class TestRlsContextAccessor : ITelemetryRlsContextAccessor
    {
        public TelemetryRlsContext Current { get; private set; } = new(Guid.Empty, "service_account", null);

        public void Set(TelemetryRlsContext context) => Current = context;

        public void Clear() => Current = new(Guid.Empty, "service_account", null);
    }
}
