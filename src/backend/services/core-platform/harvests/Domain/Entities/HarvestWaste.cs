using Harvestry.Harvests.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Harvests.Domain.Entities;

/// <summary>
/// Waste record for a harvest (METRC harvest waste)
/// </summary>
public sealed class HarvestWaste : Entity<Guid>
{
    // Private constructor for EF Core
    private HarvestWaste(Guid id) : base(id) { }

    private HarvestWaste(
        Guid id,
        Guid harvestId,
        HarvestWasteType wasteType,
        decimal wasteWeight,
        string unitOfWeight,
        WasteMethod wasteMethod,
        DateOnly actualDate,
        Guid recordedByUserId,
        string? notes = null) : base(id)
    {
        HarvestId = harvestId;
        WasteType = wasteType;
        WasteWeight = wasteWeight;
        UnitOfWeight = unitOfWeight;
        WasteMethod = wasteMethod;
        ActualDate = actualDate;
        RecordedByUserId = recordedByUserId;
        Notes = notes;
        RecordedAt = DateTime.UtcNow;
    }

    public Guid HarvestId { get; private set; }
    public HarvestWasteType WasteType { get; private set; }
    public decimal WasteWeight { get; private set; }
    public string UnitOfWeight { get; private set; } = "Grams";
    public WasteMethod WasteMethod { get; private set; }
    public DateOnly ActualDate { get; private set; }
    public Guid RecordedByUserId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime RecordedAt { get; private set; }

    // METRC sync
    public long? MetrcWasteId { get; private set; }

    /// <summary>
    /// Factory method to create a new waste record
    /// </summary>
    public static HarvestWaste Create(
        Guid harvestId,
        HarvestWasteType wasteType,
        decimal wasteWeight,
        string unitOfWeight,
        WasteMethod wasteMethod,
        DateOnly actualDate,
        Guid recordedByUserId,
        string? notes = null)
    {
        if (harvestId == Guid.Empty)
            throw new ArgumentException("Harvest ID cannot be empty", nameof(harvestId));

        if (wasteWeight <= 0)
            throw new ArgumentException("Waste weight must be greater than 0", nameof(wasteWeight));

        if (recordedByUserId == Guid.Empty)
            throw new ArgumentException("Recorded by user ID cannot be empty", nameof(recordedByUserId));

        return new HarvestWaste(
            Guid.NewGuid(),
            harvestId,
            wasteType,
            wasteWeight,
            unitOfWeight?.Trim() ?? "Grams",
            wasteMethod,
            actualDate,
            recordedByUserId,
            notes?.Trim());
    }

    /// <summary>
    /// Update METRC sync information
    /// </summary>
    public void UpdateMetrcSync(long metrcWasteId)
    {
        MetrcWasteId = metrcWasteId;
    }

    /// <summary>
    /// Restore from persistence
    /// </summary>
    public static HarvestWaste Restore(
        Guid id,
        Guid harvestId,
        HarvestWasteType wasteType,
        decimal wasteWeight,
        string unitOfWeight,
        WasteMethod wasteMethod,
        DateOnly actualDate,
        Guid recordedByUserId,
        string? notes,
        DateTime recordedAt,
        long? metrcWasteId)
    {
        return new HarvestWaste(id)
        {
            HarvestId = harvestId,
            WasteType = wasteType,
            WasteWeight = wasteWeight,
            UnitOfWeight = unitOfWeight,
            WasteMethod = wasteMethod,
            ActualDate = actualDate,
            RecordedByUserId = recordedByUserId,
            Notes = notes,
            RecordedAt = recordedAt,
            MetrcWasteId = metrcWasteId
        };
    }
}








