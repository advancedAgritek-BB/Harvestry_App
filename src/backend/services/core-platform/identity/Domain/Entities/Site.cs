using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// Site aggregate root - represents a physical grow/processing location
/// </summary>
public sealed partial class Site : AggregateRoot<Guid>
{
    // Private constructor for EF Core
    private Site(Guid id) : base(id) { }

    private Site(
        Guid id,
        Guid orgId,
        string siteName,
        string siteCode,
        string timezone) : base(id)
    {
        OrgId = orgId;
        SiteName = siteName;
        SiteCode = siteCode;
        Timezone = timezone;
        Status = SiteStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid OrgId { get; private set; }
    public string SiteName { get; private set; } = null!;
    public string SiteCode { get; private set; } = null!;
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? StateProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string Country { get; private set; } = "US";
    public string Timezone { get; private set; } = null!;
    public string? LicenseNumber { get; private set; }
    public string? LicenseType { get; private set; }
    public DateTime? LicenseExpiration { get; private set; }
    public string SiteType { get; private set; } = "cultivation";
    public SiteStatus Status { get; private set; }
    public Dictionary<string, object> SitePolicies { get; private set; } = new();
    public Dictionary<string, object> Metadata { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Is this site currently operational?
    /// </summary>
    public bool IsOperational => Status == SiteStatus.Active;

    /// <summary>
    /// Is the compliance license valid?
    /// </summary>
    public bool IsLicenseValid =>
        !string.IsNullOrEmpty(LicenseNumber) &&
        (!LicenseExpiration.HasValue || LicenseExpiration.Value > DateTime.UtcNow);

    /// <summary>
    /// Factory method to create a site
    /// </summary>
    public static Site Create(
        Guid orgId,
        string siteName,
        string siteCode,
        string timezone = "America/Denver")
    {
        if (string.IsNullOrWhiteSpace(siteName))
            throw new ArgumentException("Site name is required", nameof(siteName));

        if (string.IsNullOrWhiteSpace(siteCode))
            throw new ArgumentException("Site code is required", nameof(siteCode));

        return new Site(Guid.NewGuid(), orgId, siteName, siteCode, timezone);
    }

    /// <summary>
    /// Update site address
    /// </summary>
    public void UpdateAddress(
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateProvince,
        string? postalCode,
        string? country = null)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;

        if (!string.IsNullOrWhiteSpace(country))
            Country = country;

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update compliance license information
    /// </summary>
    public void UpdateLicense(
        string licenseNumber,
        string licenseType,
        DateTime? licenseExpiration)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required", nameof(licenseNumber));

        if (string.IsNullOrWhiteSpace(licenseType))
            throw new ArgumentException("License type is required", nameof(licenseType));

        LicenseNumber = licenseNumber;
        LicenseType = licenseType;
        LicenseExpiration = licenseExpiration;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Change site status
    /// </summary>
    public void ChangeStatus(SiteStatus newStatus)
    {
        if (Status == newStatus)
            return;

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate the site
    /// </summary>
    public void Activate()
    {
        if (!IsLicenseValid)
            throw new InvalidOperationException("Cannot activate site without valid license");

        ChangeStatus(SiteStatus.Active);
    }

    /// <summary>
    /// Suspend the site
    /// </summary>
    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason is required", nameof(reason));

        ChangeStatus(SiteStatus.Suspended);
        Metadata["suspension_reason"] = reason;
        Metadata["suspended_at"] = DateTime.UtcNow;
    }

    /// <summary>
    /// Update or add a site policy
    /// </summary>
    public void SetPolicy(string policyKey, object policyValue)
    {
        if (string.IsNullOrWhiteSpace(policyKey))
            throw new ArgumentException("Policy key is required", nameof(policyKey));

        SitePolicies[policyKey] = policyValue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get a site policy value
    /// </summary>
    public bool GetPolicyBool(string policyKey, bool defaultValue = false)
    {
        if (SitePolicies.TryGetValue(policyKey, out var value) && value is bool boolValue)
            return boolValue;

        return defaultValue;
    }
}
