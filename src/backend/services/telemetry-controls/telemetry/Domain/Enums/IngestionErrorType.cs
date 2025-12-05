namespace Harvestry.Telemetry.Domain.Enums;

/// <summary>
/// Types of ingestion errors for monitoring and debugging.
/// </summary>
public enum IngestionErrorType
{
    /// <summary>
    /// Payload format is invalid (malformed JSON, missing fields, etc.)
    /// </summary>
    InvalidFormat = 1,
    
    /// <summary>
    /// Malformed payload that cannot be parsed
    /// </summary>
    MalformedPayload = 2,
    
    /// <summary>
    /// Validation failure (value constraints, type mismatch, etc.)
    /// </summary>
    ValidationFailure = 3,
    
    /// <summary>
    /// Required field is missing from payload
    /// </summary>
    MissingField = 4,
    
    /// <summary>
    /// Stream ID not found or inactive
    /// </summary>
    InvalidStreamId = 5,
    
    /// <summary>
    /// Unit of measurement not recognized or invalid for stream type
    /// </summary>
    InvalidUnit = 6,
    
    /// <summary>
    /// Value is outside acceptable range for stream type
    /// </summary>
    OutOfRange = 7,
    
    /// <summary>
    /// Duplicate message detected (idempotency check)
    /// </summary>
    DuplicateMessage = 8,
    
    /// <summary>
    /// Ingestion rate limit exceeded
    /// </summary>
    RateLimitExceeded = 9,
    
    /// <summary>
    /// Authentication or authorization failed
    /// </summary>
    AuthenticationFailed = 10,
    
    /// <summary>
    /// General processing error during ingestion
    /// </summary>
    ProcessingError = 11,
    
    /// <summary>
    /// Unknown or unclassified error
    /// </summary>
    Unknown = 99
}

