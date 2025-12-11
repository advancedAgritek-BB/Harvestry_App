using Harvestry.Genetics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Service for generating crop steering recommendations based on current telemetry
/// and configured steering profiles. Compares sensor readings against targets
/// and suggests adjustments to achieve vegetative or generative steering goals.
/// </summary>
public interface ICropSteeringSuggestionService
{
    /// <summary>
    /// Evaluate current conditions and generate steering suggestions for a zone.
    /// </summary>
    Task<IReadOnlyList<SteeringSuggestion>> EvaluateAsync(
        Guid zoneId,
        SteeringMode targetMode,
        DailyPhase currentPhase,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determine the current daily phase based on lights-on/off schedule.
    /// </summary>
    Task<DailyPhase> GetCurrentPhaseAsync(
        Guid zoneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the effective steering profile for a zone (strain-specific or site default).
    /// </summary>
    Task<SteeringProfileSummary?> GetEffectiveProfileAsync(
        Guid zoneId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of crop steering suggestion service.
/// </summary>
public sealed class CropSteeringSuggestionService : ICropSteeringSuggestionService
{
    private readonly ICropSteeringProfileRepository _profileRepository;
    private readonly ITelemetryReadingService _telemetryService;
    private readonly IZoneScheduleService _scheduleService;
    private readonly ILogger<CropSteeringSuggestionService> _logger;

    public CropSteeringSuggestionService(
        ICropSteeringProfileRepository profileRepository,
        ITelemetryReadingService telemetryService,
        IZoneScheduleService scheduleService,
        ILogger<CropSteeringSuggestionService> logger)
    {
        _profileRepository = profileRepository;
        _telemetryService = telemetryService;
        _scheduleService = scheduleService;
        _logger = logger;
    }

    public async Task<DailyPhase> GetCurrentPhaseAsync(
        Guid zoneId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleService.GetZoneScheduleAsync(zoneId, cancellationToken);
        if (schedule == null)
        {
            _logger.LogWarning("No schedule found for zone {ZoneId}, defaulting to P2", zoneId);
            return DailyPhase.P2Maintenance;
        }

        var now = DateTime.UtcNow;
        var currentHour = now.Hour + (now.Minute / 60.0);

        // Check if lights are off
        if (currentHour < schedule.LightsOnHour || currentHour >= schedule.LightsOffHour)
        {
            return DailyPhase.Night;
        }

        // Calculate hours since lights on
        var hoursSinceLightsOn = currentHour - schedule.LightsOnHour;
        var totalLightHours = schedule.LightsOffHour - schedule.LightsOnHour;

        // P1 Ramp: First 2-4 hours (configurable, default ~25% of light period)
        var p1Duration = schedule.P1DurationHours ?? (totalLightHours * 0.25);
        if (hoursSinceLightsOn < p1Duration)
        {
            return DailyPhase.P1Ramp;
        }

        // P3 Dryback: Last 2-4 hours (configurable, default ~25% of light period)
        var p3Duration = schedule.P3DurationHours ?? (totalLightHours * 0.25);
        var p3StartHour = totalLightHours - p3Duration;
        if (hoursSinceLightsOn >= p3StartHour)
        {
            return DailyPhase.P3Dryback;
        }

        // P2 Maintenance: Middle period
        return DailyPhase.P2Maintenance;
    }

    public async Task<SteeringProfileSummary?> GetEffectiveProfileAsync(
        Guid zoneId,
        CancellationToken cancellationToken = default)
    {
        // Get zone to determine site and possibly strain
        var zoneContext = await _profileRepository.GetZoneContextAsync(zoneId, cancellationToken);
        if (zoneContext == null)
        {
            return null;
        }

        // Try strain-specific profile first, fall back to site default
        var profile = await _profileRepository.GetEffectiveProfileAsync(
            zoneContext.SiteId,
            zoneContext.StrainId,
            cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new SteeringProfileSummary(
            profile.Id,
            profile.Name,
            profile.TargetMode,
            profile.IsSiteDefault,
            zoneContext.StrainId.HasValue ? zoneContext.StrainName : null);
    }

    public async Task<IReadOnlyList<SteeringSuggestion>> EvaluateAsync(
        Guid zoneId,
        SteeringMode targetMode,
        DailyPhase currentPhase,
        CancellationToken cancellationToken = default)
    {
        var suggestions = new List<SteeringSuggestion>();

        // Get current telemetry readings
        var readings = await _telemetryService.GetLatestReadingsAsync(zoneId, cancellationToken);
        if (readings == null || readings.Count == 0)
        {
            _logger.LogWarning("No telemetry readings available for zone {ZoneId}", zoneId);
            return suggestions;
        }

        // Get reference steering levers
        var levers = SteeringLever.DefaultLevers;

        // Evaluate each lever against target mode
        foreach (var lever in levers)
        {
            var suggestion = EvaluateLever(lever, readings, targetMode, currentPhase);
            if (suggestion != null)
            {
                suggestions.Add(suggestion);
            }
        }

        // Evaluate irrigation signals if in active phase
        if (currentPhase != DailyPhase.Night)
        {
            var irrigationSuggestions = EvaluateIrrigationSignals(readings, targetMode, currentPhase);
            suggestions.AddRange(irrigationSuggestions);
        }

        // Prioritize and return top suggestions
        return suggestions
            .OrderByDescending(s => s.Priority)
            .ThenByDescending(s => s.ImpactScore)
            .Take(5)
            .ToList();
    }

    private SteeringSuggestion? EvaluateLever(
        SteeringLever lever,
        TelemetryReadings readings,
        SteeringMode targetMode,
        DailyPhase currentPhase)
    {
        // Get current value for this metric
        var currentValue = GetCurrentValue(lever.MetricName, readings);
        if (!currentValue.HasValue)
        {
            return null; // No reading for this metric
        }

        // Determine target range based on steering mode
        var (targetMin, targetMax, trend) = targetMode switch
        {
            SteeringMode.Vegetative => (lever.VegetativeMinValue, lever.VegetativeMaxValue, lever.VegetativeTrend),
            SteeringMode.Generative => (lever.GenerativeMinValue, lever.GenerativeMaxValue, lever.GenerativeTrend),
            _ => (lever.VegetativeMinValue, lever.GenerativeMaxValue, "Balanced") // Use full range
        };

        // Skip if no numeric targets defined
        if (!targetMin.HasValue || !targetMax.HasValue)
        {
            return null;
        }

        // Check if current value is outside target range
        var value = currentValue.Value;
        var isWithinRange = value >= targetMin.Value && value <= targetMax.Value;

        if (isWithinRange)
        {
            return null; // No suggestion needed
        }

        // Determine direction and severity
        var isBelowRange = value < targetMin.Value;
        var deviation = isBelowRange 
            ? targetMin.Value - value 
            : value - targetMax.Value;
        var deviationPercent = deviation / ((targetMax.Value + targetMin.Value) / 2) * 100;

        // Determine opposing mode (what the current value is trending toward)
        var currentTrend = DetermineCurrentTrend(lever, value);
        var opposingMode = currentTrend == lever.VegetativeTrend 
            ? SteeringMode.Vegetative 
            : SteeringMode.Generative;

        // Generate suggestion
        var priority = deviationPercent > 20 
            ? SteeringSuggestionPriority.High 
            : deviationPercent > 10 
                ? SteeringSuggestionPriority.Medium 
                : SteeringSuggestionPriority.Low;

        var adjustmentDirection = isBelowRange ? "increase" : "decrease";
        
        return new SteeringSuggestion
        {
            Type = SteeringSuggestionType.LeverAdjustment,
            MetricName = lever.MetricName,
            Title = $"Adjust {lever.MetricName} for {targetMode} steering",
            Description = $"Current {lever.MetricName} is {value:F1} {lever.Unit} " +
                         $"(trending {opposingMode}). For {targetMode} steering in {FormatPhase(currentPhase)}, " +
                         $"{adjustmentDirection} to {targetMin.Value:F1}-{targetMax.Value:F1} {lever.Unit}.",
            CurrentValue = $"{value:F1} {lever.Unit}",
            TargetRange = $"{targetMin.Value:F1}-{targetMax.Value:F1} {lever.Unit}",
            SuggestedAction = trend,
            Priority = priority,
            ImpactScore = (int)Math.Min(100, deviationPercent * 2),
            Phase = currentPhase,
            RelatedStreamType = GetStreamTypeForMetric(lever.MetricName)
        };
    }

    private IEnumerable<SteeringSuggestion> EvaluateIrrigationSignals(
        TelemetryReadings readings,
        SteeringMode targetMode,
        DailyPhase currentPhase)
    {
        var suggestions = new List<SteeringSuggestion>();
        var signals = IrrigationSignal.DefaultSignals;

        // Filter to applicable signals for current phase
        var applicableSignals = signals.Where(s => 
            s.ApplicablePhase == "All" || 
            s.ApplicablePhase.Contains(FormatPhaseShort(currentPhase)));

        foreach (var signal in applicableSignals)
        {
            // For dryback signals, check VWC trends
            if (signal.SignalName == "DailyDryback" || signal.SignalName == "IntershotDryback")
            {
                var drybackSuggestion = EvaluateDrybackSignal(signal, readings, targetMode, currentPhase);
                if (drybackSuggestion != null)
                {
                    suggestions.Add(drybackSuggestion);
                }
            }
        }

        return suggestions;
    }

    private SteeringSuggestion? EvaluateDrybackSignal(
        IrrigationSignal signal,
        TelemetryReadings readings,
        SteeringMode targetMode,
        DailyPhase currentPhase)
    {
        // Get dryback percentage from readings if available
        if (!readings.DrybackPercent.HasValue)
        {
            return null;
        }

        var currentDryback = readings.DrybackPercent.Value;
        var (targetMin, targetMax) = targetMode switch
        {
            SteeringMode.Vegetative => (signal.VegetativeMinValue, signal.VegetativeMaxValue),
            SteeringMode.Generative => (signal.GenerativeMinValue, signal.GenerativeMaxValue),
            _ => (signal.VegetativeMinValue, signal.GenerativeMaxValue)
        };

        if (!targetMin.HasValue || !targetMax.HasValue)
        {
            return null;
        }

        var isWithinRange = currentDryback >= targetMin.Value && currentDryback <= targetMax.Value;
        if (isWithinRange)
        {
            return null;
        }

        var isBelowTarget = currentDryback < targetMin.Value;
        var adjustmentAction = isBelowTarget 
            ? "reduce irrigation frequency to increase dryback" 
            : "increase irrigation frequency to reduce dryback";

        var targetDescription = targetMode == SteeringMode.Vegetative 
            ? signal.VegetativeValue 
            : signal.GenerativeValue;

        return new SteeringSuggestion
        {
            Type = SteeringSuggestionType.IrrigationAdjustment,
            MetricName = signal.SignalName,
            Title = $"Adjust {signal.DisplayName} for {targetMode} steering",
            Description = $"Current {signal.DisplayName.ToLower()} is {currentDryback:F1}% " +
                         $"(target: {targetDescription} for {targetMode}). Consider: {adjustmentAction}.",
            CurrentValue = $"{currentDryback:F1}%",
            TargetRange = targetDescription,
            SuggestedAction = adjustmentAction,
            Priority = SteeringSuggestionPriority.Medium,
            ImpactScore = 50,
            Phase = currentPhase,
            RelatedStreamType = 20 // StreamType.SoilMoisture
        };
    }

    private decimal? GetCurrentValue(string metricName, TelemetryReadings readings)
    {
        return metricName switch
        {
            "SubstrateEC" => readings.SubstrateEc,
            "VWC" => readings.Vwc,
            "VPD" => readings.Vpd,
            "Temperature" => readings.Temperature,
            _ => null
        };
    }

    private string DetermineCurrentTrend(SteeringLever lever, decimal value)
    {
        // If value is in vegetative range, it's trending vegetative
        if (lever.VegetativeMinValue.HasValue && lever.VegetativeMaxValue.HasValue)
        {
            if (value >= lever.VegetativeMinValue.Value && value <= lever.VegetativeMaxValue.Value)
            {
                return lever.VegetativeTrend;
            }
        }

        // If value is in generative range, it's trending generative
        if (lever.GenerativeMinValue.HasValue && lever.GenerativeMaxValue.HasValue)
        {
            if (value >= lever.GenerativeMinValue.Value && value <= lever.GenerativeMaxValue.Value)
            {
                return lever.GenerativeTrend;
            }
        }

        // Default based on lever direction
        return lever.VegetativeTrend;
    }

    private int? GetStreamTypeForMetric(string metricName)
    {
        return metricName switch
        {
            "SubstrateEC" => 22, // StreamType.SoilEc
            "VWC" => 20,         // StreamType.SoilMoisture
            "VPD" => 4,          // StreamType.Vpd
            "Temperature" => 1,  // StreamType.Temperature
            _ => null
        };
    }

    private static string FormatPhase(DailyPhase phase)
    {
        return phase switch
        {
            DailyPhase.P1Ramp => "P1 (Ramp)",
            DailyPhase.P2Maintenance => "P2 (Maintenance)",
            DailyPhase.P3Dryback => "P3 (Dryback)",
            DailyPhase.Night => "Night",
            _ => phase.ToString()
        };
    }

    private static string FormatPhaseShort(DailyPhase phase)
    {
        return phase switch
        {
            DailyPhase.P1Ramp => "P1",
            DailyPhase.P2Maintenance => "P2",
            DailyPhase.P3Dryback => "P3",
            _ => ""
        };
    }
}

#region Supporting Types

/// <summary>
/// A steering suggestion generated by evaluating telemetry against targets.
/// </summary>
public sealed class SteeringSuggestion
{
    public SteeringSuggestionType Type { get; init; }
    public string MetricName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CurrentValue { get; init; } = string.Empty;
    public string TargetRange { get; init; } = string.Empty;
    public string SuggestedAction { get; init; } = string.Empty;
    public SteeringSuggestionPriority Priority { get; init; }
    public int ImpactScore { get; init; }
    public DailyPhase Phase { get; init; }
    public int? RelatedStreamType { get; init; }
}

public enum SteeringSuggestionType
{
    LeverAdjustment,
    IrrigationAdjustment,
    PhaseTransition,
    ProfileRecommendation
}

public enum SteeringSuggestionPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Summary of an effective steering profile for display.
/// </summary>
public sealed record SteeringProfileSummary(
    Guid ProfileId,
    string ProfileName,
    SteeringMode TargetMode,
    bool IsSiteDefault,
    string? StrainName);

/// <summary>
/// Current telemetry readings for evaluation.
/// </summary>
public sealed class TelemetryReadings
{
    public decimal? Temperature { get; init; }
    public decimal? Humidity { get; init; }
    public decimal? Vpd { get; init; }
    public decimal? Co2 { get; init; }
    public decimal? Vwc { get; init; }
    public decimal? SubstrateEc { get; init; }
    public decimal? SubstratePh { get; init; }
    public decimal? DrybackPercent { get; init; }
    public DateTime ReadingTime { get; init; }
}

/// <summary>
/// Zone context for profile resolution.
/// </summary>
public sealed record ZoneContext(
    Guid SiteId,
    Guid ZoneId,
    Guid? StrainId,
    string? StrainName);

/// <summary>
/// Zone schedule information for phase calculation.
/// </summary>
public sealed record ZoneSchedule(
    int LightsOnHour,
    int LightsOffHour,
    double? P1DurationHours,
    double? P3DurationHours);

#endregion

#region Repository Interfaces

/// <summary>
/// Repository for crop steering profiles.
/// </summary>
public interface ICropSteeringProfileRepository
{
    Task<ZoneContext?> GetZoneContextAsync(Guid zoneId, CancellationToken cancellationToken = default);
    
    Task<CropSteeringProfileDto?> GetEffectiveProfileAsync(
        Guid siteId,
        Guid? strainId,
        CancellationToken cancellationToken = default);
    
    Task<CropSteeringProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<CropSteeringProfileDto>> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for crop steering profile data.
/// </summary>
public sealed record CropSteeringProfileDto(
    Guid Id,
    Guid SiteId,
    Guid? StrainId,
    string Name,
    SteeringMode TargetMode,
    SteeringConfiguration Configuration,
    bool IsActive)
{
    public bool IsSiteDefault => !StrainId.HasValue;
}

/// <summary>
/// Service for retrieving telemetry readings.
/// </summary>
public interface ITelemetryReadingService
{
    Task<TelemetryReadings?> GetLatestReadingsAsync(Guid zoneId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for retrieving zone schedules.
/// </summary>
public interface IZoneScheduleService
{
    Task<ZoneSchedule?> GetZoneScheduleAsync(Guid zoneId, CancellationToken cancellationToken = default);
}

#endregion
