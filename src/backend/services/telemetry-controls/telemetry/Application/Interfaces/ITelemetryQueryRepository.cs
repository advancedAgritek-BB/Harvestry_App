using Harvestry.Telemetry.Domain.Entities;
using Harvestry.Telemetry.Domain.Enums;
using Harvestry.Telemetry.Domain.ValueObjects;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Read-model repository for querying telemetry data.
/// </summary>
public interface ITelemetryQueryRepository
{
    Task<IReadOnlyList<SensorReading>> GetReadingsAsync(
        Guid streamId,
        DateTimeOffset start,
        DateTimeOffset end,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<SensorReading?> GetLatestReadingAsync(
        Guid streamId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RollupData>> GetRollupsAsync(
        Guid streamId,
        DateTimeOffset start,
        DateTimeOffset end,
        RollupInterval interval,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Guid SiteId, SensorReading Reading)>> GetReadingsSinceAsync(
        DateTimeOffset since,
        int limit,
        CancellationToken cancellationToken = default);
}
