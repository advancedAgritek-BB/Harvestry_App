using Harvestry.Plants.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Plants.Domain.Entities;

/// <summary>
/// Audit event for plant lifecycle changes
/// </summary>
public sealed class PlantEvent : Entity<Guid>
{
    private PlantEvent(Guid id) : base(id) { }

    private PlantEvent(
        Guid id,
        Guid siteId,
        Guid plantId,
        PlantEventType eventType,
        Guid userId,
        DateTime occurredAt,
        Dictionary<string, object>? eventData = null) : base(id)
    {
        SiteId = siteId;
        PlantId = plantId;
        EventType = eventType;
        UserId = userId;
        OccurredAt = occurredAt;
        EventData = eventData ?? new Dictionary<string, object>();
    }

    public Guid SiteId { get; private set; }
    public Guid PlantId { get; private set; }
    public PlantEventType EventType { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Dictionary<string, object> EventData { get; private set; } = new();

    /// <summary>
    /// Factory method to create a generic plant event
    /// </summary>
    public static PlantEvent Create(
        Guid siteId,
        Guid plantId,
        PlantEventType eventType,
        Guid userId,
        DateTime occurredAt,
        Dictionary<string, object>? eventData = null)
    {
        return new PlantEvent(
            Guid.NewGuid(),
            siteId,
            plantId,
            eventType,
            userId,
            occurredAt,
            eventData);
    }

    /// <summary>
    /// Create event for plant creation
    /// </summary>
    public static PlantEvent CreatePlantCreated(
        Guid siteId,
        Guid plantId,
        string plantTag,
        Guid strainId,
        string strainName,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["plantTag"] = plantTag,
            ["strainId"] = strainId,
            ["strainName"] = strainName
        };

        return Create(siteId, plantId, PlantEventType.Created, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Create event for phase transition
    /// </summary>
    public static PlantEvent CreatePhaseTransition(
        Guid siteId,
        Guid plantId,
        PlantGrowthPhase fromPhase,
        PlantGrowthPhase toPhase,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["fromPhase"] = fromPhase.ToString(),
            ["toPhase"] = toPhase.ToString()
        };

        var eventType = toPhase switch
        {
            PlantGrowthPhase.Vegetative => PlantEventType.VegetativeTransition,
            PlantGrowthPhase.Flowering => PlantEventType.FloweringTransition,
            PlantGrowthPhase.Mother => PlantEventType.DesignatedMother,
            PlantGrowthPhase.Harvested => PlantEventType.Harvested,
            PlantGrowthPhase.Destroyed => PlantEventType.Destroyed,
            _ => PlantEventType.VegetativeTransition
        };

        return Create(siteId, plantId, eventType, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Create event for location change
    /// </summary>
    public static PlantEvent CreateLocationChange(
        Guid siteId,
        Guid plantId,
        Guid? fromLocationId,
        Guid? toLocationId,
        string? toSublocation,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>();
        
        if (fromLocationId.HasValue)
            eventData["fromLocationId"] = fromLocationId.Value;
        if (toLocationId.HasValue)
            eventData["toLocationId"] = toLocationId.Value;
        if (!string.IsNullOrWhiteSpace(toSublocation))
            eventData["toSublocation"] = toSublocation;

        return Create(siteId, plantId, PlantEventType.LocationChange, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Create event for harvest
    /// </summary>
    public static PlantEvent CreateHarvested(
        Guid siteId,
        Guid plantId,
        Guid harvestId,
        decimal wetWeight,
        string weightUnit,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["harvestId"] = harvestId,
            ["wetWeight"] = wetWeight,
            ["weightUnit"] = weightUnit
        };

        return Create(siteId, plantId, PlantEventType.Harvested, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Create event for destruction
    /// </summary>
    public static PlantEvent CreateDestroyed(
        Guid siteId,
        Guid plantId,
        PlantDestroyReason reason,
        decimal wasteWeight,
        string weightUnit,
        WasteMethod wasteMethod,
        Guid userId)
    {
        var eventData = new Dictionary<string, object>
        {
            ["reason"] = reason.ToString(),
            ["wasteWeight"] = wasteWeight,
            ["weightUnit"] = weightUnit,
            ["wasteMethod"] = wasteMethod.ToString()
        };

        return Create(siteId, plantId, PlantEventType.Destroyed, userId, DateTime.UtcNow, eventData);
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static PlantEvent Restore(
        Guid id,
        Guid siteId,
        Guid plantId,
        PlantEventType eventType,
        Guid userId,
        DateTime occurredAt,
        IDictionary<string, object>? eventData)
    {
        return new PlantEvent(id)
        {
            SiteId = siteId,
            PlantId = plantId,
            EventType = eventType,
            UserId = userId,
            OccurredAt = occurredAt,
            EventData = eventData != null
                ? new Dictionary<string, object>(eventData)
                : new Dictionary<string, object>()
        };
    }
}



