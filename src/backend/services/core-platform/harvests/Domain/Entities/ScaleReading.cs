using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Records individual scale readings with full audit trail and calibration snapshot
/// </summary>
public sealed class ScaleReading : Entity<Guid>
{
    // Private constructor for EF Core
    private ScaleReading(Guid id) : base(id) { }

    private ScaleReading(
        Guid id,
        Guid? harvestId,
        Guid? harvestPlantId,
        Guid? lotId,
        Guid? scaleDeviceId,
        decimal grossWeight,
        decimal tareWeight,
        decimal netWeight,
        string unitOfWeight,
        bool isStable,
        int? stabilityDurationMs,
        DateTime readingTimestamp,
        Guid recordedByUserId) : base(id)
    {
        HarvestId = harvestId;
        HarvestPlantId = harvestPlantId;
        LotId = lotId;
        ScaleDeviceId = scaleDeviceId;
        GrossWeight = grossWeight;
        TareWeight = tareWeight;
        NetWeight = netWeight;
        UnitOfWeight = unitOfWeight;
        IsStable = isStable;
        StabilityDurationMs = stabilityDurationMs;
        ReadingTimestamp = readingTimestamp;
        RecordedByUserId = recordedByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    // ===== WHAT WAS WEIGHED =====

    /// <summary>
    /// Harvest this reading belongs to (if applicable)
    /// </summary>
    public Guid? HarvestId { get; private set; }

    /// <summary>
    /// Specific plant within harvest (if applicable)
    /// </summary>
    public Guid? HarvestPlantId { get; private set; }

    /// <summary>
    /// Inventory lot this reading belongs to (if applicable)
    /// </summary>
    public Guid? LotId { get; private set; }

    // ===== SCALE INFO =====

    /// <summary>
    /// Scale device that took this reading
    /// </summary>
    public Guid? ScaleDeviceId { get; private set; }

    // ===== CALIBRATION SNAPSHOT =====

    /// <summary>
    /// Calibration record that was active at time of reading
    /// </summary>
    public Guid? CalibrationId { get; private set; }

    /// <summary>
    /// Date of the calibration at time of reading
    /// </summary>
    public DateOnly? CalibrationDate { get; private set; }

    /// <summary>
    /// Calibration due date at time of reading
    /// </summary>
    public DateOnly? CalibrationDueDate { get; private set; }

    /// <summary>
    /// Whether the scale was within valid calibration at time of reading
    /// </summary>
    public bool CalibrationWasValid { get; private set; }

    // ===== WEIGHT DATA =====

    /// <summary>
    /// Total weight including container
    /// </summary>
    public decimal GrossWeight { get; private set; }

    /// <summary>
    /// Weight of container (subtracted)
    /// </summary>
    public decimal TareWeight { get; private set; }

    /// <summary>
    /// Net weight (gross - tare)
    /// </summary>
    public decimal NetWeight { get; private set; }

    /// <summary>
    /// Unit of measurement (Grams, Kilograms, Ounces, Pounds)
    /// </summary>
    public string UnitOfWeight { get; private set; } = "Grams";

    // ===== STABILITY =====

    /// <summary>
    /// Whether the scale reported a stable reading
    /// </summary>
    public bool IsStable { get; private set; }

    /// <summary>
    /// Duration the weight was stable before capture (milliseconds)
    /// </summary>
    public int? StabilityDurationMs { get; private set; }

    // ===== RAW DATA =====

    /// <summary>
    /// Timestamp when reading was taken
    /// </summary>
    public DateTime ReadingTimestamp { get; private set; }

    /// <summary>
    /// Raw data from scale in JSON format
    /// </summary>
    public string? RawScaleDataJson { get; private set; }

    // ===== AUDIT =====

    /// <summary>
    /// User who recorded this reading
    /// </summary>
    public Guid RecordedByUserId { get; private set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Factory method to create a new scale reading
    /// </summary>
    public static ScaleReading Create(
        Guid? harvestId,
        Guid? harvestPlantId,
        Guid? lotId,
        Guid? scaleDeviceId,
        decimal grossWeight,
        decimal tareWeight,
        decimal netWeight,
        string unitOfWeight,
        bool isStable,
        int? stabilityDurationMs,
        DateTime readingTimestamp,
        Guid recordedByUserId)
    {
        if (!harvestId.HasValue && !harvestPlantId.HasValue && !lotId.HasValue)
            throw new ArgumentException("At least one of harvestId, harvestPlantId, or lotId is required");

        if (recordedByUserId == Guid.Empty)
            throw new ArgumentException("Recorded by user ID cannot be empty", nameof(recordedByUserId));

        return new ScaleReading(
            Guid.NewGuid(),
            harvestId,
            harvestPlantId,
            lotId,
            scaleDeviceId,
            grossWeight,
            tareWeight,
            netWeight,
            unitOfWeight?.Trim() ?? "Grams",
            isStable,
            stabilityDurationMs,
            readingTimestamp,
            recordedByUserId);
    }

    /// <summary>
    /// Set calibration snapshot at time of reading
    /// </summary>
    public void SetCalibrationSnapshot(
        Guid calibrationId,
        DateOnly calibrationDate,
        DateOnly calibrationDueDate,
        bool wasValid)
    {
        CalibrationId = calibrationId;
        CalibrationDate = calibrationDate;
        CalibrationDueDate = calibrationDueDate;
        CalibrationWasValid = wasValid;
    }

    /// <summary>
    /// Set raw scale data
    /// </summary>
    public void SetRawScaleData(string? rawDataJson)
    {
        RawScaleDataJson = rawDataJson;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static ScaleReading Restore(
        Guid id,
        Guid? harvestId,
        Guid? harvestPlantId,
        Guid? lotId,
        Guid? scaleDeviceId,
        Guid? calibrationId,
        DateOnly? calibrationDate,
        DateOnly? calibrationDueDate,
        bool calibrationWasValid,
        decimal grossWeight,
        decimal tareWeight,
        decimal netWeight,
        string unitOfWeight,
        bool isStable,
        int? stabilityDurationMs,
        DateTime readingTimestamp,
        string? rawScaleDataJson,
        Guid recordedByUserId,
        DateTime createdAt)
    {
        return new ScaleReading(id)
        {
            HarvestId = harvestId,
            HarvestPlantId = harvestPlantId,
            LotId = lotId,
            ScaleDeviceId = scaleDeviceId,
            CalibrationId = calibrationId,
            CalibrationDate = calibrationDate,
            CalibrationDueDate = calibrationDueDate,
            CalibrationWasValid = calibrationWasValid,
            GrossWeight = grossWeight,
            TareWeight = tareWeight,
            NetWeight = netWeight,
            UnitOfWeight = unitOfWeight,
            IsStable = isStable,
            StabilityDurationMs = stabilityDurationMs,
            ReadingTimestamp = readingTimestamp,
            RawScaleDataJson = rawScaleDataJson,
            RecordedByUserId = recordedByUserId,
            CreatedAt = createdAt
        };
    }
}




