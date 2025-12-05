using Harvestry.Telemetry.Application.Interfaces;
using Harvestry.Telemetry.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Harvestry.Telemetry.Infrastructure.Workers;

public class TelemetrySimulationWorker : BackgroundService
{
    private readonly ITelemetrySimulationService _simulationService;
    private readonly TelemetrySimulationOptions _options;
    private readonly ILogger<TelemetrySimulationWorker> _logger;
    private readonly PeriodicTimer _timer;

    public TelemetrySimulationWorker(
        ITelemetrySimulationService simulationService,
        IOptions<TelemetrySimulationOptions> options,
        ILogger<TelemetrySimulationWorker> logger)
    {
        _simulationService = simulationService;
        _options = options.Value;
        _logger = logger;
        
        // Ensure interval is at least 1 second
        var interval = _options.IntervalSeconds < 1 ? 1 : _options.IntervalSeconds;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(interval));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Telemetry simulation is disabled.");
            return;
        }

        _logger.LogInformation("Telemetry simulation worker started with interval {Interval}s.", _options.IntervalSeconds);

        try
        {
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    // Service handles its own scope internally
                    await _simulationService.GenerateAndIngestAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during telemetry simulation cycle.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
    }
}
