using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Entities;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Mappers;

/// <summary>
/// Mapping helpers for mother plant domain objects.
/// </summary>
public static class MotherPlantMapper
{
    public static MotherPlantResponse ToResponse(MotherPlant mother)
    {
        ArgumentNullException.ThrowIfNull(mother);

        return new MotherPlantResponse(
            mother.Id,
            mother.SiteId,
            mother.BatchId,
            mother.StrainId,
            mother.PlantId.Value,
            mother.Status,
            mother.LocationId,
            mother.RoomId,
            mother.DateEstablished,
            mother.LastPropagationDate,
            mother.PropagationCount,
            mother.MaxPropagationCount,
            mother.Notes,
            new Dictionary<string, object>(mother.Metadata),
            mother.CreatedAt,
            mother.CreatedByUserId,
            mother.UpdatedAt,
            mother.UpdatedByUserId);
    }

    public static IReadOnlyList<MotherPlantResponse> ToResponseList(IEnumerable<MotherPlant> mothers)
    {
        return mothers.Select(ToResponse).ToList();
    }

    public static MotherHealthLogResponse ToHealthLogResponse(MotherHealthLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new MotherHealthLogResponse(
            log.Id,
            log.MotherPlantId,
            log.LogDate,
            log.HealthStatus,
            log.PestPressure,
            log.DiseasePressure,
            log.NutrientDeficiencies,
            log.Observations,
            log.TreatmentsApplied,
            log.EnvironmentalNotes,
            log.PhotoUrls,
            log.LoggedByUserId,
            log.CreatedAt);
    }

    public static IReadOnlyList<MotherHealthLogResponse> ToHealthLogResponseList(IEnumerable<MotherHealthLog> logs)
    {
        return logs.Select(ToHealthLogResponse).OrderByDescending(l => l.LogDate).ToList();
    }

    public static MotherPlantHealthSummaryResponse ToHealthSummary(
        MotherPlant mother,
        MotherHealthLog? latestLog,
        bool isOverdue,
        DateOnly? nextCheckDue,
        IReadOnlyList<MotherHealthLogResponse> recentLogs)
    {
        return new MotherPlantHealthSummaryResponse(
            ToResponse(mother),
            latestLog?.LogDate,
            latestLog?.HealthStatus,
            isOverdue,
            nextCheckDue,
            recentLogs);
    }

    public static PropagationSettingsResponse ToResponse(PropagationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new PropagationSettingsResponse(
            settings.Id,
            settings.SiteId,
            settings.DailyLimit,
            settings.WeeklyLimit,
            settings.MotherPropagationLimit,
            settings.RequiresOverrideApproval,
            settings.ApproverRole,
            new Dictionary<string, object>(settings.ApproverPolicy),
            settings.UpdatedAt,
            settings.UpdatedByUserId);
    }

    public static PropagationOverrideResponse ToResponse(PropagationOverrideRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new PropagationOverrideResponse(
            request.Id,
            request.SiteId,
            request.RequestedByUserId,
            request.MotherPlantId,
            request.BatchId,
            request.RequestedQuantity,
            request.Reason,
            request.Status,
            request.RequestedOn,
            request.ApprovedByUserId,
            request.ResolvedOn,
            request.DecisionNotes);
    }

    public static IReadOnlyList<PropagationOverrideResponse> ToResponseList(IEnumerable<PropagationOverrideRequest> requests)
    {
        return requests.Select(ToResponse)
            .OrderByDescending(r => r.RequestedOn)
            .ToList();
    }
}
