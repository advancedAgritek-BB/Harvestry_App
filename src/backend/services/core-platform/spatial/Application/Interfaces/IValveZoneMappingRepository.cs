using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

public interface IValveZoneMappingRepository
{
    Task<Guid> InsertAsync(ValveZoneMapping mapping, CancellationToken cancellationToken = default);

    Task UpdateAsync(ValveZoneMapping mapping, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid mappingId, CancellationToken cancellationToken = default);

    Task<ValveZoneMapping?> GetByIdAsync(Guid mappingId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValveZoneMapping>> GetByValveAsync(Guid valveEquipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValveZoneMapping>> GetByZoneAsync(Guid zoneLocationId, CancellationToken cancellationToken = default);

    Task<bool> AnyWithInterlockAsync(Guid siteId, string interlockGroup, CancellationToken cancellationToken = default);
}

