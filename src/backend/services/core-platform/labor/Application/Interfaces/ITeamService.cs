using Harvestry.Labor.Application.DTOs;

namespace Harvestry.Labor.Application.Interfaces;

/// <summary>
/// Service interface for team management operations
/// </summary>
public interface ITeamService
{
    /// <summary>
    /// Get all teams for the current site
    /// </summary>
    Task<IReadOnlyList<TeamDto>> GetTeamsAsync(Guid siteId, CancellationToken ct = default);

    /// <summary>
    /// Get a team by ID with its members
    /// </summary>
    Task<TeamDetailDto?> GetTeamDetailAsync(Guid teamId, CancellationToken ct = default);

    /// <summary>
    /// Get teams where the user is a lead or has manager/supervisor role
    /// </summary>
    Task<IReadOnlyList<TeamDto>> GetManagedTeamsAsync(Guid userId, Guid siteId, CancellationToken ct = default);

    /// <summary>
    /// Get all members the current user can assign to tasks
    /// Groups members by their teams
    /// </summary>
    Task<AssignableMembersResponse> GetAssignableMembersAsync(Guid userId, Guid siteId, CancellationToken ct = default);

    /// <summary>
    /// Create a new team
    /// </summary>
    Task<TeamDto> CreateTeamAsync(Guid siteId, CreateTeamRequest request, Guid createdBy, CancellationToken ct = default);

    /// <summary>
    /// Update an existing team
    /// </summary>
    Task<TeamDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid updatedBy, CancellationToken ct = default);

    /// <summary>
    /// Delete (archive) a team
    /// </summary>
    Task DeleteTeamAsync(Guid teamId, Guid deletedBy, CancellationToken ct = default);

    /// <summary>
    /// Add a member to a team
    /// </summary>
    Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, CancellationToken ct = default);

    /// <summary>
    /// Remove a member from a team
    /// </summary>
    Task RemoveMemberAsync(Guid teamId, Guid userId, Guid removedBy, CancellationToken ct = default);

    /// <summary>
    /// Set or remove team lead status for a member
    /// </summary>
    Task SetTeamLeadAsync(Guid teamId, Guid userId, SetTeamLeadRequest request, CancellationToken ct = default);
}
