using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Aggregate root representing a group of irrigation zones that share a pump and flow constraints.
/// Groups enable coordinated irrigation across multiple zones while respecting hardware limits.
/// </summary>
public sealed class IrrigationGroup : Entity<Guid>
{
    private readonly List<IrrigationGroupZone> _zones = new();

    private IrrigationGroup(Guid id) : base(id) { }

    public Guid SiteId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int MaxConcurrentValves { get; private set; }
    public Guid? PumpEquipmentId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public IReadOnlyList<IrrigationGroupZone> Zones => _zones.AsReadOnly();

    public static IrrigationGroup Create(
        Guid siteId,
        string code,
        string name,
        int maxConcurrentValves = 6,
        Guid? pumpEquipmentId = null,
        Guid? createdByUserId = null)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (maxConcurrentValves < 1 || maxConcurrentValves > 24)
            throw new ArgumentException("Max concurrent valves must be between 1 and 24", nameof(maxConcurrentValves));

        var now = DateTimeOffset.UtcNow;
        return new IrrigationGroup(Guid.NewGuid())
        {
            SiteId = siteId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            MaxConcurrentValves = maxConcurrentValves,
            PumpEquipmentId = pumpEquipmentId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = createdByUserId
        };
    }

    public static IrrigationGroup FromPersistence(
        Guid id,
        Guid siteId,
        string code,
        string name,
        int maxConcurrentValves,
        Guid? pumpEquipmentId,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid? createdByUserId,
        List<IrrigationGroupZone> zones)
    {
        var group = new IrrigationGroup(id)
        {
            SiteId = siteId,
            Code = code,
            Name = name,
            MaxConcurrentValves = maxConcurrentValves,
            PumpEquipmentId = pumpEquipmentId,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedByUserId = createdByUserId
        };
        group._zones.AddRange(zones);
        return group;
    }

    public void AddZone(Guid zoneId, int priority = 1)
    {
        if (_zones.Any(z => z.ZoneId == zoneId))
            throw new InvalidOperationException($"Zone {zoneId} is already in this group");

        _zones.Add(new IrrigationGroupZone(Guid.NewGuid(), Id, zoneId, priority, DateTimeOffset.UtcNow));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveZone(Guid zoneId)
    {
        var zone = _zones.FirstOrDefault(z => z.ZoneId == zoneId);
        if (zone != null)
        {
            _zones.Remove(zone);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateSettings(string name, int maxConcurrentValves, Guid? pumpEquipmentId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (maxConcurrentValves < 1 || maxConcurrentValves > 24)
            throw new ArgumentException("Max concurrent valves must be between 1 and 24", nameof(maxConcurrentValves));

        Name = name.Trim();
        MaxConcurrentValves = maxConcurrentValves;
        PumpEquipmentId = pumpEquipmentId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Association between an irrigation group and a zone
/// </summary>
public sealed class IrrigationGroupZone
{
    public IrrigationGroupZone(Guid id, Guid groupId, Guid zoneId, int priority, DateTimeOffset createdAt)
    {
        Id = id;
        GroupId = groupId;
        ZoneId = zoneId;
        Priority = priority;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public Guid GroupId { get; }
    public Guid ZoneId { get; }
    public int Priority { get; }
    public DateTimeOffset CreatedAt { get; }
}
