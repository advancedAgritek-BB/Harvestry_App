namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// METRC propagation type for plant batches
/// Maps directly to METRC's PlantBatch Type values
/// </summary>
public enum PropagationType
{
    /// <summary>
    /// Clone/cutting from another plant
    /// </summary>
    Clone = 0,

    /// <summary>
    /// Grown from seed
    /// </summary>
    Seed = 1,

    /// <summary>
    /// Other plant material (tissue culture, etc.)
    /// </summary>
    PlantMaterial = 2
}








