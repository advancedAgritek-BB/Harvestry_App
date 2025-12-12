using Harvestry.Irrigation.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Represents an execution instance of an irrigation program.
/// Tracks the complete lifecycle from queue → run → complete/abort.
/// </summary>
public sealed class IrrigationRun : Entity<Guid>
{
    private IrrigationRun(Guid id) : base(id) { }

    public Guid SiteId { get; private set; }
    public Guid ProgramId { get; private set; }
    public Guid GroupId { get; private set; }
    public Guid? ScheduleId { get; private set; }
    public RunStatus Status { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int TotalSteps { get; private set; }
    public int CompletedSteps { get; private set; }
    public Guid? AbortedByUserId { get; private set; }
    public string? AbortReason { get; private set; }
    public InterlockType? InterlockType { get; private set; }
    public string? InterlockDetails { get; private set; }
    public string? FaultMessage { get; private set; }
    public Guid? InitiatedByUserId { get; private set; }
    public string InitiatedBy { get; private set; } = "system";

    public static IrrigationRun Create(
        Guid siteId,
        Guid programId,
        Guid groupId,
        int totalSteps,
        Guid? scheduleId = null,
        Guid? initiatedByUserId = null,
        string initiatedBy = "system")
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (programId == Guid.Empty)
            throw new ArgumentException("Program ID is required", nameof(programId));
        if (groupId == Guid.Empty)
            throw new ArgumentException("Group ID is required", nameof(groupId));
        if (totalSteps < 1)
            throw new ArgumentException("Total steps must be at least 1", nameof(totalSteps));

        return new IrrigationRun(Guid.NewGuid())
        {
            SiteId = siteId,
            ProgramId = programId,
            GroupId = groupId,
            ScheduleId = scheduleId,
            Status = RunStatus.Queued,
            CurrentStepIndex = 0,
            TotalSteps = totalSteps,
            CompletedSteps = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            InitiatedByUserId = initiatedByUserId,
            InitiatedBy = initiatedBy
        };
    }

    public static IrrigationRun FromPersistence(
        Guid id,
        Guid siteId,
        Guid programId,
        Guid groupId,
        Guid? scheduleId,
        RunStatus status,
        int currentStepIndex,
        DateTimeOffset createdAt,
        DateTimeOffset? startedAt,
        DateTimeOffset? completedAt,
        int totalSteps,
        int completedSteps,
        Guid? abortedByUserId,
        string? abortReason,
        InterlockType? interlockType,
        string? interlockDetails,
        string? faultMessage,
        Guid? initiatedByUserId,
        string initiatedBy)
    {
        return new IrrigationRun(id)
        {
            SiteId = siteId,
            ProgramId = programId,
            GroupId = groupId,
            ScheduleId = scheduleId,
            Status = status,
            CurrentStepIndex = currentStepIndex,
            CreatedAt = createdAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            TotalSteps = totalSteps,
            CompletedSteps = completedSteps,
            AbortedByUserId = abortedByUserId,
            AbortReason = abortReason,
            InterlockType = interlockType,
            InterlockDetails = interlockDetails,
            FaultMessage = faultMessage,
            InitiatedByUserId = initiatedByUserId,
            InitiatedBy = initiatedBy
        };
    }

    public void Start()
    {
        if (Status != RunStatus.Queued)
            throw new InvalidOperationException($"Cannot start run in status {Status}");

        Status = RunStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void AdvanceToStep(int stepIndex)
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot advance step when status is {Status}");
        if (stepIndex < 0 || stepIndex >= TotalSteps)
            throw new ArgumentOutOfRangeException(nameof(stepIndex));

        CurrentStepIndex = stepIndex;
    }

    public void CompleteStep()
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot complete step when status is {Status}");

        CompletedSteps++;
    }

    public void Complete()
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot complete run in status {Status}");

        Status = RunStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Abort(Guid? userId, string reason)
    {
        if (Status != RunStatus.Queued && Status != RunStatus.Running && Status != RunStatus.Paused)
            throw new InvalidOperationException($"Cannot abort run in status {Status}");

        Status = RunStatus.Aborted;
        CompletedAt = DateTimeOffset.UtcNow;
        AbortedByUserId = userId;
        AbortReason = reason;
    }

    public void TripInterlock(InterlockType interlockType, string details)
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot trip interlock when status is {Status}");

        Status = RunStatus.InterlockTripped;
        CompletedAt = DateTimeOffset.UtcNow;
        InterlockType = interlockType;
        InterlockDetails = details;
    }

    public void Fault(string message)
    {
        if (Status != RunStatus.Running && Status != RunStatus.Queued)
            throw new InvalidOperationException($"Cannot fault run in status {Status}");

        Status = RunStatus.Faulted;
        CompletedAt = DateTimeOffset.UtcNow;
        FaultMessage = message;
    }

    public void Pause()
    {
        if (Status != RunStatus.Running)
            throw new InvalidOperationException($"Cannot pause run in status {Status}");

        Status = RunStatus.Paused;
    }

    public void Resume()
    {
        if (Status != RunStatus.Paused)
            throw new InvalidOperationException($"Cannot resume run in status {Status}");

        Status = RunStatus.Running;
    }

    public bool IsTerminal => Status is RunStatus.Completed
        or RunStatus.Aborted
        or RunStatus.InterlockTripped
        or RunStatus.Faulted;

    public bool IsActive => Status is RunStatus.Running or RunStatus.Paused;

    public TimeSpan? Duration => StartedAt.HasValue
        ? (CompletedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value
        : null;

    public double ProgressPercent => TotalSteps > 0 ? (CompletedSteps * 100.0 / TotalSteps) : 0;
}
