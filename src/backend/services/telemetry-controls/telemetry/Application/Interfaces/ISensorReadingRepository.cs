using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Persistence operations for sensor readings optimized for TimescaleDB.
/// </summary>
public interface ISensorReadingRepository
{
    Task BulkInsertAsync(IEnumerable<SensorReading> readings, CancellationToken cancellationToken = default);
}
