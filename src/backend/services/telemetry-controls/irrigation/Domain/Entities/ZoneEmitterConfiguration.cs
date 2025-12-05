using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Emitter configuration for an irrigation zone
/// Stores detailed emitter specifications needed for flow rate calculations
/// </summary>
public sealed class ZoneEmitterConfiguration : Entity<Guid>
{
    // Private constructor for EF Core
    private ZoneEmitterConfiguration(Guid id) : base(id) { }

    private ZoneEmitterConfiguration(
        Guid id,
        Guid siteId,
        Guid zoneId,
        string zoneName,
        Guid createdByUserId,
        int emitterCount,
        decimal emitterFlowRateLitersPerHour,
        string emitterType,
        int emittersPerPlant = 1) : base(id)
    {
        ValidateConstructorArgs(siteId, zoneId, emitterCount, emitterFlowRateLitersPerHour);

        SiteId = siteId;
        ZoneId = zoneId;
        ZoneName = zoneName;
        EmitterCount = emitterCount;
        EmitterFlowRateLitersPerHour = emitterFlowRateLitersPerHour;
        EmitterType = emitterType;
        EmittersPerPlant = emittersPerPlant;
        CreatedByUserId = createdByUserId;
        UpdatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid SiteId { get; private set; }
    public Guid ZoneId { get; private set; }
    public string ZoneName { get; private set; } = null!;
    
    /// <summary>
    /// Total number of emitters in this zone
    /// </summary>
    public int EmitterCount { get; private set; }
    
    /// <summary>
    /// Flow rate per emitter in liters per hour (L/h)
    /// Common values: 1.0, 2.0, 4.0 L/h for drip emitters
    /// </summary>
    public decimal EmitterFlowRateLitersPerHour { get; private set; }
    
    /// <summary>
    /// Type of emitter (e.g., "Drip 2L/h", "Micro-sprinkler", "Inline dripper")
    /// </summary>
    public string EmitterType { get; private set; } = null!;
    
    /// <summary>
    /// Number of emitters per plant in this zone
    /// </summary>
    public int EmittersPerPlant { get; private set; }
    
    /// <summary>
    /// Operating pressure in kPa (optional, for reference)
    /// </summary>
    public decimal? OperatingPressureKpa { get; private set; }
    
    /// <summary>
    /// Last calibration date
    /// </summary>
    public DateTime? LastCalibratedAt { get; private set; }
    
    /// <summary>
    /// User who performed the last calibration
    /// </summary>
    public Guid? CalibratedByUserId { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Calculate total zone flow rate in liters per minute when all emitters are active
    /// </summary>
    public decimal TotalZoneFlowRateLitersPerMinute =>
        EmitterCount * EmitterFlowRateLitersPerHour / 60m;

    /// <summary>
    /// Calculate total zone flow rate in liters per hour
    /// </summary>
    public decimal TotalZoneFlowRateLitersPerHour =>
        EmitterCount * EmitterFlowRateLitersPerHour;

    public static ZoneEmitterConfiguration Create(
        Guid siteId,
        Guid zoneId,
        string zoneName,
        Guid createdByUserId,
        int emitterCount,
        decimal emitterFlowRateLitersPerHour,
        string emitterType,
        int emittersPerPlant = 1)
    {
        return new ZoneEmitterConfiguration(
            Guid.NewGuid(),
            siteId,
            zoneId,
            zoneName,
            createdByUserId,
            emitterCount,
            emitterFlowRateLitersPerHour,
            emitterType,
            emittersPerPlant);
    }

    public void UpdateEmitterConfiguration(
        int emitterCount,
        decimal emitterFlowRateLitersPerHour,
        string emitterType,
        int emittersPerPlant,
        Guid updatedByUserId)
    {
        if (emitterCount <= 0)
            throw new ArgumentException("Emitter count must be positive", nameof(emitterCount));
        if (emitterFlowRateLitersPerHour <= 0)
            throw new ArgumentException("Emitter flow rate must be positive", nameof(emitterFlowRateLitersPerHour));
        if (string.IsNullOrWhiteSpace(emitterType))
            throw new ArgumentException("Emitter type is required", nameof(emitterType));
        if (emittersPerPlant <= 0)
            throw new ArgumentException("Emitters per plant must be positive", nameof(emittersPerPlant));

        EmitterCount = emitterCount;
        EmitterFlowRateLitersPerHour = emitterFlowRateLitersPerHour;
        EmitterType = emitterType;
        EmittersPerPlant = emittersPerPlant;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOperatingPressure(decimal pressureKpa, Guid updatedByUserId)
    {
        if (pressureKpa <= 0)
            throw new ArgumentException("Operating pressure must be positive", nameof(pressureKpa));

        OperatingPressureKpa = pressureKpa;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordCalibration(Guid calibratedByUserId, Guid updatedByUserId)
    {
        LastCalibratedAt = DateTime.UtcNow;
        CalibratedByUserId = calibratedByUserId;
        UpdatedByUserId = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateConstructorArgs(
        Guid siteId,
        Guid zoneId,
        int emitterCount,
        decimal emitterFlowRateLitersPerHour)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID cannot be empty", nameof(siteId));
        if (zoneId == Guid.Empty)
            throw new ArgumentException("Zone ID cannot be empty", nameof(zoneId));
        if (emitterCount <= 0)
            throw new ArgumentException("Emitter count must be positive", nameof(emitterCount));
        if (emitterFlowRateLitersPerHour <= 0)
            throw new ArgumentException("Emitter flow rate must be positive", nameof(emitterFlowRateLitersPerHour));
    }
}




