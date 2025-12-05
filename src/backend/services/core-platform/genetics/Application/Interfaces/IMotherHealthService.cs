using Harvestry.Genetics.Application.DTOs;
using Harvestry.Genetics.Domain.Enums;

namespace Harvestry.Genetics.Application.Interfaces;

/// <summary>
/// Application service contract for mother plant health and propagation management.
/// </summary>
public interface IMotherHealthService
{
    Task<MotherPlantResponse> CreateMotherPlantAsync(Guid siteId, CreateMotherPlantRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<MotherPlantResponse?> GetMotherPlantByIdAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlantResponse>> GetMotherPlantsAsync(Guid siteId, MotherPlantStatus? status, CancellationToken cancellationToken = default);
    Task<MotherPlantResponse> UpdateMotherPlantAsync(Guid siteId, Guid motherPlantId, UpdateMotherPlantRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<MotherPlantResponse> RecordHealthLogAsync(Guid siteId, Guid motherPlantId, MotherPlantHealthLogRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<MotherPlantResponse> RegisterPropagationAsync(Guid siteId, Guid motherPlantId, RegisterPropagationRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<MotherPlantHealthSummaryResponse> GetHealthSummaryAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherHealthLogResponse>> GetHealthLogsAsync(Guid siteId, Guid motherPlantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MotherPlantResponse>> GetOverdueForHealthCheckAsync(Guid siteId, CancellationToken cancellationToken = default);

    Task<PropagationSettingsResponse> GetPropagationSettingsAsync(Guid siteId, CancellationToken cancellationToken = default);
    Task<PropagationSettingsResponse> UpdatePropagationSettingsAsync(Guid siteId, UpdatePropagationSettingsRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task<PropagationOverrideResponse> RequestPropagationOverrideAsync(Guid siteId, CreatePropagationOverrideRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<PropagationOverrideResponse> DecidePropagationOverrideAsync(Guid siteId, Guid overrideId, PropagationOverrideDecisionRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropagationOverrideResponse>> GetPropagationOverridesAsync(Guid siteId, PropagationOverrideStatus? status, CancellationToken cancellationToken = default);
}
