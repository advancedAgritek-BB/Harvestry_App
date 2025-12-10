namespace Harvestry.LabTests.Domain.Enums;

/// <summary>
/// Overall status of a lab test batch
/// </summary>
public enum LabTestStatus
{
    /// <summary>
    /// Sample submitted, awaiting results
    /// </summary>
    Pending = 0,

    /// <summary>
    /// All tests passed
    /// </summary>
    Passed = 1,

    /// <summary>
    /// One or more tests failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Product requires remediation based on results
    /// </summary>
    RequiresRemediation = 3,

    /// <summary>
    /// Test was voided/cancelled
    /// </summary>
    Voided = 4
}








