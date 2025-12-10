namespace Harvestry.Analytics.Application.DTOs;

public record ShareDto(
    Guid Id, 
    string ResourceType, 
    Guid ResourceId, 
    Guid SharedWithId, 
    string SharedWithType, 
    string PermissionLevel
);

public record CreateShareDto(
    string ResourceType, 
    Guid ResourceId, 
    Guid SharedWithId, 
    string SharedWithType, 
    string PermissionLevel
);




