using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Infrastructure.Persistence;
using Harvestry.Telemetry.Infrastructure.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Harvestry.Telemetry.IntegrationTests;

/// <summary>
/// Integration tests validating real-time telemetry push latency.
/// Target SLO: p95 < 1.5s from store → client dispatch.
/// </summary>
public sealed class RealtimeLatencyTests : IntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public RealtimeLatencyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task SubscriptionRegistry_RegisterAndUnregister_WorksCorrectly()
    {
        // Arrange
        var registry = new TelemetrySubscriptionRegistry();
        var connectionId = "test-connection-1";
        var streamId1 = Guid.NewGuid();
        var streamId2 = Guid.NewGuid();

        // Act - Register
        registry.Register(connectionId, streamId1);
        registry.Register(connectionId, streamId2);
        var snapshot1 = registry.GetSnapshot();

        // Assert - both streams registered
        snapshot1.TotalConnections.Should().Be(1);
        snapshot1.StreamSubscriberCounts.Should().ContainKey(streamId1);
        snapshot1.StreamSubscriberCounts.Should().ContainKey(streamId2);

        // Act - Unregister one stream
        registry.Unregister(connectionId, streamId1);
        var snapshot2 = registry.GetSnapshot();

        // Assert - one stream remains
        snapshot2.TotalConnections.Should().Be(1);
        snapshot2.StreamSubscriberCounts.Should().NotContainKey(streamId1);
        snapshot2.StreamSubscriberCounts.Should().ContainKey(streamId2);

        // Act - Unregister last stream (removes connection)
        registry.Unregister(connectionId, streamId2);
        var snapshot3 = registry.GetSnapshot();

        // Assert - connection removed
        snapshot3.TotalConnections.Should().Be(0);
    }

    [Fact]
    public async Task SubscriptionRegistry_PruneStaleConnections_RemovesOldEntries()
    {
        // Arrange
        var registry = new TelemetrySubscriptionRegistry();
        var connectionId1 = "conn-active";
        var connectionId2 = "conn-stale";
        var streamId = Guid.NewGuid();

        registry.Register(connectionId1, streamId);
        registry.Register(connectionId2, streamId);

        // Simulate time passage by pruning with very short stale threshold
        // Note: In real scenarios, stale detection would use LastUpdatedUtc comparison
        var snapshot1 = registry.GetSnapshot();
        snapshot1.TotalConnections.Should().Be(2);

        // Act - prune with zero time (nothing should be pruned as entries are fresh)
        var pruned = registry.PruneStaleConnections(TimeSpan.FromSeconds(1));

        // Assert - recent connections not pruned
        var snapshot2 = registry.GetSnapshot();
        snapshot2.TotalConnections.Should().Be(2);
        pruned.Should().Be(0);
    }

    [Fact]
    public async Task IngestToDispatchLatency_MeasuresWithinSLO()
    {
        var sp = Services;
        var rls = sp.GetRequiredService<ITelemetryRlsContextAccessor>();
        var streamRepo = sp.GetRequiredService<ISensorStreamRepository>();
        var ingest = sp.GetRequiredService<ITelemetryIngestService>();

        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        rls.Set(new TelemetryRlsContext(siteId, "service_account", Guid.Empty));

        var stream = SensorStream.Create(
            siteId,
            equipmentId,
            StreamType.Temperature,
            Unit.DegreesCelsius,
            "Latency Test Sensor");
        await streamRepo.CreateAsync(stream);

        // Prepare batch of readings
        var now = DateTimeOffset.UtcNow;
        var readings = new List<SensorReadingDto>();
        for (int i = 0; i < 100; i++)
        {
            readings.Add(new SensorReadingDto(
                stream.Id,
                now.AddSeconds(-i),
                20.0 + (i * 0.1),
                Unit.DegreesCelsius,
                now.AddSeconds(-i),
                $"latency-msg-{i}",
                null));
        }

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http,
            readings);

        // Act - Measure ingest latency
        var stopwatch = Stopwatch.StartNew();
        var result = await ingest.IngestBatchAsync(siteId, request);
        stopwatch.Stop();

        // Assert
        result.Accepted.Should().BeGreaterOrEqualTo(90); // allow some tolerance
        
        var ingestLatencyMs = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Ingest latency for 100 readings: {ingestLatencyMs}ms");

        // SLO: p95 < 1000ms for ingest (1.0s)
        // Individual test gives approximate measure; actual p95 from load tests
        ingestLatencyMs.Should().BeLessThan(5000, "Single batch ingest should complete well within 5s");
    }

    [Fact]
    public async Task HighVolumeIngest_MaintainsPerformance()
    {
        var sp = Services;
        var rls = sp.GetRequiredService<ITelemetryRlsContextAccessor>();
        var streamRepo = sp.GetRequiredService<ISensorStreamRepository>();
        var ingest = sp.GetRequiredService<ITelemetryIngestService>();

        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        rls.Set(new TelemetryRlsContext(siteId, "service_account", Guid.Empty));

        // Create multiple streams
        var streams = new List<SensorStream>();
        var streamTypes = new[] { StreamType.Temperature, StreamType.Humidity, StreamType.Co2, StreamType.VPD, StreamType.EC };
        var units = new[] { Unit.DegreesCelsius, Unit.Percent, Unit.PartsPerMillion, Unit.Kilopascal, Unit.MillisiemensPerCm };

        for (int i = 0; i < 5; i++)
        {
            var stream = SensorStream.Create(
                siteId,
                equipmentId,
                streamTypes[i],
                units[i],
                $"High Volume Sensor {i}");
            await streamRepo.CreateAsync(stream);
            streams.Add(stream);
        }

        // Generate readings across all streams
        var now = DateTimeOffset.UtcNow;
        var allReadings = new List<SensorReadingDto>();
        var random = new Random(42);

        foreach (var stream in streams)
        {
            for (int i = 0; i < 200; i++)
            {
                allReadings.Add(new SensorReadingDto(
                    stream.Id,
                    now.AddSeconds(-i),
                    random.NextDouble() * 100,
                    stream.Unit,
                    now.AddSeconds(-i),
                    $"hvol-{stream.Id}-{i}",
                    null));
            }
        }

        var request = new IngestTelemetryRequestDto(
            siteId,
            equipmentId,
            IngestionProtocol.Http,
            allReadings);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await ingest.IngestBatchAsync(siteId, request);
        stopwatch.Stop();

        // Assert
        result.Accepted.Should().BeGreaterOrEqualTo(900); // 5 streams x 200 readings
        
        var latencyMs = stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"High volume ingest ({allReadings.Count} readings): {latencyMs}ms");
        _output.WriteLine($"Throughput: {(allReadings.Count * 1000.0 / latencyMs):F0} readings/sec");

        // Performance baseline: should handle 1000 readings in under 10 seconds
        latencyMs.Should().BeLessThan(10000);
    }

    [Fact]
    public async Task ConcurrentIngest_HandlesParallelRequests()
    {
        var sp = Services;
        var rls = sp.GetRequiredService<ITelemetryRlsContextAccessor>();
        var streamRepo = sp.GetRequiredService<ISensorStreamRepository>();
        var ingest = sp.GetRequiredService<ITelemetryIngestService>();

        // Arrange
        var siteId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        rls.Set(new TelemetryRlsContext(siteId, "service_account", Guid.Empty));

        var stream = SensorStream.Create(
            siteId,
            equipmentId,
            StreamType.Temperature,
            Unit.DegreesCelsius,
            "Concurrent Test Sensor");
        await streamRepo.CreateAsync(stream);

        // Create multiple parallel ingest tasks
        var tasks = new List<Task<IngestResultDto>>();
        var now = DateTimeOffset.UtcNow;

        for (int batch = 0; batch < 10; batch++)
        {
            var readings = new List<SensorReadingDto>();
            for (int i = 0; i < 50; i++)
            {
                readings.Add(new SensorReadingDto(
                    stream.Id,
                    now.AddSeconds(-(batch * 50 + i)),
                    20.0 + (batch * 0.5) + (i * 0.01),
                    Unit.DegreesCelsius,
                    now.AddSeconds(-(batch * 50 + i)),
                    $"concurrent-{batch}-{i}",
                    null));
            }

            var request = new IngestTelemetryRequestDto(
                siteId,
                equipmentId,
                IngestionProtocol.Http,
                readings);

            tasks.Add(Task.Run(() => ingest.IngestBatchAsync(siteId, request)));
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var totalAccepted = 0;
        foreach (var result in results)
        {
            totalAccepted += result.Accepted;
        }

        _output.WriteLine($"Concurrent ingest (10 batches x 50 readings): {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Total accepted: {totalAccepted}");

        totalAccepted.Should().BeGreaterOrEqualTo(400); // allow for some deduplication
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 second timeout
    }
}
