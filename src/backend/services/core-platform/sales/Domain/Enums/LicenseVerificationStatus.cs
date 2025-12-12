namespace Harvestry.Sales.Domain.Enums;

/// <summary>
/// Status of a customer's license verification.
/// </summary>
public enum LicenseVerificationStatus
{
    /// <summary>License has not been verified yet.</summary>
    Unknown,

    /// <summary>License verification is in progress.</summary>
    Pending,

    /// <summary>License has been verified as valid.</summary>
    Verified,

    /// <summary>License verification failed (expired, invalid, etc.).</summary>
    Failed
}
