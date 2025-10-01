namespace Harvestry.Spatial.Domain.Enums;

/// <summary>
/// Location type in hierarchical inventory system
/// Supports both cultivation paths (Room→Zone→SubZone→Row→Position)
/// and warehouse paths (Room→Rack→Shelf→Bin)
/// </summary>
public enum LocationType
{
    /// <summary>
    /// Top level - references rooms table (cultivation or warehouse room)
    /// </summary>
    Room,
    
    /// <summary>
    /// Cultivation: grow zone within a room
    /// Warehouse: storage area within a room
    /// </summary>
    Zone,
    
    /// <summary>
    /// Cultivation: sub-zone or bench within a zone
    /// </summary>
    SubZone,
    
    /// <summary>
    /// Cultivation: plant rows within a zone/sub-zone
    /// </summary>
    Row,
    
    /// <summary>
    /// Cultivation: specific plant position (matrix location) within a row
    /// </summary>
    Position,
    
    /// <summary>
    /// Warehouse: storage rack
    /// </summary>
    Rack,
    
    /// <summary>
    /// Warehouse: shelf within a rack
    /// </summary>
    Shelf,
    
    /// <summary>
    /// Warehouse: bin/tote on a shelf
    /// </summary>
    Bin
}

/// <summary>
/// Location status enumeration
/// </summary>
public enum LocationStatus
{
    /// <summary>
    /// Location is active and available
    /// </summary>
    Active,
    
    /// <summary>
    /// Location is inactive/not in use
    /// </summary>
    Inactive,
    
    /// <summary>
    /// Location is at full capacity
    /// </summary>
    Full,
    
    /// <summary>
    /// Location is reserved for future use
    /// </summary>
    Reserved,
    
    /// <summary>
    /// Location is quarantined (contamination, pest issues, etc.)
    /// </summary>
    Quarantine
}
