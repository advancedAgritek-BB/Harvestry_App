using System;
using System.Collections.Generic;

namespace Harvestry.Tasks.Application.DTOs;

public sealed class TaskGatingStatusResponse
{
    public bool IsGated { get; init; }
    public IReadOnlyCollection<Guid> MissingSopIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<Guid> MissingTrainingIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyCollection<string> Reasons { get; init; } = Array.Empty<string>();
}
