using System;
using System.Threading.Tasks;
using Harvestry.Spatial.Application.DTOs;
using Harvestry.Spatial.Application.Interfaces;
using Harvestry.Spatial.Domain.Enums;
using Harvestry.Spatial.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Harvestry.Spatial.Tests.Integration;

public sealed class RlsSpatialTests : IntegrationTestBase
{
    [IntegrationFact]
    public async Task RoomRepository_RlsBlocksAlternateSiteUser()
    {
        var hierarchyService = ServiceProvider.GetRequiredService<ISpatialHierarchyService>();
        var roomRepository = ServiceProvider.GetRequiredService<IRoomRepository>();

        SetUserContext(SpatialTestDataSeeder.ManagerUserId, "manager", SpatialTestDataSeeder.SiteId);

        var room = await hierarchyService.CreateRoomAsync(new CreateRoomRequest
        {
            SiteId = SpatialTestDataSeeder.SiteId,
            Code = $"RM-{Guid.NewGuid():N}".Substring(0, 6),
            Name = "RLS Room",
            RoomType = RoomType.Mother,
            RequestedByUserId = SpatialTestDataSeeder.ManagerUserId
        });
        
        // Assert room creation succeeded
        Assert.NotNull(room);
        Assert.NotEqual(Guid.Empty, room.Id);

        // Switch context to different user/site (no user_sites membership)
        var outsiderUserId = Guid.NewGuid();
        var outsiderSiteId = Guid.NewGuid();
        SetUserContext(outsiderUserId, "viewer", outsiderSiteId);

        var fetched = await roomRepository.GetByIdAsync(room.Id);
        Assert.Null(fetched); // RLS should prevent access
    }
}
