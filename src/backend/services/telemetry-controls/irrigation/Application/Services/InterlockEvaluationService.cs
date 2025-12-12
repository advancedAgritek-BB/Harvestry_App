using Harvestry.Irrigation.Domain.Entities;
using Harvestry.Irrigation.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Service for evaluating safety interlocks before and during irrigation.
/// Implements the 7 core interlock types defined in FRP-06.
/// </summary>
public sealed class InterlockEvaluationService : IInterlockEvaluationService
{
    private readonly IInterlockEventRepository _interlockRepository;
    private readonly ITelemetryQueryService _telemetryService;
    private readonly IIrrigationSettingsRepository _settingsRepository;
    private readonly ILogger<InterlockEvaluationService> _logger;

    // Configurable thresholds - would come from settings in production
    private const double DEFAULT_EC_MIN = 0.5;
    private const double DEFAULT_EC_MAX = 3.5;
    private const double DEFAULT_PH_MIN = 5.0;
    private const double DEFAULT_PH_MAX = 7.0;
    private const int DEFAULT_TANK_LEVEL_MIN_PERCENT = 10;
    private const int DEFAULT_TELEMETRY_STALE_MINUTES = 5;

    public InterlockEvaluationService(
        IInterlockEventRepository interlockRepository,
        ITelemetryQueryService telemetryService,
        IIrrigationSettingsRepository settingsRepository,
        ILogger<InterlockEvaluationService> logger)
    {
        _interlockRepository = interlockRepository;
        _telemetryService = telemetryService;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<InterlockCheckResult> EvaluatePreRunInterlocksAsync(
        Guid siteId,
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<InterlockWarning>();

        // Check E-Stop
        var estopResult = await CheckEStopAsync(siteId, cancellationToken);
        if (!estopResult.IsAllowed) return estopResult;

        // Check Door
        var doorResult = await CheckDoorAsync(siteId, groupId, cancellationToken);
        if (!doorResult.IsAllowed) return doorResult;

        // Check Tank Level
        var tankResult = await CheckTankLevelAsync(siteId, cancellationToken);
        if (!tankResult.IsAllowed) return tankResult;
        warnings.AddRange(tankResult.Warnings);

        // Check EC/pH
        var ecPhResult = await CheckEcPhAsync(siteId, cancellationToken);
        if (!ecPhResult.IsAllowed) return ecPhResult;
        warnings.AddRange(ecPhResult.Warnings);

        // Check CO2 Lockout
        var co2Result = await CheckCo2LockoutAsync(siteId, cancellationToken);
        if (!co2Result.IsAllowed) return co2Result;

        // Check Telemetry Freshness
        var telemetryResult = await CheckTelemetryFreshnessAsync(siteId, cancellationToken);
        if (!telemetryResult.IsAllowed) return telemetryResult;

        _logger.LogDebug(
            "Pre-run interlock check passed for site {SiteId}, group {GroupId} with {WarningCount} warnings",
            siteId, groupId, warnings.Count);

        return warnings.Count > 0
            ? InterlockCheckResult.AllowedWithWarnings(warnings)
            : InterlockCheckResult.Allowed();
    }

    public async Task<InterlockCheckResult> EvaluateRunningInterlocksAsync(
        Guid runId,
        CancellationToken cancellationToken = default)
    {
        // Get run details to get site/group
        // For now, placeholder implementation
        return InterlockCheckResult.Allowed();
    }

    public async Task<InterlockCheckResult> CheckInterlockAsync(
        Guid siteId,
        InterlockType interlockType,
        CancellationToken cancellationToken = default)
    {
        return interlockType switch
        {
            InterlockType.EmergencyStop => await CheckEStopAsync(siteId, cancellationToken),
            InterlockType.DoorOpen => await CheckDoorAsync(siteId, null, cancellationToken),
            InterlockType.TankLevelLow => await CheckTankLevelAsync(siteId, cancellationToken),
            InterlockType.EcOutOfBounds => await CheckEcPhAsync(siteId, cancellationToken),
            InterlockType.PhOutOfBounds => await CheckEcPhAsync(siteId, cancellationToken),
            InterlockType.Co2Lockout => await CheckCo2LockoutAsync(siteId, cancellationToken),
            InterlockType.TelemetryStale => await CheckTelemetryFreshnessAsync(siteId, cancellationToken),
            _ => InterlockCheckResult.Allowed()
        };
    }

    public async Task<InterlockEvent> RecordInterlockTripAsync(
        Guid siteId,
        InterlockType interlockType,
        string details,
        Guid? runId = null,
        Guid? groupId = null,
        CancellationToken cancellationToken = default)
    {
        var interlockEvent = InterlockEvent.Create(
            siteId,
            interlockType,
            details,
            runId,
            groupId,
            requiresAcknowledgment: RequiresAcknowledgment(interlockType));

        await _interlockRepository.AddAsync(interlockEvent, cancellationToken);

        _logger.LogWarning(
            "Interlock tripped: {InterlockType} for site {SiteId}, run {RunId}: {Details}",
            interlockType, siteId, runId, details);

        return interlockEvent;
    }

    public async Task<bool> ClearInterlockAsync(
        Guid interlockEventId,
        Guid? userId,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var interlockEvent = await _interlockRepository.GetByIdAsync(interlockEventId, cancellationToken);
        if (interlockEvent == null || !interlockEvent.IsActive)
        {
            return false;
        }

        interlockEvent.Clear(userId, notes);
        await _interlockRepository.UpdateAsync(interlockEvent, cancellationToken);

        _logger.LogInformation(
            "Interlock cleared: {InterlockId} by user {UserId}",
            interlockEventId, userId);

        return true;
    }

    public async Task<IReadOnlyList<InterlockEvent>> GetActiveInterlocksAsync(
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        return await _interlockRepository.GetActiveBySiteIdAsync(siteId, cancellationToken);
    }

    private async Task<InterlockCheckResult> CheckEStopAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        // Query telemetry for E-Stop status
        var estopValue = await _telemetryService.GetLatestValueAsync(
            siteId, "estop", cancellationToken);

        if (estopValue?.Value == 1.0)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.EmergencyStop,
                "Emergency stop is active");
        }

        return InterlockCheckResult.Allowed();
    }

    private async Task<InterlockCheckResult> CheckDoorAsync(
        Guid siteId,
        Guid? groupId,
        CancellationToken cancellationToken)
    {
        // Query telemetry for door sensor status
        var doorValue = await _telemetryService.GetLatestValueAsync(
            siteId, "door_sensor", cancellationToken);

        if (doorValue?.Value == 1.0)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.DoorOpen,
                "Access door is open");
        }

        return InterlockCheckResult.Allowed();
    }

    private async Task<InterlockCheckResult> CheckTankLevelAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var tankLevel = await _telemetryService.GetLatestValueAsync(
            siteId, "tank_level", cancellationToken);

        if (tankLevel == null)
        {
            return InterlockCheckResult.AllowedWithWarnings(new[]
            {
                new InterlockWarning(
                    InterlockType.TankLevelLow,
                    "Tank level sensor not reporting",
                    null,
                    null)
            });
        }

        if (tankLevel.Value < DEFAULT_TANK_LEVEL_MIN_PERCENT)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.TankLevelLow,
                $"Tank level ({tankLevel.Value:F1}%) is below minimum ({DEFAULT_TANK_LEVEL_MIN_PERCENT}%)");
        }

        // Warning if getting low
        if (tankLevel.Value < 20)
        {
            return InterlockCheckResult.AllowedWithWarnings(new[]
            {
                new InterlockWarning(
                    InterlockType.TankLevelLow,
                    $"Tank level is low ({tankLevel.Value:F1}%)",
                    tankLevel.Value,
                    DEFAULT_TANK_LEVEL_MIN_PERCENT)
            });
        }

        return InterlockCheckResult.Allowed();
    }

    private async Task<InterlockCheckResult> CheckEcPhAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var warnings = new List<InterlockWarning>();

        var ec = await _telemetryService.GetLatestValueAsync(siteId, "ec", cancellationToken);
        var ph = await _telemetryService.GetLatestValueAsync(siteId, "ph", cancellationToken);

        // Check EC
        if (ec != null)
        {
            if (ec.Value < DEFAULT_EC_MIN || ec.Value > DEFAULT_EC_MAX)
            {
                return InterlockCheckResult.Blocked(
                    InterlockType.EcOutOfBounds,
                    $"EC ({ec.Value:F2} mS/cm) is out of bounds ({DEFAULT_EC_MIN}-{DEFAULT_EC_MAX} mS/cm)");
            }
        }

        // Check pH
        if (ph != null)
        {
            if (ph.Value < DEFAULT_PH_MIN || ph.Value > DEFAULT_PH_MAX)
            {
                return InterlockCheckResult.Blocked(
                    InterlockType.PhOutOfBounds,
                    $"pH ({ph.Value:F2}) is out of bounds ({DEFAULT_PH_MIN}-{DEFAULT_PH_MAX})");
            }
        }

        return warnings.Count > 0
            ? InterlockCheckResult.AllowedWithWarnings(warnings)
            : InterlockCheckResult.Allowed();
    }

    private async Task<InterlockCheckResult> CheckCo2LockoutAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var co2Active = await _telemetryService.GetLatestValueAsync(
            siteId, "co2_enrichment_active", cancellationToken);

        if (co2Active?.Value == 1.0)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.Co2Lockout,
                "CO₂ enrichment is active - irrigation locked out");
        }

        return InterlockCheckResult.Allowed();
    }

    private async Task<InterlockCheckResult> CheckTelemetryFreshnessAsync(
        Guid siteId,
        CancellationToken cancellationToken)
    {
        var lastReading = await _telemetryService.GetLastReadingTimeAsync(siteId, cancellationToken);

        if (!lastReading.HasValue)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.TelemetryStale,
                "No telemetry data available");
        }

        var staleness = DateTimeOffset.UtcNow - lastReading.Value;
        if (staleness.TotalMinutes > DEFAULT_TELEMETRY_STALE_MINUTES)
        {
            return InterlockCheckResult.Blocked(
                InterlockType.TelemetryStale,
                $"Telemetry data is stale ({staleness.TotalMinutes:F0} minutes old)");
        }

        return InterlockCheckResult.Allowed();
    }

    private static bool RequiresAcknowledgment(InterlockType type)
    {
        return type is InterlockType.EmergencyStop
            or InterlockType.EcOutOfBounds
            or InterlockType.PhOutOfBounds
            or InterlockType.FlowAnomaly;
    }
}

/// <summary>
/// Interface for querying telemetry data (implemented by telemetry service)
/// </summary>
public interface ITelemetryQueryService
{
    Task<TelemetryReading?> GetLatestValueAsync(Guid siteId, string streamType, CancellationToken cancellationToken = default);
    Task<DateTimeOffset?> GetLastReadingTimeAsync(Guid siteId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Simple telemetry reading
/// </summary>
public sealed record TelemetryReading(double Value, DateTimeOffset Timestamp);

/// <summary>
/// Repository interface for interlock events
/// </summary>
public interface IInterlockEventRepository
{
    Task<InterlockEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InterlockEvent>> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<InterlockEvent> AddAsync(InterlockEvent interlockEvent, CancellationToken cancellationToken = default);
    Task<InterlockEvent> UpdateAsync(InterlockEvent interlockEvent, CancellationToken cancellationToken = default);
}
