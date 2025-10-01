using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.ViewModels;

namespace Harvestry.Spatial.Application.Interfaces;

public interface ICalibrationService
{
    Task<CalibrationResponse> RecordAsync(Guid siteId, Guid equipmentId, CreateCalibrationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalibrationResponse>> GetHistoryAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default);

    Task<CalibrationResponse?> GetLatestAsync(Guid siteId, Guid equipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalibrationResponse>> GetOverdueAsync(Guid siteId, DateTime? dueBeforeUtc, CancellationToken cancellationToken = default);
}
