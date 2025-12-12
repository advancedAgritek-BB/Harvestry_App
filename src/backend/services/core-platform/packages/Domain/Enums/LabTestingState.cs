namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Lab testing state for a package (METRC lab testing states)
/// </summary>
public enum LabTestingState
{
    /// <summary>
    /// Package has not been submitted for testing
    /// </summary>
    NotSubmitted = 0,

    /// <summary>
    /// Test results pending
    /// </summary>
    TestPending = 1,

    /// <summary>
    /// Package passed testing
    /// </summary>
    TestPassed = 2,

    /// <summary>
    /// Package failed testing
    /// </summary>
    TestFailed = 3,

    /// <summary>
    /// Testing not required for this package type
    /// </summary>
    NotRequired = 4
}









