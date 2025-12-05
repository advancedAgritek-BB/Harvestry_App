namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Origin source of batch
/// </summary>
public enum BatchSourceType
{
    /// <summary>
    /// Purchased from external supplier
    /// </summary>
    Purchase = 0,
    
    /// <summary>
    /// Propagated internally from mother plants
    /// </summary>
    Propagation = 1,
    
    /// <summary>
    /// Result of internal breeding program
    /// </summary>
    Breeding = 2,
    
    /// <summary>
    /// Created from tissue culture
    /// </summary>
    TissueCulture = 3,

    /// <summary>
    /// Created by splitting an existing batch
    /// </summary>
    Split = 4,

    /// <summary>
    /// Created by merging multiple batches
    /// </summary>
    Merge = 5
}
