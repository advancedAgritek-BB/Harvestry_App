using Harvestry.Telemetry.Domain.Entities;

namespace Harvestry.Telemetry.Application.Interfaces;

/// <summary>
/// Repository interface for sensor stream operations.
/// </summary>
public interface ISensorStreamRepository
{
    Task<SensorStream?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SensorStream>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<List<SensorStream>> GetByEquipmentIdAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<SensorStream> CreateAsync(SensorStream stream, CancellationToken cancellationToken = default);
    Task UpdateAsync(SensorStream stream, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SensorStream>> GetByIdsAsync(IEnumerable<Guid> streamIds, CancellationToken cancellationToken = default);
}
