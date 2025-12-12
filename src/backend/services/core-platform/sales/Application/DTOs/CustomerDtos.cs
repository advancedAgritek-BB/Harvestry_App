namespace Harvestry.Sales.Application.DTOs;

/// <summary>
/// Customer summary for list views.
/// </summary>
public sealed record CustomerSummaryDto(
    Guid Id,
    Guid SiteId,
    string Name,
    string LicenseNumber,
    string? FacilityName,
    string? FacilityType,
    string? PrimaryContactName,
    string? Email,
    string? Phone,
    string LicenseVerifiedStatus,
    int OrderCount,
    bool IsActive
);

/// <summary>
/// Full customer detail.
/// </summary>
public sealed record CustomerDetailDto(
    Guid Id,
    Guid SiteId,
    string Name,
    string LicenseNumber,
    string? FacilityName,
    string? FacilityType,
    string? Address,
    string? City,
    string? State,
    string? Zip,
    string? PrimaryContactName,
    string? Email,
    string? Phone,
    string LicenseVerifiedStatus,
    DateTime? LicenseVerifiedAt,
    string? LicenseVerificationSource,
    string? LicenseVerificationNotes,
    string? MetrcRecipientId,
    bool IsActive,
    string? Notes,
    string? Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Paginated list response for customers.
/// </summary>
public sealed record CustomerListResponse(
    IReadOnlyList<CustomerSummaryDto> Customers,
    int TotalCount,
    int Page,
    int PageSize
);

/// <summary>
/// Request to create a new customer.
/// </summary>
public sealed record CreateCustomerRequest(
    string Name,
    string LicenseNumber,
    string? FacilityName,
    string? FacilityType,
    string? Address,
    string? City,
    string? State,
    string? Zip,
    string? PrimaryContactName,
    string? Email,
    string? Phone,
    string? MetrcRecipientId,
    string? Notes,
    string? Tags
);

/// <summary>
/// Request to update an existing customer.
/// </summary>
public sealed record UpdateCustomerRequest(
    string Name,
    string LicenseNumber,
    string? FacilityName,
    string? FacilityType,
    string? Address,
    string? City,
    string? State,
    string? Zip,
    string? PrimaryContactName,
    string? Email,
    string? Phone,
    string? MetrcRecipientId,
    string? Notes,
    string? Tags,
    bool? IsActive
);

/// <summary>
/// Request to update license verification status.
/// </summary>
public sealed record UpdateLicenseVerificationRequest(
    string Status,
    string? Source,
    string? Notes
);
