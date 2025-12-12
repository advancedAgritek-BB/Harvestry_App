using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

/// <summary>
/// Team aggregate root - represents a team of employees within a site
/// </summary>
public sealed partial class Team : AggregateRoot<Guid>
{
    private readonly List<TeamMember> _members = new();

    private Team(Guid id) : base(id) { }

    private Team(
        Guid id,
        Guid siteId,
        string name,
        string? description,
        Guid createdBy) : base(id)
    {
        SiteId = siteId;
        Name = name.Trim();
        Description = description?.Trim();
        Status = TeamStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        CreatedBy = createdBy;
    }

    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TeamStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    /// <summary>
    /// Get active (non-removed) members only
    /// </summary>
    public IEnumerable<TeamMember> ActiveMembers => 
        _members.Where(m => m.RemovedAt == null);

    /// <summary>
    /// Get team leads
    /// </summary>
    public IEnumerable<TeamMember> TeamLeads => 
        ActiveMembers.Where(m => m.IsTeamLead);

    /// <summary>
    /// Factory method to create a new team
    /// </summary>
    public static Team Create(
        Guid siteId,
        string name,
        string? description,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name is required", nameof(name));

        return new Team(Guid.NewGuid(), siteId, name, description, createdBy);
    }

    /// <summary>
    /// Update team details
    /// </summary>
    public void Update(string name, string? description, Guid updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Add a member to the team
    /// </summary>
    public TeamMember AddMember(Guid userId, bool isTeamLead = false)
    {
        var existingMember = _members.FirstOrDefault(m => m.UserId == userId);
        
        if (existingMember != null)
        {
            if (existingMember.RemovedAt == null)
                throw new InvalidOperationException($"User {userId} is already a member of this team");
            
            // Reactivate previously removed member
            existingMember.Reactivate(isTeamLead);
            UpdatedAt = DateTime.UtcNow;
            return existingMember;
        }

        var member = TeamMember.Create(Id, userId, isTeamLead);
        _members.Add(member);
        UpdatedAt = DateTime.UtcNow;
        return member;
    }

    /// <summary>
    /// Remove a member from the team
    /// </summary>
    public void RemoveMember(Guid userId, Guid removedBy)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId && m.RemovedAt == null);
        if (member == null)
            throw new InvalidOperationException($"User {userId} is not an active member of this team");

        member.Remove(removedBy);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set or remove team lead status for a member
    /// </summary>
    public void SetTeamLead(Guid userId, bool isTeamLead)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId && m.RemovedAt == null);
        if (member == null)
            throw new InvalidOperationException($"User {userId} is not an active member of this team");

        member.SetTeamLead(isTeamLead);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a user is a member of this team
    /// </summary>
    public bool HasMember(Guid userId) => 
        _members.Any(m => m.UserId == userId && m.RemovedAt == null);

    /// <summary>
    /// Check if a user is a team lead
    /// </summary>
    public bool IsTeamLead(Guid userId) => 
        _members.Any(m => m.UserId == userId && m.IsTeamLead && m.RemovedAt == null);

    /// <summary>
    /// Archive the team
    /// </summary>
    public void Archive(Guid archivedBy)
    {
        Status = TeamStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = archivedBy;
    }

    /// <summary>
    /// Deactivate the team
    /// </summary>
    public void Deactivate(Guid deactivatedBy)
    {
        Status = TeamStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deactivatedBy;
    }

    /// <summary>
    /// Reactivate the team
    /// </summary>
    public void Activate(Guid activatedBy)
    {
        Status = TeamStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = activatedBy;
    }
}

/// <summary>
/// Team status enum
/// </summary>
public enum TeamStatus
{
    Active,
    Inactive,
    Archived
}
