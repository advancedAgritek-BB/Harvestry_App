using Harvestry.Labor.Domain.Entities;

namespace Harvestry.Labor.Application.Interfaces;

/// <summary>
/// Repository interface for Team aggregate
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Get a team by ID
    /// </summary>
    Task<Team?> GetByIdAsync(Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Get a team by ID with members loaded
    /// </summary>
    Task<Team?> GetByIdWithMembersAsync(Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Get all teams for a site
    /// </summary>
    Task<IReadOnlyList<Team>> GetBySiteIdAsync(Guid siteId, CancellationToken ct = default);

    /// <summary>
    /// Get teams where user is a member
    /// </summary>
    Task<IReadOnlyList<Team>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get teams where user is a team lead
    /// </summary>
    Task<IReadOnlyList<Team>> GetByTeamLeadAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Check if a team name already exists for the site
    /// </summary>
    Task<bool> ExistsWithNameAsync(Guid siteId, string name, Guid? excludeTeamId = null, CancellationToken ct = default);

    /// <summary>
    /// Add a new team
    /// </summary>
    Task AddAsync(Team team, CancellationToken ct = default);

    /// <summary>
    /// Update an existing team
    /// </summary>
    Task UpdateAsync(Team team, CancellationToken ct = default);

    /// <summary>
    /// Delete a team
    /// </summary>
    Task DeleteAsync(Guid teamId, CancellationToken ct = default);
}
