using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskLibraryRepository
{
    Task AddAsync(TaskLibraryItem item, CancellationToken cancellationToken);
    Task<TaskLibraryItem?> GetByIdAsync(Guid orgId, Guid itemId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskLibraryItem>> GetByOrgAsync(Guid orgId, bool? activeOnly, CancellationToken cancellationToken);
    Task UpdateAsync(TaskLibraryItem item, CancellationToken cancellationToken);
    Task DeleteAsync(Guid orgId, Guid itemId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

