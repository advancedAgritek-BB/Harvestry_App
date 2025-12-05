using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Tasks.Domain.ValueObjects;

/// <summary>
/// Encapsulates whether task dependencies are satisfied.
/// </summary>
public readonly record struct TaskDependencyResult(
    bool IsSatisfied,
    IReadOnlyCollection<Guid> BlockingTaskIds,
    IReadOnlyCollection<string> Reasons)
{
    public static TaskDependencyResult Satisfied() => new(
        IsSatisfied: true,
        BlockingTaskIds: Array.Empty<Guid>(),
        Reasons: Array.Empty<string>());

    public static TaskDependencyResult Blocked(
        IEnumerable<Guid> blockingTaskIds,
        IEnumerable<string> reasons)
    {
        var blockingIds = blockingTaskIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();
        var reasonList = reasons?.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToArray() ?? Array.Empty<string>();

        return new TaskDependencyResult(
            IsSatisfied: false,
            BlockingTaskIds: blockingIds,
            Reasons: reasonList);
    }

    public TaskDependencyResult Merge(TaskDependencyResult other)
    {
        if (IsSatisfied && other.IsSatisfied)
        {
            return Satisfied();
        }

        var mergedBlockingIds = BlockingTaskIds.Concat(other.BlockingTaskIds)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
        var mergedReasons = Reasons.Concat(other.Reasons)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .ToArray();

        // When merging and at least one operand was not satisfied, the result must also be unsatisfied
        // regardless of whether the merged collections happen to be empty
        return new TaskDependencyResult(
            IsSatisfied: false,
            BlockingTaskIds: mergedBlockingIds,
            Reasons: mergedReasons);
    }
}
