using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Equipment channel for multi-channel devices
/// Example: 12-channel EC sensor (HSES12), 24-valve controller
/// </summary>
public partial class EquipmentChannel : Entity<Guid>
{
    private EquipmentChannel(Guid id) : base(id) { } // EF Core

    private EquipmentChannel(
        Guid id,
        Guid equipmentId,
        string channelCode,
        string? role = null,
        string? portMetaJson = null) : base(id)
    {
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("Equipment ID cannot be empty", nameof(equipmentId));
        
        if (string.IsNullOrWhiteSpace(channelCode))
            throw new ArgumentException("Channel code cannot be empty", nameof(channelCode));
        
        EquipmentId = equipmentId;
        ChannelCode = channelCode.Trim();
        Role = role?.Trim();
        PortMetaJson = portMetaJson?.Trim();
        Enabled = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid EquipmentId { get; private set; }
    
    // Channel identification
    public string ChannelCode { get; private set; } = string.Empty;
    public string? Role { get; private set; }
    
    // Port/address metadata (hardware port config: DI/DO pin, relay index, etc.)
    public string? PortMetaJson { get; private set; }
    
    // Status
    public bool Enabled { get; private set; }
    
    // Assignment (links to zones, etc.)
    public Guid? AssignedZoneId { get; private set; }
    
    // Metadata
    public string? Notes { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Updates channel role (semantic meaning)
    /// </summary>
    public void UpdateRole(string? role)
    {
        Role = role?.Trim();
    }

    /// <summary>
    /// Updates port metadata
    /// </summary>
    public void UpdatePortMetadata(string? portMetaJson)
    {
        PortMetaJson = portMetaJson?.Trim();
    }

    /// <summary>
    /// Assigns channel to a zone
    /// </summary>
    public void AssignToZone(Guid zoneId)
    {
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone ID cannot be empty", nameof(zoneId));
        
        AssignedZoneId = zoneId;
    }

    /// <summary>
    /// Unassigns channel from current zone
    /// </summary>
    public void UnassignFromZone()
    {
        AssignedZoneId = null;
    }

    /// <summary>
    /// Enables the channel
    /// </summary>
    public void Enable()
    {
        Enabled = true;
    }

    /// <summary>
    /// Disables the channel
    /// </summary>
    public void Disable()
    {
        Enabled = false;
    }

    /// <summary>
    /// Updates notes
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    public EquipmentChannel(
        Guid equipmentId,
        string channelCode,
        string? role = null,
        string? portMetaJson = null)
        : this(Guid.NewGuid(), equipmentId, channelCode, role, portMetaJson)
    {
    }
}
