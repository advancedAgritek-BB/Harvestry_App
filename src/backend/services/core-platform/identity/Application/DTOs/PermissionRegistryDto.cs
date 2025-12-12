using System.Collections.Generic;

namespace Harvestry.Identity.Application.DTOs;

/// <summary>
/// Complete permission registry response containing all sections and bundles
/// </summary>
public sealed class PermissionRegistryDto
{
    public IReadOnlyList<PermissionSectionDto> Sections { get; init; } = new List<PermissionSectionDto>();
    public IReadOnlyList<PermissionBundleDto> Bundles { get; init; } = new List<PermissionBundleDto>();
    public IReadOnlyList<string> BundleCategories { get; init; } = new List<string>();
}

/// <summary>
/// A permission section containing related permissions
/// </summary>
public sealed class PermissionSectionDto
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyList<PermissionDto> Permissions { get; init; } = new List<PermissionDto>();
}

/// <summary>
/// A single permission definition
/// </summary>
public sealed class PermissionDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool RequiresTwoPersonApproval { get; init; }
    public bool RequiresReason { get; init; }
}

/// <summary>
/// A preset bundle of permissions
/// </summary>
public sealed class PermissionBundleDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Category { get; init; }
    public int DisplayOrder { get; init; }
    public int PermissionCount { get; init; }
    public IReadOnlyList<string> Permissions { get; init; } = new List<string>();
}
