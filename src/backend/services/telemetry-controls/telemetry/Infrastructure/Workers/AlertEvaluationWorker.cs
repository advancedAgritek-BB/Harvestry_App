using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Telemetry.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Harvestry.Telemetry.Infrastructure.Workers;

/// <summary>
/// Background worker that periodically evaluates alert rules for all sites.
/// </summary>
public sealed class AlertEvaluationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertEvaluationWorker> _logger;
    private readonly TimeSpan _interval;

    public AlertEvaluationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AlertEvaluationWorker> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _interval = TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alert evaluation worker starting with interval {Interval}", _interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var ruleRepository = scope.ServiceProvider.GetRequiredService<IAlertRuleRepository>();
                var evaluationService = scope.ServiceProvider.GetRequiredService<IAlertEvaluationService>();

                var activeRules = await ruleRepository.GetAllActiveAsync(stoppingToken).ConfigureAwait(false);
                var rulesBySite = activeRules.GroupBy(rule => rule.SiteId);

                foreach (var siteGroup in rulesBySite)
                {
                    try
                    {
                        await evaluationService.EvaluateRulesAsync(siteGroup.Key, stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to evaluate alert rules for site {SiteId}", siteGroup.Key);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert evaluation worker encountered an unexpected error");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Exit loop on cancellation
            }
        }

        _logger.LogInformation("Alert evaluation worker stopping");
    }
}
