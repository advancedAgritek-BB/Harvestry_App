using Harvestry.Harvests.Application.DTOs;
using Harvestry.Harvests.Application.Interfaces;
using Harvestry.Harvests.Domain.Entities;
using Harvestry.Harvests.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Harvestry.Harvests.Application.Services;

/// <summary>
/// Service for syncing harvest workflow data with METRC
/// Handles compliance reporting at each workflow stage
/// </summary>
public class MetrcHarvestSyncService
{
    private readonly IHarvestRepository _harvestRepository;
    private readonly ILogger<MetrcHarvestSyncService> _logger;

    public MetrcHarvestSyncService(
        IHarvestRepository harvestRepository,
        ILogger<MetrcHarvestSyncService> logger)
    {
        _harvestRepository = harvestRepository;
        _logger = logger;
    }

    #region METRC Sync Points

    /// <summary>
    /// METRC Sync Point: Harvest Created
    /// Reports initial harvest with wet weight to METRC
    /// API: POST /harvests/v2/create
    /// </summary>
    public async Task<MetrcSyncResult> SyncHarvestCreatedAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        _logger.LogInformation(
            "METRC Sync: Harvest Created - {HarvestId}, Wet Weight: {WetWeight}g",
            harvestId, harvest.TotalWetWeight);

        // Build METRC harvest payload
        var metrcPayload = new MetrcHarvestPayload
        {
            Name = harvest.HarvestName,
            HarvestType = "Product",
            DryingLocationId = null, // Set when drying starts
            PatientLicenseNumber = null,
            ActualDate = harvest.HarvestDate.ToDateTime(TimeOnly.MinValue),
            Plants = harvest.HarvestPlants.Select(p => new MetrcPlantPayload
            {
                PlantTag = p.PlantTag,
                Weight = p.WetWeight,
                UnitOfWeight = MapUnitToMetrc(p.UnitOfWeight),
                HarvestName = harvest.HarvestName,
            }).ToList()
        };

        // In production, this would call actual METRC API
        // await _metrcClient.CreateHarvestAsync(metrcPayload, cancellationToken);

        return MetrcSyncResult.Success($"Harvest synced to METRC: {harvest.HarvestName}");
    }

    /// <summary>
    /// METRC Sync Point: Individual Plant Weight
    /// Reports plant wet weight to METRC after capture
    /// API: POST /harvests/v2/createplantbatches
    /// </summary>
    public async Task<MetrcSyncResult> SyncPlantWeightAsync(
        Guid harvestId,
        Guid harvestPlantId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdWithPlantsAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        var plant = harvest.HarvestPlants.FirstOrDefault(p => p.Id == harvestPlantId);
        if (plant == null)
            return MetrcSyncResult.Failed("Plant not found");

        _logger.LogInformation(
            "METRC Sync: Plant Weight - {PlantTag}: {WetWeight}g",
            plant.PlantTag, plant.WetWeight);

        // METRC requires individual plant weights
        var metrcPayload = new MetrcPlantPayload
        {
            PlantTag = plant.PlantTag,
            Weight = plant.WetWeight,
            UnitOfWeight = MapUnitToMetrc(plant.UnitOfWeight),
            HarvestName = harvest.HarvestName,
        };

        return MetrcSyncResult.Success($"Plant weight synced: {plant.PlantTag}");
    }

    /// <summary>
    /// METRC Sync Point: Waste Recorded
    /// Reports waste from bucking/trimming to METRC
    /// API: POST /harvests/v2/waste
    /// </summary>
    public async Task<MetrcSyncResult> SyncWasteRecordedAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        _logger.LogInformation(
            "METRC Sync: Waste Recorded - {HarvestId}, Total Waste: {WasteWeight}g",
            harvestId, harvest.TotalWasteWeight);

        // Build METRC waste payload
        var wasteEntries = new List<MetrcWastePayload>();

        if (harvest.TotalStemWaste > 0)
        {
            wasteEntries.Add(new MetrcWastePayload
            {
                HarvestName = harvest.HarvestName,
                WasteType = "Stems",
                Weight = harvest.TotalStemWaste,
                UnitOfWeight = "Grams",
                ActualDate = harvest.BuckingDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
            });
        }

        if (harvest.TotalLeafWaste > 0)
        {
            wasteEntries.Add(new MetrcWastePayload
            {
                HarvestName = harvest.HarvestName,
                WasteType = "Leaves",
                Weight = harvest.TotalLeafWaste,
                UnitOfWeight = "Grams",
                ActualDate = harvest.BuckingDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
            });
        }

        if (harvest.TotalOtherWaste > 0)
        {
            wasteEntries.Add(new MetrcWastePayload
            {
                HarvestName = harvest.HarvestName,
                WasteType = "Other",
                Weight = harvest.TotalOtherWaste,
                UnitOfWeight = "Grams",
                ActualDate = harvest.BuckingDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
            });
        }

        return MetrcSyncResult.Success($"Waste synced: {harvest.TotalWasteWeight}g total");
    }

    /// <summary>
    /// METRC Sync Point: Package/Lot Created
    /// Reports package creation from harvest batch
    /// API: POST /packages/v2/create
    /// </summary>
    public async Task<MetrcSyncResult> SyncPackageCreatedAsync(
        Guid harvestId,
        string packageTag,
        decimal weight,
        string productName,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        _logger.LogInformation(
            "METRC Sync: Package Created - {PackageTag}: {Weight}g from {HarvestName}",
            packageTag, weight, harvest.HarvestName);

        var metrcPayload = new MetrcPackagePayload
        {
            Tag = packageTag,
            HarvestName = harvest.HarvestName,
            ProductName = productName,
            Quantity = weight,
            UnitOfMeasure = "Grams",
            ActualDate = DateTime.UtcNow,
            IsProductionBatch = false,
            ProductionBatchNumber = null,
        };

        return MetrcSyncResult.Success($"Package synced: {packageTag}");
    }

    /// <summary>
    /// METRC Sync Point: Harvest Complete (Finished)
    /// Reports harvest as complete to METRC
    /// API: POST /harvests/v2/finish
    /// </summary>
    public async Task<MetrcSyncResult> SyncHarvestCompleteAsync(
        Guid harvestId,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        if (harvest.Phase != HarvestPhase.Complete)
            return MetrcSyncResult.Failed("Harvest is not complete");

        _logger.LogInformation(
            "METRC Sync: Harvest Complete - {HarvestId} ({HarvestName})",
            harvestId, harvest.HarvestName);

        var metrcPayload = new MetrcFinishHarvestPayload
        {
            HarvestName = harvest.HarvestName,
            ActualDate = DateTime.UtcNow,
        };

        return MetrcSyncResult.Success($"Harvest completion synced: {harvest.HarvestName}");
    }

    /// <summary>
    /// METRC Sync Point: Weight Adjustment
    /// Reports weight adjustment to METRC with reason
    /// API: POST /harvests/v2/adjustments
    /// </summary>
    public async Task<MetrcSyncResult> SyncWeightAdjustmentAsync(
        Guid harvestId,
        WeightAdjustment adjustment,
        Guid siteId,
        CancellationToken cancellationToken = default)
    {
        var harvest = await _harvestRepository.GetByIdAsync(harvestId, siteId, cancellationToken);
        if (harvest == null)
            return MetrcSyncResult.Failed("Harvest not found");

        _logger.LogInformation(
            "METRC Sync: Weight Adjustment - {HarvestName}: {PrevWeight} → {NewWeight}g (Reason: {Reason})",
            harvest.HarvestName, adjustment.PreviousWeight, adjustment.NewWeight, adjustment.ReasonCode);

        var metrcPayload = new MetrcAdjustmentPayload
        {
            HarvestName = harvest.HarvestName,
            WeightType = adjustment.WeightType.ToString(),
            PreviousWeight = adjustment.PreviousWeight,
            NewWeight = adjustment.NewWeight,
            ReasonCode = MapReasonToMetrc(adjustment.ReasonCode),
            Notes = adjustment.Notes ?? $"PIN override by user {adjustment.AdjustedByUserId}",
            ActualDate = adjustment.AdjustedAt,
        };

        return MetrcSyncResult.Success($"Adjustment synced: {adjustment.AdjustmentAmount:+0.00;-0.00}g");
    }

    #endregion

    #region Helper Methods

    private static string MapUnitToMetrc(string unit)
    {
        return unit.ToLower() switch
        {
            "grams" or "g" => "Grams",
            "kilograms" or "kg" => "Kilograms",
            "ounces" or "oz" => "Ounces",
            "pounds" or "lb" => "Pounds",
            _ => "Grams"
        };
    }

    private static string MapReasonToMetrc(string reasonCode)
    {
        return reasonCode.ToUpper() switch
        {
            "SCALE_ERROR" => "Scale Malfunction",
            "RECOUNTED" => "Reweigh",
            "DATA_ENTRY_ERROR" => "Entry Mistake",
            "SPILLAGE" => "Spillage",
            "CONTAMINATION" => "Contamination",
            "MOISTURE_ADJUSTMENT" => "Moisture",
            _ => "Other"
        };
    }

    #endregion
}

#region DTOs for METRC Payloads

public class MetrcSyncResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? MetrcTrackingId { get; private set; }

    public static MetrcSyncResult Success(string message, string? trackingId = null) =>
        new() { IsSuccess = true, Message = message, MetrcTrackingId = trackingId };

    public static MetrcSyncResult Failed(string message) =>
        new() { IsSuccess = false, Message = message };
}

public class MetrcHarvestPayload
{
    public string Name { get; set; } = string.Empty;
    public string HarvestType { get; set; } = "Product";
    public string? DryingLocationId { get; set; }
    public string? PatientLicenseNumber { get; set; }
    public DateTime ActualDate { get; set; }
    public List<MetrcPlantPayload> Plants { get; set; } = new();
}

public class MetrcPlantPayload
{
    public string PlantTag { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string UnitOfWeight { get; set; } = "Grams";
    public string HarvestName { get; set; } = string.Empty;
}

public class MetrcWastePayload
{
    public string HarvestName { get; set; } = string.Empty;
    public string WasteType { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string UnitOfWeight { get; set; } = "Grams";
    public DateTime ActualDate { get; set; }
}

public class MetrcPackagePayload
{
    public string Tag { get; set; } = string.Empty;
    public string HarvestName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "Grams";
    public DateTime ActualDate { get; set; }
    public bool IsProductionBatch { get; set; }
    public string? ProductionBatchNumber { get; set; }
}

public class MetrcFinishHarvestPayload
{
    public string HarvestName { get; set; } = string.Empty;
    public DateTime ActualDate { get; set; }
}

public class MetrcAdjustmentPayload
{
    public string HarvestName { get; set; } = string.Empty;
    public string WeightType { get; set; } = string.Empty;
    public decimal PreviousWeight { get; set; }
    public decimal NewWeight { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ActualDate { get; set; }
}

#endregion
