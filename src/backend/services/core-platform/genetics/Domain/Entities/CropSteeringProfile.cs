using Harvestry.Genetics.Domain.ValueObjects;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Genetics.Domain.Entities;

/// <summary>
/// Crop steering profile - defines steering parameters for a site or specific strain.
/// When StrainId is null, this serves as the site-wide default profile.
/// Strain-specific profiles inherit from site defaults but can override any parameter.
/// </summary>
public sealed class CropSteeringProfile : AggregateRoot<Guid>
{
    // Private constructor for EF Core/rehydration
    private CropSteeringProfile(Guid id) : base(id) { }

    private CropSteeringProfile(
        Guid id,
        Guid siteId,
        Guid? strainId,
        string name,
        string? description,
        SteeringMode targetMode,
        SteeringConfiguration configuration,
        Guid createdByUserId,
        bool isActive = true) : base(id)
    {
        ValidateConstructorArgs(siteId, name, createdByUserId);

        SiteId = siteId;
        StrainId = strainId;
        Name = name.Trim();
        Description = description?.Trim();
        TargetMode = targetMode;
        Configuration = configuration;
        IsActive = isActive;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Site this profile belongs to</summary>
    public Guid SiteId { get; private set; }
    
    /// <summary>
    /// Strain this profile is specific to. 
    /// Null indicates this is the site-wide default profile.
    /// </summary>
    public Guid? StrainId { get; private set; }
    
    /// <summary>Profile name for display and identification</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Optional description of the profile's purpose or characteristics</summary>
    public string? Description { get; private set; }
    
    /// <summary>Target steering direction (Vegetative, Generative, or Balanced)</summary>
    public SteeringMode TargetMode { get; private set; }
    
    /// <summary>Complete steering configuration (stored as JSONB)</summary>
    public SteeringConfiguration Configuration { get; private set; }
    
    /// <summary>Whether this profile is currently active</summary>
    public bool IsActive { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Factory method to create a new site-wide default profile.
    /// </summary>
    public static CropSteeringProfile CreateSiteDefault(
        Guid siteId,
        string name,
        SteeringMode targetMode,
        Guid createdByUserId,
        string? description = null)
    {
        var configuration = targetMode switch
        {
            SteeringMode.Vegetative => SteeringConfiguration.DefaultVegetative,
            SteeringMode.Generative => SteeringConfiguration.DefaultGenerative,
            _ => SteeringConfiguration.DefaultVegetative // Balanced starts from vegetative
        };

        return new CropSteeringProfile(
            Guid.NewGuid(),
            siteId,
            strainId: null, // Site default has no strain
            name,
            description,
            targetMode,
            configuration,
            createdByUserId);
    }

    /// <summary>
    /// Factory method to create a strain-specific profile.
    /// </summary>
    public static CropSteeringProfile CreateForStrain(
        Guid siteId,
        Guid strainId,
        string name,
        SteeringMode targetMode,
        Guid createdByUserId,
        string? description = null,
        SteeringConfiguration? customConfiguration = null)
    {
        var configuration = customConfiguration ?? targetMode switch
        {
            SteeringMode.Vegetative => SteeringConfiguration.DefaultVegetative,
            SteeringMode.Generative => SteeringConfiguration.DefaultGenerative,
            _ => SteeringConfiguration.DefaultVegetative
        };

        return new CropSteeringProfile(
            Guid.NewGuid(),
            siteId,
            strainId,
            name,
            description,
            targetMode,
            configuration,
            createdByUserId);
    }

    /// <summary>
    /// Factory method to rehydrate profile from persistence.
    /// </summary>
    public static CropSteeringProfile FromPersistence(
        Guid id,
        Guid siteId,
        Guid? strainId,
        string name,
        string? description,
        SteeringMode targetMode,
        SteeringConfiguration configuration,
        bool isActive,
        DateTime createdAt,
        Guid createdByUserId,
        DateTime updatedAt,
        Guid updatedByUserId)
    {
        var profile = new CropSteeringProfile(id)
        {
            SiteId = siteId,
            StrainId = strainId,
            Name = name,
            Description = description,
            TargetMode = targetMode,
            Configuration = configuration,
            IsActive = isActive,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
            UpdatedAt = updatedAt,
            UpdatedByUserId = updatedByUserId
        };

        return profile;
    }

    /// <summary>
    /// Update the profile's steering mode and regenerate default configuration.
    /// </summary>
    public void UpdateSteeringMode(SteeringMode newMode, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        TargetMode = newMode;
        Configuration = newMode switch
        {
            SteeringMode.Vegetative => SteeringConfiguration.DefaultVegetative,
            SteeringMode.Generative => SteeringConfiguration.DefaultGenerative,
            _ => Configuration // Keep existing for Balanced
        };
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the profile with a custom configuration.
    /// </summary>
    public void UpdateConfiguration(
        SteeringConfiguration configuration,
        Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Configuration = configuration;
        TargetMode = configuration.TargetMode;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update profile metadata (name, description).
    /// </summary>
    public void UpdateMetadata(
        string name,
        string? description,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activate or deactivate the profile.
    /// </summary>
    public void SetActive(bool isActive, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        IsActive = isActive;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get the phase configuration for a specific daily phase.
    /// </summary>
    public PhaseConfig GetPhaseConfig(DailyPhase phase)
    {
        return phase switch
        {
            DailyPhase.P1Ramp => Configuration.P1Config,
            DailyPhase.P2Maintenance => Configuration.P2Config,
            DailyPhase.P3Dryback => Configuration.P3Config,
            DailyPhase.Night => Configuration.P3Config, // Use P3 config for night (minimal activity)
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Unknown daily phase")
        };
    }

    /// <summary>
    /// Check if this profile is a site-wide default (no strain specified).
    /// </summary>
    public bool IsSiteDefault => !StrainId.HasValue;

    private static void ValidateConstructorArgs(
        Guid siteId,
        string name,
        Guid createdByUserId)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
    }
}

