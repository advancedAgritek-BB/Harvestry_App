using Harvestry.Labor.Application.DTOs;
using Harvestry.Labor.Application.Interfaces;
using Harvestry.Labor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Harvestry.Labor.Application.Services;

/// <summary>
/// Service implementation for team management operations
/// </summary>
public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly ILogger<TeamService> _logger;

    public TeamService(
        ITeamRepository teamRepository,
        IUserInfoProvider userInfoProvider,
        ILogger<TeamService> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _userInfoProvider = userInfoProvider ?? throw new ArgumentNullException(nameof(userInfoProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(Guid siteId, CancellationToken ct = default)
    {
        var teams = await _teamRepository.GetBySiteIdAsync(siteId, ct);
        return teams.Select(MapToDto).ToList();
    }

    public async Task<TeamDetailDto?> GetTeamDetailAsync(Guid teamId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithMembersAsync(teamId, ct);
        if (team == null) return null;

        var memberUserIds = team.ActiveMembers.Select(m => m.UserId).ToList();
        var userInfos = await _userInfoProvider.GetUserInfoBatchAsync(memberUserIds, ct);

        var memberDtos = team.ActiveMembers
            .Select(m => MapToMemberDto(m, userInfos.GetValueOrDefault(m.UserId)))
            .OrderByDescending(m => m.IsTeamLead)
            .ThenBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToList();

        return new TeamDetailDto(
            team.Id,
            team.SiteId,
            team.Name,
            team.Description,
            team.Status.ToString(),
            team.CreatedAt,
            team.UpdatedAt,
            memberDtos);
    }

    public async Task<IReadOnlyList<TeamDto>> GetManagedTeamsAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        // Check if user is manager/supervisor at site level
        var isManager = await _userInfoProvider.IsManagerOrSupervisorAsync(userId, siteId, ct);

        if (isManager)
        {
            // Managers/supervisors can see all teams for the site
            var allTeams = await _teamRepository.GetBySiteIdAsync(siteId, ct);
            return allTeams.Select(MapToDto).ToList();
        }

        // Otherwise, only show teams where user is a team lead
        var leadTeams = await _teamRepository.GetByTeamLeadAsync(userId, ct);
        return leadTeams
            .Where(t => t.SiteId == siteId)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<AssignableMembersResponse> GetAssignableMembersAsync(Guid userId, Guid siteId, CancellationToken ct = default)
    {
        var managedTeams = await GetManagedTeamsForAssignment(userId, siteId, ct);
        var result = new List<TeamWithMembersDto>();

        foreach (var team in managedTeams)
        {
            var teamWithMembers = await _teamRepository.GetByIdWithMembersAsync(team.Id, ct);
            if (teamWithMembers == null) continue;

            var memberUserIds = teamWithMembers.ActiveMembers.Select(m => m.UserId).ToList();
            var userInfos = await _userInfoProvider.GetUserInfoBatchAsync(memberUserIds, ct);

            var assignableMembers = teamWithMembers.ActiveMembers
                .Select(m =>
                {
                    var userInfo = userInfos.GetValueOrDefault(m.UserId);
                    return new AssignableMemberDto(
                        m.UserId,
                        userInfo?.FirstName ?? "Unknown",
                        userInfo?.LastName ?? "User",
                        $"{userInfo?.FirstName ?? "Unknown"} {userInfo?.LastName ?? "User"}",
                        userInfo?.AvatarUrl,
                        userInfo?.Role,
                        team.Id,
                        team.Name);
                })
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToList();

            result.Add(new TeamWithMembersDto(team.Id, team.Name, assignableMembers));
        }

        return new AssignableMembersResponse(result);
    }

    public async Task<TeamDto> CreateTeamAsync(Guid siteId, CreateTeamRequest request, Guid createdBy, CancellationToken ct = default)
    {
        // Check for duplicate name
        if (await _teamRepository.ExistsWithNameAsync(siteId, request.Name, ct: ct))
        {
            throw new InvalidOperationException($"A team with name '{request.Name}' already exists");
        }

        var team = Team.Create(siteId, request.Name, request.Description, createdBy);
        await _teamRepository.AddAsync(team, ct);

        _logger.LogInformation("Created team {TeamId} '{TeamName}' for site {SiteId}", team.Id, team.Name, siteId);

        return MapToDto(team);
    }

    public async Task<TeamDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid updatedBy, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        // Check for duplicate name (excluding this team)
        if (await _teamRepository.ExistsWithNameAsync(team.SiteId, request.Name, teamId, ct))
        {
            throw new InvalidOperationException($"A team with name '{request.Name}' already exists");
        }

        team.Update(request.Name, request.Description, updatedBy);
        await _teamRepository.UpdateAsync(team, ct);

        _logger.LogInformation("Updated team {TeamId} for site {SiteId}", teamId, team.SiteId);

        return MapToDto(team);
    }

    public async Task DeleteTeamAsync(Guid teamId, Guid deletedBy, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        team.Archive(deletedBy);
        await _teamRepository.UpdateAsync(team, ct);

        _logger.LogInformation("Archived team {TeamId}", teamId);
    }

    public async Task<TeamMemberDto> AddMemberAsync(Guid teamId, AddTeamMemberRequest request, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithMembersAsync(teamId, ct);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        var member = team.AddMember(request.UserId, request.IsTeamLead);
        await _teamRepository.UpdateAsync(team, ct);

        var userInfo = await _userInfoProvider.GetUserInfoAsync(request.UserId, ct);

        _logger.LogInformation("Added user {UserId} to team {TeamId}", request.UserId, teamId);

        return MapToMemberDto(member, userInfo);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, Guid removedBy, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithMembersAsync(teamId, ct);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        team.RemoveMember(userId, removedBy);
        await _teamRepository.UpdateAsync(team, ct);

        _logger.LogInformation("Removed user {UserId} from team {TeamId}", userId, teamId);
    }

    public async Task SetTeamLeadAsync(Guid teamId, Guid userId, SetTeamLeadRequest request, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdWithMembersAsync(teamId, ct);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        team.SetTeamLead(userId, request.IsTeamLead);
        await _teamRepository.UpdateAsync(team, ct);

        _logger.LogInformation("Set team lead status for user {UserId} in team {TeamId} to {IsTeamLead}",
            userId, teamId, request.IsTeamLead);
    }

    private async Task<IReadOnlyList<Team>> GetManagedTeamsForAssignment(Guid userId, Guid siteId, CancellationToken ct)
    {
        var isManager = await _userInfoProvider.IsManagerOrSupervisorAsync(userId, siteId, ct);

        if (isManager)
        {
            return await _teamRepository.GetBySiteIdAsync(siteId, ct);
        }

        var leadTeams = await _teamRepository.GetByTeamLeadAsync(userId, ct);
        return leadTeams.Where(t => t.SiteId == siteId).ToList();
    }

    private static TeamDto MapToDto(Team team) => new(
        team.Id,
        team.SiteId,
        team.Name,
        team.Description,
        team.Status.ToString(),
        team.ActiveMembers.Count(),
        team.TeamLeads.Count(),
        team.CreatedAt,
        team.UpdatedAt);

    private static TeamMemberDto MapToMemberDto(TeamMember member, UserInfo? userInfo) => new(
        member.Id,
        member.UserId,
        userInfo?.FirstName ?? "Unknown",
        userInfo?.LastName ?? "User",
        userInfo?.AvatarUrl,
        userInfo?.Role,
        member.IsTeamLead,
        member.JoinedAt);
}
