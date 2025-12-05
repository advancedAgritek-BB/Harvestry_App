using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class SlackNotificationEntityConfiguration : IEntityTypeConfiguration<SlackNotificationRecord>
{
    public void Configure(EntityTypeBuilder<SlackNotificationRecord> builder)
    {
        builder.ToTable("slack_notification_queue", schema: "tasks");

        builder.HasKey(x => x.SlackNotificationId);

        builder.Property(x => x.SlackNotificationId)
            .HasColumnName("slack_notification_id");

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

        builder.Property(x => x.NotificationType)
            .HasColumnName("notification_type")
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RequestId)
            .HasColumnName("request_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(x => x.MaxAttempts)
            .HasColumnName("max_attempts")
            .IsRequired();

        builder.Property(x => x.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => new { x.SiteId, x.Status })
            .HasDatabaseName("ix_slack_notification_queue_site_status");

        builder.HasIndex(x => new { x.RequestId, x.SlackWorkspaceId, x.ChannelId })
            .IsUnique()
            .HasDatabaseName("ux_slack_notification_queue_request");

        builder.HasIndex(x => x.NextAttemptAt)
            .HasDatabaseName("ix_slack_notification_queue_next_attempt")
            .HasFilter("status IN (0,3)");
    }
}
