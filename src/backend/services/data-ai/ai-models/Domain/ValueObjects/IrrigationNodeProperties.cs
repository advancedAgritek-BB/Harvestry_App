using System.Text.Json;

namespace Harvestry.AiModels.Domain.ValueObjects;

/// <summary>
/// Properties specific to IrrigationRun nodes for irrigation response anomaly detection.
/// </summary>
public sealed record IrrigationRunNodeProperties
{
    /// <summary>Program ID</summary>
    public Guid ProgramId { get; init; }

    /// <summary>Group ID</summary>
    public Guid GroupId { get; init; }

    /// <summary>Schedule ID if scheduled</summary>
    public Guid? ScheduleId { get; init; }

    /// <summary>Run status</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Total steps in run</summary>
    public int TotalSteps { get; init; }

    /// <summary>Completed steps</summary>
    public int CompletedSteps { get; init; }

    /// <summary>Target zone IDs</summary>
    public Guid[] TargetZoneIds { get; init; } = Array.Empty<Guid>();

    /// <summary>Expected total liters delivered</summary>
    public decimal? ExpectedLitersDelivered { get; init; }

    /// <summary>Actual duration in seconds</summary>
    public int? ActualDurationSeconds { get; init; }

    /// <summary>Initiated by (system/user)</summary>
    public string InitiatedBy { get; init; } = string.Empty;

    /// <summary>Initiated by user ID</summary>
    public Guid? InitiatedByUserId { get; init; }

    /// <summary>Interlock type if tripped</summary>
    public string? InterlockType { get; init; }

    /// <summary>Fault message if faulted</summary>
    public string? FaultMessage { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static IrrigationRunNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<IrrigationRunNodeProperties>(json);
}

/// <summary>
/// Properties specific to SensorStream nodes for telemetry graph.
/// </summary>
public sealed record SensorStreamNodeProperties
{
    /// <summary>Stream type (VWC, EC, Temperature, etc.)</summary>
    public int StreamType { get; init; }

    /// <summary>Stream type name</summary>
    public string StreamTypeName { get; init; } = string.Empty;

    /// <summary>Unit of measurement</summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>Display name</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Equipment ID</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Equipment channel ID</summary>
    public Guid? EquipmentChannelId { get; init; }

    /// <summary>Location ID</summary>
    public Guid? LocationId { get; init; }

    /// <summary>Room ID</summary>
    public Guid? RoomId { get; init; }

    /// <summary>Zone ID</summary>
    public Guid? ZoneId { get; init; }

    /// <summary>Is active</summary>
    public bool IsActive { get; init; }

    /// <summary>Latest reading value</summary>
    public double? LatestValue { get; init; }

    /// <summary>Latest reading timestamp</summary>
    public DateTime? LatestReadingAt { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static SensorStreamNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SensorStreamNodeProperties>(json);
}

/// <summary>
/// Properties specific to ZoneEmitterConfig nodes.
/// </summary>
public sealed record ZoneEmitterConfigNodeProperties
{
    /// <summary>Zone ID</summary>
    public Guid ZoneId { get; init; }

    /// <summary>Zone name</summary>
    public string ZoneName { get; init; } = string.Empty;

    /// <summary>Emitter count</summary>
    public int EmitterCount { get; init; }

    /// <summary>Emitter flow rate (L/h)</summary>
    public decimal EmitterFlowRateLitersPerHour { get; init; }

    /// <summary>Emitter type</summary>
    public string EmitterType { get; init; } = string.Empty;

    /// <summary>Emitters per plant</summary>
    public int EmittersPerPlant { get; init; }

    /// <summary>Total zone flow rate (L/min)</summary>
    public decimal TotalZoneFlowRateLitersPerMinute { get; init; }

    /// <summary>Operating pressure (kPa)</summary>
    public decimal? OperatingPressureKpa { get; init; }

    /// <summary>Last calibrated at</summary>
    public DateTime? LastCalibratedAt { get; init; }

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static ZoneEmitterConfigNodeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<ZoneEmitterConfigNodeProperties>(json);
}

/// <summary>
/// Edge properties for irrigation response analysis.
/// Captures expected vs actual VWC change after irrigation.
/// </summary>
public sealed record IrrigationResponseEdgeProperties
{
    /// <summary>Expected VWC increase based on liters delivered</summary>
    public decimal ExpectedVwcIncrease { get; init; }

    /// <summary>Actual VWC increase observed</summary>
    public decimal? ActualVwcIncrease { get; init; }

    /// <summary>VWC before irrigation</summary>
    public decimal? VwcBefore { get; init; }

    /// <summary>VWC after irrigation (peak)</summary>
    public decimal? VwcAfter { get; init; }

    /// <summary>Time to peak VWC (seconds)</summary>
    public int? TimeToPeakSeconds { get; init; }

    /// <summary>Liters delivered in this irrigation</summary>
    public decimal LitersDelivered { get; init; }

    /// <summary>Duration of irrigation (seconds)</summary>
    public int DurationSeconds { get; init; }

    /// <summary>Response ratio (actual/expected)</summary>
    public decimal? ResponseRatio => ExpectedVwcIncrease > 0 && ActualVwcIncrease.HasValue
        ? ActualVwcIncrease.Value / ExpectedVwcIncrease
        : null;

    /// <summary>Serialize to JSON</summary>
    public string ToJson() => JsonSerializer.Serialize(this);

    /// <summary>Deserialize from JSON</summary>
    public static IrrigationResponseEdgeProperties? FromJson(string? json)
        => string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<IrrigationResponseEdgeProperties>(json);
}
