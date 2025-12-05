using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskLibraryItemEntityConfiguration : IEntityTypeConfiguration<TaskLibraryItemRecord>
{
    public void Configure(EntityTypeBuilder<TaskLibraryItemRecord> builder)
    {
        builder.ToTable("task_library_items");

        builder.HasKey(x => x.TaskLibraryItemId);

        builder.Property(x => x.TaskLibraryItemId)
            .HasColumnName("task_library_item_id");

        builder.Property(x => x.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(x => x.DefaultPriority)
            .HasColumnName("default_priority")
            .IsRequired();

        builder.Property(x => x.TaskType)
            .HasColumnName("task_type")
            .IsRequired();

        builder.Property(x => x.CustomTaskType)
            .HasColumnName("custom_task_type")
            .HasMaxLength(100);

        builder.Property(x => x.DefaultAssignedToRole)
            .HasColumnName("default_assigned_to_role")
            .HasMaxLength(100);

        builder.Property(x => x.DefaultDueDaysOffset)
            .HasColumnName("default_due_days_offset");

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.OrgId)
            .HasDatabaseName("ix_task_library_items_org_id");

        builder.HasIndex(x => new { x.OrgId, x.IsActive })
            .HasDatabaseName("ix_task_library_items_org_active");

        builder.HasMany(x => x.DefaultSops)
            .WithOne(x => x.TaskLibraryItem)
            .HasForeignKey(x => x.TaskLibraryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TaskLibraryItemSopEntityConfiguration : IEntityTypeConfiguration<TaskLibraryItemSopRecord>
{
    public void Configure(EntityTypeBuilder<TaskLibraryItemSopRecord> builder)
    {
        builder.ToTable("task_library_item_sops");

        builder.HasKey(x => x.TaskLibraryItemSopId);

        builder.Property(x => x.TaskLibraryItemSopId)
            .HasColumnName("task_library_item_sop_id");

        builder.Property(x => x.TaskLibraryItemId)
            .HasColumnName("task_library_item_id")
            .IsRequired();

        builder.Property(x => x.SopId)
            .HasColumnName("sop_id")
            .IsRequired();

        builder.HasIndex(x => new { x.TaskLibraryItemId, x.SopId })
            .IsUnique()
            .HasDatabaseName("ux_task_library_item_sops_unique");
    }
}

