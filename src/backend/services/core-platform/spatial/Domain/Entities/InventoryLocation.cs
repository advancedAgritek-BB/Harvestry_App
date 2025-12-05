using System;
using Harvestry.Spatial.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Universal inventory location supporting both cultivation and warehouse hierarchies
/// Cultivation path: Room → Zone → SubZone → Row → Position
/// Warehouse path: Room → Rack → Shelf → Bin
/// </summary>
public partial class InventoryLocation : AggregateRoot<Guid>
{
    private InventoryLocation(Guid id) : base(id) { } // EF Core

    private InventoryLocation(
        Guid id,
        Guid siteId,
        string code,
        string name,
        LocationType locationType,
        Guid createdByUserId,
        Guid? roomId = null,
        Guid? parentId = null,
        string? barcode = null) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
        
        // Validate hierarchy rules
        if (locationType == LocationType.Room && parentId.HasValue)
            throw new ArgumentException("Room-level locations cannot have a parent", nameof(parentId));
        
        if (locationType == LocationType.Room && !roomId.HasValue)
            throw new ArgumentException("Room-level locations must have a room ID", nameof(roomId));
        
        if (locationType != LocationType.Room && !parentId.HasValue)
            throw new ArgumentException($"{locationType} locations must have a parent", nameof(parentId));
        
        SiteId = siteId;
        RoomId = roomId;
        ParentId = parentId;
        LocationType = locationType;
        Code = code.Trim();
        Name = name.Trim();
        Barcode = barcode?.Trim();
        Status = LocationStatus.Active;
        Depth = 0; // Will be calculated by trigger
        Path = name.Trim(); // Will be calculated by trigger
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public InventoryLocation(
        Guid siteId,
        string code,
        string name,
        LocationType locationType,
        Guid createdByUserId,
        Guid? roomId = null,
        Guid? parentId = null,
        string? barcode = null)
        : this(Guid.NewGuid(), siteId, code, name, locationType, createdByUserId, roomId, parentId, barcode)
    {
    }

    public Guid SiteId { get; private set; }
    public Guid? RoomId { get; private set; }
    public Guid? ParentId { get; private set; }
    public LocationType LocationType { get; private set; }
    
    // Identification
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Barcode { get; private set; }
    
    // Hierarchy (materialized by trigger)
    public string Path { get; private set; } = string.Empty;
    public int Depth { get; private set; }
    
    // Status
    public LocationStatus Status { get; private set; }
    
    // Dimensions
    public decimal? LengthFt { get; private set; }
    public decimal? WidthFt { get; private set; }
    public decimal? HeightFt { get; private set; }
    
    // Cultivation-specific
    public int? PlantCapacity { get; private set; }
    public int CurrentPlantCount { get; private set; }
    
    // Matrix coordinates (for Position type)
    public int? RowNumber { get; private set; }
    public int? ColumnNumber { get; private set; }
    
    // Warehouse-specific
    public decimal? WeightCapacityLbs { get; private set; }
    public decimal CurrentWeightLbs { get; private set; }
    
    // Metadata
    public string? Notes { get; private set; }
    public string? MetadataJson { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Updates basic location information
    /// </summary>
    public void UpdateInfo(
        string name,
        string? barcode,
        string? notes,
        Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        Name = name.Trim();
        Barcode = barcode?.Trim();
        Notes = notes?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates physical dimensions
    /// </summary>
    public void UpdateDimensions(
        decimal? lengthFt,
        decimal? widthFt,
        decimal? heightFt,
        Guid updatedByUserId)
    {
        if (lengthFt.HasValue && lengthFt.Value < 0)
            throw new ArgumentException("Length cannot be negative", nameof(lengthFt));
        
        if (widthFt.HasValue && widthFt.Value < 0)
            throw new ArgumentException("Width cannot be negative", nameof(widthFt));
        
        if (heightFt.HasValue && heightFt.Value < 0)
            throw new ArgumentException("Height cannot be negative", nameof(heightFt));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        LengthFt = lengthFt;
        WidthFt = widthFt;
        HeightFt = heightFt;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates cultivation capacity settings
    /// </summary>
    public void UpdateCultivationCapacity(
        int? plantCapacity,
        int? rowNumber,
        int? columnNumber,
        Guid updatedByUserId)
    {
        if (plantCapacity.HasValue && plantCapacity.Value < 0)
            throw new ArgumentException("Plant capacity cannot be negative", nameof(plantCapacity));
        
        if (plantCapacity.HasValue && plantCapacity.Value < CurrentPlantCount)
            throw new ArgumentException($"Plant capacity cannot be less than current plant count ({CurrentPlantCount})", nameof(plantCapacity));
        
        if (rowNumber.HasValue && rowNumber.Value < 0)
            throw new ArgumentException("Row number cannot be negative", nameof(rowNumber));
        
        if (columnNumber.HasValue && columnNumber.Value < 0)
            throw new ArgumentException("Column number cannot be negative", nameof(columnNumber));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        PlantCapacity = plantCapacity;
        RowNumber = rowNumber;
        ColumnNumber = columnNumber;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates warehouse capacity settings
    /// </summary>
    public void UpdateWarehouseCapacity(
        decimal? weightCapacityLbs,
        Guid updatedByUserId)
    {
        if (weightCapacityLbs.HasValue && weightCapacityLbs.Value < 0)
            throw new ArgumentException("Weight capacity cannot be negative", nameof(weightCapacityLbs));

        if (weightCapacityLbs.HasValue && weightCapacityLbs.Value < CurrentWeightLbs)
            throw new ArgumentException($"Weight capacity cannot be less than current weight ({CurrentWeightLbs} lbs)", nameof(weightCapacityLbs));

        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        WeightCapacityLbs = weightCapacityLbs;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates metadata stored for the inventory location
    /// </summary>
    public void UpdateMetadata(string? metadataJson, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        MetadataJson = string.IsNullOrWhiteSpace(metadataJson)
            ? null
            : metadataJson.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increments plant count (for cultivation locations)
    /// </summary>
    public void AddPlants(int count, Guid userId)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        var newCount = CurrentPlantCount + count;
        
        if (PlantCapacity.HasValue && newCount > PlantCapacity.Value)
            throw new InvalidOperationException($"Adding {count} plants would exceed capacity of {PlantCapacity.Value}");
        
        CurrentPlantCount = newCount;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-update status to Full if at capacity
        if (PlantCapacity.HasValue && CurrentPlantCount >= PlantCapacity.Value)
        {
            Status = LocationStatus.Full;
        }
    }

    /// <summary>
    /// Decrements plant count (for cultivation locations)
    /// </summary>
    public void RemovePlants(int count, Guid userId)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (count > CurrentPlantCount)
            throw new InvalidOperationException($"Cannot remove {count} plants - only {CurrentPlantCount} present");
        
        CurrentPlantCount -= count;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-update status if no longer full
        if (Status == LocationStatus.Full)
        {
            Status = LocationStatus.Active;
        }
    }

    /// <summary>
    /// Adds weight to warehouse location
    /// </summary>
    public void AddWeight(decimal weightLbs, Guid userId)
    {
        if (weightLbs <= 0)
            throw new ArgumentException("Weight must be positive", nameof(weightLbs));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        var newWeight = CurrentWeightLbs + weightLbs;
        
        if (WeightCapacityLbs.HasValue && newWeight > WeightCapacityLbs.Value)
            throw new InvalidOperationException($"Adding {weightLbs} lbs would exceed capacity of {WeightCapacityLbs.Value} lbs");
        
        CurrentWeightLbs = newWeight;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-update status to Full if at capacity
        if (WeightCapacityLbs.HasValue && CurrentWeightLbs >= WeightCapacityLbs.Value)
        {
            Status = LocationStatus.Full;
        }
    }

    /// <summary>
    /// Removes weight from warehouse location
    /// </summary>
    public void RemoveWeight(decimal weightLbs, Guid userId)
    {
        if (weightLbs <= 0)
            throw new ArgumentException("Weight must be positive", nameof(weightLbs));
        
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (weightLbs > CurrentWeightLbs)
            throw new InvalidOperationException($"Cannot remove {weightLbs} lbs - only {CurrentWeightLbs} lbs present");
        
        CurrentWeightLbs -= weightLbs;
        UpdatedByUserId = userId;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-update status if no longer full
        if (Status == LocationStatus.Full)
        {
            Status = LocationStatus.Active;
        }
    }

    /// <summary>
    /// Changes location status
    /// </summary>
    public void ChangeStatus(LocationStatus newStatus, Guid updatedByUserId)
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
    /// Checks if location is a cultivation location (Zone, SubZone, Row, Position)
    /// </summary>
    public bool IsCultivationLocation()
    {
        return LocationType is LocationType.Zone or LocationType.SubZone 
            or LocationType.Row or LocationType.Position;
    }

    /// <summary>
    /// Checks if location is a warehouse location (Rack, Shelf, Bin)
    /// </summary>
    public bool IsWarehouseLocation()
    {
        return LocationType is LocationType.Rack or LocationType.Shelf or LocationType.Bin;
    }

    /// <summary>
    /// Checks if location has available capacity
    /// </summary>
    public bool HasCapacity()
    {
        if (Status == LocationStatus.Full)
            return false;
        
        if (IsCultivationLocation() && PlantCapacity.HasValue)
            return CurrentPlantCount < PlantCapacity.Value;
        
        if (IsWarehouseLocation() && WeightCapacityLbs.HasValue)
            return CurrentWeightLbs < WeightCapacityLbs.Value;
        
        return true; // No capacity limits defined
    }
}
