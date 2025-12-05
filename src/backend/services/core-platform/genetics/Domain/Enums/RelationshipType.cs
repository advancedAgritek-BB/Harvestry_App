namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Type of relationship between batches
/// </summary>
public enum RelationshipType
{
    /// <summary>
    /// Parent batch was split into child batches
    /// </summary>
    Split = 0,
    
    /// <summary>
    /// Parent batches were merged into child batch
    /// </summary>
    Merge = 1,
    
    /// <summary>
    /// Child batch was propagated from parent (cloning)
    /// </summary>
    Propagation = 2,
    
    /// <summary>
    /// Child batch is a transformation of parent (e.g., harvest → processing)
    /// </summary>
    Transformation = 3
}

