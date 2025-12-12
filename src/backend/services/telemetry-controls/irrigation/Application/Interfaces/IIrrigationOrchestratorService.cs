using Harvestry.Irrigation.Domain.Entities;
using Harvestry.Irrigation.Domain.Enums;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Core service for orchestrating irrigation operations.
/// Coordinates runs, commands, and safety checks.
/// </summary>
public interface IIrrigationOrchestratorService
{
    /// <summary>
    /// Starts an irrigation run for a program
    /// </summary>
    Task<IrrigationRun> StartRunAsync(
        Guid programId,
        Guid? scheduleId,
        Guid? initiatedByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts a running irrigation
    /// </summary>
    Task<bool> AbortRunAsync(
        Guid runId,
        Guid? userId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a running irrigation
    /// </summary>
    Task<bool> PauseRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused irrigation
    /// </summary>
    Task<bool> ResumeRunAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a run
    /// </summary>
    Task<IrrigationRun?> GetRunStatusAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active runs for a site
    /// </summary>
    Task<IReadOnlyList<IrrigationRun>> GetActiveRunsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues a device command
    /// </summary>
    Task<DeviceCommand> EnqueueCommandAsync(
        Guid siteId,
        Guid equipmentId,
        CommandType commandType,
        object payload,
        Guid? runId = null,
        CommandPriority priority = CommandPriority.Normal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an emergency stop command
    /// </summary>
    Task EmergencyStopAsync(
        Guid siteId,
        Guid? groupId,
        string reason,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for evaluating safety interlocks
/// </summary>
public interface IInterlockEvaluationService
{
    /// <summary>
    /// Evaluates all interlocks before starting a run
    /// </summary>
    Task<InterlockCheckResult> EvaluatePreRunInterlocksAsync(
        Guid siteId,
        Guid groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates interlocks during a running irrigation
    /// </summary>
    Task<InterlockCheckResult> EvaluateRunningInterlocksAsync(
        Guid runId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks a specific interlock type
    /// </summary>
    Task<InterlockCheckResult> CheckInterlockAsync(
        Guid siteId,
        InterlockType interlockType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an interlock trip event
    /// </summary>
    Task<InterlockEvent> RecordInterlockTripAsync(
        Guid siteId,
        InterlockType interlockType,
        string details,
        Guid? runId = null,
        Guid? groupId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears an interlock
    /// </summary>
    Task<bool> ClearInterlockAsync(
        Guid interlockEventId,
        Guid? userId,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active interlocks for a site
    /// </summary>
    Task<IReadOnlyList<InterlockEvent>> GetActiveInterlocksAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an interlock evaluation
/// </summary>
public sealed record InterlockCheckResult
{
    public bool IsAllowed { get; init; }
    public InterlockType? BlockingInterlock { get; init; }
    public string? BlockingReason { get; init; }
    public IReadOnlyList<InterlockWarning> Warnings { get; init; } = Array.Empty<InterlockWarning>();

    public static InterlockCheckResult Allowed() => new() { IsAllowed = true };

    public static InterlockCheckResult Blocked(InterlockType type, string reason) => new()
    {
        IsAllowed = false,
        BlockingInterlock = type,
        BlockingReason = reason
    };

    public static InterlockCheckResult AllowedWithWarnings(IReadOnlyList<InterlockWarning> warnings) => new()
    {
        IsAllowed = true,
        Warnings = warnings
    };
}

/// <summary>
/// Warning from interlock evaluation (not blocking, but notable)
/// </summary>
public sealed record InterlockWarning(
    InterlockType Type,
    string Message,
    double? CurrentValue,
    double? ThresholdValue);
