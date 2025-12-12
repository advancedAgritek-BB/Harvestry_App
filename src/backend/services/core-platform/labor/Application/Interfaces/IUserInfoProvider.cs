namespace Harvestry.Labor.Application.Interfaces;

/// <summary>
/// Interface for retrieving user information needed for team operations
/// </summary>
public interface IUserInfoProvider
{
    /// <summary>
    /// Get basic user info for a single user
    /// </summary>
    Task<UserInfo?> GetUserInfoAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get basic user info for multiple users
    /// </summary>
    Task<IReadOnlyDictionary<Guid, UserInfo>> GetUserInfoBatchAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);

    /// <summary>
    /// Get all users for a site
    /// </summary>
    Task<IReadOnlyList<UserInfo>> GetUsersBySiteAsync(Guid siteId, CancellationToken ct = default);

    /// <summary>
    /// Check if user has manager/supervisor role at site
    /// </summary>
    Task<bool> IsManagerOrSupervisorAsync(Guid userId, Guid siteId, CancellationToken ct = default);
}

/// <summary>
/// Basic user information needed for team operations
/// </summary>
public record UserInfo(
    Guid UserId,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string? Role);
