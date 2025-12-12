using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

public interface IHRISAdapter
{
    Task<IEnumerable<Employee>> PullEmployeesAsync(Guid siteId, CancellationToken ct);
    Task PushTimecardsAsync(Guid siteId, IEnumerable<TimeEntry> entries, CancellationToken ct);
    Task SyncCompensationAsync(Guid siteId, CancellationToken ct);
}



