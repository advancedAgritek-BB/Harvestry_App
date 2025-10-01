using System;
using Harvestry.Spatial.Domain.Entities;
using Harvestry.Spatial.Domain.Enums;
using Xunit;

namespace Harvestry.Spatial.Tests.Unit.Domain;

public sealed class RoomTests
{
    private static readonly Guid SiteId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void ChangeStatus_UpdatesStatusAndAudit()
    {
        var room = new Room(SiteId, "ROOM-1", "Propagation", RoomType.Clone, UserId);

        room.ChangeStatus(RoomStatus.Maintenance, UserId);

        Assert.Equal(RoomStatus.Maintenance, room.Status);
        Assert.Equal(UserId, room.UpdatedByUserId);
    }

    [Fact]
    public void UpdateEnvironmentTargets_InvalidHumidity_Throws()
    {
        var room = new Room(SiteId, "ROOM-2", "Flower", RoomType.Flower, UserId);

        Assert.Throws<ArgumentException>(() =>
            room.UpdateEnvironmentTargets(75m, 200m, 800, UserId));
    }

    [Fact]
    public void CustomRoomType_MustProvideCustomName()
    {
        Assert.Throws<ArgumentException>(() =>
            new Room(SiteId, "ROOM-3", "Custom", RoomType.Custom, UserId));
    }
}
