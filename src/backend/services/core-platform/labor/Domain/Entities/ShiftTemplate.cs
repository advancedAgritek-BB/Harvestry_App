using Harvestry.Labor.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class ShiftTemplate : AggregateRoot<Guid>
{
    private ShiftTemplate(Guid id) : base(id) { }

    private ShiftTemplate(
        Guid id,
        Guid siteId,
        string role,
        TimeSpan startTime,
        TimeSpan endTime,
        int headcount,
        string? roomCode,
        IEnumerable<string>? requiredSkills) : base(id)
    {
        SiteId = siteId;
        Role = role.Trim();
        StartTime = startTime;
        EndTime = endTime;
        Headcount = headcount;
        RoomCode = roomCode;
        RequiredSkills = requiredSkills?.ToList() ?? new List<string>();
    }

    public Guid SiteId { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int Headcount { get; private set; }
    public string? RoomCode { get; private set; }
    public IReadOnlyCollection<string> RequiredSkills { get; private set; } = Array.Empty<string>();
    public bool IsRecurring { get; private set; }
    public DayOfWeek[]? DaysOfWeek { get; private set; }
    public SchedulingDemandType DemandType { get; private set; }
    public string? DemandReferenceId { get; private set; }

    public static ShiftTemplate Create(
        Guid siteId,
        string role,
        TimeSpan startTime,
        TimeSpan endTime,
        int headcount,
        string? roomCode,
        IEnumerable<string>? requiredSkills,
        SchedulingDemandType demandType,
        string? demandReferenceId)
    {
        return new ShiftTemplate(
            Guid.NewGuid(),
            siteId,
            role,
            startTime,
            endTime,
            headcount,
            roomCode,
            requiredSkills)
        {
            DemandType = demandType,
            DemandReferenceId = demandReferenceId
        };
    }
}



