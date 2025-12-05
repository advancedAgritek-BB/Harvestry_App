using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskBlueprintEntityConfiguration : IEntityTypeConfiguration<TaskBlueprintRecord>
{
    public void Configure(EntityTypeBuilder<TaskBlueprintRecord> builder)
    {
        builder.ToTable("task_blueprints");

        builder.HasKey(x => x.TaskBlueprintId);

        builder.Property(x => x.TaskBlueprintId)
            .HasColumnName("task_blueprint_id");

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(x => x.GrowthPhase)
            .HasColumnName("growth_phase")
            .IsRequired();

        builder.Property(x => x.RoomType)
            .HasColumnName("room_type")
            .IsRequired();

        builder.Property(x => x.StrainId)
            .HasColumnName("strain_id");

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.TimeOffsetTicks)
            .HasColumnName("time_offset_ticks")
            .IsRequired();

        builder.Property(x => x.AssignedToRole)
            .HasColumnName("assigned_to_role")
            .HasMaxLength(100);

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

        builder.HasIndex(x => x.SiteId)
            .HasDatabaseName("ix_task_blueprints_site_id");

        builder.HasIndex(x => new { x.SiteId, x.GrowthPhase, x.RoomType, x.IsActive })
            .HasDatabaseName("ix_task_blueprints_matching");

        builder.HasMany(x => x.RequiredSops)
            .WithOne(x => x.TaskBlueprint)
            .HasForeignKey(x => x.TaskBlueprintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RequiredTrainings)
            .WithOne(x => x.TaskBlueprint)
            .HasForeignKey(x => x.TaskBlueprintId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class TaskBlueprintRequiredSopEntityConfiguration : IEntityTypeConfiguration<TaskBlueprintRequiredSopRecord>
{
    public void Configure(EntityTypeBuilder<TaskBlueprintRequiredSopRecord> builder)
    {
        builder.ToTable("task_blueprint_required_sops");

        builder.HasKey(x => x.TaskBlueprintRequiredSopId);

        builder.Property(x => x.TaskBlueprintRequiredSopId)
            .HasColumnName("task_blueprint_required_sop_id");

        builder.Property(x => x.TaskBlueprintId)
            .HasColumnName("task_blueprint_id")
            .IsRequired();

        builder.Property(x => x.SopId)
            .HasColumnName("sop_id")
            .IsRequired();

        builder.HasIndex(x => new { x.TaskBlueprintId, x.SopId })
            .IsUnique()
            .HasDatabaseName("ux_task_blueprint_required_sops_unique");
    }
}

public sealed class TaskBlueprintRequiredTrainingEntityConfiguration : IEntityTypeConfiguration<TaskBlueprintRequiredTrainingRecord>
{
    public void Configure(EntityTypeBuilder<TaskBlueprintRequiredTrainingRecord> builder)
    {
        builder.ToTable("task_blueprint_required_trainings");

        builder.HasKey(x => x.TaskBlueprintRequiredTrainingId);

        builder.Property(x => x.TaskBlueprintRequiredTrainingId)
            .HasColumnName("task_blueprint_required_training_id");

        builder.Property(x => x.TaskBlueprintId)
            .HasColumnName("task_blueprint_id")
            .IsRequired();

        builder.Property(x => x.TrainingModuleId)
            .HasColumnName("training_module_id")
            .IsRequired();

        builder.HasIndex(x => new { x.TaskBlueprintId, x.TrainingModuleId })
            .IsUnique()
            .HasDatabaseName("ux_task_blueprint_required_trainings_unique");
    }
}

