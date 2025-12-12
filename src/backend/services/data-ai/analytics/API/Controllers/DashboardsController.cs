using Harvestry.Analytics.Application.DTOs;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Harvestry.Analytics.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Analytics.API.Controllers;

[ApiController]
[Route("api/v1/analytics/dashboards")]
[Authorize]
public class DashboardsController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IRlsContextAccessor _rlsContextAccessor;

    public DashboardsController(
        IDashboardRepository dashboardRepository,
        IRlsContextAccessor rlsContextAccessor)
    {
        _dashboardRepository = dashboardRepository;
        _rlsContextAccessor = rlsContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DashboardDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        var dashboards = await _dashboardRepository.GetAllAsync(userId, cancellationToken);
        return Ok(dashboards.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DashboardDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardRepository.GetByIdAsync(id, cancellationToken);
        if (dashboard == null) return NotFound();
        return Ok(MapToDto(dashboard));
    }

    [HttpPost]
    public async Task<ActionResult<DashboardDto>> Create(CreateDashboardDto dto, CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        var dashboard = Dashboard.Create(
            dto.Name,
            dto.Description,
            dto.LayoutConfig,
            userId,
            dto.IsPublic
        );

        await _dashboardRepository.AddAsync(dashboard, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dashboard.Id }, MapToDto(dashboard));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateDashboardDto dto, CancellationToken cancellationToken)
    {
        var dashboard = await _dashboardRepository.GetByIdAsync(id, cancellationToken);
        if (dashboard == null) return NotFound();

        var userId = _rlsContextAccessor.Current.UserId;
        dashboard.Update(dto.Name, dto.Description, dto.LayoutConfig, userId);
        dashboard.SetPublic(dto.IsPublic, userId);

        await _dashboardRepository.UpdateAsync(dashboard, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _dashboardRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static DashboardDto MapToDto(Dashboard dashboard)
    {
        return new DashboardDto(
            dashboard.Id,
            dashboard.Name,
            dashboard.Description,
            dashboard.LayoutConfig,
            dashboard.IsPublic,
            dashboard.OwnerId,
            dashboard.CreatedAt,
            dashboard.UpdatedAt
        );
    }
}





