using System;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Periodically generates cultivation insights using telemetry context and the LLM gateway.
/// </summary>
public sealed class CultivationInsightsWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    private readonly ICultivationInsightProvider _contextProvider;
    private readonly ICultivationInsightService _insightService;
    private readonly ILogger<CultivationInsightsWorker> _logger;

    public CultivationInsightsWorker(
        ICultivationInsightProvider contextProvider,
        ICultivationInsightService insightService,
        ILogger<CultivationInsightsWorker> logger)
    {
        _contextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
        _insightService = insightService ?? throw new ArgumentNullException(nameof(insightService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cultivation insights worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateInsightsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate cultivation insights");
            }

            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Cultivation insights worker stopped");
    }

    private async Task GenerateInsightsAsync(CancellationToken cancellationToken)
    {
        var contexts = await _contextProvider.GetActiveContextsAsync(cancellationToken).ConfigureAwait(false);
        if (contexts.Count == 0)
        {
            _logger.LogDebug("No cultivation contexts available for insight generation");
            return;
        }

        foreach (var context in contexts)
        {
            var insight = await _insightService.GenerateEnvironmentInsightAsync(context, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Cultivation insight for {Room}: {Insight}", context.Room, insight);
        }
    }
}




