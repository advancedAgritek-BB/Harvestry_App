namespace Harvestry.Tasks.Domain.Enums;

/// <summary>
/// Room type for task blueprint matching (mirrors Spatial.RoomType for loose coupling).
/// </summary>
public enum BlueprintRoomType
{
    /// <summary>
    /// Any room type - wildcard for matching.
    /// </summary>
    Any = 0,

    /// <summary>
    /// Vegetative growth room.
    /// </summary>
    Veg = 1,

    /// <summary>
    /// Flowering room.
    /// </summary>
    Flower = 2,

    /// <summary>
    /// Mother plant room.
    /// </summary>
    Mother = 3,

    /// <summary>
    /// Clone/propagation room.
    /// </summary>
    Clone = 4,

    /// <summary>
    /// Drying room.
    /// </summary>
    Dry = 5,

    /// <summary>
    /// Curing room.
    /// </summary>
    Cure = 6,

    /// <summary>
    /// Extraction/processing room.
    /// </summary>
    Extraction = 7,

    /// <summary>
    /// Manufacturing room.
    /// </summary>
    Manufacturing = 8,

    /// <summary>
    /// Vault - secure storage.
    /// </summary>
    Vault = 9
}

