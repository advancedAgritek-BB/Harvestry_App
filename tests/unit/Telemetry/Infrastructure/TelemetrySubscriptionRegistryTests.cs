using System.Threading.Tasks;
using FluentAssertions;
using Harvestry.Telemetry.Infrastructure.Realtime;
using Xunit;

namespace Harvestry.Telemetry.Tests.Infrastructure;

public sealed class TelemetrySubscriptionRegistryTests
{
    private readonly TelemetrySubscriptionRegistry _registry = new();

    [Fact]
    public void Register_ShouldTrackNewConnection()
    {
        var connectionId = "conn-1";
        var streamA = Guid.NewGuid();
        var streamB = Guid.NewGuid();

        _registry.Register(connectionId, streamA);
        _registry.Register(connectionId, streamB);

        var snapshot = _registry.GetSnapshot();

        snapshot.TotalConnections.Should().Be(1);
        snapshot.StreamCounts.Should().ContainKey(streamA).WhoseValue.Should().Be(1);
        snapshot.StreamCounts.Should().ContainKey(streamB).WhoseValue.Should().Be(1);
    }

    [Fact]
    public void Register_ShouldNotDoubleCountDuplicateSubscription()
    {
        var connectionId = "conn-dup";
        var streamId = Guid.NewGuid();

        _registry.Register(connectionId, streamId);
        _registry.Register(connectionId, streamId);

        var snapshot = _registry.GetSnapshot();

        snapshot.TotalConnections.Should().Be(1);
        snapshot.StreamCounts.Should().ContainSingle()
            .Which.Value.Should().Be(1);
    }

    [Fact]
    public void Unregister_ShouldRemoveConnectionWhenLastStreamRemoved()
    {
        var connectionId = "conn-remove";
        var streamId = Guid.NewGuid();

        _registry.Register(connectionId, streamId);
        _registry.Unregister(connectionId, streamId);

        var snapshot = _registry.GetSnapshot();

        snapshot.TotalConnections.Should().Be(0);
        snapshot.StreamCounts.Should().BeEmpty();
    }

    [Fact]
    public async Task PruneStaleConnections_ShouldRemoveExpiredEntries()
    {
        var connectionId = "conn-stale";
        var streamId = Guid.NewGuid();

        _registry.Register(connectionId, streamId);

        await Task.Delay(5); // allow LastUpdated to fall behind threshold

        var removed = _registry.PruneStaleConnections(TimeSpan.FromMilliseconds(1));
        removed.Should().Be(1);

        var snapshot = _registry.GetSnapshot();
        snapshot.TotalConnections.Should().Be(0);
    }
}
