using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// User aggregate root - represents a system user account
/// </summary>
public sealed partial class User : AggregateRoot<Guid>
{
    private readonly List<UserSite> _userSites = new();

    // Private constructor for EF Core
    private User(Guid id) : base(id) { }

    private User(
        Guid id,
        Email email,
        string firstName,
        string lastName,
        PhoneNumber? phoneNumber = null) : base(id)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        DisplayName = $"{firstName} {lastName}";
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Email Email { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool PhoneVerified { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public string? PasswordSalt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }
    public string LanguagePreference { get; private set; } = "en";
    public string Timezone { get; private set; } = "UTC";
    public UserStatus Status { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public IReadOnlyCollection<UserSite> UserSites => _userSites.AsReadOnly();

    /// <summary>
    /// Is this user account locked due to failed login attempts?
    /// </summary>
    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Can this user login with a password?
    /// </summary>
    public bool CanLoginWithPassword => !string.IsNullOrEmpty(PasswordHash) && !IsLocked && Status == UserStatus.Active;

    /// <summary>
    /// Factory method to create a new user
    /// </summary>
    public static User Create(
        Email email,
        string firstName,
        string lastName,
        PhoneNumber? phoneNumber = null,
        Guid? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var user = new User(Guid.NewGuid(), email, firstName, lastName, phoneNumber)
        {
            CreatedBy = createdBy
        };

        return user;
    }

    /// <summary>
    /// Set password for this user
    /// </summary>
    public void SetPassword(string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(passwordSalt))
            throw new ArgumentException("Password salt cannot be empty", nameof(passwordSalt));

        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a successful login
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a failed login attempt
    /// </summary>
    public void RecordFailedLoginAttempt(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    /// <summary>
    /// Unlock this user account
    /// </summary>
    public void Unlock(Guid unlockedBy)
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = unlockedBy;
    }

    /// <summary>
    /// Update user profile information. Null parameters preserve existing values (no change).
    /// </summary>
    /// <param name="firstName">New first name, or null to keep existing</param>
    /// <param name="lastName">New last name, or null to keep existing</param>
    /// <param name="phoneNumber">New phone number, or null to keep existing</param>
    /// <param name="profilePhotoUrl">New profile photo URL, or null to keep existing</param>
    /// <param name="languagePreference">New language preference, or null to keep existing</param>
    /// <param name="timezone">New timezone, or null to keep existing</param>
    /// <param name="updatedBy">User ID who is performing the update</param>
    public void UpdateProfile(
        string? firstName = null,
        string? lastName = null,
        PhoneNumber? phoneNumber = null,
        string? profilePhotoUrl = null,
        string? languagePreference = null,
        string? timezone = null,
        Guid? updatedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        if (phoneNumber != null)
            PhoneNumber = phoneNumber;

        if (!string.IsNullOrWhiteSpace(profilePhotoUrl))
            ProfilePhotoUrl = profilePhotoUrl;

        if (!string.IsNullOrWhiteSpace(languagePreference))
            LanguagePreference = languagePreference;

        if (!string.IsNullOrWhiteSpace(timezone))
            Timezone = timezone;

        // Update display name if we have meaningful names
        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
            DisplayName = $"{FirstName} {LastName}";

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Change user status
    /// </summary>
    public void ChangeStatus(UserStatus newStatus, Guid changedBy, string? reason = null)
    {
        if (Status == newStatus)
            return;

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = changedBy;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Metadata["status_change_reason"] = reason;
            Metadata["status_changed_at"] = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Suspend this user account
    /// </summary>
    public void Suspend(Guid suspendedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension must include a reason.", nameof(reason));
            
        ChangeStatus(UserStatus.Suspended, suspendedBy, reason);
    }

    /// <summary>
    /// Terminate this user account
    /// </summary>
    public void Terminate(Guid terminatedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Termination reason is required", nameof(reason));

        ChangeStatus(UserStatus.Terminated, terminatedBy, reason);
    }

    /// <summary>
    /// Reactivate this user account
    /// </summary>
    public void Reactivate(Guid reactivatedBy)
    {
        ChangeStatus(UserStatus.Active, reactivatedBy);
    }

    /// <summary>
    /// Verify email address
    /// </summary>
    public void VerifyEmail()
    {
        EmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verify phone number
    /// </summary>
    public void VerifyPhone()
    {
        if (PhoneNumber == null)
            throw new InvalidOperationException("Cannot verify phone - no phone number set");

        PhoneVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assign user to a site with a role
    /// </summary>
    public void AssignToSite(Guid siteId, Guid roleId, bool isPrimarySite, Guid assignedBy)
    {
        // Check if already assigned
        if (_userSites.Exists(us => us.SiteId == siteId && us.RevokedAt == null))
            throw new InvalidOperationException($"User is already assigned to site {siteId}");

        // If marking as primary, ensure no other active primary site exists
        if (isPrimarySite)
        {
            var existingPrimary = _userSites.Find(us => us.IsPrimarySite && us.RevokedAt == null);
            if (existingPrimary != null)
            {
                throw new InvalidOperationException(
                    $"User already has a primary site (SiteId: {existingPrimary.SiteId}). "
                    + "Revoke the existing primary site first or set isPrimarySite to false.");
            }
        }

        var userSite = UserSite.Create(Id, siteId, roleId, isPrimarySite, assignedBy);
        _userSites.Add(userSite);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove user from a site
    /// </summary>
    public void RemoveFromSite(Guid siteId, Guid revokedBy, string reason)
    {
        var userSite = _userSites.Find(us => us.SiteId == siteId && us.RevokedAt == null);
        if (userSite == null)
            throw new InvalidOperationException($"User is not assigned to site {siteId}");

        userSite.Revoke(revokedBy, reason);
        UpdatedAt = DateTime.UtcNow;
    }
}
