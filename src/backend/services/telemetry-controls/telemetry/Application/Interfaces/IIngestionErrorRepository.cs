using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Persistence operations for ingestion errors.
/// </summary>
public interface IIngestionErrorRepository
{
    Task LogAsync(IEnumerable<IngestionError> errors, CancellationToken cancellationToken = default);
}
