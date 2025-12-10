using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Harvestry.Shared.Utilities.Llm;

public static class LlmServiceCollectionExtensions
{
    /// <summary>
    /// Registers the OpenAI-backed LLM gateway with resilience, safety, and redaction defaults.
    /// </summary>
    public static IServiceCollection AddLlmGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LlmGatewayOptions>(configuration.GetSection(LlmGatewayOptions.SectionName));

        services.AddSingleton<SensitiveDataRedactor>();
        services.AddSingleton<ContentSafetyEvaluator>();

        services.AddHttpClient("openai-llm", (provider, client) =>
            {
                var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LlmGatewayOptions>>().Value;
                client.BaseAddress = new Uri($"{options.BaseUrl.TrimEnd('/')}/");
                client.Timeout = options.Timeout;
            })
            .AddResilienceHandler("openai-llm", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromMilliseconds(500),
                    UseJitter = true
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 10,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30)
                });

                builder.AddTimeout(TimeSpan.FromSeconds(15));
            });

        services.AddTransient<ILlmGateway, OpenAiLlmGateway>();
        return services;
    }

    /// <summary>
    /// Registers prompt tooling (registry + evaluation harness).
    /// </summary>
    public static IServiceCollection AddLlmPromptToolkit(this IServiceCollection services)
    {
        services.AddSingleton<PromptTemplateRegistry>();
        services.AddTransient<PromptEvaluationHarness>();
        return services;
    }

    /// <summary>
    /// Registers shared data QA filtering with configurable thresholds.
    /// </summary>
    public static IServiceCollection AddDataQaFiltering(this IServiceCollection services, IConfiguration configuration)
    {
        var thresholds = new DataQaThresholds();
        configuration.GetSection("DataQaThresholds").Bind(thresholds);

        services.AddSingleton(thresholds);
        services.AddSingleton<DataQaSuggestionFilter>();
        return services;
    }
}



