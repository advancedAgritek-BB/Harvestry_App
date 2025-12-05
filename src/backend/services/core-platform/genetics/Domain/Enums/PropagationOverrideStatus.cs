namespace Harvestry.Genetics.Domain.Enums;

/// <summary>
/// Status of a propagation limit override request
/// </summary>
public enum PropagationOverrideStatus
{
    /// <summary>
    /// Override request is pending approval
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Override request has been approved
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// Override request has been rejected
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// Override request has expired (approval timeout exceeded)
    /// </summary>
    Expired = 3
}

