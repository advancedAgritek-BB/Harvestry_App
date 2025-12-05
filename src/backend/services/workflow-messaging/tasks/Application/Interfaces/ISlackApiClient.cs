using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Harvestry.Tasks.Application.Interfaces;

/// <summary>
/// Client interface for interacting with the Slack API.
/// </summary>
public interface ISlackApiClient
{
    /// <summary>
    /// Sends a message to a Slack channel using the provided bot token and payload.
    /// </summary>
    /// <param name="botToken">Secure bot token for Slack API authentication.</param>
    /// <param name="channelId">The Slack channel identifier to send the message to.</param>
    /// <param name="payload">Strongly-typed payload containing the message details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the response from Slack.</returns>
    Task<Harvestry.Tasks.DTOs.SlackMessageResponse> SendMessageAsync(SecureString botToken, string channelId, object payload, CancellationToken cancellationToken);
}
