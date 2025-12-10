namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// Context used to generate cultivation insights from telemetry and operations data.
/// </summary>
public sealed record CultivationInsightContext(
    string Room,
    string Phase,
    string TelemetrySummary,
    string Issues);



