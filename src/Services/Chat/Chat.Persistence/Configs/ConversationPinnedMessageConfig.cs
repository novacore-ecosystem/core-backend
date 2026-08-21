using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationPinnedMessageConfig : IEntityTypeConfiguration<ConversationPinnedMessage>
{
    public void Configure(EntityTypeBuilder<ConversationPinnedMessage> builder)
    {
        // Table
        builder.ToTable("conversation_pinned_messages");

        // Properties
        builder.HasKey(x => new { x.ConversationId, x.MessageId });

        builder.Property(x => x.PinnedByUserId).IsRequired();
        builder.Property(x => x.PinnedAt).IsRequired();

        // Relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.PinnedMessages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // MessageId points at Message (a separate, independently-queried aggregate root - see
        // Message.cs) - no navigation exposed, matching MessageReference's own reasoning.
        builder.HasOne<Message>()
            .WithMany()
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.MessageId);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
