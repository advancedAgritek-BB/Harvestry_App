using Harvestry.Integration.Growlink.Application.DTOs;
using Harvestry.Integration.Growlink.Domain.Entities;

namespace Harvestry.Integration.Growlink.Application.Interfaces;

/// <summary>
/// Maps Growlink sensors to Harvestry SensorStreams.
/// </summary>
public interface IGrowlinkStreamMapper
{
    /// <summary>
    /// Gets the Harvestry StreamId for a Growlink sensor.
    /// Returns null if no mapping exists and auto-create is disabled.
    /// </summary>
    Task<Guid?> GetHarvestryStreamIdAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        bool autoCreate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active mappings for a site.
    /// </summary>
    Task<Dictionary<string, Guid>> GetMappingsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a manual mapping.
    /// </summary>
    Task<GrowlinkStreamMapping> CreateMappingAsync(
        Guid siteId,
        string growlinkDeviceId,
        string growlinkSensorId,
        string growlinkSensorName,
        string growlinkSensorType,
        Guid harvestryStreamId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mapping.
    /// </summary>
    Task DeleteMappingAsync(
        Guid mappingId,
        CancellationToken cancellationToken = default);
}




