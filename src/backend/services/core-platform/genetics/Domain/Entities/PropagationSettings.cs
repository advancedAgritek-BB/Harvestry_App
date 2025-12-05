using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Propagation settings - site-wide propagation control limits
/// Admin-configurable with nullable limits (null = unlimited)
/// </summary>
public sealed class PropagationSettings : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private PropagationSettings(Guid id) : base(id) { }

    private PropagationSettings(
        Guid id,
        Guid siteId,
        Guid createdByUserId,
        int? dailyLimit = null,
        int? weeklyLimit = null,
        int? motherPropagationLimit = null,
        bool requiresOverrideApproval = true,
        string? approverRole = null,
        Dictionary<string, object>? approverPolicy = null) : base(id)
    {
        ValidateConstructorArgs(siteId, dailyLimit, weeklyLimit, motherPropagationLimit, createdByUserId);

        SiteId = siteId;
        DailyLimit = dailyLimit;
        WeeklyLimit = weeklyLimit;
        MotherPropagationLimit = motherPropagationLimit;
        RequiresOverrideApproval = requiresOverrideApproval;
        ApproverRole = approverRole?.Trim();
        ApproverPolicy = approverPolicy ?? new Dictionary<string, object>();
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    
    /// <summary>
    /// Daily propagation limit across all mothers (null = unlimited)
    /// </summary>
    public int? DailyLimit { get; private set; }
    
    /// <summary>
    /// Weekly propagation limit across all mothers (null = unlimited)
    /// </summary>
    public int? WeeklyLimit { get; private set; }
    
    /// <summary>
    /// Per-mother propagation limit (null = unlimited)
    /// </summary>
    public int? MotherPropagationLimit { get; private set; }
    
    /// <summary>
    /// Whether override approval is required when limits are exceeded
    /// </summary>
    public bool RequiresOverrideApproval { get; private set; }
    
    /// <summary>
    /// Role required for approving overrides (e.g., "Manager", "Cultivation Lead")
    /// </summary>
    public string? ApproverRole { get; private set; }
    
    /// <summary>
    /// Site-specific ABAC policy mapping for approvers (stored as JSON)
    /// </summary>
    public Dictionary<string, object> ApproverPolicy { get; private set; } = new();
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new propagation settings
    /// </summary>
    public static PropagationSettings Create(
        Guid siteId,
        Guid createdByUserId,
        int? dailyLimit = null,
        int? weeklyLimit = null,
        int? motherPropagationLimit = null,
        bool requiresOverrideApproval = true,
        string? approverRole = null,
        Dictionary<string, object>? approverPolicy = null)
    {
        return new PropagationSettings(
            Guid.NewGuid(),
            siteId,
            createdByUserId,
            dailyLimit,
            weeklyLimit,
            motherPropagationLimit,
            requiresOverrideApproval,
            approverRole,
            approverPolicy);
    }

    public static PropagationSettings FromPersistence(
        Guid id,
        Guid siteId,
        int? dailyLimit,
        int? weeklyLimit,
        int? motherPropagationLimit,
        bool requiresOverrideApproval,
        string? approverRole,
        Dictionary<string, object> approverPolicy,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var aggregate = new PropagationSettings(id)
        {
            SiteId = siteId,
            DailyLimit = dailyLimit,
            WeeklyLimit = weeklyLimit,
            MotherPropagationLimit = motherPropagationLimit,
            RequiresOverrideApproval = requiresOverrideApproval,
            ApproverRole = approverRole?.Trim(),
            ApproverPolicy = approverPolicy ?? new Dictionary<string, object>(),
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return aggregate;
    }

    /// <summary>
    /// Update propagation limits
    /// </summary>
    public void UpdateLimits(
        int? dailyLimit,
        int? weeklyLimit,
        int? motherPropagationLimit,
        bool requiresOverrideApproval,
        string? approverRole,
        Dictionary<string, object>? approverPolicy,
        Guid updatedByUserId)
    {
        ValidateLimits(dailyLimit, weeklyLimit, motherPropagationLimit);

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        DailyLimit = dailyLimit;
        WeeklyLimit = weeklyLimit;
        MotherPropagationLimit = motherPropagationLimit;
        RequiresOverrideApproval = requiresOverrideApproval;
        ApproverRole = approverRole?.Trim();
        ApproverPolicy = approverPolicy ?? new Dictionary<string, object>();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a propagation request is within limits
    /// </summary>
    public bool IsWithinLimits(
        int requestedCount,
        int dailyTotal,
        int weeklyTotal,
        int motherCurrentCount)
    {
        if (DailyLimit.HasValue && (dailyTotal + requestedCount) > DailyLimit.Value)
            return false;

        if (WeeklyLimit.HasValue && (weeklyTotal + requestedCount) > WeeklyLimit.Value)
            return false;

        if (MotherPropagationLimit.HasValue && (motherCurrentCount + requestedCount) > MotherPropagationLimit.Value)
            return false;

        return true;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        int? dailyLimit,
        int? weeklyLimit,
        int? motherPropagationLimit,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        ValidateLimits(dailyLimit, weeklyLimit, motherPropagationLimit);

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }

    private static void ValidateLimits(
        int? dailyLimit,
        int? weeklyLimit,
        int? motherPropagationLimit)
    {
        if (dailyLimit.HasValue && dailyLimit.Value < 1)
            throw new ArgumentException("Daily limit must be at least 1 if specified", nameof(dailyLimit));

        if (weeklyLimit.HasValue && weeklyLimit.Value < 1)
            throw new ArgumentException("Weekly limit must be at least 1 if specified", nameof(weeklyLimit));

        if (motherPropagationLimit.HasValue && motherPropagationLimit.Value < 1)
            throw new ArgumentException("Mother propagation limit must be at least 1 if specified", nameof(motherPropagationLimit));

        // Ensure weekly >= daily if both are set
        if (dailyLimit.HasValue && weeklyLimit.HasValue && weeklyLimit.Value < dailyLimit.Value)
            throw new ArgumentException("Weekly limit cannot be less than daily limit", nameof(weeklyLimit));
    }
}

