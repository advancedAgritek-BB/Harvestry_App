using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskWatcherEntityConfiguration : IEntityTypeConfiguration<TaskWatcherRecord>
{
    public void Configure(EntityTypeBuilder<TaskWatcherRecord> builder)
    {
        builder.ToTable("task_watchers");

        builder.HasKey(x => x.TaskWatcherId);

        builder.Property(x => x.TaskWatcherId)
            .HasColumnName("task_watcher_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => new { x.TaskId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ux_task_watchers_task_user");
    }
}
