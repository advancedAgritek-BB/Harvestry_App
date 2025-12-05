using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class ConversationParticipantEntityConfiguration : IEntityTypeConfiguration<ConversationParticipantRecord>
{
    public void Configure(EntityTypeBuilder<ConversationParticipantRecord> builder)
    {
        builder.ToTable("conversation_participants");

        builder.HasKey(x => x.ConversationParticipantId);
        builder.Property(x => x.ConversationParticipantId).HasColumnName("conversation_participant_id");
        builder.Property(x => x.ConversationId).HasColumnName("conversation_id");
        builder.Property(x => x.SiteId).HasColumnName("site_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Role).HasColumnName("role");
        builder.Property(x => x.JoinedAt).HasColumnName("joined_at");
        builder.Property(x => x.LastReadAt).HasColumnName("last_read_at");

        builder.HasIndex(x => new { x.ConversationId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_conversation_participants_conversation_user_unique");

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
