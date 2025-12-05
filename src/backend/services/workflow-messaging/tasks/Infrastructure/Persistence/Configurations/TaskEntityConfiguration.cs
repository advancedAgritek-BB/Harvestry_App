using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskRecord>
{
    public void Configure(EntityTypeBuilder<TaskRecord> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(x => x.TaskId);

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id");

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.TaskType)
            .HasColumnName("task_type")
            .IsRequired();

        builder.Property(x => x.CustomTaskType)
            .HasColumnName("custom_task_type")
            .HasMaxLength(100);

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description");

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.AssignedByUserId)
            .HasColumnName("assigned_by_user_id")
            .IsRequired();

        builder.Property(x => x.AssignedToUserId)
            .HasColumnName("assigned_to_user_id");

        builder.Property(x => x.AssignedToRole)
            .HasColumnName("assigned_to_role")
            .HasMaxLength(100);

        builder.Property(x => x.AssignedAt)
            .HasColumnName("assigned_at");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date");

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at");

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(x => x.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(x => x.CancellationReason)
            .HasColumnName("cancellation_reason");

        builder.Property(x => x.BlockingReason)
            .HasColumnName("blocking_reason");

        builder.Property(x => x.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(100);

        builder.Property(x => x.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.HasMany(x => x.StateHistory)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Dependencies)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Watchers)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TimeEntries)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RequiredSops)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RequiredTrainings)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SiteId, x.Status })
            .HasDatabaseName("ix_tasks_site_status");

        builder.HasIndex(x => new { x.SiteId, x.AssignedToUserId })
            .HasDatabaseName("ix_tasks_site_assigned_user");

        builder.HasIndex(x => x.DueDate)
            .HasDatabaseName("ix_tasks_due_date");
    }
}
