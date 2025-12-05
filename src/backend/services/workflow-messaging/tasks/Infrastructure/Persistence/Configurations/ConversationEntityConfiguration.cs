using Harvestry.Tasks.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harvestry.Tasks.Infrastructure.Persistence.Configurations;

public sealed class ConversationEntityConfiguration : IEntityTypeConfiguration<ConversationRecord>
{
    public void Configure(EntityTypeBuilder<ConversationRecord> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(x => x.ConversationId);
        builder.Property(x => x.ConversationId).HasColumnName("conversation_id");
        builder.Property(x => x.SiteId).HasColumnName("site_id");
        builder.Property(x => x.ConversationType).HasColumnName("conversation_type");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(500);
        builder.Property(x => x.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(100);
        builder.Property(x => x.RelatedEntityId).HasColumnName("related_entity_id");
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.LastMessageAt).HasColumnName("last_message_at");

        builder.HasMany(x => x.Participants)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
