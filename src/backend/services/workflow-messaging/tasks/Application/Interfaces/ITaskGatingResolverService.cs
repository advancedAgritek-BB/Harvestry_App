using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;

namespace Harvestry.Tasks.Application.Interfaces;

/// <summary>
/// Service responsible for evaluating task gating conditions (SOP completion, training requirements, etc.)
/// to determine whether a task can be started by a user.
/// </summary>
public interface ITaskGatingResolverService
{
    /// <summary>
    /// Evaluates all gating conditions for a task and user, returning a status indicating whether the task is gated.
    /// </summary>
    /// <param name="task">The task to evaluate (must not be null).</param>
    /// <param name="userId">The user for whom to evaluate gating (must not be Guid.Empty).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A TaskGatingStatusResponse indicating whether the task is gated and which requirements are missing.
    /// Never returns null; on error, throws an exception.
    /// </returns>
    Task<TaskGatingStatusResponse> EvaluateAsync(DomainTask task, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the subset of required SOP IDs that the user has completed.
    /// </summary>
    /// <param name="userId">The user to check (must not be Guid.Empty).</param>
    /// <param name="requiredSopIds">Collection of SOP IDs to check (must not be null; empty collection is valid).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A collection of completed SOP IDs (subset of requiredSopIds).
    /// Returns an empty collection if none are completed or if requiredSopIds is empty.
    /// Never returns null.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetCompletedSopsAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the subset of required training module IDs that the user has completed.
    /// </summary>
    /// <param name="userId">The user to check (must not be Guid.Empty).</param>
    /// <param name="requiredTrainingIds">Collection of training module IDs to check (must not be null; empty collection is valid).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A collection of completed training module IDs (subset of requiredTrainingIds).
    /// Returns an empty collection if none are completed or if requiredTrainingIds is empty.
    /// Never returns null.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetCompletedTrainingModulesAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken cancellationToken);
}
