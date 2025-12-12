using System.Threading;
using System.Threading.Tasks;

namespace Harvestry.Shared.Utilities.Llm;

public interface ILlmGateway
{
    Task<LlmChatResponse> CompleteChatAsync(LlmChatRequest request, CancellationToken cancellationToken);
}




