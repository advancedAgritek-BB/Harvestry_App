using System.ComponentModel.DataAnnotations;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.DTOs;

/// <summary>
/// Request payload to create a mother plant record.
/// </summary>
public record CreateMotherPlantRequest(
    Guid BatchId,
    Guid StrainId,
    string PlantTag,
    DateOnly DateEstablished,
    Guid? LocationId = null,
    Guid? RoomId = null,
    [Range(0, int.MaxValue, ErrorMessage = "MaxPropagationCount must be non-negative.")]
    int? MaxPropagationCount = null,
    string? Notes = null,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Request payload to update an existing mother plant.
/// </summary>
public record UpdateMotherPlantRequest(
    Guid? LocationId,
    Guid? RoomId,
    [Range(0, int.MaxValue, ErrorMessage = "MaxPropagationCount must be non-negative.")]
    int? MaxPropagationCount,
    string? Notes,
    Dictionary<string, object>? Metadata,
    MotherPlantStatusUpdate? StatusUpdate);

/// <summary>
/// Status mutation descriptor for a mother plant.
/// </summary>
public record MotherPlantStatusUpdate(
    MotherPlantStatusAction Action,
    string? Reason = null);

/// <summary>
/// Supported lifecycle actions for mother plants.
/// </summary>
public enum MotherPlantStatusAction
{
    Retire = 0,
    Reactivate = 1,
    Quarantine = 2,
    ReleaseFromQuarantine = 3,
    Destroy = 4
}

/// <summary>
/// Mother plant response payload.
/// </summary>
public record MotherPlantResponse(
    Guid Id,
    Guid SiteId,
    Guid BatchId,
    Guid StrainId,
    string PlantTag,
    MotherPlantStatus Status,
    Guid? LocationId,
    Guid? RoomId,
    DateOnly DateEstablished,
    DateOnly? LastPropagationDate,
    int PropagationCount,
    int? MaxPropagationCount,
    string? Notes,
    Dictionary<string, object> Metadata,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    DateTime UpdatedAt,
    Guid? UpdatedByUserId);

/// <summary>
/// Request payload for recording a health assessment.
/// </summary>
public record MotherPlantHealthLogRequest(
    DateOnly? LogDate,
    HealthStatus Status,
    PressureLevel PestPressure,
    PressureLevel DiseasePressure,
    IReadOnlyCollection<string> NutrientDeficiencies,
    string? Observations,
    string? TreatmentsApplied,
    string? EnvironmentalNotes,
    IReadOnlyCollection<string> PhotoUrls);

/// <summary>
/// Health log response payload.
/// </summary>
public record MotherHealthLogResponse(
    Guid Id,
    Guid MotherPlantId,
    DateOnly LogDate,
    HealthStatus Status,
    PressureLevel PestPressure,
    PressureLevel DiseasePressure,
    IReadOnlyCollection<string> NutrientDeficiencies,
    string? Observations,
    string? TreatmentsApplied,
    string? EnvironmentalNotes,
    IReadOnlyCollection<string> PhotoUrls,
    Guid LoggedByUserId,
    DateTime CreatedAt);

/// <summary>
/// Aggregate health summary for a mother plant.
/// </summary>
public record MotherPlantHealthSummaryResponse(
    MotherPlantResponse MotherPlant,
    DateOnly? LastHealthCheck,
    HealthStatus? LatestStatus,
    bool IsOverdue,
    DateOnly? NextHealthCheckDue,
    IReadOnlyList<MotherHealthLogResponse> RecentLogs);

/// <summary>
/// Request payload to register a propagation event.
/// </summary>
public record RegisterPropagationRequest(
    [Range(1, int.MaxValue, ErrorMessage = "PropagatedCount must be greater than zero.")]
    int PropagatedCount,
    string? Notes = null);

/// <summary>
/// Propagation settings response payload.
/// </summary>
public record PropagationSettingsResponse(
    Guid? Id,
    Guid SiteId,
    int? DailyLimit,
    int? WeeklyLimit,
    int? MotherPropagationLimit,
    bool RequiresOverrideApproval,
    string? ApproverRole,
    Dictionary<string, object> ApproverPolicy,
    DateTime UpdatedAt,
    Guid? UpdatedByUserId);

/// <summary>
/// Request payload to update propagation settings.
/// </summary>
public record UpdatePropagationSettingsRequest(
    [Range(0, int.MaxValue, ErrorMessage = "DailyLimit must be non-negative.")]
    int? DailyLimit,
    [Range(0, int.MaxValue, ErrorMessage = "WeeklyLimit must be non-negative.")]
    int? WeeklyLimit,
    [Range(0, int.MaxValue, ErrorMessage = "MotherPropagationLimit must be non-negative.")]
    int? MotherPropagationLimit,
    bool RequiresOverrideApproval,
    string? ApproverRole,
    Dictionary<string, object>? ApproverPolicy);

/// <summary>
/// Request payload to create a propagation override.
/// </summary>
public record CreatePropagationOverrideRequest(
    Guid? MotherPlantId,
    Guid? BatchId,
    [Range(1, int.MaxValue, ErrorMessage = "RequestedQuantity must be greater than zero.")]
    int RequestedQuantity,
    [Required]
    [MinLength(5, ErrorMessage = "Reason must be at least 5 characters.")]
    string Reason) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MotherPlantId is null && BatchId is null)
        {
            yield return new ValidationResult(
                "At least one of MotherPlantId or BatchId must be provided.",
                new[] { nameof(MotherPlantId), nameof(BatchId) });
        }
    }
}

/// <summary>
/// Override decision directive.
/// </summary>
public record PropagationOverrideDecisionRequest(
    PropagationOverrideDecision Decision,
    string? Notes = null);

/// <summary>
/// Decision types for propagation overrides.
/// </summary>
public enum PropagationOverrideDecision
{
    Approve = 0,
    Reject = 1,
    Expire = 2
}

/// <summary>
/// Propagation override response payload.
/// </summary>
public record PropagationOverrideResponse(
    Guid Id,
    Guid SiteId,
    Guid RequestedByUserId,
    Guid? MotherPlantId,
    Guid? BatchId,
    int RequestedQuantity,
    string Reason,
    PropagationOverrideStatus Status,
    DateTime RequestedOn,
    Guid? ApprovedByUserId,
    DateTime? ResolvedOn,
    string? DecisionNotes);
