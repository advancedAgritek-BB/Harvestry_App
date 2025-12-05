using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class SlackChannelMappingEntityConfiguration : IEntityTypeConfiguration<SlackChannelMappingRecord>
{
    public void Configure(EntityTypeBuilder<SlackChannelMappingRecord> builder)
    {
        builder.ToTable("slack_channel_mappings", schema: "tasks");

        builder.HasKey(x => x.SlackChannelMappingId);

        builder.Property(x => x.SlackChannelMappingId)
            .HasColumnName("slack_channel_mapping_id");

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.SlackWorkspaceId)
            .HasColumnName("slack_workspace_id")
            .IsRequired();

        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ChannelName)
            .HasColumnName("channel_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .HasColumnName("notification_type")
            .HasMaxLength(50)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<NotificationType>(v, true))
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => new { x.SlackWorkspaceId, x.ChannelId, x.NotificationType })
            .IsUnique();
    }
}
