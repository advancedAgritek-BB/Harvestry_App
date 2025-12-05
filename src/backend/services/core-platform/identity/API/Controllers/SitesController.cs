using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Identity.API.Controllers;

[ApiController]
[Route("api/v1/sites")]
public class SitesController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;
    
    public SitesController(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSites(CancellationToken cancellationToken)
    {
        // For admin/dev purposes, get all sites. 
        // Real auth would filter by user permissions.
        var sites = await _siteRepository.GetAllAsync(cancellationToken);
        return Ok(sites.Select(s => new { s.Id, s.Name, s.SiteType }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        var site = Site.Create(request.Name, "cultivation", "UTC", "en-US");
        
        await _siteRepository.AddAsync(site, cancellationToken);
        
        return CreatedAtAction(nameof(GetSites), new { id = site.Id }, new { site.Id, site.Name });
    }
}

public record CreateSiteRequestDto(string Name, string? OrganizationId);
