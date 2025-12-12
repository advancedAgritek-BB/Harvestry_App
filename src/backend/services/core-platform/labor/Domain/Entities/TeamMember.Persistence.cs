namespace Harvestry.Labor.Domain.Entities;

public sealed partial class TeamMember
{
    /// <summary>
    /// Restores a TeamMember entity from persistence
    /// </summary>
    public static TeamMember Restore(
        Guid id,
        Guid teamId,
        Guid userId,
        bool isTeamLead,
        DateTime joinedAt,
        DateTime? removedAt,
        Guid? removedBy,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new TeamMember(id)
        {
            TeamId = teamId,
            UserId = userId,
            IsTeamLead = isTeamLead,
            JoinedAt = joinedAt,
            RemovedAt = removedAt,
            RemovedBy = removedBy,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
