using Harvestry.Labor.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class SchedulingDemand : AggregateRoot<Guid>
{
    private SchedulingDemand(Guid id) : base(id) { }

    private SchedulingDemand(
        Guid id,
        Guid siteId,
        SchedulingDemandType demandType,
        string referenceId,
        DateOnly startDate,
        DateOnly endDate,
        int requiredHeadcount,
        IEnumerable<string>? requiredSkills) : base(id)
    {
        SiteId = siteId;
        DemandType = demandType;
        ReferenceId = referenceId;
        StartDate = startDate;
        EndDate = endDate;
        RequiredHeadcount = requiredHeadcount;
        RequiredSkills = requiredSkills?.ToList() ?? new List<string>();
    }

    public Guid SiteId { get; private set; }
    public SchedulingDemandType DemandType { get; private set; }
    public string ReferenceId { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int RequiredHeadcount { get; private set; }
    public IReadOnlyCollection<string> RequiredSkills { get; private set; } = Array.Empty<string>();

    public static SchedulingDemand Create(
        Guid siteId,
        SchedulingDemandType demandType,
        string referenceId,
        DateOnly startDate,
        DateOnly endDate,
        int requiredHeadcount,
        IEnumerable<string>? requiredSkills)
    {
        return new SchedulingDemand(
            Guid.NewGuid(),
            siteId,
            demandType,
            referenceId,
            startDate,
            endDate,
            requiredHeadcount,
            requiredSkills);
    }
}



