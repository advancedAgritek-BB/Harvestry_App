using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;

namespace Harvestry.Identity.Domain.Entities;

public sealed partial class Site
{
    public static Site Restore(
        Guid id,
        Guid orgId,
        string siteName,
        string siteCode,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateProvince,
        string? postalCode,
        string country,
        string timezone,
        string? licenseNumber,
        string? licenseType,
        DateTime? licenseExpiration,
        string siteType,
        SiteStatus status,
        IDictionary<string, object>? sitePolicies,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        DateTime updatedAt,
        // METRC fields
        long? metrcFacilityId = null,
        FacilityType? facilityType = null,
        string? metrcApiKeyEncrypted = null,
        string? metrcUserKeyEncrypted = null,
        string? stateCode = null,
        bool isMetrcEnabled = false,
        DateTime? metrcLastSyncAt = null,
        string? metrcSyncStatus = null)
    {
        var site = new Site(id)
        {
            OrgId = orgId,
            SiteName = siteName,
            SiteCode = siteCode,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            StateProvince = stateProvince,
            PostalCode = postalCode,
            Country = string.IsNullOrWhiteSpace(country) ? "US" : country,
            Timezone = string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone,
            LicenseNumber = licenseNumber,
            LicenseType = licenseType,
            LicenseExpiration = licenseExpiration,
            SiteType = string.IsNullOrWhiteSpace(siteType) ? "cultivation" : siteType,
            Status = status,
            SitePolicies = sitePolicies != null
                ? new Dictionary<string, object>(sitePolicies)
                : new Dictionary<string, object>(),
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            // METRC fields
            MetrcFacilityId = metrcFacilityId,
            FacilityType = facilityType,
            MetrcApiKeyEncrypted = metrcApiKeyEncrypted,
            MetrcUserKeyEncrypted = metrcUserKeyEncrypted,
            StateCode = stateCode,
            IsMetrcEnabled = isMetrcEnabled,
            MetrcLastSyncAt = metrcLastSyncAt,
            MetrcSyncStatus = metrcSyncStatus
        };

        return site;
    }
}
