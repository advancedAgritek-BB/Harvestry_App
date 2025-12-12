using Harvestry.Irrigation.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Irrigation.Domain.Entities;

/// <summary>
/// Represents a command in the outbox queue to be sent to a device.
/// Implements the outbox pattern for reliable command delivery.
/// </summary>
public sealed class DeviceCommand : Entity<Guid>
{
    private DeviceCommand(Guid id) : base(id) { }

    public Guid SiteId { get; private set; }
    public Guid? RunId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public CommandType CommandType { get; private set; }
    public CommandStatus Status { get; private set; }
    public CommandPriority Priority { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ResponseJson { get; private set; }
    public int TimeoutSeconds { get; private set; }

    public static DeviceCommand Create(
        Guid siteId,
        Guid equipmentId,
        CommandType commandType,
        string payloadJson,
        Guid? runId = null,
        CommandPriority priority = CommandPriority.Normal,
        int maxRetries = 3,
        int timeoutSeconds = 30)
    {
        if (siteId == Guid.Empty)
            throw new ArgumentException("Site ID is required", nameof(siteId));
        if (equipmentId == Guid.Empty)
            throw new ArgumentException("Equipment ID is required", nameof(equipmentId));
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Payload JSON is required", nameof(payloadJson));

        return new DeviceCommand(Guid.NewGuid())
        {
            SiteId = siteId,
            RunId = runId,
            EquipmentId = equipmentId,
            CommandType = commandType,
            Status = CommandStatus.Pending,
            Priority = priority,
            PayloadJson = payloadJson,
            CorrelationId = Guid.NewGuid().ToString(),
            CreatedAt = DateTimeOffset.UtcNow,
            MaxRetries = maxRetries,
            TimeoutSeconds = timeoutSeconds
        };
    }

    public static DeviceCommand CreateEmergency(
        Guid siteId,
        Guid equipmentId,
        CommandType commandType,
        string payloadJson,
        Guid? runId = null)
    {
        var command = Create(siteId, equipmentId, commandType, payloadJson, runId);
        command.Priority = CommandPriority.Emergency;
        command.MaxRetries = 5;
        command.TimeoutSeconds = 10;
        return command;
    }

    public static DeviceCommand FromPersistence(
        Guid id,
        Guid siteId,
        Guid? runId,
        Guid equipmentId,
        CommandType commandType,
        CommandStatus status,
        CommandPriority priority,
        string payloadJson,
        string? correlationId,
        DateTimeOffset createdAt,
        DateTimeOffset? sentAt,
        DateTimeOffset? acknowledgedAt,
        DateTimeOffset? completedAt,
        int retryCount,
        int maxRetries,
        string? errorMessage,
        string? responseJson,
        int timeoutSeconds)
    {
        return new DeviceCommand(id)
        {
            SiteId = siteId,
            RunId = runId,
            EquipmentId = equipmentId,
            CommandType = commandType,
            Status = status,
            Priority = priority,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            CreatedAt = createdAt,
            SentAt = sentAt,
            AcknowledgedAt = acknowledgedAt,
            CompletedAt = completedAt,
            RetryCount = retryCount,
            MaxRetries = maxRetries,
            ErrorMessage = errorMessage,
            ResponseJson = responseJson,
            TimeoutSeconds = timeoutSeconds
        };
    }

    public void MarkSent()
    {
        if (Status != CommandStatus.Pending && Status != CommandStatus.Failed)
            throw new InvalidOperationException($"Cannot mark as sent when status is {Status}");

        Status = CommandStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkAcknowledged(string? responseJson = null)
    {
        if (Status != CommandStatus.Sent)
            throw new InvalidOperationException($"Cannot mark as acknowledged when status is {Status}");

        Status = CommandStatus.Acknowledged;
        AcknowledgedAt = DateTimeOffset.UtcNow;
        ResponseJson = responseJson;
    }

    public void MarkCompleted(string? responseJson = null)
    {
        if (Status != CommandStatus.Acknowledged && Status != CommandStatus.Sent)
            throw new InvalidOperationException($"Cannot mark as completed when status is {Status}");

        Status = CommandStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        if (responseJson != null)
            ResponseJson = responseJson;
    }

    public void MarkFailed(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = errorMessage;

        if (RetryCount >= MaxRetries)
        {
            Status = CommandStatus.FailedPermanent;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            Status = CommandStatus.Failed;
        }
    }

    public void MarkTimedOut()
    {
        if (Status != CommandStatus.Sent)
            throw new InvalidOperationException($"Cannot mark as timed out when status is {Status}");

        Status = CommandStatus.TimedOut;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = $"Command timed out after {TimeoutSeconds} seconds";
    }

    public void Cancel(string? reason = null)
    {
        if (Status != CommandStatus.Pending)
            throw new InvalidOperationException($"Cannot cancel command in status {Status}");

        Status = CommandStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        ErrorMessage = reason;
    }

    public bool CanRetry => Status == CommandStatus.Failed && RetryCount < MaxRetries;
    public bool IsTerminal => Status is CommandStatus.Completed
        or CommandStatus.FailedPermanent
        or CommandStatus.Cancelled
        or CommandStatus.TimedOut;

    public bool IsTimedOut => Status == CommandStatus.Sent
        && SentAt.HasValue
        && (DateTimeOffset.UtcNow - SentAt.Value).TotalSeconds > TimeoutSeconds;

    public TimeSpan? LatencyToAck => SentAt.HasValue && AcknowledgedAt.HasValue
        ? AcknowledgedAt.Value - SentAt.Value
        : null;
}
