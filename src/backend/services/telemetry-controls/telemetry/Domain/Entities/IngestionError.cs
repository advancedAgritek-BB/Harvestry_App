using Harvestry.Telemetry.Domain.Enums;

namespace Harvestry.Telemetry.Domain.Entities;

/// <summary>
/// Represents an ingestion error for monitoring and debugging.
/// Used to track failed ingestion attempts and patterns.
/// </summary>
public class IngestionError : Entity<Guid>
{
    public Guid SiteId { get; private set; }
    public Guid? SessionId { get; private set; }
    public Guid? EquipmentId { get; private set; }
    public IngestionProtocol Protocol { get; private set; }
    public IngestionErrorType ErrorType { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public Dictionary<string, object>? RawPayload { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    
    // For EF Core
    private IngestionError() { }
    
    private IngestionError(
        Guid id,
        Guid siteId,
        IngestionProtocol protocol,
        IngestionErrorType errorType,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message is required", nameof(errorMessage));
            
        Id = id;
        SiteId = siteId;
        Protocol = protocol;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        OccurredAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Creates a new ingestion error.
    /// </summary>
    public static IngestionError Create(
        Guid siteId,
        IngestionProtocol protocol,
        IngestionErrorType errorType,
        string errorMessage,
        Guid? sessionId = null,
        Guid? equipmentId = null,
        Dictionary<string, object>? rawPayload = null)
    {
        var error = new IngestionError(Guid.NewGuid(), siteId, protocol, errorType, errorMessage)
        {
            SessionId = sessionId,
            EquipmentId = equipmentId,
            RawPayload = rawPayload
        };
        
        return error;
    }
    
    /// <summary>
    /// Rehydrates ingestion error from persistence layer.
    /// </summary>
    public static IngestionError FromPersistence(
        Guid id,
        Guid siteId,
        Guid? sessionId,
        Guid? equipmentId,
        IngestionProtocol protocol,
        IngestionErrorType errorType,
        string errorMessage,
        Dictionary<string, object>? rawPayload,
        DateTimeOffset occurredAt)
    {
        return new IngestionError
        {
            Id = id,
            SiteId = siteId,
            SessionId = sessionId,
            EquipmentId = equipmentId,
            Protocol = protocol,
            ErrorType = errorType,
            ErrorMessage = errorMessage,
            RawPayload = rawPayload,
            OccurredAt = occurredAt
        };
    }
}

