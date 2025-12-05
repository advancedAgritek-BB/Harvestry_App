namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Overall status of a batch (independent of current stage)
/// </summary>
public enum BatchStatus
{
    /// <summary>
    /// Batch is actively growing
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Batch is quarantined (pest/disease/compliance hold)
    /// </summary>
    Quarantine = 1,
    
    /// <summary>
    /// Batch is on hold (administrative/quality hold)
    /// </summary>
    Hold = 2,
    
    /// <summary>
    /// Batch has been destroyed
    /// </summary>
    Destroyed = 3,
    
    /// <summary>
    /// Batch lifecycle is complete (harvested and transferred to inventory)
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Batch has been transferred to another facility/site
    /// </summary>
    Transferred = 5
}

