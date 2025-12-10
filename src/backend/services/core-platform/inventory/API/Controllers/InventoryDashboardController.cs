using Harvestry.Inventory.Application.DTOs;
using Harvestry.Inventory.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Inventory.API.Controllers;

/// <summary>
/// API controller for Inventory Dashboard metrics
/// </summary>
[ApiController]
[Route("api/v1/sites/{siteId:guid}/inventory/dashboard")]
[Authorize]
public class InventoryDashboardController : ControllerBase
{
    private readonly IInventoryValuationService _valuationService;
    private readonly IAlertService _alertService;
    private readonly ILogger<InventoryDashboardController> _logger;

    public InventoryDashboardController(
        IInventoryValuationService valuationService,
        IAlertService alertService,
        ILogger<InventoryDashboardController> logger)
    {
        _valuationService = valuationService;
        _alertService = alertService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete financial summary
    /// </summary>
    [HttpGet("financial-summary")]
    [ProducesResponseType(typeof(FinancialSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _valuationService.GetFinancialSummaryAsync(siteId, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Get inventory value by category
    /// </summary>
    [HttpGet("value-by-category")]
    [ProducesResponseType(typeof(List<CategoryValueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryValueDto>>> GetValueByCategory(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var values = await _valuationService.GetValueByCategoryAsync(siteId, cancellationToken);
        return Ok(values);
    }

    /// <summary>
    /// Get value at risk breakdown
    /// </summary>
    [HttpGet("value-at-risk")]
    [ProducesResponseType(typeof(ValueAtRiskDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValueAtRiskDto>> GetValueAtRisk(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var risk = await _valuationService.GetValueAtRiskAsync(siteId, cancellationToken);
        return Ok(risk);
    }

    /// <summary>
    /// Get COGS trend data
    /// </summary>
    [HttpGet("cogs-trend")]
    [ProducesResponseType(typeof(List<CogsTrendPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CogsTrendPointDto>>> GetCogsTrend(
        [FromRoute] Guid siteId,
        [FromQuery] int days = 90,
        CancellationToken cancellationToken = default)
    {
        var trend = await _valuationService.GetCogsTrendAsync(siteId, days, cancellationToken);
        return Ok(trend);
    }

    /// <summary>
    /// Get inventory aging analysis
    /// </summary>
    [HttpGet("aging-analysis")]
    [ProducesResponseType(typeof(AgingAnalysisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgingAnalysisDto>> GetAgingAnalysis(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var analysis = await _valuationService.GetAgingAnalysisAsync(siteId, cancellationToken);
        return Ok(analysis);
    }

    /// <summary>
    /// Get turnover metrics
    /// </summary>
    [HttpGet("turnover-metrics")]
    [ProducesResponseType(typeof(TurnoverMetricsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TurnoverMetricsDto>> GetTurnoverMetrics(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _valuationService.GetTurnoverMetricsAsync(siteId, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get all alerts summary
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(AlertSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertSummaryDto>> GetAlerts(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _alertService.GetAlertSummaryAsync(siteId, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Get dashboard KPIs
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(DashboardKpisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardKpisDto>> GetKpis(
        [FromRoute] Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var kpis = await _valuationService.GetDashboardKpisAsync(siteId, cancellationToken);
        return Ok(kpis);
    }
}



