using Harvestry.Sales.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Sales.Domain.Entities;

/// <summary>
/// Represents a customer/account in the CRM.
/// </summary>
public sealed class Customer : AggregateRoot<Guid>
{
    private Customer(Guid id) : base(id) { }

    private Customer(
        Guid id,
        Guid siteId,
        string name,
        string licenseNumber,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("SiteId cannot be empty", nameof(siteId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new ArgumentException("License number is required", nameof(licenseNumber));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("CreatedByUserId cannot be empty", nameof(createdByUserId));

        SiteId = siteId;
        Name = name.Trim();
        LicenseNumber = licenseNumber.Trim();
        LicenseVerifiedStatus = LicenseVerificationStatus.Unknown;
        IsActive = true;

        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = createdByUserId;
    }

    public Guid SiteId { get; private set; }

    // Business information
    public string Name { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string? FacilityName { get; private set; }
    public string? FacilityType { get; private set; }

    // Address
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Zip { get; private set; }

    // Contact
    public string? PrimaryContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    // Compliance / License verification
    public LicenseVerificationStatus LicenseVerifiedStatus { get; private set; }
    public DateTime? LicenseVerifiedAt { get; private set; }
    public string? LicenseVerificationSource { get; private set; }
    public string? LicenseVerificationNotes { get; private set; }
    public string? MetrcRecipientId { get; private set; }

    // Metadata
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }
    public string? Tags { get; private set; } // Comma-separated

    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    public static Customer Create(
        Guid siteId,
        string name,
        string licenseNumber,
        Guid createdByUserId)
    {
        return new Customer(
            Guid.NewGuid(),
            siteId,
            name,
            licenseNumber,
            createdByUserId);
    }

    public void UpdateBusinessInfo(
        string name,
        string licenseNumber,
        string? facilityName,
        string? facilityType,
        Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new ArgumentException("License number is required", nameof(licenseNumber));

        Name = name.Trim();
        LicenseNumber = licenseNumber.Trim();
        FacilityName = facilityName?.Trim();
        FacilityType = facilityType?.Trim();
        Touch(userId);
    }

    public void UpdateAddress(
        string? address,
        string? city,
        string? state,
        string? zip,
        Guid userId)
    {
        Address = address?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        Zip = zip?.Trim();
        Touch(userId);
    }

    public void UpdateContact(
        string? contactName,
        string? email,
        string? phone,
        Guid userId)
    {
        PrimaryContactName = contactName?.Trim();
        Email = email?.Trim();
        Phone = phone?.Trim();
        Touch(userId);
    }

    public void SetLicenseVerificationStatus(
        LicenseVerificationStatus status,
        string? source,
        string? notes,
        Guid userId)
    {
        LicenseVerifiedStatus = status;
        LicenseVerifiedAt = DateTime.UtcNow;
        LicenseVerificationSource = source?.Trim();
        LicenseVerificationNotes = notes?.Trim();
        Touch(userId);
    }

    public void SetMetrcRecipientId(string? recipientId, Guid userId)
    {
        MetrcRecipientId = recipientId?.Trim();
        Touch(userId);
    }

    public void SetNotes(string? notes, Guid userId)
    {
        Notes = notes?.Trim();
        Touch(userId);
    }

    public void SetTags(string? tags, Guid userId)
    {
        Tags = tags?.Trim();
        Touch(userId);
    }

    public void Activate(Guid userId)
    {
        IsActive = true;
        Touch(userId);
    }

    public void Deactivate(Guid userId)
    {
        IsActive = false;
        Touch(userId);
    }

    private void Touch(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty", nameof(userId));
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = userId;
    }
}
