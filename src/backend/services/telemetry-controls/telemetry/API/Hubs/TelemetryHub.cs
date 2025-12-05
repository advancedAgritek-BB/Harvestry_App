using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.API.Hubs;

/// <summary>
/// SignalR hub for telemetry real-time subscriptions.
/// </summary>
public sealed class TelemetryHub : Hub
{
    private static string StreamGroup(Guid streamId) => $"stream:{streamId}";

    private readonly ITelemetrySubscriptionRegistry _subscriptionRegistry;
    private readonly ILogger<TelemetryHub> _logger;

    public TelemetryHub(
        ITelemetrySubscriptionRegistry subscriptionRegistry,
        ILogger<TelemetryHub> logger)
    {
        _subscriptionRegistry = subscriptionRegistry ?? throw new ArgumentNullException(nameof(subscriptionRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _subscriptionRegistry.RemoveConnection(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove connection {ConnectionId} from subscription registry", Context.ConnectionId);
        }

        _logger.LogDebug("Connection {ConnectionId} disconnected from telemetry hub", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public async Task Subscribe(Guid streamId)
    {
        if (streamId == Guid.Empty)
        {
            throw new ArgumentException("Stream ID cannot be empty", nameof(streamId));
        }

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, StreamGroup(streamId)).ConfigureAwait(false);
            _subscriptionRegistry.Register(Context.ConnectionId, streamId);
            _logger.LogDebug("Connection {ConnectionId} subscribed to stream {StreamId}", Context.ConnectionId, streamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe connection {ConnectionId} to stream {StreamId}", Context.ConnectionId, streamId);
            throw;
        }
    }

    public async Task Unsubscribe(Guid streamId)
    {
        if (streamId == Guid.Empty)
        {
            throw new ArgumentException("Stream ID cannot be empty", nameof(streamId));
        }

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, StreamGroup(streamId)).ConfigureAwait(false);
            _subscriptionRegistry.Unregister(Context.ConnectionId, streamId);
            _logger.LogDebug("Connection {ConnectionId} unsubscribed from stream {StreamId}", Context.ConnectionId, streamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe connection {ConnectionId} from stream {StreamId}", Context.ConnectionId, streamId);
            throw;
        }
    }
}
