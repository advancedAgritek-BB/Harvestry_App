using System;
using System.Collections.Generic;
using System.Linq;

namespace Harvestry.Tasks.Domain.ValueObjects;

/// <summary>
/// Describes the outcome of evaluating SOP/training gating requirements for a task.
/// </summary>
public readonly record struct TaskGatingResult(
    bool IsGated,
    IReadOnlyCollection<Guid> MissingSopIds,
    IReadOnlyCollection<Guid> MissingTrainingIds,
    IReadOnlyCollection<string> Reasons)
{
    public static TaskGatingResult NotGated() => new(
        IsGated: false,
        MissingSopIds: Array.Empty<Guid>(),
        MissingTrainingIds: Array.Empty<Guid>(),
        Reasons: Array.Empty<string>());

    public static TaskGatingResult Gated(
        IEnumerable<Guid> missingSopIds,
        IEnumerable<Guid> missingTrainingIds,
        IEnumerable<string> reasons)
    {
        var sopList = missingSopIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        var trainingList = missingTrainingIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        var reasonList = reasons?.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToArray() ?? Array.Empty<string>();

        return new TaskGatingResult(
            IsGated: sopList.Length > 0 || trainingList.Length > 0 || reasonList.Length > 0,
            MissingSopIds: sopList,
            MissingTrainingIds: trainingList,
            Reasons: reasonList);
    }

    public bool HasMissingRequirements => MissingSopIds.Count > 0 || MissingTrainingIds.Count > 0;

    public TaskGatingResult Merge(TaskGatingResult other)
    {
        if (!IsGated && !other.IsGated)
        {
            return NotGated();
        }

        var mergedSops = MissingSopIds.Concat(other.MissingSopIds).Distinct().ToArray();
        var mergedTraining = MissingTrainingIds.Concat(other.MissingTrainingIds).Distinct().ToArray();
        var mergedReasons = Reasons.Concat(other.Reasons).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToArray();

        return new TaskGatingResult(
            IsGated: mergedSops.Length > 0 || mergedTraining.Length > 0 || mergedReasons.Length > 0,
            MissingSopIds: mergedSops,
            MissingTrainingIds: mergedTraining,
            Reasons: mergedReasons);
    }
}
