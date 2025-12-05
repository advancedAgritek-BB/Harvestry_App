namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Type of batch event for audit trail
/// </summary>
public enum EventType
{
    /// <summary>
    /// Batch was created
    /// </summary>
    Created = 0,
    
    /// <summary>
    /// Batch moved to a different growth stage
    /// </summary>
    StageChange = 1,
    
    /// <summary>
    /// Batch moved to a different physical location
    /// </summary>
    LocationChange = 2,
    
    /// <summary>
    /// Plant count changed (culling, mortality, etc.)
    /// </summary>
    PlantCountChange = 3,
    
    /// <summary>
    /// Batch was harvested
    /// </summary>
    Harvest = 4,
    
    /// <summary>
    /// Batch was split into multiple batches
    /// </summary>
    Split = 5,
    
    /// <summary>
    /// Multiple batches were merged
    /// </summary>
    Merge = 6,
    
    /// <summary>
    /// Batch was placed in quarantine
    /// </summary>
    Quarantine = 7,
    
    /// <summary>
    /// Batch was released from quarantine
    /// </summary>
    ReleaseFromQuarantine = 13,
    
    /// <summary>
    /// Batch was placed on hold
    /// </summary>
    Hold = 8,
    
    /// <summary>
    /// Batch was destroyed
    /// </summary>
    Destroy = 9,
    
    /// <summary>
    /// Note added to batch
    /// </summary>
    NoteAdded = 10,
    
    /// <summary>
    /// Photo added to batch
    /// </summary>
    PhotoAdded = 11,
    
    /// <summary>
    /// Measurement/observation recorded
    /// </summary>
    MeasurementRecorded = 12
}

