using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Harvestry.Shared.Utilities.Http;

/// <summary>
/// Extension methods for adding resilience patterns to HttpClient.
/// Uses Polly v8 via Microsoft.Extensions.Http.Resilience.
/// </summary>
public static class ResilientHttpClientExtensions
{
    /// <summary>
    /// Adds standard Harvestry resilience patterns to an HttpClient:
    /// - Retry with exponential backoff (3 attempts)
    /// - Circuit breaker (50% failure ratio triggers 30s break)
    /// - Timeout (10 seconds)
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The resilience pipeline builder for further configuration.</returns>
    public static IHttpResiliencePipelineBuilder AddHarvestryResilience(this IHttpClientBuilder builder)
    {
        return builder.AddResilienceHandler("harvestry-standard", ConfigureStandardResilience);
    }

    /// <summary>
    /// Adds resilience patterns optimized for high-throughput APIs like Slack.
    /// - Retry with shorter delays (2 attempts)
    /// - Aggressive circuit breaker (10 failures triggers break)
    /// - Shorter timeout (5 seconds)
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The resilience pipeline builder for further configuration.</returns>
    public static IHttpResiliencePipelineBuilder AddHighThroughputResilience(this IHttpClientBuilder builder)
    {
        return builder.AddResilienceHandler("harvestry-high-throughput", ConfigureHighThroughputResilience);
    }

    /// <summary>
    /// Adds resilience patterns for critical operations that cannot fail silently.
    /// - More retries (5 attempts)
    /// - Conservative circuit breaker
    /// - Longer timeout (30 seconds)
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The resilience pipeline builder for further configuration.</returns>
    public static IHttpResiliencePipelineBuilder AddCriticalOperationResilience(this IHttpClientBuilder builder)
    {
        return builder.AddResilienceHandler("harvestry-critical", ConfigureCriticalResilience);
    }

    private static void ConfigureStandardResilience(ResiliencePipelineBuilder<HttpResponseMessage> pipeline)
    {
        // Retry strategy
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
        });

        // Circuit breaker
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = args => ValueTask.FromResult(ShouldTripCircuitBreaker(args.Outcome))
        });

        // Timeout
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));
    }

    private static void ConfigureHighThroughputResilience(ResiliencePipelineBuilder<HttpResponseMessage> pipeline)
    {
        // Minimal retries for high throughput
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = DelayBackoffType.Linear,
            UseJitter = true,
            ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
        });

        // Aggressive circuit breaker
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.3,
            MinimumThroughput = 20,
            SamplingDuration = TimeSpan.FromSeconds(20),
            BreakDuration = TimeSpan.FromSeconds(15),
            ShouldHandle = args => ValueTask.FromResult(ShouldTripCircuitBreaker(args.Outcome))
        });

        // Short timeout
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));
    }

    private static void ConfigureCriticalResilience(ResiliencePipelineBuilder<HttpResponseMessage> pipeline)
    {
        // Aggressive retries for critical operations
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 5,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
        });

        // Conservative circuit breaker
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.7,
            MinimumThroughput = 5,
            SamplingDuration = TimeSpan.FromSeconds(60),
            BreakDuration = TimeSpan.FromSeconds(60),
            ShouldHandle = args => ValueTask.FromResult(ShouldTripCircuitBreaker(args.Outcome))
        });

        // Longer timeout
        pipeline.AddTimeout(TimeSpan.FromSeconds(30));
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        // Retry on network errors
        if (outcome.Exception is HttpRequestException)
            return true;

        // Retry on timeout
        if (outcome.Exception is TaskCanceledException)
            return true;

        // Retry on server errors
        if (outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError)
            return true;

        // Retry on rate limiting (with respect for Retry-After)
        if (outcome.Result?.StatusCode == HttpStatusCode.TooManyRequests)
            return true;

        // Retry on service unavailable
        if (outcome.Result?.StatusCode == HttpStatusCode.ServiceUnavailable)
            return true;

        return false;
    }

    private static bool ShouldTripCircuitBreaker(Outcome<HttpResponseMessage> outcome)
    {
        // Trip on network errors
        if (outcome.Exception is HttpRequestException)
            return true;

        // Trip on server errors
        if (outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError)
            return true;

        return false;
    }
}
