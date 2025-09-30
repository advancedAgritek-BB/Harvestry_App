using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Application.Interfaces;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.Enums;
using Harvestry.Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Application.Services;

/// <summary>
/// Service for badge-based authentication and session management
/// </summary>
public sealed class BadgeAuthService : IBadgeAuthService
{
    private readonly IBadgeRepository _badgeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<BadgeAuthService> _logger;
    private readonly TimeSpan _badgeSessionDuration = TimeSpan.FromHours(12); // 12-hour shifts

    public BadgeAuthService(
        IBadgeRepository badgeRepository,
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        ILogger<BadgeAuthService> logger)
    {
        _badgeRepository = badgeRepository ?? throw new ArgumentNullException(nameof(badgeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticate a user via badge scan
    /// </summary>
    public async Task<BadgeLoginResult> LoginWithBadgeAsync(
        string badgeCode,
        Guid siteId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(badgeCode))
            throw new ArgumentException("Badge code is required", nameof(badgeCode));

        _logger.LogInformation(
            "Badge login attempt with code {BadgeCode} at site {SiteId}",
            MaskBadgeCode(badgeCode), siteId);

        try
        {
            // Parse and validate badge code
            BadgeCode parsedBadgeCode;
            try
            {
                parsedBadgeCode = BadgeCode.Create(badgeCode);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid badge code format: {Message}", ex.Message);
                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid badge code format"
                };
            }

            // Find badge
            var badge = await _badgeRepository.GetByCodeAsync(parsedBadgeCode, cancellationToken);
            if (badge == null)
            {
                _logger.LogWarning("Badge not found: {BadgeCode}", MaskBadgeCode(badgeCode));
                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid badge or credentials"
                };
            }

            // Verify badge is active and for correct site
            if (!badge.IsActive)
            {
                _logger.LogWarning(
                    "Badge {BadgeId} is not active (status: {Status})",
                    badge.Id, badge.Status);

                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid badge or credentials"
                };
            }

            if (badge.SiteId != siteId)
            {
                _logger.LogWarning(
                    "Badge {BadgeId} belongs to site {BadgeSiteId} but login attempted at site {LoginSiteId}",
                    badge.Id, badge.SiteId, siteId);

                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = "Invalid badge or credentials"
                };
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(badge.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found for badge {BadgeId}", badge.UserId, badge.Id);
                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = "User account not found"
                };
            }

            // Verify user is active
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning(
                    "User {UserId} login attempt with status {Status}",
                    user.Id, user.Status);

                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = $"User account is {user.Status.ToString().ToLower()}"
                };
            }

            // Check if user is locked
            if (user.IsLocked)
            {
                _logger.LogWarning(
                    "User {UserId} account is locked until {LockedUntil}",
                    user.Id, user.LockedUntil);

                return new BadgeLoginResult
                {
                    Success = false,
                    ErrorMessage = $"Account is locked until {user.LockedUntil:HH:mm}"
                };
            }

            // Generate secure session token
            var sessionToken = GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.Add(_badgeSessionDuration);

            // Create session
            var session = Session.Create(
                user.Id,
                sessionToken,
                LoginMethod.Badge,
                _badgeSessionDuration,
                siteId,
                null, // No refresh token for badge sessions
                null, // No device fingerprint
                ipAddress,
                userAgent);

            await _sessionRepository.AddAsync(session, cancellationToken);

            // Record badge usage
            badge.RecordUsage();
            await _badgeRepository.UpdateAsync(badge, cancellationToken);

            // Record successful login on user
            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);

            _logger.LogInformation(
                "Badge login successful for user {UserId} at site {SiteId}. Session {SessionId} expires at {ExpiresAt}",
                user.Id, siteId, session.Id, expiresAt);

            return new BadgeLoginResult
            {
                Success = true,
                SessionId = session.Id,
                SessionToken = sessionToken,
                UserId = user.Id,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during badge login for code {BadgeCode}", MaskBadgeCode(badgeCode));
            throw;
        }
    }

    /// <summary>
    /// Revoke a badge
    /// </summary>
    public async Task<bool> RevokeBadgeAsync(
        Guid badgeId,
        Guid revokedBy,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revoke reason is required", nameof(reason));

        _logger.LogInformation(
            "Revoking badge {BadgeId} by user {RevokedBy}. Reason: {Reason}",
            badgeId, revokedBy, reason);

        try
        {
            var badge = await _badgeRepository.GetByIdAsync(badgeId, cancellationToken);
            if (badge == null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", badgeId);
                return false;
            }

            // Revoke badge
            badge.Revoke(revokedBy, reason);
            await _badgeRepository.UpdateAsync(badge, cancellationToken);

            // Revoke all active sessions for this user
            await _sessionRepository.RevokeAllByUserIdAsync(
                badge.UserId,
                $"Badge revoked: {reason}",
                cancellationToken);

            _logger.LogInformation("Badge {BadgeId} revoked successfully", badgeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking badge {BadgeId}", badgeId);
            throw;
        }
    }

    /// <summary>
    /// Get active sessions for a user
    /// </summary>
    public async Task<IEnumerable<SessionInfo>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching active sessions for user {UserId}", userId);

        try
        {
            var sessions = await _sessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            return sessions
                .Where(s => s.IsActive)
                .Select(s => new SessionInfo
            {
                SessionId = s.Id,
                UserId = s.UserId,
                SiteId = s.SiteId,
                SessionStart = s.SessionStart,
                LastActivity = s.LastActivity,
                ExpiresAt = s.ExpiresAt,
                IpAddress = s.IpAddress,
                UserAgent = s.UserAgent,
                LoginMethod = s.LoginMethod.ToString()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active sessions for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Revoke a session (logout)
    /// </summary>
    public async Task<bool> RevokeSessionAsync(
        Guid sessionId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking session {SessionId}. Reason: {Reason}", sessionId, reason);

        try
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                return false;
            }

            session.Revoke(reason);
            await _sessionRepository.UpdateAsync(session, cancellationToken);

            _logger.LogInformation("Session {SessionId} revoked successfully", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Generate a cryptographically secure session token
    /// </summary>
    private static string GenerateSecureToken()
    {
        // Generate 32 bytes (256 bits) of random data
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert to base64 URL-safe string
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Mask badge code for logging (show first 4 and last 4 characters)
    /// </summary>
    private static string MaskBadgeCode(string badgeCode)
    {
        if (string.IsNullOrEmpty(badgeCode) || badgeCode.Length <= 8)
            return "****";

        return $"{badgeCode.Substring(0, 4)}****{badgeCode.Substring(badgeCode.Length - 4)}";
    }
}
