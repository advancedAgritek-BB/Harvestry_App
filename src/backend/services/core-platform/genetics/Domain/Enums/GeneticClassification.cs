namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Genetic classification of cannabis strains (METRC strain genetics)
/// </summary>
public enum GeneticClassification
{
    /// <summary>
    /// Indica-dominant strain
    /// </summary>
    Indica = 0,

    /// <summary>
    /// Sativa-dominant strain
    /// </summary>
    Sativa = 1,

    /// <summary>
    /// Hybrid strain (mix of indica and sativa)
    /// </summary>
    Hybrid = 2,

    /// <summary>
    /// Classification not specified
    /// </summary>
    Unspecified = 99
}



