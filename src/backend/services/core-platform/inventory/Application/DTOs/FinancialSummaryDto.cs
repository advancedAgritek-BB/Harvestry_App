namespace Harvestry.Inventory.Application.DTOs;

/// <summary>
/// Financial summary for inventory dashboard
/// </summary>
public record FinancialSummaryDto
{
    public Dictionary<string, decimal> ValueByCategory { get; init; } = new();
    public decimal TotalInventoryValue { get; init; }
    public decimal CogsLast30Days { get; init; }
    public decimal CogsLast90Days { get; init; }
    public List<CogsTrendPointDto> CogsTrend { get; init; } = new();
    public decimal GrossMarginPercent { get; init; }
    public string GrossMarginTrend { get; init; } = "stable";
    public ValueAtRiskDto ValueAtRisk { get; init; } = new();
}

/// <summary>
/// COGS trend data point
/// </summary>
public record CogsTrendPointDto
{
    public DateOnly Date { get; init; }
    public decimal Value { get; init; }
}

/// <summary>
/// Value at risk breakdown
/// </summary>
public record ValueAtRiskDto
{
    public decimal Expiring7Days { get; init; }
    public decimal Expiring30Days { get; init; }
    public decimal Expired { get; init; }
    public decimal Quarantined { get; init; }
    public decimal CoaFailed { get; init; }
    public decimal OnHold { get; init; }
    public decimal PendingLab { get; init; }
    public decimal Total { get; init; }
}

/// <summary>
/// Category value summary
/// </summary>
public record CategoryValueDto
{
    public string Category { get; init; } = string.Empty;
    public int PackageCount { get; init; }
    public decimal TotalQuantity { get; init; }
    public decimal TotalValue { get; init; }
    public decimal AverageUnitCost { get; init; }
    public decimal TotalReserved { get; init; }
    public decimal TotalAvailable { get; init; }
}

/// <summary>
/// Inventory aging analysis
/// </summary>
public record AgingAnalysisDto
{
    public Dictionary<string, AgingBucketDto> ByCategory { get; init; } = new();
    public AgingBucketDto Total { get; init; } = new();
}

public record AgingBucketDto
{
    public decimal Value0To30 { get; init; }
    public decimal Value31To60 { get; init; }
    public decimal Value61To90 { get; init; }
    public decimal Value91To180 { get; init; }
    public decimal Value180Plus { get; init; }
    public int Count0To30 { get; init; }
    public int Count31To60 { get; init; }
    public int Count61To90 { get; init; }
    public int Count91To180 { get; init; }
    public int Count180Plus { get; init; }
}

/// <summary>
/// Inventory turnover metrics
/// </summary>
public record TurnoverMetricsDto
{
    public decimal AverageInventoryValue { get; init; }
    public decimal CogsLast30Days { get; init; }
    public decimal CogsLast90Days { get; init; }
    public decimal CogsLastYear { get; init; }
    public decimal TurnoverRateAnnualized { get; init; }
    public int? DaysOnHand { get; init; }
    public Dictionary<string, decimal> TurnoverByCategory { get; init; } = new();
}

/// <summary>
/// Alert summary for dashboard
/// </summary>
public record AlertSummaryDto
{
    public int LowStockCount { get; init; }
    public int ExpiringCount { get; init; }
    public int OnHoldCount { get; init; }
    public int PendingSyncCount { get; init; }
    public int FailedLabTestCount { get; init; }
    public int VarianceCount { get; init; }
    public decimal TotalAlertValue { get; init; }
    public List<AlertItemDto> TopAlerts { get; init; } = new();
}

public record AlertItemDto
{
    public string Type { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? RelatedId { get; init; }
    public string? RelatedLabel { get; init; }
    public decimal? ValueImpact { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Dashboard KPIs
/// </summary>
public record DashboardKpisDto
{
    public decimal TotalInventoryValue { get; init; }
    public decimal TotalInventoryValueChange { get; init; }
    public int TotalPackages { get; init; }
    public int TotalPackagesChange { get; init; }
    public int ActiveHolds { get; init; }
    public decimal ValueOnHold { get; init; }
    public int PendingSyncs { get; init; }
    public decimal DaysOnHandAverage { get; init; }
    public decimal CogsLast30Days { get; init; }
    public decimal CogsChange { get; init; }
    public decimal ValueAtRisk { get; init; }
    public int LowStockItems { get; init; }
    public int ExpiringPackages { get; init; }
}



