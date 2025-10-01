namespace Harvestry.Spatial.Domain.Enums;

/// <summary>
/// Room type enumeration supporting both core types and custom user-defined types
/// </summary>
public enum RoomType
{
    /// <summary>
    /// Vegetative growth room
    /// </summary>
    Veg,
    
    /// <summary>
    /// Flowering room
    /// </summary>
    Flower,
    
    /// <summary>
    /// Mother plant room
    /// </summary>
    Mother,
    
    /// <summary>
    /// Clone/propagation room
    /// </summary>
    Clone,
    
    /// <summary>
    /// Drying room
    /// </summary>
    Dry,
    
    /// <summary>
    /// Curing room
    /// </summary>
    Cure,
    
    /// <summary>
    /// Extraction/processing room
    /// </summary>
    Extraction,
    
    /// <summary>
    /// Manufacturing room
    /// </summary>
    Manufacturing,
    
    /// <summary>
    /// Vault - secure storage for finished/unfinished goods
    /// </summary>
    Vault,
    
    /// <summary>
    /// Custom user-defined room type (uses custom_room_type field)
    /// </summary>
    Custom
}

/// <summary>
/// Room status enumeration
/// </summary>
public enum RoomStatus
{
    /// <summary>
    /// Room is active and operational
    /// </summary>
    Active,
    
    /// <summary>
    /// Room is inactive/not in use
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Room is under maintenance
    /// </summary>
    Maintenance,
    
    /// <summary>
    /// Room is quarantined due to contamination or other issues
    /// </summary>
    Quarantine
}
