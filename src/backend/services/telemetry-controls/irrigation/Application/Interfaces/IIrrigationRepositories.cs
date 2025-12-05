using Harvestry.Irrigation.Domain.Entities;

namespace Harvestry.Irrigation.Application.Services;

/// <summary>
/// Repository for irrigation settings
/// </summary>
public interface IIrrigationSettingsRepository
{
    Task<IrrigationSettings?> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IrrigationSettings> UpsertAsync(IrrigationSettings settings, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for zone emitter configurations
/// </summary>
public interface IZoneEmitterConfigurationRepository
{
    Task<ZoneEmitterConfiguration?> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ZoneEmitterConfiguration>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<ZoneEmitterConfiguration> AddAsync(ZoneEmitterConfiguration config, CancellationToken cancellationToken = default);
    Task<ZoneEmitterConfiguration> UpdateAsync(ZoneEmitterConfiguration config, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for queued irrigation events
/// </summary>
public interface IQueuedIrrigationEventRepository
{
    Task<QueuedIrrigationEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QueuedIrrigationEvent>> GetPendingEventsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QueuedIrrigationEvent>> GetEventsInRangeAsync(Guid siteId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<QueuedIrrigationEvent> AddAsync(QueuedIrrigationEvent queuedEvent, CancellationToken cancellationToken = default);
    Task<QueuedIrrigationEvent> UpdateAsync(QueuedIrrigationEvent queuedEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for querying active irrigation runs
/// </summary>
public interface IActiveIrrigationRunRepository
{
    Task<IReadOnlyList<ActiveIrrigationRun>> GetActiveRunsAsync(Guid siteId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an active irrigation run for flow rate calculation
/// </summary>
public sealed record ActiveIrrigationRun(
    Guid RunId,
    Guid ProgramId,
    Guid[] ActiveZoneIds,
    DateTime StartedAt,
    DateTime? ExpectedEndAt);

/// <summary>
/// Repository for irrigation programs (extends existing)
/// </summary>
public interface IIrrigationProgramRepository
{
    Task<object?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
}




