using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISlackWebhookHandler
{
    Task<SlackWebhookResult> HandleAsync(SlackWebhookRequest request, CancellationToken cancellationToken);
}
