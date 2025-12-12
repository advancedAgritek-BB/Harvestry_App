using Harvestry.Inventory.Application.DTOs;
using Harvestry.Inventory.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Harvestry.Inventory.Application.Services;

/// <summary>
/// Service implementation for inventory alerts
/// </summary>
public class AlertService : IAlertService
{
    private readonly string _connectionString;
    private readonly ILogger<AlertService> _logger;

    public AlertService(string connectionString, ILogger<AlertService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<AlertSummaryDto> GetAlertSummaryAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var lowStockAlerts = await GetLowStockAlertsAsync(siteId, cancellationToken);
        var expiringAlerts = await GetExpiringAlertsAsync(siteId, 30, cancellationToken);
        var holdAlerts = await GetHoldAlertsAsync(siteId, cancellationToken);
        var syncAlerts = await GetSyncAlertsAsync(siteId, cancellationToken);

        var allAlerts = lowStockAlerts.Concat(expiringAlerts).Concat(holdAlerts).Concat(syncAlerts)
            .OrderByDescending(a => a.Severity == "critical" ? 3 : a.Severity == "warning" ? 2 : 1)
            .ThenByDescending(a => a.ValueImpact ?? 0)
            .ToList();

        return new AlertSummaryDto
        {
            LowStockCount = lowStockAlerts.Count,
            ExpiringCount = expiringAlerts.Count,
            OnHoldCount = holdAlerts.Count,
            PendingSyncCount = syncAlerts.Count,
            FailedLabTestCount = 0, // Would query lab test status
            VarianceCount = 0, // Would query cycle count variances
            TotalAlertValue = allAlerts.Sum(a => a.ValueImpact ?? 0),
            TopAlerts = allAlerts.Take(10).ToList()
        };
    }

    public async Task<List<AlertItemDto>> GetLowStockAlertsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var alerts = new List<AlertItemDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT item_id, item_name, sku, stock_status, shortage, shortage_value
            FROM low_stock_alerts
            WHERE site_id = @siteId
            ORDER BY shortage_value DESC
            LIMIT 50", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var status = reader.GetString(3);
            alerts.Add(new AlertItemDto
            {
                Type = "low_stock",
                Severity = status == "critical" ? "critical" : "warning",
                Title = $"Low Stock: {reader.GetString(1)}",
                Description = $"SKU {(reader.IsDBNull(2) ? "N/A" : reader.GetString(2))} is {reader.GetDecimal(4):N0} units below reorder point",
                RelatedId = reader.GetGuid(0),
                RelatedLabel = reader.IsDBNull(2) ? null : reader.GetString(2),
                ValueImpact = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                CreatedAt = DateTime.UtcNow
            });
        }

        return alerts;
    }

    public async Task<List<AlertItemDto>> GetExpiringAlertsAsync(Guid siteId, int withinDays = 30, CancellationToken cancellationToken = default)
    {
        var alerts = new List<AlertItemDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT package_id, package_label, item_name, days_until_expiry, value_at_risk, expiry_status
            FROM expiring_inventory
            WHERE site_id = @siteId
            ORDER BY days_until_expiry, value_at_risk DESC
            LIMIT 50", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var status = reader.GetString(5);
            var daysUntil = reader.GetInt32(3);
            alerts.Add(new AlertItemDto
            {
                Type = "expiring",
                Severity = status == "expired" || status == "critical" ? "critical" : "warning",
                Title = daysUntil <= 0 ? $"Expired: {reader.GetString(2)}" : $"Expiring: {reader.GetString(2)}",
                Description = daysUntil <= 0 
                    ? $"Package {reader.GetString(1)} has expired"
                    : $"Package {reader.GetString(1)} expires in {daysUntil} days",
                RelatedId = reader.GetGuid(0),
                RelatedLabel = reader.GetString(1),
                ValueImpact = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                CreatedAt = DateTime.UtcNow
            });
        }

        return alerts;
    }

    public async Task<List<AlertItemDto>> GetHoldAlertsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var alerts = new List<AlertItemDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT id, package_label, item_name, hold_reason_code, hold_placed_at, 
                   quantity * COALESCE(unit_cost, 0) as value_on_hold
            FROM packages
            WHERE site_id = @siteId AND status = 'on_hold'
            ORDER BY hold_placed_at
            LIMIT 50", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var holdReason = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3);
            var holdDate = reader.IsDBNull(4) ? DateTime.UtcNow : reader.GetDateTime(4);
            var daysSinceHold = (DateTime.UtcNow - holdDate).Days;

            alerts.Add(new AlertItemDto
            {
                Type = "hold",
                Severity = daysSinceHold > 14 ? "warning" : "info",
                Title = $"On Hold: {reader.GetString(2)}",
                Description = $"Package {reader.GetString(1)} on hold for {holdReason} ({daysSinceHold} days)",
                RelatedId = reader.GetGuid(0),
                RelatedLabel = reader.GetString(1),
                ValueImpact = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                CreatedAt = holdDate
            });
        }

        return alerts;
    }

    public async Task<List<AlertItemDto>> GetSyncAlertsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var alerts = new List<AlertItemDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT id, package_label, item_name, metrc_sync_status, updated_at
            FROM packages
            WHERE site_id = @siteId 
              AND metrc_sync_status IN ('pending', 'failed')
            ORDER BY updated_at
            LIMIT 50", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var syncStatus = reader.IsDBNull(3) ? "pending" : reader.GetString(3);
            alerts.Add(new AlertItemDto
            {
                Type = "sync",
                Severity = syncStatus == "failed" ? "warning" : "info",
                Title = syncStatus == "failed" ? $"Sync Failed: {reader.GetString(2)}" : $"Pending Sync: {reader.GetString(2)}",
                Description = $"Package {reader.GetString(1)} {syncStatus} for METRC sync",
                RelatedId = reader.GetGuid(0),
                RelatedLabel = reader.GetString(1),
                CreatedAt = reader.GetDateTime(4)
            });
        }

        return alerts;
    }
}




