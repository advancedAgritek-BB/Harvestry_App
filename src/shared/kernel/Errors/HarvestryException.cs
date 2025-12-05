using System;

namespace Harvestry.Shared.Kernel.Errors;

/// <summary>
/// Base exception class for all Harvestry domain exceptions.
/// Includes structured error code support for consistent error handling.
/// </summary>
public class HarvestryException : Exception
{
    /// <summary>
    /// The structured error code from <see cref="ErrorCodes"/>.
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Additional context data for debugging and logging.
    /// </summary>
    public object? Context { get; }
    
    /// <summary>
    /// Creates a new HarvestryException with an error code.
    /// </summary>
    /// <param name="errorCode">The error code from <see cref="ErrorCodes"/>.</param>
    /// <param name="message">The error message.</param>
    public HarvestryException(string errorCode, string message) 
        : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }
    
    /// <summary>
    /// Creates a new HarvestryException with an error code and context.
    /// </summary>
    /// <param name="errorCode">The error code from <see cref="ErrorCodes"/>.</param>
    /// <param name="message">The error message.</param>
    /// <param name="context">Additional context data.</param>
    public HarvestryException(string errorCode, string message, object? context) 
        : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Context = context;
    }
    
    /// <summary>
    /// Creates a new HarvestryException with an error code and inner exception.
    /// </summary>
    /// <param name="errorCode">The error code from <see cref="ErrorCodes"/>.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public HarvestryException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }
    
    /// <summary>
    /// Creates a new HarvestryException with full details.
    /// </summary>
    /// <param name="errorCode">The error code from <see cref="ErrorCodes"/>.</param>
    /// <param name="message">The error message.</param>
    /// <param name="context">Additional context data.</param>
    /// <param name="innerException">The inner exception.</param>
    public HarvestryException(string errorCode, string message, object? context, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Context = context;
    }
}

/// <summary>
/// Exception for authentication and authorization failures.
/// </summary>
public class AuthenticationException : HarvestryException
{
    public AuthenticationException(string errorCode, string message) 
        : base(errorCode, message) { }
    
    public AuthenticationException(string errorCode, string message, object? context) 
        : base(errorCode, message, context) { }
}

/// <summary>
/// Exception for entity not found scenarios.
/// </summary>
public class EntityNotFoundException : HarvestryException
{
    public string EntityType { get; }
    public object EntityId { get; }
    
    public EntityNotFoundException(string errorCode, string entityType, object entityId) 
        : base(errorCode, $"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
    
    public EntityNotFoundException(string errorCode, string entityType, object entityId, string message) 
        : base(errorCode, message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception for validation failures.
/// </summary>
public class ValidationException : HarvestryException
{
    public IDictionary<string, string[]>? Errors { get; }
    
    public ValidationException(string errorCode, string message) 
        : base(errorCode, message) { }
    
    public ValidationException(string errorCode, string message, IDictionary<string, string[]> errors) 
        : base(errorCode, message, errors)
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception for business rule violations.
/// </summary>
public class BusinessRuleException : HarvestryException
{
    public BusinessRuleException(string errorCode, string message) 
        : base(errorCode, message) { }
    
    public BusinessRuleException(string errorCode, string message, object? context) 
        : base(errorCode, message, context) { }
}

/// <summary>
/// Exception for external integration failures.
/// </summary>
public class IntegrationException : HarvestryException
{
    public string IntegrationName { get; }
    
    public IntegrationException(string errorCode, string integrationName, string message) 
        : base(errorCode, message)
    {
        IntegrationName = integrationName;
    }
    
    public IntegrationException(string errorCode, string integrationName, string message, Exception innerException) 
        : base(errorCode, message, innerException)
    {
        IntegrationName = integrationName;
    }
}

/// <summary>
/// Exception for rate limiting scenarios.
/// </summary>
public class RateLimitException : HarvestryException
{
    public TimeSpan? RetryAfter { get; }
    
    public RateLimitException(string message, TimeSpan? retryAfter = null) 
        : base(ErrorCodes.SysRateLimitExceeded, message)
    {
        RetryAfter = retryAfter;
    }
}

