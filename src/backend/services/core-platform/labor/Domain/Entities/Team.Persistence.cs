namespace Harvestry.Labor.Domain.Entities;

public sealed partial class Team
{
    /// <summary>
    /// Restores a Team entity from persistence
    /// </summary>
    public static Team Restore(
        Guid id,
        Guid siteId,
        string name,
        string? description,
        TeamStatus status,
        DateTime createdAt,
        DateTime updatedAt,
        Guid createdBy,
        Guid? updatedBy,
        IEnumerable<TeamMember>? members = null)
    {
        var team = new Team(id)
        {
            SiteId = siteId,
            Name = name,
            Description = description,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy
        };

        if (members != null)
        {
            team._members.AddRange(members);
        }

        return team;
    }

    /// <summary>
    /// Internal method to add restored members (for EF Core)
    /// </summary>
    internal void AddRestoredMember(TeamMember member)
    {
        _members.Add(member);
    }
}
