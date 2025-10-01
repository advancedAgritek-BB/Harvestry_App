using System;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Spatial.Domain.Entities;

/// <summary>
/// Represents the routing relationship between a valve (equipment/channel) and a target location zone.
/// </summary>
public partial class ValveZoneMapping : Entity<Guid>
{
    private ValveZoneMapping(Guid id) : base(id)
    {
    }

    public ValveZoneMapping(
        Guid siteId,
        Guid valveEquipmentId,
        Guid zoneLocationId,
        Guid createdByUserId,
        string? valveChannelCode = null,
        int priority = 1,
        bool normallyOpen = false,
        string? interlockGroup = null,
        string? notes = null)
        : this(Guid.NewGuid(), siteId, valveEquipmentId, zoneLocationId, createdByUserId,
            valveChannelCode, priority, normallyOpen, interlockGroup, notes)
    {
    }

    private ValveZoneMapping(
        Guid id,
        Guid siteId,
        Guid valveEquipmentId,
        Guid zoneLocationId,
        Guid createdByUserId,
        string? valveChannelCode,
        int priority,
        bool normallyOpen,
        string? interlockGroup,
        string? notes) : base(id)
    {
        if (siteId == Guid.Empty) throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        if (valveEquipmentId == Guid.Empty) throw new ArgumentException("Valve equipment ID cannot be empty", nameof(valveEquipmentId));
        if (zoneLocationId == Guid.Empty) throw new ArgumentException("Zone location ID cannot be empty", nameof(zoneLocationId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("Created by user ID cannot be empty", nameof(createdByUserId));
        if (priority <= 0) throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be positive");

        SiteId = siteId;
        ValveEquipmentId = valveEquipmentId;
        ZoneLocationId = zoneLocationId;
        ValveChannelCode = NormalizeOptional(valveChannelCode);
        Priority = priority;
        NormallyOpen = normallyOpen;
        InterlockGroup = NormalizeOptional(interlockGroup);
        Notes = NormalizeOptional(notes);
        Enabled = true;

        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid SiteId { get; private set; }

    public Guid ValveEquipmentId { get; private set; }

    public string? ValveChannelCode { get; private set; }

    public Guid ZoneLocationId { get; private set; }

    public int Priority { get; private set; }

    public bool NormallyOpen { get; private set; }

    public string? InterlockGroup { get; private set; }

    public bool Enabled { get; private set; }

    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Guid UpdatedByUserId { get; private set; }

    public void UpdateRouting(int priority, bool normallyOpen, string? interlockGroup, string? notes, Guid updatedByUserId)
    {
        if (priority <= 0) throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be positive");
        if (updatedByUserId == Guid.Empty) throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Priority = priority;
        NormallyOpen = normallyOpen;
        InterlockGroup = NormalizeOptional(interlockGroup);
        Notes = NormalizeOptional(notes);

        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEnabled(bool enabled, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty) throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        Enabled = enabled;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReassignZone(Guid zoneLocationId, Guid updatedByUserId)
    {
        if (zoneLocationId == Guid.Empty) throw new ArgumentException("Zone location ID cannot be empty", nameof(zoneLocationId));
        if (updatedByUserId == Guid.Empty) throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        ZoneLocationId = zoneLocationId;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetChannel(string? valveChannelCode, Guid updatedByUserId)
    {
        if (updatedByUserId == Guid.Empty) throw new ArgumentException("Updated by user ID cannot be empty", nameof(updatedByUserId));

        ValveChannelCode = NormalizeOptional(valveChannelCode);
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

