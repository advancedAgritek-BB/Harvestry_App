using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

public sealed record EquipmentListResult(IReadOnlyList<Equipment> Items, int TotalCount);

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<Equipment?> GetByCodeAsync(Guid siteId, string code, CancellationToken cancellationToken = default);

    Task<EquipmentListResult> GetBySiteAsync(Guid siteId, EquipmentListQuery query, CancellationToken cancellationToken = default);

    Task<Guid> InsertAsync(Equipment equipment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}
