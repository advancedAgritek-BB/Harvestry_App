namespace Harvestry.Tasks.Domain.Enums;

public enum NotificationType
{
    Undefined = 0,
    TaskCreated = 1,
    TaskAssigned = 2,
    TaskCompleted = 3,
    TaskOverdue = 4,
    TaskBlocked = 5,
    ConversationMention = 6,
    AlertCritical = 7,
    AlertWarning = 8
}
