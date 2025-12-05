using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Harvestry.Tasks.Application.Interfaces;

/// <summary>
/// Provides read access to SOP signoffs and training completion records needed for task gating.
/// </summary>
public interface IUserReadinessProvider
{
    Task<IReadOnlyCollection<Guid>> GetCompletedSopIdsAsync(Guid userId, IReadOnlyCollection<Guid> requiredSopIds, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Guid>> GetCompletedTrainingIdsAsync(Guid userId, IReadOnlyCollection<Guid> requiredTrainingIds, CancellationToken cancellationToken);
}
