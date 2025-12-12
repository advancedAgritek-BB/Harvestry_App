using System.Text.Json;

namespace Harvestry.AiModels.Domain.ValueObjects;

/// <summary>
/// Properties specific to Strain nodes for genetics graph.
/// </summary>
public sealed record StrainNodeProperties
{
    /// <summary>Strain name</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Breeder</summary>
    public string? Breeder { get; init; }

    /// <summary>Seed bank</summary>
    public string? SeedBank { get; init; }

    /// <summary>Genetic classification (Indica, Sativa, Hybrid)</summary>
    public string GeneticClassification { get; init; } = string.Empty;

    /// <summary>Testing status</summary>
    public string TestingStatus { get; init; } = string.Empty;

    /// <summary>Nominal THC percentage</summary>
    public decimal? NominalThcPercent { get; init; }

    /// <summary>Nominal CBD percentage</summary>
    public decimal? NominalCbdPercent { get; init; }

    /// <summary>Expected harvest window (days)</summary>
    public int? ExpectedHarvestWindowDays { get; init; }

    /// <summary>Has custom steering profile</summary>
    public bool HasCustomSteeringProfile { get; init; }

    /// <summary>Crop steering profile ID</summary>
    public Guid? CropSteeringProfileId { get; init; }

    /// <summary>METRC strain ID</summary>
    public long? MetrcStrainId { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static StrainNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<StrainNodeProperties>(json);
}

/// <summary>
/// Properties specific to CropSteeringProfile nodes.
/// </summary>
public sealed record SteeringProfileNodeProperties
{
    /// <summary>Profile name</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Target steering mode (Vegetative, Generative, Balanced)</summary>
    public string TargetMode { get; init; } = string.Empty;

    /// <summary>Is site default profile</summary>
    public bool IsSiteDefault { get; init; }

    /// <summary>Is active</summary>
    public bool IsActive { get; init; }

    /// <summary>Associated strain ID (null for site default)</summary>
    public Guid? StrainId { get; init; }

    /// <summary>P1 target VWC min</summary>
    public decimal? P1TargetVwcMin { get; init; }

    /// <summary>P1 target VWC max</summary>
    public decimal? P1TargetVwcMax { get; init; }

    /// <summary>P2 target VWC min</summary>
    public decimal? P2TargetVwcMin { get; init; }

    /// <summary>P2 target VWC max</summary>
    public decimal? P2TargetVwcMax { get; init; }

    /// <summary>P3 dryback target percent</summary>
    public decimal? P3DrybackTargetPercent { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static SteeringProfileNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SteeringProfileNodeProperties>(json);
}

/// <summary>
/// Properties specific to Harvest nodes.
/// </summary>
public sealed record HarvestNodeProperties
{
    /// <summary>Harvest name</summary>
    public string HarvestName { get; init; } = string.Empty;

    /// <summary>Harvest type</summary>
    public string HarvestType { get; init; } = string.Empty;

    /// <summary>Harvest status</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Harvest phase</summary>
    public string Phase { get; init; } = string.Empty;

    /// <summary>Strain ID</summary>
    public Guid? StrainId { get; init; }

    /// <summary>Strain name</summary>
    public string? StrainName { get; init; }

    /// <summary>Plant count</summary>
    public int PlantCount { get; init; }

    /// <summary>Total wet weight (grams)</summary>
    public decimal? TotalWetWeightGrams { get; init; }

    /// <summary>Total dry weight (grams)</summary>
    public decimal? TotalDryWeightGrams { get; init; }

    /// <summary>Average yield per plant (grams)</summary>
    public decimal? AvgYieldPerPlantGrams { get; init; }

    /// <summary>Waste weight (grams)</summary>
    public decimal? WasteWeightGrams { get; init; }

    /// <summary>Harvest date</summary>
    public DateTime? HarvestDate { get; init; }

    /// <summary>METRC harvest ID</summary>
    public long? MetrcHarvestId { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static HarvestNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<HarvestNodeProperties>(json);
}

/// <summary>
/// Properties specific to LabTestBatch nodes.
/// </summary>
public sealed record LabTestBatchNodeProperties
{
    /// <summary>Batch number</summary>
    public string BatchNumber { get; init; } = string.Empty;

    /// <summary>Lab name</summary>
    public string? LabName { get; init; }

    /// <summary>Test status</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Overall pass/fail</summary>
    public bool? Passed { get; init; }

    /// <summary>THC percentage</summary>
    public decimal? ThcPercent { get; init; }

    /// <summary>CBD percentage</summary>
    public decimal? CbdPercent { get; init; }

    /// <summary>Total cannabinoids</summary>
    public decimal? TotalCannabinoids { get; init; }

    /// <summary>Requires remediation</summary>
    public bool RequiresRemediation { get; init; }

    /// <summary>Sample collected date</summary>
    public DateTime? SampleCollectedAt { get; init; }

    /// <summary>Results received date</summary>
    public DateTime? ResultsReceivedAt { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static LabTestBatchNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<LabTestBatchNodeProperties>(json);
}
