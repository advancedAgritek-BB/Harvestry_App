using System;

namespace Harvestry.Tasks.Infrastructure.Persistence.Models;

public sealed class StandardOperatingProcedureRecord
{
    public Guid SopId { get; set; }
    public Guid OrgId { get; set; }
    public string Title { get; set; } = null!;
    public string? Content { get; set; }
    public string? Category { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

