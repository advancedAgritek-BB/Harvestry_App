using Harvestry.Integration.Growlink.Domain.Entities;

namespace Harvestry.Integration.Growlink.Application.Interfaces;

/// <summary>
/// Repository for Growlink stream mappings.
/// </summary>
public interface IGrowlinkStreamMappingRepository
{
    /// <summary>
    /// Gets all active mappings for a site.
    /// </summary>
    Task<List<GrowlinkStreamMapping>> GetBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mapping by Growlink sensor key.
    /// </summary>
    Task<GrowlinkStreamMapping?> GetByGrowlinkSensorAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mapping by ID.
    /// </summary>
    Task<GrowlinkStreamMapping?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new mapping.
    /// </summary>
    Task CreateAsync(
        GrowlinkStreamMapping mapping,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple mappings.
    /// </summary>
    Task CreateManyAsync(
        IEnumerable<GrowlinkStreamMapping> mappings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mapping.
    /// </summary>
    Task UpdateAsync(
        GrowlinkStreamMapping mapping,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mapping.
    /// </summary>
    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all mappings for a site.
    /// </summary>
    Task DeleteBySiteIdAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);
}





