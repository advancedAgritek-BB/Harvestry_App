namespace Harvestry.Compliance.BioTrack.Domain.Enums;

/// <summary>
/// Types of entities that can be synchronized with BioTrack
/// </summary>
public enum BioTrackEntityType
{
    Plant = 1,
    PlantRoom = 2,
    Inventory = 3,
    InventoryRoom = 4,
    InventoryAdjust = 5,
    InventoryTransfer = 6,
    InventoryTransferInbound = 7,
    Manifest = 8,
    QualityAssurance = 9,
    Destruction = 10,
    Conversion = 11,
    Employee = 12
}

/// <summary>
/// Direction of data synchronization with BioTrack
/// </summary>
public enum BioTrackSyncDirection
{
    Inbound = 1,
    Outbound = 2,
    Bidirectional = 3
}

/// <summary>
/// Status of a BioTrack synchronization job or queue item
/// </summary>
public enum BioTrackSyncStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    FailedPermanent = 5,
    Cancelled = 6,
    ManualReviewRequired = 7
}

/// <summary>
/// Types of operations that can be performed against BioTrack
/// </summary>
public enum BioTrackOperationType
{
    Create = 1,
    Update = 2,
    Void = 3,
    Transfer = 4,
    Adjust = 5,
    Convert = 6,
    Destroy = 7,
    QualityAssurance = 8,
    Read = 9
}
