using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Batch code rule - defines jurisdiction-compliant batch code generation
/// </summary>
public sealed class BatchCodeRule : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private BatchCodeRule(Guid id) : base(id) { }

    private BatchCodeRule(
        Guid id,
        Guid siteId,
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        Guid createdByUserId,
        bool isActive = true) : base(id)
    {
        ValidateConstructorArgs(siteId, name, ruleDefinition, resetPolicy, createdByUserId);

        SiteId = siteId;
        Name = name.Trim();
        RuleDefinition = ruleDefinition;
        ResetPolicy = resetPolicy;
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Rule definition as flexible JSON structure
    /// Example: { "format": "{SitePrefix}-{StrainCode}-{YYMMDD}-{Seq}", "sitePrefix": "DEN", "sequenceStart": 1, "sequencePadding": 3 }
    /// </summary>
    public Dictionary<string, object> RuleDefinition { get; private set; } = new();
    
    /// <summary>
    /// Reset policy: "never", "annual", "monthly", "per_harvest"
    /// </summary>
    public string ResetPolicy { get; private set; } = "annual";
    
    public bool IsActive { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create new batch code rule
    /// </summary>
    public static BatchCodeRule Create(
        Guid siteId,
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        Guid createdByUserId,
        bool isActive = true)
    {
        return new BatchCodeRule(
            Guid.NewGuid(),
            siteId,
            name,
            ruleDefinition,
            resetPolicy,
            createdByUserId,
            isActive);
    }

    /// <summary>
    /// Update rule definition
    /// </summary>
    public void Update(
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        Guid updatedByUserId)
    {
        ValidateUpdateArgs(name, ruleDefinition, resetPolicy, updatedByUserId);

        Name = name.Trim();
        RuleDefinition = ruleDefinition;
        ResetPolicy = resetPolicy;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate this rule (deactivates other rules for the site)
    /// </summary>
    public void Activate(Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        IsActive = true;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivate this rule
    /// </summary>
    public void Deactivate(Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        IsActive = false;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to rehydrate batch code rule from persistence
    /// </summary>
    public static BatchCodeRule FromPersistence(
        Guid id,
        Guid siteId,
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        bool isActive,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var rule = new BatchCodeRule(id)
        {
            SiteId = siteId,
            Name = name,
            RuleDefinition = ruleDefinition,
            ResetPolicy = resetPolicy,
            IsActive = isActive,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };
        return rule;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        if (ruleDefinition == null || ruleDefinition.Count == 0)
            throw new ArgumentException("Rule definition cannot be empty", nameof(ruleDefinition));

        var validResetPolicies = new[] { "never", "annual", "monthly", "per_harvest" };
        if (!validResetPolicies.Contains(resetPolicy?.ToLowerInvariant()))
            throw new ArgumentException($"Reset policy must be one of: {string.Join(", ", validResetPolicies)}", nameof(resetPolicy));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }

    private static void ValidateUpdateArgs(
        string name,
        Dictionary<string, object> ruleDefinition,
        string resetPolicy,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        if (ruleDefinition == null || ruleDefinition.Count == 0)
            throw new ArgumentException("Rule definition cannot be empty", nameof(ruleDefinition));

        var validResetPolicies = new[] { "never", "annual", "monthly", "per_harvest" };
        if (!validResetPolicies.Contains(resetPolicy?.ToLowerInvariant()))
            throw new ArgumentException($"Reset policy must be one of: {string.Join(", ", validResetPolicies)}", nameof(resetPolicy));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
    }
}

