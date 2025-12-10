namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Quality grade classification for products
/// </summary>
public enum QualityGrade
{
    /// <summary>
    /// Premium/top-shelf quality
    /// </summary>
    Premium = 0,

    /// <summary>
    /// Grade A - High quality
    /// </summary>
    A = 1,

    /// <summary>
    /// Grade B - Good quality
    /// </summary>
    B = 2,

    /// <summary>
    /// Grade C - Acceptable quality
    /// </summary>
    C = 3,

    /// <summary>
    /// Standard quality
    /// </summary>
    Standard = 4,

    /// <summary>
    /// Economy/value grade
    /// </summary>
    Economy = 5,

    /// <summary>
    /// Rejected - not suitable for sale
    /// </summary>
    Reject = 99
}



