using System;
using Harvestry.Spatial.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Room entity - top level of spatial hierarchy within a site
/// Represents physical rooms for cultivation or warehouse operations
/// </summary>
public partial class Room : AggregateRoot<Guid>
{
    private Room(Guid id) : base(id) { } // EF Core

    private Room(
        Guid id,
        Guid siteId,
        string code,
        string name,
        RoomType roomType,
        Guid createdByUserId,
        string? customRoomType = null,
        string? description = null) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
        
        // Validate custom room type requirement
        if (roomType == RoomType.Custom && string.IsNullOrWhiteSpace(customRoomType))
            throw new ArgumentException("Custom room type must be specified when RoomType is Custom", nameof(customRoomType));
        
        if (roomType != RoomType.Custom && !string.IsNullOrWhiteSpace(customRoomType))
            throw new ArgumentException("Custom room type should only be specified when RoomType is Custom", nameof(customRoomType));
        
        SiteId = siteId;
        Code = code.Trim();
        Name = name.Trim();
        RoomType = roomType;
        CustomRoomType = customRoomType?.Trim();
        Description = description?.Trim();
        Status = RoomStatus.Active;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Room(
        Guid siteId,
        string code,
        string name,
        RoomType roomType,
        Guid createdByUserId,
        string? customRoomType = null,
        string? description = null)
        : this(Guid.NewGuid(), siteId, code, name, roomType, createdByUserId, customRoomType, description)
    {
    }

    public Guid SiteId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public RoomType RoomType { get; private set; }
    public string? CustomRoomType { get; private set; }
    public RoomStatus Status { get; private set; }
    public string? Description { get; private set; }
    public int? FloorLevel { get; private set; }
    public decimal? AreaSqft { get; private set; }
    public decimal? HeightFt { get; private set; }
    
    // Environment targets (optional)
    public decimal? TargetTempF { get; private set; }
    public decimal? TargetHumidityPct { get; private set; }
    public int? TargetCo2Ppm { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Updates room basic information
    /// </summary>
    public void UpdateInfo(
        string name,
        string? description,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates room physical dimensions
    /// </summary>
    public void UpdateDimensions(
        int? floorLevel,
        decimal? areaSqft,
        decimal? heightFt,
        Guid updatedByUserId)
    {
        if (areaSqft.HasValue && areaSqft.Value < 0)
            throw new ArgumentException("Area cannot be negative", nameof(areaSqft));
        
        if (heightFt.HasValue && heightFt.Value < 0)
            throw new ArgumentException("Height cannot be negative", nameof(heightFt));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        FloorLevel = floorLevel;
        AreaSqft = areaSqft;
        HeightFt = heightFt;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates room environment targets
    /// </summary>
    public void UpdateEnvironmentTargets(
        decimal? targetTempF,
        decimal? targetHumidityPct,
        int? targetCo2Ppm,
        Guid updatedByUserId)
    {
        if (targetHumidityPct.HasValue && (targetHumidityPct.Value < 0 || targetHumidityPct.Value > 100))
            throw new ArgumentException("Humidity must be between 0 and 100", nameof(targetHumidityPct));
        
        if (targetCo2Ppm.HasValue && targetCo2Ppm.Value < 0)
            throw new ArgumentException("CO2 PPM cannot be negative", nameof(targetCo2Ppm));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        TargetTempF = targetTempF;
        TargetHumidityPct = targetHumidityPct;
        TargetCo2Ppm = targetCo2Ppm;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes room status
    /// </summary>
    public void ChangeStatus(RoomStatus newStatus, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        if (Status == newStatus)
            return; // No change
        
        Status = newStatus;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the room
    /// </summary>
    public void Activate(Guid updatedByUserId)
    {
        ChangeStatus(RoomStatus.Active, updatedByUserId);
    }

    /// <summary>
    /// Deactivates the room
    /// </summary>
    public void Deactivate(Guid updatedByUserId)
    {
        ChangeStatus(RoomStatus.Inactive, updatedByUserId);
    }

    /// <summary>
    /// Puts the room under maintenance
    /// </summary>
    public void SetMaintenance(Guid updatedByUserId)
    {
        ChangeStatus(RoomStatus.Maintenance, updatedByUserId);
    }

    /// <summary>
    /// Quarantines the room
    /// </summary>
    public void Quarantine(Guid updatedByUserId)
    {
        ChangeStatus(RoomStatus.Quarantine, updatedByUserId);
    }

    /// <summary>
    /// Gets the display room type (either enum value or custom type name)
    /// </summary>
    public string GetDisplayRoomType()
    {
        return RoomType == RoomType.Custom && !string.IsNullOrWhiteSpace(CustomRoomType)
            ? CustomRoomType
            : RoomType.ToString();
    }

    /// <summary>
    /// Checks if room is operational (active or maintenance)
    /// </summary>
    public bool IsOperational()
    {
        return Status == RoomStatus.Active || Status == RoomStatus.Maintenance;
    }
}
