using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// DTO for ingestion error.
/// </summary>
public record IngestionErrorDto(
    Guid Id,
    Guid SiteId,
    Guid? SessionId,
    Guid? EquipmentId,
    IngestionProtocol Protocol,
    IngestionErrorType ErrorType,
    string ErrorMessage,
    DateTimeOffset OccurredAt
);

