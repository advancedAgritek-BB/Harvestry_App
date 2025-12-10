using Harvestry.Labor.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class ShiftAssignment : AggregateRoot<Guid>
{
    private ShiftAssignment(Guid id) : base(id) { }

    private ShiftAssignment(
        Guid id,
        Guid siteId,
        Guid employeeId,
        Guid? shiftTemplateId,
        DateOnly shiftDate,
        TimeSpan startTime,
        TimeSpan endTime,
        ShiftStatus status,
        string? roomCode) : base(id)
    {
        SiteId = siteId;
        EmployeeId = employeeId;
        ShiftTemplateId = shiftTemplateId;
        ShiftDate = shiftDate;
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
        RoomCode = roomCode;
    }

    public Guid SiteId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid? ShiftTemplateId { get; private set; }
    public DateOnly ShiftDate { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public ShiftStatus Status { get; private set; }
    public string? RoomCode { get; private set; }
    public string? Task { get; private set; }

    public static ShiftAssignment Create(
        Guid siteId,
        Guid employeeId,
        Guid? shiftTemplateId,
        DateOnly shiftDate,
        TimeSpan startTime,
        TimeSpan endTime,
        string? roomCode)
    {
        return new ShiftAssignment(
            Guid.NewGuid(),
            siteId,
            employeeId,
            shiftTemplateId,
            shiftDate,
            startTime,
            endTime,
            ShiftStatus.Assigned,
            roomCode);
    }

    public void MarkInProgress() => Status = ShiftStatus.InProgress;

    public void Complete() => Status = ShiftStatus.Completed;

    public void Cancel(string? reason = null)
    {
        Status = ShiftStatus.Cancelled;
        Task = reason;
    }
}


