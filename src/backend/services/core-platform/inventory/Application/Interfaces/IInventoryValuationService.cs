using Harvestry.Inventory.Application.DTOs;

namespace Harvestry.Inventory.Application.Interfaces;

/// <summary>
/// Service interface for inventory valuation and financial metrics
/// </summary>
public interface IInventoryValuationService
{
    /// <summary>
    /// Get complete financial summary
    /// </summary>
    Task<FinancialSummaryDto> GetFinancialSummaryAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory value by category
    /// </summary>
    Task<List<CategoryValueDto>> GetValueByCategoryAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get value at risk breakdown
    /// </summary>
    Task<ValueAtRiskDto> GetValueAtRiskAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get COGS trend data
    /// </summary>
    Task<List<CogsTrendPointDto>> GetCogsTrendAsync(Guid siteId, int days = 90, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory aging analysis
    /// </summary>
    Task<AgingAnalysisDto> GetAgingAnalysisAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get turnover metrics
    /// </summary>
    Task<TurnoverMetricsDto> GetTurnoverMetricsAsync(Guid siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dashboard KPIs
    /// </summary>
    Task<DashboardKpisDto> GetDashboardKpisAsync(Guid siteId, CancellationToken cancellationToken = default);
}




