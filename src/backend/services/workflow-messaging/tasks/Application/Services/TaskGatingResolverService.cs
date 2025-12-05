using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Application.DTOs;
using Harvestry.Tasks.Application.Interfaces;
using DomainTask = Harvestry.Tasks.Domain.Entities.Task;

namespace Harvestry.Tasks.Application.Services;

public sealed class TaskGatingResolverService : ITaskGatingResolverService
{
    private readonly IUserReadinessProvider _readinessProvider;

    public TaskGatingResolverService(IUserReadinessProvider readinessProvider)
    {
        _readinessProvider = readinessProvider ?? throw new ArgumentNullException(nameof(readinessProvider));
    }

    public async Task<TaskGatingStatusResponse> EvaluateAsync(DomainTask task, Guid userId, CancellationToken cancellationToken)
    {
        if (task is null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var requiredSops = task.RequiredSopIds;
        var requiredTraining = task.RequiredTrainingIds;

        IReadOnlyCollection<Guid> completedSops = Array.Empty<Guid>();
        IReadOnlyCollection<Guid> completedTraining = Array.Empty<Guid>();

        if (requiredSops.Count > 0)
        {
            completedSops = await _readinessProvider
                .GetCompletedSopIdsAsync(userId, requiredSops, cancellationToken)
                .ConfigureAwait(false);
        }

        if (requiredTraining.Count > 0)
        {
            completedTraining = await _readinessProvider
                .GetCompletedTrainingIdsAsync(userId, requiredTraining, cancellationToken)
                .ConfigureAwait(false);
        }

        var missingSops = requiredSops.Except(completedSops).ToArray();
        var missingTraining = requiredTraining.Except(completedTraining).ToArray();

        if (missingSops.Length == 0 && missingTraining.Length == 0)
        {
            return new TaskGatingStatusResponse
            {
                IsGated = false,
                MissingSopIds = Array.Empty<Guid>(),
                MissingTrainingIds = Array.Empty<Guid>(),
                Reasons = Array.Empty<string>()
            };
        }

        var reasons = new List<string>(2);
        if (missingSops.Length > 0)
        {
            reasons.Add("SOP signoff required");
        }

        if (missingTraining.Length > 0)
        {
            reasons.Add("Training completion required");
        }

        return new TaskGatingStatusResponse
        {
            IsGated = true,
            MissingSopIds = missingSops,
            MissingTrainingIds = missingTraining,
            Reasons = reasons
        };
    }

    public Task<IReadOnlyCollection<Guid>> GetCompletedSopsAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken cancellationToken)
    {
        return _readinessProvider.GetCompletedSopIdsAsync(userId, requiredSopIds, cancellationToken);
    }

    public Task<IReadOnlyCollection<Guid>> GetCompletedTrainingModulesAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken cancellationToken)
    {
        return _readinessProvider.GetCompletedTrainingIdsAsync(userId, requiredTrainingIds, cancellationToken);
    }
}
