namespace Harvestry.Plants.Domain.Enums;

/// <summary>
/// Types of events that can occur on a plant (for audit trail)
/// </summary>
public enum PlantEventType
{
    /// <summary>
    /// Plant was created/tagged
    /// </summary>
    Created = 0,

    /// <summary>
    /// Plant transitioned to vegetative phase
    /// </summary>
    VegetativeTransition = 1,

    /// <summary>
    /// Plant transitioned to flowering phase
    /// </summary>
    FloweringTransition = 2,

    /// <summary>
    /// Plant designated as mother plant
    /// </summary>
    DesignatedMother = 3,

    /// <summary>
    /// Plant location changed
    /// </summary>
    LocationChange = 4,

    /// <summary>
    /// Plant was harvested
    /// </summary>
    Harvested = 5,

    /// <summary>
    /// Plant was destroyed
    /// </summary>
    Destroyed = 6,

    /// <summary>
    /// Plant placed on hold
    /// </summary>
    PlacedOnHold = 7,

    /// <summary>
    /// Plant released from hold
    /// </summary>
    ReleasedFromHold = 8,

    /// <summary>
    /// Plant synced to METRC
    /// </summary>
    MetrcSynced = 9,

    /// <summary>
    /// Notes added to plant
    /// </summary>
    NotesAdded = 10,

    /// <summary>
    /// Tag replaced/changed
    /// </summary>
    TagReplaced = 11
}




