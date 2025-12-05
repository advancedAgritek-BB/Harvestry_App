using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Persistence operations for ingestion sessions.
/// </summary>
public interface IIngestionSessionRepository
{
    Task CreateAsync(IngestionSession session, CancellationToken cancellationToken = default);
    Task UpdateHeartbeatAsync(Guid sessionId, DateTimeOffset heartbeatAt, CancellationToken cancellationToken = default);
    Task EndAsync(Guid sessionId, DateTimeOffset endedAt, CancellationToken cancellationToken = default);
    Task<int> EndStaleSessionsAsync(TimeSpan staleThreshold, DateTimeOffset endedAt, CancellationToken cancellationToken = default);
}
