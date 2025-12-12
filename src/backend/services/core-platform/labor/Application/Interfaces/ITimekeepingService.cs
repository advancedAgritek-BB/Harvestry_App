using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface ITimekeepingService
{
    Task<TimeEntry> ClockInAsync(Guid siteId, Guid employeeId, Guid? shiftAssignmentId, string source, string? taskRef, CancellationToken ct);
    Task<TimeEntry> ClockOutAsync(Guid timeEntryId, DateTime clockOutUtc, CancellationToken ct);
    Task<TimeEntry> ApproveAsync(Guid timeEntryId, Guid managerId, CancellationToken ct);
    Task FlagExceptionAsync(Guid timeEntryId, string reason, CancellationToken ct);
}



