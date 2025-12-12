using Harvestry.Analytics.Application.DTOs;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Harvestry.Analytics.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Analytics.API.Controllers;

[ApiController]
[Route("api/v1/analytics/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly IQueryBuilderService _queryBuilderService;
    private readonly IRlsContextAccessor _rlsContextAccessor;

    public ReportsController(
        IReportRepository reportRepository,
        IQueryBuilderService queryBuilderService,
        IRlsContextAccessor rlsContextAccessor)
    {
        _reportRepository = reportRepository;
        _queryBuilderService = queryBuilderService;
        _rlsContextAccessor = rlsContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        var reports = await _reportRepository.GetAllAsync(userId, cancellationToken);
        return Ok(reports.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null) return NotFound();
        return Ok(MapToDto(report));
    }

    [HttpPost]
    public async Task<ActionResult<ReportDto>> Create(CreateReportDto dto, CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        var report = Report.Create(
            dto.Name,
            dto.Description,
            dto.Config,
            userId,
            dto.IsPublic,
            dto.VisualizationConfigJson
        );

        await _reportRepository.AddAsync(report, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = report.Id }, MapToDto(report));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateReportDto dto, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(id, cancellationToken);
        if (report == null) return NotFound();

        var userId = _rlsContextAccessor.Current.UserId;
        report.Update(dto.Name, dto.Description, dto.Config, dto.VisualizationConfigJson, userId);
        report.SetPublic(dto.IsPublic, userId);

        await _reportRepository.UpdateAsync(report, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _reportRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("query")]
    public async Task<ActionResult<IEnumerable<dynamic>>> PreviewQuery([FromBody] ReportConfig config, CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        try
        {
            var results = await _queryBuilderService.ExecuteQueryAsync(config, userId, cancellationToken);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static ReportDto MapToDto(Report report)
    {
        return new ReportDto(
            report.Id,
            report.Name,
            report.Description,
            report.Config,
            report.VisualizationConfigJson,
            report.IsPublic,
            report.OwnerId,
            report.CreatedAt,
            report.UpdatedAt
        );
    }
}





