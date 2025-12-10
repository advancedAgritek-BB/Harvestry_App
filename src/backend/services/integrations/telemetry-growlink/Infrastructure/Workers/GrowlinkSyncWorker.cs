using Harvestry.Integration.Growlink.Application.Interfaces;
using Harvestry.Integration.Growlink.Infrastructure.External;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Integration.Growlink.Infrastructure.Workers;

/// <summary>
/// Background service that continuously syncs data from Growlink.
/// </summary>
public sealed class GrowlinkSyncWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly GrowlinkApiConfiguration _config;
    private readonly ILogger<GrowlinkSyncWorker> _logger;

    public GrowlinkSyncWorker(
        IServiceProvider serviceProvider,
        IOptions<GrowlinkApiConfiguration> config,
        ILogger<GrowlinkSyncWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Growlink sync worker starting. Sync interval: {Interval}s",
            _config.SyncIntervalSeconds);

        // Initial delay to allow app startup
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_config.SyncIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAllSitesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in Growlink sync worker");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Growlink sync worker stopped");
    }

    private async Task SyncAllSitesAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Starting Growlink sync cycle");

        // Create a scope to resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var syncService = scope.ServiceProvider.GetRequiredService<IGrowlinkSyncService>();

        try
        {
            var results = await syncService.SyncAllSitesAsync(cancellationToken);

            var successCount = results.Count(r => r.Status == Domain.Enums.GrowlinkSyncStatus.Success);
            var partialCount = results.Count(r => r.Status == Domain.Enums.GrowlinkSyncStatus.PartialSuccess);
            var errorCount = results.Count(r =>
                r.Status == Domain.Enums.GrowlinkSyncStatus.ApiError ||
                r.Status == Domain.Enums.GrowlinkSyncStatus.AuthenticationError);
            var totalReadings = results.Sum(r => r.ReadingsIngested);

            _logger.LogInformation(
                "Growlink sync cycle completed: Sites={Total}, Success={Success}, Partial={Partial}, Errors={Errors}, Readings={Readings}",
                results.Count, successCount, partialCount, errorCount, totalReadings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Growlink sync cycle");
        }
    }
}




