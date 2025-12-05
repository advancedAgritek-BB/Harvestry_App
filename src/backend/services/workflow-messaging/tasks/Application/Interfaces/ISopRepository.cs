using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Entities;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ISopRepository
{
    Task AddAsync(StandardOperatingProcedure sop, CancellationToken cancellationToken);
    Task<StandardOperatingProcedure?> GetByIdAsync(Guid orgId, Guid sopId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StandardOperatingProcedure>> GetByOrgAsync(Guid orgId, bool? activeOnly, string? category, CancellationToken cancellationToken);
    Task<IReadOnlyList<StandardOperatingProcedure>> GetByIdsAsync(Guid orgId, IReadOnlyCollection<Guid> sopIds, CancellationToken cancellationToken);
    Task UpdateAsync(StandardOperatingProcedure sop, CancellationToken cancellationToken);
    Task DeleteAsync(Guid orgId, Guid sopId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

