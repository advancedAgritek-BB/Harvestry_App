using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Identity.Domain.Entities;
using Harvestry.Identity.Domain.ValueObjects;

namespace Harvestry.Identity.Application.Interfaces;

/// <summary>
/// Repository for User aggregate
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Badge entity
/// </summary>
public interface IBadgeRepository
{
    Task<Badge?> GetByIdAsync(Guid badgeId, CancellationToken cancellationToken = default);
    Task<Badge?> GetByCodeAsync(BadgeCode badgeCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Badge>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Badge>> GetActiveBySiteIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<Badge> AddAsync(Badge badge, CancellationToken cancellationToken = default);
    Task UpdateAsync(Badge badge, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Session entity
/// </summary>
public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<Session?> GetByTokenAsync(string sessionToken, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    Task UpdateAsync(Session session, CancellationToken cancellationToken = default);
    Task<int> RevokeAllByUserIdAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Role entity
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for Site aggregate
/// </summary>
public interface ISiteRepository
{
    Task<Site?> GetByIdAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<Site?> GetByCodeAsync(string siteCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Site>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken = default);
    Task<Site> AddAsync(Site site, CancellationToken cancellationToken = default);
    Task UpdateAsync(Site site, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository for database-level operations (calling PostgreSQL functions)
/// </summary>
public interface IDatabaseRepository
{
    /// <summary>
    /// Call the check_abac_permission PostgreSQL function
    /// </summary>
    Task<(bool granted, bool requiresTwoPersonApproval, string? denyReason)> CheckAbacPermissionAsync(
        Guid userId,
        string action,
        string resourceType,
        Guid siteId,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Call the check_task_gating PostgreSQL function
    /// </summary>
    Task<(bool isAllowed, List<TaskGatingRequirement> missingRequirements)> CheckTaskGatingAsync(
        Guid userId,
        string taskType,
        Guid siteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve task gating requirements for a specific task type and site
    /// </summary>
    /// <param name="taskType">The task type to query requirements for (must not be null or empty)</param>
    /// <param name="siteId">Optional site ID to filter requirements by site</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of task gating requirements</returns>
    Task<IReadOnlyCollection<TaskGatingRequirement>> GetTaskGatingRequirementsAsync(
        string taskType,
        Guid? siteId = null,
        CancellationToken cancellationToken = default);
}
