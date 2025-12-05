using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskRequiredTrainingEntityConfiguration : IEntityTypeConfiguration<TaskRequiredTrainingRecord>
{
    public void Configure(EntityTypeBuilder<TaskRequiredTrainingRecord> builder)
    {
        builder.ToTable("task_required_training");

        builder.HasKey(x => x.TaskRequiredTrainingId);

        builder.Property(x => x.TaskRequiredTrainingId)
            .HasColumnName("task_required_training_id");

        builder.Property(x => x.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(x => x.TrainingModuleId)
            .HasColumnName("training_module_id")
            .IsRequired();

        builder.HasIndex(x => new { x.TaskId, x.TrainingModuleId })
            .IsUnique()
            .HasDatabaseName("ux_task_required_training_unique");
    }
}
