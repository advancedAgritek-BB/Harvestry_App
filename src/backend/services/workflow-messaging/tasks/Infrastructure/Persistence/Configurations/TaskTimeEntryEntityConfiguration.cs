using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskTimeEntryEntityConfiguration : IEntityTypeConfiguration<TaskTimeEntryRecord>
{
    public void Configure(EntityTypeBuilder<TaskTimeEntryRecord> builder)
    {
        builder.ToTable("task_time_entries");

        builder.HasKey(x => x.TaskTimeEntryId);

        builder.Property(x => x.TaskTimeEntryId)
            .HasColumnName("task_time_entry_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(x => x.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(x => x.Notes)
            .HasColumnName("notes");

        builder.HasOne(x => x.Task)
            .WithMany(t => t.TimeEntries)
            .HasForeignKey(x => x.TaskId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TaskId, x.UserId, x.StartedAt })
            .HasDatabaseName("ix_task_time_entries_task_user_start");
    }
}
