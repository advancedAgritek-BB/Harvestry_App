using Harvestry.Inventory.Application.DTOs;
using Harvestry.Inventory.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Harvestry.Inventory.Application.Services;

/// <summary>
/// Service implementation for inventory valuation using database views
/// </summary>
public class InventoryValuationService : IInventoryValuationService
{
    private readonly string _connectionString;
    private readonly ILogger<InventoryValuationService> _logger;

    public InventoryValuationService(string connectionString, ILogger<InventoryValuationService> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var categories = await GetValueByCategoryAsync(siteId, cancellationToken);
        var valueAtRisk = await GetValueAtRiskAsync(siteId, cancellationToken);
        var cogsTrend = await GetCogsTrendAsync(siteId, 90, cancellationToken);
        var turnover = await GetTurnoverMetricsAsync(siteId, cancellationToken);

        return new FinancialSummaryDto
        {
            ValueByCategory = categories.ToDictionary(c => c.Category, c => c.TotalValue),
            TotalInventoryValue = categories.Sum(c => c.TotalValue),
            CogsLast30Days = turnover.CogsLast30Days,
            CogsLast90Days = turnover.CogsLast90Days,
            CogsTrend = cogsTrend,
            GrossMarginPercent = CalculateGrossMargin(categories.Sum(c => c.TotalValue), turnover.CogsLast30Days),
            GrossMarginTrend = DetermineMarginTrend(cogsTrend),
            ValueAtRisk = valueAtRisk
        };
    }

    public async Task<List<CategoryValueDto>> GetValueByCategoryAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var results = new List<CategoryValueDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT 
                inventory_category,
                package_count,
                total_quantity,
                total_value,
                avg_unit_cost,
                total_reserved,
                total_available
            FROM inventory_value_by_category
            WHERE site_id = @siteId", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CategoryValueDto
            {
                Category = reader.GetString(0),
                PackageCount = reader.GetInt32(1),
                TotalQuantity = reader.GetDecimal(2),
                TotalValue = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                AverageUnitCost = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                TotalReserved = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                TotalAvailable = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6)
            });
        }

        return results;
    }

    public async Task<ValueAtRiskDto> GetValueAtRiskAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT 
                expiring_7_days, expiring_30_days, expired, on_hold,
                coa_failed, contaminated, quality_issues, pending_lab, total_at_risk
            FROM value_at_risk
            WHERE site_id = @siteId", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new ValueAtRiskDto
            {
                Expiring7Days = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0),
                Expiring30Days = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                Expired = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                OnHold = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                CoaFailed = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                Quarantined = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                PendingLab = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                Total = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8)
            };
        }

        return new ValueAtRiskDto();
    }

    public async Task<List<CogsTrendPointDto>> GetCogsTrendAsync(Guid siteId, int days = 90, CancellationToken cancellationToken = default)
    {
        var results = new List<CogsTrendPointDto>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT period_date, SUM(total_cost) as daily_cogs
            FROM cogs_by_period
            WHERE site_id = @siteId
              AND period_date >= CURRENT_DATE - @days
            GROUP BY period_date
            ORDER BY period_date", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);
        cmd.Parameters.AddWithValue("days", days);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new CogsTrendPointDto
            {
                Date = DateOnly.FromDateTime(reader.GetDateTime(0)),
                Value = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
            });
        }

        return results;
    }

    public async Task<AgingAnalysisDto> GetAgingAnalysisAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var byCategory = new Dictionary<string, AgingBucketDto>();
        var total = new AgingBucketDto();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT 
                inventory_category,
                age_bucket,
                package_count,
                total_value
            FROM inventory_aging_summary
            WHERE site_id = @siteId", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        var categoryBuckets = new Dictionary<string, Dictionary<string, (int count, decimal value)>>();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var category = reader.GetString(0);
            var bucket = reader.GetString(1);
            var count = reader.GetInt32(2);
            var value = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);

            if (!categoryBuckets.ContainsKey(category))
                categoryBuckets[category] = new Dictionary<string, (int, decimal)>();
            categoryBuckets[category][bucket] = (count, value);
        }

        foreach (var (category, buckets) in categoryBuckets)
        {
            byCategory[category] = BuildAgingBucket(buckets);
        }

        // Calculate totals
        var allBuckets = categoryBuckets.Values.SelectMany(b => b);
        var totalBuckets = allBuckets.GroupBy(b => b.Key).ToDictionary(g => g.Key, g => (g.Sum(x => x.Value.count), g.Sum(x => x.Value.value)));

        return new AgingAnalysisDto
        {
            ByCategory = byCategory,
            Total = BuildAgingBucket(totalBuckets)
        };
    }

    public async Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(@"
            SELECT 
                avg_inventory_value, cogs_last_30_days, cogs_last_90_days,
                cogs_last_year, turnover_rate_annualized, days_on_hand
            FROM inventory_turnover
            WHERE site_id = @siteId", conn);

        cmd.Parameters.AddWithValue("siteId", siteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new TurnoverMetricsDto
            {
                AverageInventoryValue = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0),
                CogsLast30Days = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                CogsLast90Days = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                CogsLastYear = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                TurnoverRateAnnualized = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                DaysOnHand = reader.IsDBNull(5) ? null : reader.GetInt32(5)
            };
        }

        return new TurnoverMetricsDto();
    }

    public async Task<DashboardKpisDto> GetDashboardKpisAsync(Guid siteId, CancellationToken cancellationToken = default)
    {
        var categories = await GetValueByCategoryAsync(siteId, cancellationToken);
        var valueAtRisk = await GetValueAtRiskAsync(siteId, cancellationToken);
        var turnover = await GetTurnoverMetricsAsync(siteId, cancellationToken);

        return new DashboardKpisDto
        {
            TotalInventoryValue = categories.Sum(c => c.TotalValue),
            TotalPackages = categories.Sum(c => c.PackageCount),
            ActiveHolds = 0, // Would query holds_summary view
            ValueOnHold = valueAtRisk.OnHold,
            PendingSyncs = 0, // Would query sync status
            DaysOnHandAverage = turnover.DaysOnHand ?? 0,
            CogsLast30Days = turnover.CogsLast30Days,
            ValueAtRisk = valueAtRisk.Total,
            LowStockItems = 0, // Would query low_stock_alerts view
            ExpiringPackages = 0 // Would query expiring_inventory view
        };
    }

    private static AgingBucketDto BuildAgingBucket(Dictionary<string, (int count, decimal value)> buckets)
    {
        return new AgingBucketDto
        {
            Value0To30 = buckets.GetValueOrDefault("0-30").value,
            Value31To60 = buckets.GetValueOrDefault("31-60").value,
            Value61To90 = buckets.GetValueOrDefault("61-90").value,
            Value91To180 = buckets.GetValueOrDefault("91-180").value,
            Value180Plus = buckets.GetValueOrDefault("180+").value,
            Count0To30 = buckets.GetValueOrDefault("0-30").count,
            Count31To60 = buckets.GetValueOrDefault("31-60").count,
            Count61To90 = buckets.GetValueOrDefault("61-90").count,
            Count91To180 = buckets.GetValueOrDefault("91-180").count,
            Count180Plus = buckets.GetValueOrDefault("180+").count
        };
    }

    private static decimal CalculateGrossMargin(decimal inventoryValue, decimal cogs30Days)
    {
        if (inventoryValue + cogs30Days <= 0) return 0;
        var revenue = inventoryValue + cogs30Days; // Simplified
        return Math.Round((revenue - cogs30Days) / revenue * 100, 2);
    }

    private static string DetermineMarginTrend(List<CogsTrendPointDto> cogsTrend)
    {
        if (cogsTrend.Count < 14) return "stable";
        var recent = cogsTrend.TakeLast(7).Average(c => c.Value);
        var previous = cogsTrend.SkipLast(7).TakeLast(7).Average(c => c.Value);
        if (recent > previous * 1.1m) return "up";
        if (recent < previous * 0.9m) return "down";
        return "stable";
    }
}




