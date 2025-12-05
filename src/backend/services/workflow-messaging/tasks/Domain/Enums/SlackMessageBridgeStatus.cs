namespace Harvestry.Tasks.Domain.Enums;

public enum SlackMessageBridgeStatus
{
    Pending = 0,
    Retrying = 1,
    Sent = 2,
    Failed = 3
}
