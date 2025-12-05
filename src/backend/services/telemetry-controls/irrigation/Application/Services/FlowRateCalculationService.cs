using Harvestry.Irrigation.Domain.Entities;
using Harvestry.Irrigation.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Service for calculating and monitoring irrigation flow rates
/// </summary>
public interface IFlowRateCalculationService
{
    /// <summary>
    /// Get current flow rate snapshot for a site
    /// </summary>
    Task<FlowRateSnapshot> GetCurrentFlowRateSnapshotAsync(
        Guid siteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the total flow rate for a set of zones
    /// </summary>
    Task<decimal> CalculateZonesFlowRateAsync(
        Guid siteId,
        IEnumerable<Guid> zoneIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if running zones would exceed flow rate limits
    /// </summary>
    Task<FlowRateCheckResult> CheckFlowRateAsync(
        Guid siteId,
        IEnumerable<Guid> zoneIds,
        CancellationToken cancellationToken = default);
}

public sealed class FlowRateCalculationService : IFlowRateCalculationService
{
    private readonly IIrrigationSettingsRepository _settingsRepository;
    private readonly IZoneEmitterConfigurationRepository _emitterRepository;
    private readonly IActiveIrrigationRunRepository _activeRunRepository;
    private readonly ILogger<FlowRateCalculationService> _logger;

    public FlowRateCalculationService(
        IIrrigationSettingsRepository settingsRepository,
        IZoneEmitterConfigurationRepository emitterRepository,
        IActiveIrrigationRunRepository activeRunRepository,
        ILogger<FlowRateCalculationService> logger)
    {
        _settingsRepository = settingsRepository;
        _emitterRepository = emitterRepository;
        _activeRunRepository = activeRunRepository;
        _logger = logger;
    }

    public async Task<FlowRateSnapshot> GetCurrentFlowRateSnapshotAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetBySiteIdAsync(siteId, cancellationToken);
        if (settings == null)
        {
            _logger.LogWarning("No irrigation settings found for site {SiteId}, using defaults", siteId);
            // Return default with effectively unlimited flow rate
            return new FlowRateSnapshot(
                0,
                decimal.MaxValue,
                decimal.MaxValue,
                Array.Empty<ActiveZoneFlow>());
        }

        // Get all currently running irrigation zones
        var activeRuns = await _activeRunRepository.GetActiveRunsAsync(siteId, cancellationToken);
        var activeZoneFlows = new List<ActiveZoneFlow>();
        decimal totalCurrentFlow = 0;

        foreach (var run in activeRuns)
        {
            foreach (var zoneId in run.ActiveZoneIds)
            {
                var emitterConfig = await _emitterRepository.GetByZoneIdAsync(zoneId, cancellationToken);
                if (emitterConfig != null)
                {
                    var flowRate = emitterConfig.TotalZoneFlowRateLitersPerMinute;
                    totalCurrentFlow += flowRate;
                    
                    activeZoneFlows.Add(new ActiveZoneFlow(
                        zoneId,
                        emitterConfig.ZoneName,
                        flowRate,
                        run.StartedAt,
                        run.ExpectedEndAt));
                }
            }
        }

        return new FlowRateSnapshot(
            totalCurrentFlow,
            settings.MaxSystemFlowRateLitersPerMinute,
            settings.EffectiveMaxFlowRateLitersPerMinute,
            activeZoneFlows);
    }

    public async Task<decimal> CalculateZonesFlowRateAsync(
        Guid siteId,
        IEnumerable<Guid> zoneIds,
        CancellationToken cancellationToken = default)
    {
        decimal totalFlowRate = 0;

        foreach (var zoneId in zoneIds)
        {
            var emitterConfig = await _emitterRepository.GetByZoneIdAsync(zoneId, cancellationToken);
            if (emitterConfig != null)
            {
                totalFlowRate += emitterConfig.TotalZoneFlowRateLitersPerMinute;
            }
            else
            {
                _logger.LogWarning(
                    "No emitter configuration found for zone {ZoneId}, cannot calculate flow rate",
                    zoneId);
            }
        }

        return totalFlowRate;
    }

    public async Task<FlowRateCheckResult> CheckFlowRateAsync(
        Guid siteId,
        IEnumerable<Guid> zoneIds,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await GetCurrentFlowRateSnapshotAsync(siteId, cancellationToken);
        var requestedFlowRate = await CalculateZonesFlowRateAsync(siteId, zoneIds, cancellationToken);

        if (!snapshot.WouldExceedLimit(requestedFlowRate))
        {
            return FlowRateCheckResult.Allowed(
                snapshot.CurrentFlowRateLitersPerMinute,
                snapshot.CurrentFlowRateLitersPerMinute + requestedFlowRate,
                snapshot.EffectiveMaxFlowRateLitersPerMinute);
        }

        var excessFlow = snapshot.ExcessFlowRate(requestedFlowRate);
        var estimatedWaitTime = CalculateEstimatedWaitTime(snapshot, requestedFlowRate);

        return FlowRateCheckResult.Exceeded(
            snapshot.CurrentFlowRateLitersPerMinute,
            snapshot.CurrentFlowRateLitersPerMinute + requestedFlowRate,
            snapshot.EffectiveMaxFlowRateLitersPerMinute,
            excessFlow,
            estimatedWaitTime);
    }

    private TimeSpan CalculateEstimatedWaitTime(FlowRateSnapshot snapshot, decimal requestedFlowRate)
    {
        // Find the earliest ending zone that would free up enough capacity
        var sortedEndTimes = snapshot.ActiveZoneFlows
            .Where(z => z.ExpectedEndAt.HasValue)
            .OrderBy(z => z.ExpectedEndAt!.Value)
            .ToList();

        decimal freedFlowRate = 0;
        var requiredRelease = requestedFlowRate - snapshot.AvailableFlowRateLitersPerMinute;

        foreach (var zone in sortedEndTimes)
        {
            freedFlowRate += zone.FlowRateLitersPerMinute;
            if (freedFlowRate >= requiredRelease)
            {
                var waitUntil = zone.ExpectedEndAt!.Value;
                return waitUntil > DateTime.UtcNow 
                    ? waitUntil - DateTime.UtcNow 
                    : TimeSpan.Zero;
            }
        }

        // If we can't determine, estimate based on average run duration
        return TimeSpan.FromMinutes(5);
    }
}

/// <summary>
/// Result of a flow rate check
/// </summary>
public sealed record FlowRateCheckResult
{
    public bool IsAllowed { get; init; }
    public decimal CurrentFlowRateLitersPerMinute { get; init; }
    public decimal ProjectedFlowRateLitersPerMinute { get; init; }
    public decimal EffectiveMaxFlowRateLitersPerMinute { get; init; }
    public decimal? ExcessFlowRateLitersPerMinute { get; init; }
    public TimeSpan? EstimatedWaitTime { get; init; }
    public string Message { get; init; } = string.Empty;

    public static FlowRateCheckResult Allowed(
        decimal currentFlow,
        decimal projectedFlow,
        decimal maxFlow)
    {
        return new FlowRateCheckResult
        {
            IsAllowed = true,
            CurrentFlowRateLitersPerMinute = currentFlow,
            ProjectedFlowRateLitersPerMinute = projectedFlow,
            EffectiveMaxFlowRateLitersPerMinute = maxFlow,
            Message = "Flow rate within limits"
        };
    }

    public static FlowRateCheckResult Exceeded(
        decimal currentFlow,
        decimal projectedFlow,
        decimal maxFlow,
        decimal excessFlow,
        TimeSpan estimatedWait)
    {
        return new FlowRateCheckResult
        {
            IsAllowed = false,
            CurrentFlowRateLitersPerMinute = currentFlow,
            ProjectedFlowRateLitersPerMinute = projectedFlow,
            EffectiveMaxFlowRateLitersPerMinute = maxFlow,
            ExcessFlowRateLitersPerMinute = excessFlow,
            EstimatedWaitTime = estimatedWait,
            Message = $"Would exceed flow rate limit by {excessFlow:F2} L/min. Est. wait: {estimatedWait.TotalMinutes:F0} min"
        };
    }
}



