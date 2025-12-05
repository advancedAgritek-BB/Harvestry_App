using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskStateHistoryEntityConfiguration : IEntityTypeConfiguration<TaskStateHistoryRecord>
{
    public void Configure(EntityTypeBuilder<TaskStateHistoryRecord> builder)
    {
        builder.ToTable("task_state_history");

        builder.HasKey(x => x.TaskStateHistoryId);

        builder.Property(x => x.TaskStateHistoryId)
            .HasColumnName("task_state_history_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .HasColumnName("from_status")
            .IsRequired();

        builder.Property(x => x.ToStatus)
            .HasColumnName("to_status")
            .IsRequired();

        builder.Property(x => x.ChangedBy)
            .HasColumnName("changed_by")
            .IsRequired();

        builder.Property(x => x.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasMaxLength(1000);

        builder.HasOne(e => e.Task)
            .WithMany(t => t.StateHistory)
            .HasForeignKey(e => e.TaskId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TaskId, x.ChangedAt })
            .HasDatabaseName("ix_task_state_history_task_changed_at");
    }
}
