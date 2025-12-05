using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Application.Interfaces.Models;

namespace Harvestry.Telemetry.Infrastructure.Realtime;

/// <summary>
/// In-memory registry that tracks SignalR telemetry subscriptions per connection.
/// </summary>
public sealed class TelemetrySubscriptionRegistry : ITelemetrySubscriptionRegistry
{
    private sealed class ConnectionEntry
    {
        private readonly ConcurrentDictionary<Guid, byte> _streams = new();

        public ConnectionEntry(string connectionId)
        {
            ConnectionId = connectionId;
            LastUpdatedUtc = DateTimeOffset.UtcNow;
        }

        public string ConnectionId { get; }

        public DateTimeOffset LastUpdatedUtc { get; private set; }

        public ICollection<Guid> Streams => _streams.Keys;

        public bool AddStream(Guid streamId)
        {
            var added = _streams.TryAdd(streamId, 0);
            LastUpdatedUtc = DateTimeOffset.UtcNow;
            return added;
        }

        public bool RemoveStream(Guid streamId)
        {
            var removed = _streams.TryRemove(streamId, out _);
            LastUpdatedUtc = DateTimeOffset.UtcNow;
            return removed;
        }

        public int StreamCount => _streams.Count;
    }

    private readonly ConcurrentDictionary<string, ConnectionEntry> _connections = new(StringComparer.Ordinal);

    public void Register(string connectionId, Guid streamId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            throw new ArgumentException("ConnectionId cannot be null or whitespace", nameof(connectionId));
        }

        var entry = _connections.GetOrAdd(connectionId, static id => new ConnectionEntry(id));
        entry.AddStream(streamId);
    }

    public void Unregister(string connectionId, Guid streamId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            throw new ArgumentException("ConnectionId cannot be null or whitespace", nameof(connectionId));
        }

        if (!_connections.TryGetValue(connectionId, out var entry))
        {
            return;
        }

        entry.RemoveStream(streamId);

        if (entry.StreamCount == 0)
        {
            _connections.TryRemove(connectionId, out _);
        }
    }

    public void RemoveConnection(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
        {
            return;
        }

        _connections.TryRemove(connectionId, out _);
    }

    public TelemetrySubscriptionSnapshot GetSnapshot()
    {
        var capturedAt = DateTimeOffset.UtcNow;
        var streamCounts = new Dictionary<Guid, int>();

        foreach (var entry in _connections.Values)
        {
            foreach (var streamId in entry.Streams)
            {
                if (streamCounts.TryGetValue(streamId, out var count))
                {
                    streamCounts[streamId] = count + 1;
                }
                else
                {
                    streamCounts[streamId] = 1;
                }
            }
        }

        return new TelemetrySubscriptionSnapshot(
            capturedAt,
            _connections.Count,
            streamCounts);
    }

    public int PruneStaleConnections(TimeSpan staleAfter)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            return 0;
        }

        var cutoff = DateTimeOffset.UtcNow - staleAfter;
        var removed = 0;

        foreach (var kvp in _connections)
        {
            // Attempt removal first to avoid TOCTOU race
            if (_connections.TryRemove(kvp.Key, out var removedEntry))
            {
                // Only count as pruned if the removed entry was actually stale
                if (removedEntry.LastUpdatedUtc < cutoff)
                {
                    removed++;
                }
                else
                {
                    // Entry was not stale, add it back
                    _connections.TryAdd(kvp.Key, removedEntry);
                }
            }
        }

        return removed;
    }
}
