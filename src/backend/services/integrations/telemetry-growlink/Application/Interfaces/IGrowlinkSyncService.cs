using Harvestry.Integration.Growlink.Application.DTOs;

namespace Harvestry.Integration.Growlink.Application.Interfaces;

/// <summary>
/// Service for syncing data from Growlink to Harvestry.
/// </summary>
public interface IGrowlinkSyncService
{
    /// <summary>
    /// Syncs the latest readings for a site.
    /// </summary>
    Task<GrowlinkSyncResultDto> SyncLatestReadingsAsync(
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs all active sites.
    /// </summary>
    Task<List<GrowlinkSyncResultDto>> SyncAllSitesAsync(
        CancellationToken cancellationToken = default);
}





