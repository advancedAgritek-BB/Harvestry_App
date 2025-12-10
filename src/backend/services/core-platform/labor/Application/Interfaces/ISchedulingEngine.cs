using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface ISchedulingEngine
{
    Task<ShiftAssignment> AssignAsync(Guid shiftTemplateId, Guid employeeId, DateOnly shiftDate, CancellationToken ct);
    Task<IEnumerable<ShiftAssignment>> AutoScheduleAsync(Guid siteId, DateOnly startDate, DateOnly endDate, CancellationToken ct);
    Task<IEnumerable<SchedulingDemand>> GetDemandAsync(Guid siteId, DateOnly startDate, DateOnly endDate, CancellationToken ct);
}


