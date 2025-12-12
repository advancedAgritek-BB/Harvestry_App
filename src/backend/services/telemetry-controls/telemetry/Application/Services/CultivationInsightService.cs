using System;
using System.Collections.Generic;
using Harvestry.Shared.Utilities.Llm;
using Harvestry.Telemetry.Application.DTOs;
using Harvestry.Telemetry.Application.Interfaces;

namespace Harvestry.Telemetry.Application.Services;

/// <summary>
/// Generates LLM-backed cultivation insights using the environment-watch prompt.
/// </summary>
public sealed class CultivationInsightService : ICultivationInsightService
{
    private readonly ILlmGateway _llmGateway;
    private readonly PromptTemplateRegistry _templates;

    public CultivationInsightService(ILlmGateway llmGateway, PromptTemplateRegistry templates)
    {
        _llmGateway = llmGateway ?? throw new ArgumentNullException(nameof(llmGateway));
        _templates = templates ?? throw new ArgumentNullException(nameof(templates));

        CultivationAssistantPrompts.RegisterDefaults(_templates);
    }

    public async Task<string> GenerateEnvironmentInsightAsync(CultivationInsightContext context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var template = _templates.Get(CultivationAssistantPrompts.EnvironmentWatchId, "v1");
        var prompt = template.Render(new Dictionary<string, string>
        {
            { "room", context.Room },
            { "phase", context.Phase },
            { "telemetry_summary", context.TelemetrySummary },
            { "issues", context.Issues }
        });

        var response = await _llmGateway.CompleteChatAsync(new LlmChatRequest
        {
            Model = string.Empty,
            SystemPrompt = prompt,
            Messages = Array.Empty<LlmMessage>()
        }, cancellationToken).ConfigureAwait(false);

        return response.Content;
    }
}




