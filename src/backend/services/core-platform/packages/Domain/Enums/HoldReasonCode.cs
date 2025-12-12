namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Reason codes for placing inventory on hold
/// </summary>
public enum HoldReasonCode
{
    /// <summary>
    /// COA/Lab test failed
    /// </summary>
    CoaFailed = 0,

    /// <summary>
    /// Waiting for COA/Lab results
    /// </summary>
    CoaPending = 1,

    /// <summary>
    /// Contamination detected or suspected
    /// </summary>
    Contamination = 2,

    /// <summary>
    /// Quality issue identified
    /// </summary>
    QualityIssue = 3,

    /// <summary>
    /// Regulatory hold required by authorities
    /// </summary>
    Regulatory = 4,

    /// <summary>
    /// Customer return requiring inspection
    /// </summary>
    CustomerReturn = 5,

    /// <summary>
    /// Under investigation for discrepancy
    /// </summary>
    Investigation = 6,

    /// <summary>
    /// Audit review required
    /// </summary>
    AuditReview = 7,

    /// <summary>
    /// Physical damage detected
    /// </summary>
    Damaged = 8,

    /// <summary>
    /// Product expired or expiring
    /// </summary>
    Expired = 9,

    /// <summary>
    /// Other reason - requires notes
    /// </summary>
    Other = 99
}




