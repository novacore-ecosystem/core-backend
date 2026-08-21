using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NovaCore.Chat.Persistence.Configs;

public sealed class ConversationQueueItemConfig : IEntityTypeConfiguration<ConversationQueueItem>
{
    public void Configure(EntityTypeBuilder<ConversationQueueItem> builder)
    {
        // Table
        builder.ToTable("conversation_queue_items");

        // Properties
        builder.HasKey(x => new { x.QueueId, x.ConversationId });

        builder.Property(x => x.EnqueuedAt).IsRequired();
        builder.Property(x => x.Priority).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();

        // Relationships
        builder.HasOne(x => x.Queue)
            .WithMany()
            .HasForeignKey(x => x.QueueId)
            .OnDelete(DeleteBehavior.Cascade);

        // ConversationId points at Conversation, kept as a plain id (no navigation) - a queue
        // placement is not owned by the Conversation aggregate.
        builder.HasOne<Conversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.Status);

        // Audit & Concurrency
        builder.ConfigureCommonFields();
    }
}
