using Harvestry.Tasks.Domain.Enums;
using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class SlackMessageBridgeLogEntityConfiguration : IEntityTypeConfiguration<SlackMessageBridgeLogRecord>
{
    public void Configure(EntityTypeBuilder<SlackMessageBridgeLogRecord> builder)
    {
        builder.ToTable("slack_message_bridge_log", schema: "tasks");

        builder.HasKey(x => x.SlackMessageBridgeLogId);

        builder.Property(x => x.SlackMessageBridgeLogId)
            .HasColumnName("slack_message_bridge_log_id");

        builder.Property(x => x.SiteId)
            .HasColumnName("site_id")
            .IsRequired();

        builder.Property(x => x.SlackWorkspaceId)
            .HasColumnName("slack_workspace_id")
            .IsRequired();

        builder.Property(x => x.InternalMessageId)
            .HasColumnName("internal_message_id");

        builder.Property(x => x.InternalMessageType)
            .HasColumnName("internal_message_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SlackChannelId)
            .HasColumnName("slack_channel_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SlackMessageTs)
            .HasColumnName("slack_message_ts")
            .HasMaxLength(50);

        builder.Property(x => x.SlackThreadTs)
            .HasColumnName("slack_thread_ts")
            .HasMaxLength(50);

        builder.Property(x => x.RequestId)
            .HasColumnName("request_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<SlackMessageBridgeStatus>(v, true))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.LastAttemptAt)
            .HasColumnName("last_attempt_at");

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message");

        builder.Property(x => x.SentAt)
            .HasColumnName("sent_at");

        builder.HasIndex(x => new { x.RequestId, x.SlackWorkspaceId })
            .IsUnique();

        builder.HasIndex(x => x.InternalMessageId);
    }
}
