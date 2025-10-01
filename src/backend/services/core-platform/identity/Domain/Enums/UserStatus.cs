namespace Harvestry.Identity.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is active and can login
    /// </summary>
    Active,

    /// <summary>
    /// User account is temporarily inactive
    /// </summary>
    Inactive,

    /// <summary>
    /// User account is suspended (e.g., for policy violations)
    /// </summary>
    Suspended,

    /// <summary>
    /// User has been terminated and cannot access the system
    /// </summary>
    Terminated
}

/// <summary>
/// Site status
/// </summary>
public enum SiteStatus
{
    /// <summary>
    /// Site is active and operational
    /// </summary>
    Active,

    /// <summary>
    /// Site is temporarily inactive
    /// </summary>
    Inactive,

    /// <summary>
    /// Site is pending setup/licensing
    /// </summary>
    Pending,

    /// <summary>
    /// Site operations are suspended
    /// </summary>
    Suspended
}

/// <summary>
/// Badge status
/// </summary>
public enum BadgeStatus
{
    /// <summary>
    /// Badge is active and can be used for authentication
    /// </summary>
    Active,

    /// <summary>
    /// Badge is inactive
    /// </summary>
    Inactive,

    /// <summary>
    /// Badge has been lost and should not be honored
    /// </summary>
    Lost,

    /// <summary>
    /// Badge has been revoked
    /// </summary>
    Revoked
}

/// <summary>
/// Badge type
/// </summary>
public enum BadgeType
{
    /// <summary>
    /// Physical RFID badge
    /// </summary>
    Physical,

    /// <summary>
    /// Virtual/mobile badge
    /// </summary>
    Virtual,

    /// <summary>
    /// Temporary badge for visitors
    /// </summary>
    Temp
}

/// <summary>
/// Login method used for authentication
/// </summary>
public enum LoginMethod
{
    /// <summary>
    /// Password-based login
    /// </summary>
    Password,

    /// <summary>
    /// Badge scan login
    /// </summary>
    Badge,

    /// <summary>
    /// Single Sign-On
    /// </summary>
    SSO,

    /// <summary>
    /// API key authentication
    /// </summary>
    ApiKey
}
