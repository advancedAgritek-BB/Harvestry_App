using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Data access abstraction for <see cref="Room"/> aggregates.
/// </summary>
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task<Room?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Room>> GetBySiteAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<Guid> InsertAsync(Room room, CancellationToken cancellationToken = default);

    Task UpdateAsync(Room room, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid roomId, CancellationToken cancellationToken = default);
}
