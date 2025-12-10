using Harvestry.Analytics.Application.DTOs;
using Harvestry.Analytics.Application.Interfaces;
using Harvestry.Analytics.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvestry.Analytics.API.Controllers;

[ApiController]
[Route("api/v1/analytics/shares")]
[Authorize]
public class SharesController : ControllerBase
{
    private readonly IShareRepository _shareRepository;
    private readonly IRlsContextAccessor _rlsContextAccessor;

    public SharesController(
        IShareRepository shareRepository,
        IRlsContextAccessor rlsContextAccessor)
    {
        _shareRepository = shareRepository;
        _rlsContextAccessor = rlsContextAccessor;
    }

    [HttpGet("{resourceType}/{resourceId}")]
    public async Task<ActionResult<IEnumerable<ShareDto>>> GetByResource(string resourceType, Guid resourceId, CancellationToken cancellationToken)
    {
        // RLS should ensure only owner/admin can see shares? Or maybe anyone with access?
        // Usually only owner/admin can manage shares.
        var shares = await _shareRepository.GetByResourceAsync(resourceType, resourceId, cancellationToken);
        return Ok(shares.Select(s => new ShareDto(s.Id, s.ResourceType, s.ResourceId, s.SharedWithId, s.SharedWithType, s.PermissionLevel)));
    }

    [HttpPost]
    public async Task<ActionResult<ShareDto>> Share(CreateShareDto dto, CancellationToken cancellationToken)
    {
        var userId = _rlsContextAccessor.Current.UserId;
        // Verify user owns resource or has admin rights before adding share. 
        // This logic should be in a Domain Service or Application Service.
        // For MVP, relying on RLS policies on 'analytics.shares' INSERT (which I defined in migration to allow owner/admin).
        
        var share = Analytics.Domain.Entities.Share.Create(
            dto.ResourceType,
            dto.ResourceId,
            dto.SharedWithId,
            dto.SharedWithType,
            dto.PermissionLevel,
            userId
        );

        await _shareRepository.AddAsync(share, cancellationToken);
        return Ok(new ShareDto(share.Id, share.ResourceType, share.ResourceId, share.SharedWithId, share.SharedWithType, share.PermissionLevel));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Unshare(Guid id, CancellationToken cancellationToken)
    {
        await _shareRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}




