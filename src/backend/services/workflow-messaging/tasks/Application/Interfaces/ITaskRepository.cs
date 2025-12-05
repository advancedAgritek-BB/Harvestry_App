using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;
using TaskStatusEnum = Harvestry.Tasks.Domain.Enums.TaskStatus;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(DomainTask task, CancellationToken cancellationToken);
    Task<DomainTask?> GetByIdAsync(Guid siteId, Guid taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainTask>> GetBySiteAsync(Guid siteId, TaskStatusEnum? statusFilter, Guid? assignedToUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainTask>> GetByIdsAsync(Guid siteId, IReadOnlyCollection<Guid> taskIds, CancellationToken cancellationToken);
    Task UpdateAsync(DomainTask task, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainTask>> GetOverdueAsync(Guid siteId, DateTimeOffset referenceTime, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainTask>> GetOverdueAsync(DateTimeOffset referenceTime, int batchSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<DomainTask>> GetBlockedWithDependenciesAsync(int batchSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
