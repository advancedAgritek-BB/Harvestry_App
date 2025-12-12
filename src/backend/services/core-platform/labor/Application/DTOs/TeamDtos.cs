namespace Harvestry.Labor.Application.DTOs;

// Request DTOs
public record CreateTeamRequest(
    string Name,
    string? Description);

public record UpdateTeamRequest(
    string Name,
    string? Description);

public record AddTeamMemberRequest(
    Guid UserId,
    bool IsTeamLead = false);

public record SetTeamLeadRequest(
    bool IsTeamLead);

// Response DTOs
public record TeamDto(
    Guid Id,
    Guid SiteId,
    string Name,
    string? Description,
    string Status,
    int MemberCount,
    int TeamLeadCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TeamDetailDto(
    Guid Id,
    Guid SiteId,
    string Name,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<TeamMemberDto> Members);

public record TeamMemberDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string? Role,
    bool IsTeamLead,
    DateTime JoinedAt);

/// <summary>
/// Simplified member DTO for assignment pickers
/// </summary>
public record AssignableMemberDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    string? Role,
    Guid TeamId,
    string TeamName);

/// <summary>
/// Grouped assignable members response
/// </summary>
public record AssignableMembersResponse(
    IReadOnlyList<TeamWithMembersDto> Teams);

public record TeamWithMembersDto(
    Guid TeamId,
    string TeamName,
    IReadOnlyList<AssignableMemberDto> Members);
