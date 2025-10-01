using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

/// <summary>
/// Provides access to <see cref="InventoryLocation"/> nodes and hierarchy queries.
/// </summary>
public interface IInventoryLocationRepository
{
    Task<InventoryLocation?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<InventoryLocation?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryLocation>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryLocation>> GetDescendantsAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryLocation>> GetByRoomAsync(Guid roomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InventoryLocation>> GetPathAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<Guid> InsertAsync(InventoryLocation location, CancellationToken cancellationToken = default);

    Task UpdateAsync(InventoryLocation location, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid locationId, CancellationToken cancellationToken = default);
}
