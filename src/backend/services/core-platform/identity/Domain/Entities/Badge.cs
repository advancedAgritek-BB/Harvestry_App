using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// Badge entity - physical or virtual authentication badge
/// </summary>
public sealed partial class Badge : Entity<Guid>
{
    // Private constructor for EF Core
    private Badge(Guid id) : base(id) { }

    private Badge(
        Guid id,
        Guid userId,
        Guid siteId,
        BadgeCode badgeCode,
        BadgeType badgeType) : base(id)
    {
        UserId = userId;
        SiteId = siteId;
        BadgeCode = badgeCode;
        BadgeType = badgeType;
        Status = BadgeStatus.Active;
        IssuedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid SiteId { get; private set; }
    public BadgeCode BadgeCode { get; private set; } = null!;
    public BadgeType BadgeType { get; private set; }
    public BadgeStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }
    public string? RevokeReason { get; private set; }
    // Private backing field for metadata
    private readonly Dictionary<string, object> _metadata = new();
    
    /// <summary>
    /// Read-only view of badge metadata
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata => _metadata;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Is this badge currently active and usable?
    /// </summary>
    public bool IsActive =>
        Status == BadgeStatus.Active &&
        (!ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow);

    /// <summary>
    /// Has this badge expired?
    /// </summary>
    public bool IsExpired =>
        ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Factory method to create a badge
    /// </summary>
    public static Badge Create(
        Guid userId,
        Guid siteId,
        BadgeCode badgeCode,
        BadgeType badgeType = BadgeType.Physical,
        DateTime? expiresAt = null)
    {
        var badge = new Badge(
            Guid.NewGuid(),
            userId,
            siteId,
            badgeCode,
            badgeType)
        {
            ExpiresAt = expiresAt
        };

        return badge;
    }

    /// <summary>
    /// Add or update a metadata entry
    /// </summary>
    public void AddOrUpdateMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));
        
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Metadata value cannot be null");

        _metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a metadata entry
    /// </summary>
    public void RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        if (_metadata.Remove(key))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Record usage of this badge for authentication
    /// </summary>
    public void RecordUsage()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Badge is {Status} and cannot be used");

        if (IsExpired)
            throw new InvalidOperationException("Badge has expired");

        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revoke this badge
    /// </summary>
    public void Revoke(Guid revokedBy, string reason)
    {
        if (Status == BadgeStatus.Revoked)
            throw new InvalidOperationException("Badge is already revoked");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revoke reason is required", nameof(reason));

        Status = BadgeStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevokeReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark this badge as lost
    /// </summary>
    public void MarkAsLost(Guid reportedBy)
    {
        if (Status == BadgeStatus.Lost || Status == BadgeStatus.Revoked)
            throw new InvalidOperationException($"Badge is already {Status}");

        Status = BadgeStatus.Lost;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = reportedBy;
        RevokeReason = "Badge reported as lost";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate this badge temporarily
    /// </summary>
    public void Deactivate()
    {
        if (Status == BadgeStatus.Revoked)
            throw new InvalidOperationException("Cannot deactivate a revoked badge");

        Status = BadgeStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate this badge
    /// </summary>
    public void Reactivate()
    {
        if (Status == BadgeStatus.Revoked)
            throw new InvalidOperationException("Cannot reactivate a revoked badge");

        if (Status == BadgeStatus.Lost)
            throw new InvalidOperationException("Cannot reactivate a lost badge");

        Status = BadgeStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set expiration date for this badge
    /// </summary>
    public void SetExpiration(DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
