using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.ProcessingJobs.Domain.Entities;

/// <summary>
/// Defines a type of processing job (e.g., extraction, infusion)
/// </summary>
public sealed class ProcessingJobType : Entity<Guid>
{
    // Private constructor for EF Core
    private ProcessingJobType(Guid id) : base(id) { }

    private ProcessingJobType(
        Guid id,
        Guid siteId,
        string name,
        string? description,
        Guid createdByUserId) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        SiteId = siteId;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = true;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    // METRC mapping
    public long? MetrcJobTypeId { get; private set; }
    public string? MetrcJobTypeName { get; private set; }

    // Attributes (METRC job type attributes)
    public Dictionary<string, object> Attributes { get; private set; } = new();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new processing job type
    /// </summary>
    public static ProcessingJobType Create(
        Guid siteId,
        string name,
        Guid createdByUserId,
        string? description = null)
    {
        return new ProcessingJobType(
            Guid.NewGuid(),
            siteId,
            name,
            description,
            createdByUserId);
    }

    /// <summary>
    /// Update job type details
    /// </summary>
    public void Update(string name, string? description, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set METRC mapping
    /// </summary>
    public void SetMetrcMapping(long metrcJobTypeId, string metrcJobTypeName, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        MetrcJobTypeId = metrcJobTypeId;
        MetrcJobTypeName = metrcJobTypeName?.Trim();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set attributes
    /// </summary>
    public void SetAttributes(Dictionary<string, object> attributes, Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        Attributes = attributes != null
            ? new Dictionary<string, object>(attributes)
            : new Dictionary<string, object>();
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate job type
    /// </summary>
    public void Deactivate(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        IsActive = false;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivate job type
    /// </summary>
    public void Activate(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        IsActive = true;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ProcessingJobType Restore(
        Guid id,
        Guid siteId,
        string name,
        string? description,
        bool isActive,
        long? metrcJobTypeId,
        string? metrcJobTypeName,
        IDictionary<string, object>? attributes,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        return new ProcessingJobType(id)
        {
            SiteId = siteId,
            Name = name,
            Description = description,
            IsActive = isActive,
            MetrcJobTypeId = metrcJobTypeId,
            MetrcJobTypeName = metrcJobTypeName,
            Attributes = attributes != null
                ? new Dictionary<string, object>(attributes)
                : new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };
    }
}








