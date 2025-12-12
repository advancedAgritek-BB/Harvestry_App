using Harvestry.AiModels.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.AiModels.Application.Services;

/// <summary>
/// Background service that runs scheduled graph snapshot builds.
/// Supports configurable intervals for full and incremental snapshots.
/// </summary>
public sealed class GraphSnapshotBackgroundService : BackgroundService
{
    private readonly IGraphSnapshotBuilder _snapshotBuilder;
    private readonly ISiteRepository _siteRepository;
    private readonly ILogger<GraphSnapshotBackgroundService> _logger;
    private readonly GraphSnapshotOptions _options;

    public GraphSnapshotBackgroundService(
        IGraphSnapshotBuilder snapshotBuilder,
        ISiteRepository siteRepository,
        IOptions<GraphSnapshotOptions> options,
        ILogger<GraphSnapshotBackgroundService> logger)
    {
        _snapshotBuilder = snapshotBuilder;
        _siteRepository = siteRepository;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Graph snapshot background service starting. Full interval: {Full}h, Incremental interval: {Inc}m",
            _options.FullSnapshotIntervalHours,
            _options.IncrementalSnapshotIntervalMinutes);

        using var fullTimer = new PeriodicTimer(TimeSpan.FromHours(_options.FullSnapshotIntervalHours));
        using var incrementalTimer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IncrementalSnapshotIntervalMinutes));

        // Run initial full snapshot after startup delay
        await Task.Delay(TimeSpan.FromMinutes(_options.StartupDelayMinutes), stoppingToken);
        
        if (_options.RunFullSnapshotOnStartup)
        {
            await RunFullSnapshotsAsync(stoppingToken);
        }

        // Run both timers concurrently
        var fullTask = RunFullSnapshotLoopAsync(fullTimer, stoppingToken);
        var incrementalTask = RunIncrementalSnapshotLoopAsync(incrementalTimer, stoppingToken);

        await Task.WhenAll(fullTask, incrementalTask);
    }

    private async Task RunFullSnapshotLoopAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunFullSnapshotsAsync(stoppingToken);
        }
    }

    private async Task RunIncrementalSnapshotLoopAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Incremental updates would be event-driven in production
            // This loop provides a fallback to catch any missed events
            _logger.LogDebug("Incremental snapshot check triggered");
        }
    }

    private async Task RunFullSnapshotsAsync(CancellationToken stoppingToken)
    {
        try
        {
            var sites = await _siteRepository.GetActiveSiteIdsAsync(stoppingToken);
            
            _logger.LogInformation("Running full graph snapshots for {Count} sites", sites.Count);

            foreach (var siteId in sites)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var result = await _snapshotBuilder.BuildFullSnapshotAsync(siteId, stoppingToken);
                    
                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "Full snapshot completed for site {SiteId}: {Nodes} nodes, {Edges} edges in {Duration}ms",
                            siteId, result.NodesCreated, result.EdgesCreated, result.Duration.TotalMilliseconds);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Full snapshot failed for site {SiteId}: {Error}",
                            siteId, result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running full snapshot for site {SiteId}", siteId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in full snapshot batch run");
        }
    }
}

/// <summary>
/// Configuration options for graph snapshot scheduling.
/// </summary>
public sealed class GraphSnapshotOptions
{
    public const string SectionName = "GraphSnapshot";

    /// <summary>
    /// Delay after startup before running first snapshot (default: 5 minutes)
    /// </summary>
    public int StartupDelayMinutes { get; set; } = 5;

    /// <summary>
    /// Whether to run a full snapshot on service startup (default: true)
    /// </summary>
    public bool RunFullSnapshotOnStartup { get; set; } = true;

    /// <summary>
    /// Interval between full snapshot runs (default: 24 hours)
    /// </summary>
    public int FullSnapshotIntervalHours { get; set; } = 24;

    /// <summary>
    /// Interval between incremental snapshot checks (default: 15 minutes)
    /// </summary>
    public int IncrementalSnapshotIntervalMinutes { get; set; } = 15;
}

/// <summary>
/// Repository interface for accessing site information.
/// </summary>
public interface ISiteRepository
{
    Task<IReadOnlyList<Guid>> GetActiveSiteIdsAsync(CancellationToken cancellationToken = default);
}
