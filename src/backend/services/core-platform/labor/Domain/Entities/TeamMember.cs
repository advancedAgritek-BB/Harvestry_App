using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

/// <summary>
/// Team member entity - represents a user's membership in a team
/// </summary>
public sealed partial class TeamMember : Entity<Guid>
{
    private TeamMember(Guid id) : base(id) { }

    private TeamMember(
        Guid id,
        Guid teamId,
        Guid userId,
        bool isTeamLead) : base(id)
    {
        TeamId = teamId;
        UserId = userId;
        IsTeamLead = isTeamLead;
        JoinedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsTeamLead { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? RemovedAt { get; private set; }
    public Guid? RemovedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new team member
    /// </summary>
    internal static TeamMember Create(Guid teamId, Guid userId, bool isTeamLead = false)
    {
        return new TeamMember(Guid.NewGuid(), teamId, userId, isTeamLead);
    }

    /// <summary>
    /// Set team lead status
    /// </summary>
    internal void SetTeamLead(bool isTeamLead)
    {
        IsTeamLead = isTeamLead;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove this member from the team
    /// </summary>
    internal void Remove(Guid removedBy)
    {
        RemovedAt = DateTime.UtcNow;
        RemovedBy = removedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate a previously removed member
    /// </summary>
    internal void Reactivate(bool isTeamLead = false)
    {
        RemovedAt = null;
        RemovedBy = null;
        IsTeamLead = isTeamLead;
        JoinedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
