using Harvestry.Labor.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class TimeEntry : AggregateRoot<Guid>
{
    private TimeEntry(Guid id) : base(id) { }

    private TimeEntry(
        Guid id,
        Guid siteId,
        Guid employeeId,
        Guid? shiftAssignmentId,
        DateTime clockInUtc,
        string source,
        string? taskReference) : base(id)
    {
        SiteId = siteId;
        EmployeeId = employeeId;
        ShiftAssignmentId = shiftAssignmentId;
        ClockInUtc = clockInUtc;
        Source = source;
        TaskReference = taskReference;
        Status = TimeEntryStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? ShiftAssignmentId { get; private set; }
    public DateTime ClockInUtc { get; private set; }
    public DateTime? ClockOutUtc { get; private set; }
    public DateTime? BreakStartUtc { get; private set; }
    public DateTime? BreakEndUtc { get; private set; }
    public string Source { get; private set; } = "web";
    public string? TaskReference { get; private set; }
    public TimeEntryStatus Status { get; private set; }
    public decimal? CalculatedHours { get; private set; }
    public decimal? CalculatedCost { get; private set; }
    public string? ExceptionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    public static TimeEntry Start(
        Guid siteId,
        Guid employeeId,
        Guid? shiftAssignmentId,
        DateTime clockInUtc,
        string source,
        string? taskReference)
    {
        return new TimeEntry(Guid.NewGuid(), siteId, employeeId, shiftAssignmentId, clockInUtc, source, taskReference);
    }

    public void ClockOut(DateTime clockOutUtc, decimal calculatedHours, decimal calculatedCost)
    {
        ClockOutUtc = clockOutUtc;
        CalculatedHours = calculatedHours;
        CalculatedCost = calculatedCost;
    }

    public void StartBreak(DateTime breakStartUtc) => BreakStartUtc = breakStartUtc;

    public void EndBreak(DateTime breakEndUtc) => BreakEndUtc = breakEndUtc;

    public void Approve(Guid approvedBy)
    {
        Status = TimeEntryStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void FlagException(string reason)
    {
        Status = TimeEntryStatus.Exception;
        ExceptionReason = reason;
    }
}


