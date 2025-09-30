using System;
using System.Collections.Generic;
using System.Linq;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;

namespace Harvestry.Identity.Domain.Entities;

internal sealed partial class User
{
    internal static User Restore(
        Guid id,
        Email email,
        bool emailVerified,
        string firstName,
        string lastName,
        PhoneNumber? phoneNumber,
        bool phoneVerified,
        string displayName,
        string? passwordHash,
        string? passwordSalt,
        int failedLoginAttempts,
        DateTime? lockedUntil,
        DateTime? lastLoginAt,
        string? profilePhotoUrl,
        string languagePreference,
        string timezone,
        UserStatus status,
        IDictionary<string, object>? metadata,
        DateTime createdAt,
        DateTime updatedAt,
        Guid? createdBy,
        Guid? updatedBy,
        IEnumerable<UserSite>? userSites)
    {
        var user = new User(id)
        {
            Email = email,
            EmailVerified = emailVerified,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            PhoneVerified = phoneVerified,
            DisplayName = displayName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            FailedLoginAttempts = failedLoginAttempts,
            LockedUntil = lockedUntil,
            LastLoginAt = lastLoginAt,
            ProfilePhotoUrl = profilePhotoUrl,
            LanguagePreference = string.IsNullOrWhiteSpace(languagePreference) ? "en" : languagePreference,
            Timezone = string.IsNullOrWhiteSpace(timezone) ? "UTC" : timezone,
            Status = status,
            Metadata = metadata != null
                ? new Dictionary<string, object>(metadata, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy
        };

        if (userSites != null)
        {
            foreach (var site in userSites)
            {
                user._userSites.Add(site);
            }
        }

        return user;
    }
}
