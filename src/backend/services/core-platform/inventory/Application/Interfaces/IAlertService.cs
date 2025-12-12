using Harvestry.Inventory.Application.DTOs;

namespace Harvestry.Inventory.Application.Interfaces;

/// <summary>
/// Service interface for inventory alerts
/// </summary>
public interface IAlertService
{
    /// <summary>
    /// Get alert summary for dashboard
    /// </summary>
    Task<AlertSummaryDto> GetAlertSummaryAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock alerts
    /// </summary>
    Task<List<AlertItemDto>> GetLowStockAlertsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expiring inventory alerts
    /// </summary>
    Task<List<AlertItemDto>> GetExpiringAlertsAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hold alerts
    /// </summary>
    Task<List<AlertItemDto>> GetHoldAlertsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sync status alerts
    /// </summary>
    Task<List<AlertItemDto>> GetSyncAlertsAsync(Guid siteId, CancellationToken cancellationToken = default);
}




