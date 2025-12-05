using System;
using System.Collections.Generic;
using System.Linq;


namespace Harvestry.Telemetry.Application.Interfaces.Models;

/// <summary>
/// Snapshot of the current telemetry real-time subscription state.
/// </summary>
public sealed class TelemetrySubscriptionSnapshot
{
    public TelemetrySubscriptionSnapshot(
        DateTimeOffset capturedAt,
        int totalConnections,
        IReadOnlyDictionary<Guid, int> streamCounts)
    {
        CapturedAt = capturedAt;
        TotalConnections = totalConnections;
        StreamCounts = streamCounts;
        TotalSubscriptions = streamCounts.Values.Sum();
    }

    /// <summary>
    /// Timestamp when the snapshot was captured (UTC).
    /// </summary>
    public DateTimeOffset CapturedAt { get; }

    /// <summary>
    /// Number of SignalR connections currently tracked.
    /// </summary>
    public int TotalConnections { get; }

    /// <summary>
    /// Total number of stream subscriptions across all connections.
    /// </summary>
    public int TotalSubscriptions { get; }

    /// <summary>
    /// Count of subscribers per stream identifier.
    /// </summary>
    public IReadOnlyDictionary<Guid, int> StreamCounts { get; }

    /// <summary>
    /// Number of distinct streams with at least one subscriber.
    /// </summary>
    public int ActiveStreamCount => StreamCounts.Count;
}
