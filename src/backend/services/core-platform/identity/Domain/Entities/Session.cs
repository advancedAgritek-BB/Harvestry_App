using System;
using System.Collections.Generic;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Identity.Domain.Entities;

/// <summary>
/// Session entity - represents an active user session
/// </summary>
public sealed partial class Session : Entity<Guid>
{
    // Private constructor for EF Core
    private Session(Guid id) : base(id) { }

    private Session(
        Guid id,
        Guid userId,
        Guid? siteId,
        string sessionToken,
        LoginMethod loginMethod,
        DateTime expiresAt) : base(id)
    {
        UserId = userId;
        SiteId = siteId;
        SessionToken = sessionToken;
        LoginMethod = loginMethod;
        SessionStart = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public Guid UserId { get; private set; }
    public Guid? SiteId { get; private set; }
    public string SessionToken { get; private set; } = null!;
    public string? RefreshToken { get; private set; }
    public string? DeviceFingerprint { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public LoginMethod LoginMethod { get; private set; }
    public DateTime SessionStart { get; private set; }
    public DateTime? SessionEnd { get; private set; }
    public DateTime LastActivity { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokeReason { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    /// <summary>
    /// Is this session currently active?
    /// </summary>
    public bool IsActive =>
        !IsRevoked &&
        !SessionEnd.HasValue &&
        ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Has this session expired?
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    /// <summary>
    /// Factory method to create a session
    /// </summary>
    public static Session Create(
        Guid userId,
        string sessionToken,
        LoginMethod loginMethod,
        TimeSpan expirationDuration,
        Guid? siteId = null,
        string? refreshToken = null,
        string? deviceFingerprint = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Validate inputs
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new ArgumentException("Session token is required", nameof(sessionToken));
            
        if (expirationDuration <= TimeSpan.Zero)
            throw new ArgumentException("Expiration duration must be positive", nameof(expirationDuration));

        var session = new Session(
            Guid.NewGuid(),
            userId,
            siteId,
            sessionToken,
            loginMethod,
            DateTime.UtcNow.Add(expirationDuration))
        {
            RefreshToken = refreshToken,
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        return session;
    }

    /// <summary>
    /// Record activity to keep session alive
    /// </summary>
    public void RecordActivity()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot record activity on inactive session");

        LastActivity = DateTime.UtcNow;
    }

    /// <summary>
    /// Extend session expiration
    /// </summary>
    public void ExtendExpiration(TimeSpan extensionDuration)
    {
        if (extensionDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(extensionDuration), "Extension duration must be positive");
            
        if (!IsActive)
            throw new InvalidOperationException("Cannot extend expired or revoked session");

        // Add to existing expiration instead of replacing
        ExpiresAt = ExpiresAt.Add(extensionDuration);
    }

    /// <summary>
    /// Revoke this session (logout)
    /// </summary>
    public void Revoke(string? reason = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Session is already revoked");

        IsRevoked = true;
        SessionEnd = DateTime.UtcNow;
        RevokeReason = reason ?? "User logout";
    }

    /// <summary>
    /// End this session normally
    /// </summary>
    public void End()
    {
        if (SessionEnd.HasValue)
            throw new InvalidOperationException("Session is already ended");

        SessionEnd = DateTime.UtcNow;
    }

    /// <summary>
    /// Update refresh token
    /// </summary>
    public void UpdateRefreshToken(string refreshToken)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update refresh token on inactive session");

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty", nameof(refreshToken));

        RefreshToken = refreshToken;
    }

    /// <summary>
    /// Validate session token
    /// </summary>
    public bool ValidateToken(string token)
    {
        return IsActive && SessionToken == token;
    }
}
