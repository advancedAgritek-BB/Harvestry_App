using Harvestry.Analytics.Domain.ValueObjects;

namespace Harvestry.Analytics.Application.DTOs;

public record DashboardDto(
    Guid Id,
    string Name,
    string? Description,
    List<DashboardWidget> LayoutConfig,
    bool IsPublic,
    Guid OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateDashboardDto(
    string Name,
    string? Description,
    List<DashboardWidget> LayoutConfig,
    bool IsPublic
);

public record UpdateDashboardDto(
    string Name,
    string? Description,
    List<DashboardWidget> LayoutConfig,
    bool IsPublic
);




