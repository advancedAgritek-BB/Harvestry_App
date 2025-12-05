using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Harvestry.Tasks.Infrastructure.Persistence;

public sealed class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<TaskRecord> Tasks => Set<TaskRecord>();
    public DbSet<TaskDependencyRecord> TaskDependencies => Set<TaskDependencyRecord>();
    public DbSet<TaskStateHistoryRecord> TaskStateHistory => Set<TaskStateHistoryRecord>();
    public DbSet<TaskWatcherRecord> TaskWatchers => Set<TaskWatcherRecord>();
    public DbSet<TaskTimeEntryRecord> TaskTimeEntries => Set<TaskTimeEntryRecord>();
    public DbSet<TaskRequiredSopRecord> TaskRequiredSops => Set<TaskRequiredSopRecord>();
    public DbSet<TaskRequiredTrainingRecord> TaskRequiredTraining => Set<TaskRequiredTrainingRecord>();
    public DbSet<TaskBlueprintRecord> TaskBlueprints => Set<TaskBlueprintRecord>();
    public DbSet<TaskBlueprintRequiredSopRecord> TaskBlueprintRequiredSops => Set<TaskBlueprintRequiredSopRecord>();
    public DbSet<TaskBlueprintRequiredTrainingRecord> TaskBlueprintRequiredTrainings => Set<TaskBlueprintRequiredTrainingRecord>();
    public DbSet<StandardOperatingProcedureRecord> StandardOperatingProcedures => Set<StandardOperatingProcedureRecord>();
    public DbSet<TaskLibraryItemRecord> TaskLibraryItems => Set<TaskLibraryItemRecord>();
    public DbSet<TaskLibraryItemSopRecord> TaskLibraryItemSops => Set<TaskLibraryItemSopRecord>();
    public DbSet<UserNotificationRecord> UserNotifications => Set<UserNotificationRecord>();
    public DbSet<ConversationRecord> Conversations => Set<ConversationRecord>();
    public DbSet<ConversationParticipantRecord> ConversationParticipants => Set<ConversationParticipantRecord>();
    public DbSet<MessageRecord> Messages => Set<MessageRecord>();
    public DbSet<MessageAttachmentRecord> MessageAttachments => Set<MessageAttachmentRecord>();
    public DbSet<MessageReadReceiptRecord> MessageReadReceipts => Set<MessageReadReceiptRecord>();
    public DbSet<SlackWorkspaceRecord> SlackWorkspaces => Set<SlackWorkspaceRecord>();
    public DbSet<SlackChannelMappingRecord> SlackChannelMappings => Set<SlackChannelMappingRecord>();
    public DbSet<SlackNotificationRecord> SlackNotifications => Set<SlackNotificationRecord>();
    public DbSet<SlackMessageBridgeLogRecord> SlackMessageBridgeLogs => Set<SlackMessageBridgeLogRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TasksDbContext).Assembly);
    }
}
