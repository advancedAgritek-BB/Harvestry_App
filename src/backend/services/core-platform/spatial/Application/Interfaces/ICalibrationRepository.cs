using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Spatial.Domain.Entities;

namespace Harvestry.Spatial.Application.Interfaces;

public interface ICalibrationRepository
{
    Task<Guid> InsertAsync(Calibration calibration, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Calibration>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<Calibration?> GetLatestByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Calibration>> GetOverdueAsync(Guid siteId, DateTime dueBeforeUtc, CancellationToken cancellationToken = default);
}
