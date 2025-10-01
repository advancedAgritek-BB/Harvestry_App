using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.ViewModels;

namespace Harvestry.Spatial.Application.Interfaces;

public interface IValveZoneMappingService
{
    Task<ValveZoneMappingResponse> CreateAsync(Guid siteId, Guid equipmentId, CreateValveZoneMappingRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValveZoneMappingResponse>> GetByValveAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ValveZoneMappingResponse>> GetByZoneAsync(Guid siteId, Guid locationId, CancellationToken cancellationToken = default);

    Task<ValveZoneMappingResponse> UpdateAsync(Guid siteId, Guid mappingId, UpdateValveZoneMappingRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid siteId, Guid mappingId, Guid requestedByUserId, CancellationToken cancellationToken = default);
}

