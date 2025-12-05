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

    // METRC Compliance Fields
    /// <summary>
    /// METRC's internal facility identifier (returned from Facilities API)
    /// </summary>
    public long? MetrcFacilityId { get; private set; }

    /// <summary>
    /// Type of cannabis facility (Cultivator, Processor, CultivatorProcessor, Lab)
    /// </summary>
    public FacilityType? FacilityType { get; private set; }

    /// <summary>
    /// Encrypted METRC vendor/integrator API key for this facility
    /// </summary>
    public string? MetrcApiKeyEncrypted { get; private set; }

    /// <summary>
    /// Encrypted METRC user API key for this facility
    /// </summary>
    public string? MetrcUserKeyEncrypted { get; private set; }

    /// <summary>
    /// Two-letter state code for jurisdiction-specific rules (IL, CO, NY, etc.)
    /// </summary>
    public string? StateCode { get; private set; }

    /// <summary>
    /// Whether METRC integration is enabled for this site
    /// </summary>
    public bool IsMetrcEnabled { get; private set; }

    /// <summary>
    /// Last successful sync with METRC
    /// </summary>
    public DateTime? MetrcLastSyncAt { get; private set; }

    /// <summary>
    /// METRC sync status message (for troubleshooting)
    /// </summary>
    public string? MetrcSyncStatus { get; private set; }

    /// <summary>
    /// Is this site currently operational?
    /// </summary>
    public bool IsOperational => Status == SiteStatus.Active;

    /// <summary>
    /// Is the compliance license valid?
    /// Note: LicenseExpiration is assumed to be stored in UTC
    /// </summary>
    public bool IsLicenseValid
    {
        get
        {
            if (string.IsNullOrEmpty(LicenseNumber))
                return false;

            if (!LicenseExpiration.HasValue)
                return true;

            // Direct UTC comparison (license expiration is stored in UTC)
            return LicenseExpiration.Value > DateTime.UtcNow;
        }
    }

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

    /// <summary>
    /// Configure METRC integration for this site
    /// </summary>
    public void ConfigureMetrc(
        long metrcFacilityId,
        FacilityType facilityType,
        string stateCode,
        string metrcApiKeyEncrypted,
        string metrcUserKeyEncrypted)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
            throw new ArgumentException("State code is required for METRC configuration", nameof(stateCode));

        if (stateCode.Length != 2)
            throw new ArgumentException("State code must be a two-letter code (e.g., IL, CO)", nameof(stateCode));

        if (string.IsNullOrWhiteSpace(metrcApiKeyEncrypted))
            throw new ArgumentException("METRC API key is required", nameof(metrcApiKeyEncrypted));

        if (string.IsNullOrWhiteSpace(metrcUserKeyEncrypted))
            throw new ArgumentException("METRC user key is required", nameof(metrcUserKeyEncrypted));

        MetrcFacilityId = metrcFacilityId;
        FacilityType = facilityType;
        StateCode = stateCode.ToUpperInvariant();
        MetrcApiKeyEncrypted = metrcApiKeyEncrypted;
        MetrcUserKeyEncrypted = metrcUserKeyEncrypted;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enable METRC integration for this site
    /// </summary>
    public void EnableMetrc()
    {
        if (!MetrcFacilityId.HasValue)
            throw new InvalidOperationException("Cannot enable METRC without configuring facility ID first");

        if (string.IsNullOrWhiteSpace(MetrcApiKeyEncrypted) || string.IsNullOrWhiteSpace(MetrcUserKeyEncrypted))
            throw new InvalidOperationException("Cannot enable METRC without API keys configured");

        IsMetrcEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disable METRC integration for this site
    /// </summary>
    public void DisableMetrc()
    {
        IsMetrcEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC sync status after a sync operation
    /// </summary>
    public void UpdateMetrcSyncStatus(bool success, string? statusMessage = null)
    {
        if (success)
        {
            MetrcLastSyncAt = DateTime.UtcNow;
            MetrcSyncStatus = statusMessage ?? "Sync successful";
        }
        else
        {
            MetrcSyncStatus = statusMessage ?? "Sync failed";
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update METRC API credentials (encrypted)
    /// </summary>
    public void UpdateMetrcCredentials(string metrcApiKeyEncrypted, string metrcUserKeyEncrypted)
    {
        if (string.IsNullOrWhiteSpace(metrcApiKeyEncrypted))
            throw new ArgumentException("METRC API key is required", nameof(metrcApiKeyEncrypted));

        if (string.IsNullOrWhiteSpace(metrcUserKeyEncrypted))
            throw new ArgumentException("METRC user key is required", nameof(metrcUserKeyEncrypted));

        MetrcApiKeyEncrypted = metrcApiKeyEncrypted;
        MetrcUserKeyEncrypted = metrcUserKeyEncrypted;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if METRC is properly configured and enabled
    /// </summary>
    public bool IsMetrcReady => IsMetrcEnabled 
        && MetrcFacilityId.HasValue 
        && !string.IsNullOrWhiteSpace(MetrcApiKeyEncrypted) 
        && !string.IsNullOrWhiteSpace(MetrcUserKeyEncrypted)
        && !string.IsNullOrWhiteSpace(StateCode);
}
