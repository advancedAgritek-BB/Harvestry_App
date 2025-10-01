using System;
using Harvestry.Spatial.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Equipment entity representing devices, sensors, controllers, actuators
/// Supports multi-channel devices with device twin for discovered capabilities
/// </summary>
public partial class Equipment : AggregateRoot<Guid>
{
    private Equipment(Guid id) : base(id) { } // EF Core

    private Equipment(
        Guid id,
        Guid siteId,
        string code,
        string typeCode,
        CoreEquipmentType coreType,
        Guid createdByUserId,
        string? manufacturer = null,
        string? model = null,
        string? serialNumber = null) : base(id)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));
        
        if (string.IsNullOrWhiteSpace(typeCode))
            throw new ArgumentException("Type code cannot be empty", nameof(typeCode));
        
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
        
        SiteId = siteId;
        Code = code.Trim();
        TypeCode = typeCode.Trim();
        CoreType = coreType;
        Manufacturer = manufacturer?.Trim();
        Model = model?.Trim();
        SerialNumber = serialNumber?.Trim();
        Status = EquipmentStatus.Active;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Equipment(
        Guid siteId,
        string code,
        string typeCode,
        CoreEquipmentType coreType,
        Guid createdByUserId,
        string? manufacturer = null,
        string? model = null,
        string? serialNumber = null)
        : this(Guid.NewGuid(), siteId, code, typeCode, coreType, createdByUserId, manufacturer, model, serialNumber)
    {
    }

    public Guid SiteId { get; private set; }
    
    // Identification
    public string Code { get; private set; } = string.Empty;
    public string TypeCode { get; private set; } = string.Empty;
    public CoreEquipmentType CoreType { get; private set; }
    
    // Status
    public EquipmentStatus Status { get; private set; }
    public DateTime? InstalledAt { get; private set; }
    public DateTime? DecommissionedAt { get; private set; }
    
    // Location
    public Guid? LocationId { get; private set; }
    
    // Hardware details
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? FirmwareVersion { get; private set; }
    
    // Network attributes (nullable - discovered/assigned)
    public string? IpAddress { get; private set; }
    public string? MacAddress { get; private set; }
    public string? MqttTopic { get; private set; }
    
    // Device twin (capabilities, channel map, discovered metadata)
    public string? DeviceTwinJson { get; private set; }
    
    // Calibration tracking
    public DateTime? LastCalibrationAt { get; private set; }
    public DateTime? NextCalibrationDueAt { get; private set; }
    public int? CalibrationIntervalDays { get; private set; }
    
    // Health (current snapshot - history in separate hypertable)
    public DateTime? LastHeartbeatAt { get; private set; }
    // Note: Online is computed column in database
    public int? SignalStrengthDbm { get; private set; }
    public int? BatteryPercent { get; private set; }
    public int ErrorCount { get; private set; }
    public long? UptimeSeconds { get; private set; }
    
    // Metadata
    public string? Notes { get; private set; }
    public string? MetadataJson { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByUserId { get; private set; }

    /// <summary>
    /// Updates basic equipment information
    /// </summary>
    public void UpdateInfo(
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        Manufacturer = manufacturer?.Trim();
        Model = model?.Trim();
        SerialNumber = serialNumber?.Trim();
        Notes = notes?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets or updates firmware version
    /// </summary>
    public void UpdateFirmwareVersion(string firmwareVersion, Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(firmwareVersion))
            throw new ArgumentException("Firmware version cannot be empty", nameof(firmwareVersion));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        FirmwareVersion = firmwareVersion.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns equipment to a location
    /// </summary>
    public void AssignToLocation(Guid locationId, Guid updatedByUserId)
    {
        if (locationId == Guid.Empty)
            throw new ArgumentException("Location ID cannot be empty", nameof(locationId));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        LocationId = locationId;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes equipment from current location
    /// </summary>
    public void UnassignFromLocation(Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        LocationId = null;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records installation timestamp
    /// </summary>
    public void MarkAsInstalled(DateTime installedAt, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        InstalledAt = installedAt;
        Status = EquipmentStatus.Active;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decommissions the equipment
    /// </summary>
    public void Decommission(DateTime decommissionedAt, Guid updatedByUserId)
    {
        if (decommissionedAt < InstalledAt)
            throw new ArgumentException("Decommission date cannot be before installation date", nameof(decommissionedAt));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        DecommissionedAt = decommissionedAt;
        Status = EquipmentStatus.Inactive;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates network configuration
    /// </summary>
    public void UpdateNetworkConfig(
        string? ipAddress,
        string? macAddress,
        string? mqttTopic,
        Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        IpAddress = ipAddress?.Trim();
        MacAddress = macAddress?.Trim();
        MqttTopic = mqttTopic?.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates device twin (discovered capabilities, channel map)
    /// </summary>
    public void UpdateDeviceTwin(string deviceTwinJson, Guid updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(deviceTwinJson))
            throw new ArgumentException("Device twin JSON cannot be empty", nameof(deviceTwinJson));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        DeviceTwinJson = deviceTwinJson.Trim();
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a heartbeat from the device
    /// </summary>
    public void RecordHeartbeat(
        DateTime heartbeatAt,
        int? signalStrengthDbm = null,
        int? batteryPercent = null,
        long? uptimeSeconds = null)
    {
        if (batteryPercent.HasValue && (batteryPercent.Value < 0 || batteryPercent.Value > 100))
            throw new ArgumentException("Battery percent must be between 0 and 100", nameof(batteryPercent));
        
        LastHeartbeatAt = heartbeatAt;
        SignalStrengthDbm = signalStrengthDbm;
        BatteryPercent = batteryPercent;
        UptimeSeconds = uptimeSeconds;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-recover from Faulty status if device comes online
        if (Status == EquipmentStatus.Faulty && IsOnline())
        {
            Status = EquipmentStatus.Active;
        }
    }

    /// <summary>
    /// Increments error count
    /// </summary>
    public void IncrementErrorCount()
    {
        ErrorCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // Auto-set to Faulty if error count exceeds threshold
        if (ErrorCount > 10 && Status == EquipmentStatus.Active)
        {
            Status = EquipmentStatus.Faulty;
        }
    }

    /// <summary>
    /// Resets error count (after maintenance or recovery)
    /// </summary>
    public void ResetErrorCount(Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        ErrorCount = 0;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records calibration
    /// </summary>
    public void RecordCalibration(
        DateTime calibratedAt,
        DateTime nextDueAt,
        int intervalDays,
        Guid updatedByUserId)
    {
        if (nextDueAt <= calibratedAt)
            throw new ArgumentException("Next due date must be after calibration date", nameof(nextDueAt));
        
        if (intervalDays <= 0)
            throw new ArgumentException("Calibration interval must be positive", nameof(intervalDays));
        
        if (updatedByUserId == Guid.Empty)
            throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));
        
        LastCalibrationAt = calibratedAt;
        NextCalibrationDueAt = nextDueAt;
        CalibrationIntervalDays = intervalDays;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes equipment status
    /// </summary>
    public void ChangeStatus(EquipmentStatus newStatus, Guid updatedByUserId)
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
    /// Checks if equipment is online (heartbeat within last 5 minutes)
    /// Note: Database has computed column for this
    /// </summary>
    public bool IsOnline()
    {
        if (!LastHeartbeatAt.HasValue)
            return false;
        
        return DateTime.UtcNow - LastHeartbeatAt.Value < TimeSpan.FromMinutes(5);
    }

    /// <summary>
    /// Checks if calibration is overdue
    /// </summary>
    public bool IsCalibrationOverdue()
    {
        if (!NextCalibrationDueAt.HasValue)
            return false;
        
        return DateTime.UtcNow > NextCalibrationDueAt.Value;
    }

    /// <summary>
    /// Checks if calibration is due soon (within 7 days)
    /// </summary>
    public bool IsCalibrationDueSoon()
    {
        if (!NextCalibrationDueAt.HasValue)
            return false;
        
        return DateTime.UtcNow > NextCalibrationDueAt.Value.AddDays(-7);
    }

    /// <summary>
    /// Checks if equipment is operational (active or maintenance)
    /// </summary>
    public bool IsOperational()
    {
        return Status == EquipmentStatus.Active || Status == EquipmentStatus.Maintenance;
    }
}
