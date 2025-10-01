using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

public interface IEquipmentChannelRepository
{
    Task<IReadOnlyList<EquipmentChannel>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<EquipmentChannel?> GetByIdAsync(Guid channelId, CancellationToken cancellationToken = default);

    Task<Guid> InsertAsync(EquipmentChannel channel, CancellationToken cancellationToken = default);

    Task UpdateAsync(EquipmentChannel channel, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid channelId, CancellationToken cancellationToken = default);
}
