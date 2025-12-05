using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskGenerationService
{
    /// <summary>
    /// Generates tasks based on matching blueprints when a batch/plant phase changes.
    /// </summary>
    /// <param name="siteId">The site where the phase change occurred.</param>
    /// <param name="batchId">The batch that changed phase.</param>
    /// <param name="strainId">The strain of the batch (optional for generic blueprints).</param>
    /// <param name="newPhase">The new growth phase.</param>
    /// <param name="roomType">The room type where the batch is located.</param>
    /// <param name="triggeredByUserId">The user who triggered the phase change (for task creation audit).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created tasks.</returns>
    Task<IReadOnlyList<TaskResponse>> GenerateTasksForPhaseChangeAsync(
        Guid siteId,
        Guid batchId,
        Guid? strainId,
        GrowthPhase newPhase,
        BlueprintRoomType roomType,
        Guid triggeredByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually triggers task generation for a batch (useful for testing or manual runs).
    /// </summary>
    Task<IReadOnlyList<TaskResponse>> ManuallyGenerateTasksAsync(
        Guid siteId,
        Guid batchId,
        Guid? strainId,
        GrowthPhase phase,
        BlueprintRoomType roomType,
        Guid triggeredByUserId,
        CancellationToken cancellationToken = default);
}

