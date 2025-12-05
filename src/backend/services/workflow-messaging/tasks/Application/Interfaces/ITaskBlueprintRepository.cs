using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Harvestry.Tasks.Domain.Entities;
using Harvestry.Tasks.Domain.Enums;

namespace Harvestry.Tasks.Application.Interfaces;

public interface ITaskBlueprintRepository
{
    Task AddAsync(TaskBlueprint blueprint, CancellationToken cancellationToken);
    Task<TaskBlueprint?> GetByIdAsync(Guid siteId, Guid blueprintId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskBlueprint>> GetBySiteAsync(Guid siteId, bool? activeOnly, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskBlueprint>> GetMatchingBlueprintsAsync(
        Guid siteId,
        GrowthPhase growthPhase,
        BlueprintRoomType roomType,
        Guid? strainId,
        CancellationToken cancellationToken);
    Task UpdateAsync(TaskBlueprint blueprint, CancellationToken cancellationToken);
    Task DeleteAsync(Guid siteId, Guid blueprintId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

