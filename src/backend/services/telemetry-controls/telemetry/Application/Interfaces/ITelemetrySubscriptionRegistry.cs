using System;
using Harvestry.Telemetry.Application.Interfaces.Models;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Tracks real-time telemetry subscriptions for SignalR connections.
/// </summary>
public interface ITelemetrySubscriptionRegistry
{
    /// <summary>
    /// Records that a connection subscribed to the specified stream.
    /// </summary>
    void Register(string connectionId, Guid streamId);

    /// <summary>
    /// Records that a connection unsubscribed from the specified stream.
    /// </summary>
    void Unregister(string connectionId, Guid streamId);

    /// <summary>
    /// Removes all subscriptions associated with the given connection.
    /// </summary>
    void RemoveConnection(string connectionId);

    /// <summary>
    /// Returns a snapshot of current subscription metrics.
    /// </summary>
    TelemetrySubscriptionSnapshot GetSnapshot();

    /// <summary>
    /// Removes connections that have not updated within the specified window.
    /// </summary>
    int PruneStaleConnections(TimeSpan staleAfter);
}
